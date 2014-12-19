using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization;

public class LightRailGame : MonoBehaviour 
{
	[HideInInspector,NonSerialized]
	public List<Train> trainList = new List<Train>();
	[HideInInspector,NonSerialized]
	public bool paused = false;	
	[HideInInspector,NonSerialized]
	public IIncident ClickedIncident;

	public static int Difficulty = 6;

	public event Action<GameObject> OnSelectedGameObjectChanged;
	public GameObject SelectedGameObject { get; private set; }
	private Action<Train> selectedTrainPathChangeAction;
	
	public readonly LineDrawMaster LineMaster = LineDrawMaster.getInstance();

	private LineRenderer selectionLine;

	[NonSerialized]
	private LineOptions LineOpts;

	private Graph _graph;
	[HideInInspector]
	public Graph graph { 
		get { return _graph ?? (_graph = GameObject.FindObjectOfType<Graph> ()); } 
		set { _graph = value; } 
	}
	
	private static ScoreManager _scoreManager;
	[HideInInspector]
	public static ScoreManager ScoreManager { 
		get { return _scoreManager ?? (_scoreManager = GameObject.FindObjectOfType<ScoreManager> ()); } 
		set { _scoreManager = value; } 
	}

	private static EdgeRaycaster _edgeRaycaster;
	[HideInInspector]
	public static EdgeRaycaster EdgeRaycaster { 
		get { return _edgeRaycaster ?? (_edgeRaycaster = GameObject.FindObjectOfType<EdgeRaycaster> ()); } 
		set { _edgeRaycaster = value; } 
	}

	// TODO Roger move this field to ScoreManager
	[HideInInspector,NonSerialized]
	public ObstacleMaster Obstacles;

	private Mouse mouse = new Mouse ();

	public Transform Train;

	public Transform WarningPrefab;

	// Set Line for Unity to package in Build
	public Material LineRendererMaterial;

	[SerializeField]
	public List<LineSchedule> Schedule = new List<LineSchedule> ();

	[NonSerialized]
	private GameObject Knot;
	
	// Use this for initialization
	void Start () {
		QualitySettings.antiAliasing = 4;

		Knot = GameObject.Find ("ReRouteKnot");
		Knot.SetActive (false);

		if (LineRendererMaterial == null)
			Debug.LogWarning ("You did not set the Material of the LineRenderer. Please go to the Inspector of the LightRailGame object and set its material");
	
		LineOpts = new LineOptions {
			materials = new [] { LineRendererMaterial },
			widths = new [] { .8f, .8f },
			colors = new [] { Color.blue, Color.blue },
			offset = Vector3.back
		};

		// Do not show FPS in non-dev Build
		GameObject.Find ("FPS").SetActive (Debug.isDebugBuild);

		// Initialize obstacle's
		// TODO Rogier: move this constructor to ScoreManager
		Obstacles = gameObject.GetComponent<ObstacleMaster>() ?? gameObject.AddComponent<ObstacleMaster> ();
		Obstacles.init (obstacle => {
			Debug.Log("An obstacle was placed.");
		},obstacle => {
			Debug.Log("An obstacle was actioned by the user.");
		},obstacle => {
			Debug.Log("An obstacle was resolved.");
			ScoreManager.Score++;
		});

		StartGame ();
	}

	/**
	 * Handle mouse/scrolling/events
	 */
	static Train _train;
	void FixedUpdate () {
		mouse.OnFrame ();

		// Do scrolling
		if(Input.mouseScrollDelta.magnitude > 0){
			Camera.main.orthographicSize -= Input.mouseScrollDelta.y;
			Camera.main.orthographicSize = Math.Max(3f, Camera.main.orthographicSize);
		}

		// Show edge dragger
		if (SelectedGameObject != null){
			if(LightRailGame.EdgeRaycaster.CurrentHover != null) {
				Knot.transform.position = Camera.main.WorldToScreenPoint(LightRailGame.EdgeRaycaster.CurrentHover.pos);
				Screen.showCursor = false;
				Knot.SetActive(true);
				if(mouse.Events.Any() && (_train = SelectedGameObject.GetComponent<Train>())){
					HandleKnotClick(mouse.Events.Dequeue(), LightRailGame.EdgeRaycaster.CurrentHover.Edge, _train);
				}
			} else {
				Knot.SetActive(false);
				Screen.showCursor = true;
			}
		}

		// Handle all mouse events
		while (mouse.Events.Any()) {
			var e = mouse.Events.Dequeue();

			// Handle panning
			var speed = 0.5f * Camera.main.orthographicSize / 100f;
			var background = this.GetComponentAtScreen2DPosition<BoxCollider2D>(e.position);
			if(background != null && background.gameObject.name == "Quad"){
				var lastPos = e.position;
				e.OnDrag += (Vector3 newPos) => {
					// Pan background using the new mouse position
					var diff = newPos - lastPos;
					Camera.main.transform.Translate(-diff.x*speed, -diff.y*speed, 0, Space.Self);
					lastPos = newPos;
				};
				return;
			}
		}
	}

	public void DoSelect(GameObject obj){
		if (SelectedGameObject != null) RequestDeselect ();

		SelectedGameObject = obj;
		if (OnSelectedGameObjectChanged != null)
			OnSelectedGameObjectChanged (obj);

		var train = obj.GetComponent<Train>();
		if(train != null){
			ILine line = null;
			selectedTrainPathChangeAction = changedTrain => {
				if(line != null) 
					LineMaster.HideLine(line);
				line = new CombinedLine(changedTrain.Path.AsEnumerable().Cast<ILine>());
				LineMaster.ShowLine(line, LineOpts);

				AddKnots(train.WayPoints);
			};
			train.OnPathChange += selectedTrainPathChangeAction;
			selectedTrainPathChangeAction(train);
		}
	}

	// Allows remote request of deselection
	public void RequestDeselect ()
	{
		this.OnDeselect ();
	}
	
	private void OnDeselect(){
		var train = this.SelectedGameObject.GetComponent<Train>();
		if (train != null) {
			LineMaster.RemoveAll ();
			this.CancelReroute (train);
			train.OnPathChange -= selectedTrainPathChangeAction;
		}
		this.SelectedGameObject = null;

		if (OnSelectedGameObjectChanged != null)
			OnSelectedGameObjectChanged (null);
	}

	// Draw menu's
	void OnGUI(){
		// Handle Obstacle clicks
		if (ClickedIncident != null) {
			// If user chooses an action this is true
			if(ClickedIncident.IncidentGUI()){
				var obs = (ClickedIncident as ObstacleBlockage);
				if(obs!=null)
					obs.Subject().GetComponent<Obstacle>().DoUserAction();
				ClickedIncident = null;
			}
		}
	}

	// On start: form routes, add trams
	private void StartGame(){
		var edges = graph.edges.ToArray ();

		foreach (LineSchedule line in Schedule) {
			var path = line.RouteFromWayPoints(edges);

			// Add Trams
			if(path.Count > 0){
				var totalLength = path.Sum(e => e.GetUnitLength());
				var segment = (1f / line.TramCount) * totalLength;
				for(int i = 0; i < line.TramCount; i++){
					var model = Instantiate(Train, Vector3.zero, Quaternion.LookRotation(Vector3.down)) as Transform;
					model.name = line.Name + " Tram "+(i+1);
					model.localScale = new Vector3(2, 2, 2);
					model.parent = this.transform;
					Train train = model.GetComponent<Train>();
					train.Init (line, path, segment * i);
					trainList.Add(train);
				}
			}
		}
	}

	private T GetComponentAtScreenPosition<T> (Vector3 position, bool increaseTouchRadius = false) where T : Component{
		Ray ray = Camera.main.ScreenPointToRay( position );
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit)) {
			return hit.collider.GetComponent<T>() ?? hit.collider.GetComponentInParent<T>();
		}
		if (increaseTouchRadius) {
			return 
				GetComponentAtScreenPosition<T>(position+new Vector3(.2f,.2f), false) ??
				GetComponentAtScreenPosition<T>(position+new Vector3(.2f,0), false) ??
				GetComponentAtScreenPosition<T>(position+new Vector3(0,0), false) ?? 
                GetComponentAtScreenPosition<T>(position+new Vector3(0,.2f), false);
		}
		return null;
	}

	private T GetComponentAtScreen2DPosition<T> (Vector3 position) where T : Component{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);

		if (hit.collider != null)
		{
			return hit.collider.GetComponent<T>() ?? hit.collider.GetComponentInParent<T>();
		}     
		return null;
	}

	public static LightRailGame GetInstance(){
		return GameObject.FindObjectOfType<LightRailGame>();
	}

	void HandleKnotClick(MouseEvent evt, Edge currentEdge, Train train){
		Node from = null; Node to = null;
		int edgeIndex = train.Path.IndexOf(currentEdge);
		// Loop back, find previous WayPoint
		for (int i = (edgeIndex-1+train.Path.Count)%train.Path.Count; i < edgeIndex || i > edgeIndex; i = (i-1+train.Path.Count)%train.Path.Count) {
			if(train.WayPoints.Contains(train.Path[i].To)){
				from = train.Path[i].To;
				break;
			}
		}
		// Loop forward, find next WayPoint
		for (int i = (edgeIndex+1)%train.Path.Count; i < edgeIndex || i > edgeIndex; i = (i+1)%train.Path.Count) {
			Debug.Log (i);
			if(train.WayPoints.Contains(train.Path[i].From)){
				to = train.Path[i].From;
				break;
			}
		}

		var edges = currentEdge.graph.edges.ToList ();
		ILine reroute = null;
		Edge lastEdge = currentEdge;

		evt.OnDrag += (Vector3 obj) => {
			// If we can Snap
			if(LightRailGame.EdgeRaycaster.CurrentHover != null){
				if(lastEdge == LightRailGame.EdgeRaycaster.CurrentHover.Edge)
					return;

				lastEdge = LightRailGame.EdgeRaycaster.CurrentHover.Edge;

				Debug.Log ("From: "+from);
				Debug.Log ("Visiting: "+lastEdge.From + " and " +lastEdge.To);
				Debug.Log ("To: "+to);
				var a = new Dijkstra<Edge,Node>(edges).PlanRoute(from, lastEdge.To);
				var b = new Dijkstra<Edge,Node>(edges).PlanRoute(lastEdge.To, from);

				// Clear previous
				if(reroute != null) LineDrawMaster.getInstance().HideLine(reroute);
				reroute = new CombinedLine(a.Concat(b).Cast<ILine>());
				LineDrawMaster.getInstance().ShowLine(reroute, LineOpts);
				Debug.Log ("Patch path = "+a+b);
			} 
			// Cannot Snap
			else {
				lastEdge = currentEdge;
				Knot.transform.position = obj;
				var c = Camera.main.ScreenToWorldPoint(obj).FixZ(currentEdge.To.position.z);
				// Clear previous
				if(reroute != null) LineDrawMaster.getInstance().HideLine(reroute);
				reroute = new CombinedLine(new [] {
					new StraightLine(from.position, c),
					new StraightLine(c, to.position)
				});
				LineDrawMaster.getInstance().ShowLine(reroute, new LineOptions {
					materials = new [] { LineRendererMaterial },
					widths = new [] { .8f, .8f },
					colors = new [] { Color.blue, Color.blue },
					offset = Vector3.back
				});
			}
		};
		evt.OnRelease += (Vector3 obj) => {
			if(reroute != null) LineDrawMaster.getInstance().HideLine(reroute);
			Debug.LogWarning("REROUTED!!");
		};
	}

	void AddKnots (IList<Node> wayPoints)
	{
		foreach(Node n in wayPoints){
			var knot = Instantiate(Knot, n.position, Quaternion.identity) as GameObject;
			knot.SetActive (true);
		}
	}
}

