using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization;
using Eppy;
using UnityEngine.UI;

public class LightRailGame : MonoBehaviour 
{
	[HideInInspector,NonSerialized]
	public bool paused = false;	
	[HideInInspector,NonSerialized]
	public IIncident ClickedIncident;
	public event Action<IIncident> OnIncidentMenuOpen;

	public static int Difficulty = 6;

	public event Action<GameObject> OnSelectedGameObjectChanged;
	public GameObject SelectedGameObject { get; private set; }
	private Action<Train> selectedTrainPathChangeAction;

	public LineDrawMaster LineMaster { get; private set; }

	private LineRenderer selectionLine;

	[NonSerialized]
	private LineOptions LineOpts;
	[NonSerialized]
	private LineOptions LineOptsReRoute;

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

	public Texture RailTexture;
	public Shader RailShader;

	[SerializeField]
	public List<LineSchedule> Schedule = new List<LineSchedule> ();

	[NonSerialized]
	private GameObject Knot;

	[NonSerialized]
	private Tuple<GameObject,Node>[] WPKnots;
	
	[NonSerialized]
	private BoxCollider2D Background;
	
	// Use this for initialization
	void Start () {
		QualitySettings.antiAliasing = 4;

		LineMaster = LineDrawMaster.getInstance ();
		
		Background = GameObject.Find ("Quad").GetComponent<BoxCollider2D> ();
		
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
		LineOptsReRoute = new LineOptions {
			materials = new [] { LineRendererMaterial },
			widths = new [] { .8f, .8f },
			colors = new [] { Color.green, Color.green },
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
	
	void Update(){
		UpdateKnots ();
	}
	
	/**
     * Handle mouse/scrolling/events
     */
	static Train _train;
	void FixedUpdate () {
		mouse.OnFrame ();
		
		//      // Do scrolling
		Camera.main.orthographicSize -= Input.mouseScrollDelta.y;
		if(Input.mouseScrollDelta.y > 0){
			Camera.main.orthographicSize = Math.Max(Camera.main.orthographicSize,20f);
		}
		else{
			Camera.main.orthographicSize = Math.Min(Camera.main.orthographicSize,Background.bounds.size.x / Camera.main.aspect / 2 );
		}
		if (Input.mouseScrollDelta.y != 0)
			FixCameraPosition (Vector3.zero, 0);
		
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
					FixCameraPosition (diff, speed);
					lastPos = newPos;
				};
				return;
			}
		}
	}

	private float rightmenuoffset;
	void FixCameraPosition (Vector3 diff, float speed)
	{

		var c_w = Camera.main.orthographicSize * Camera.main.aspect;
		var c_h = Camera.main.orthographicSize;
		var pos = Camera.main.transform.position;
		pos.x = Math.Max (Background.bounds.min.y  + c_w, Math.Min (Background.bounds.max.x - c_w + rightmenuoffset, pos.x - diff.x * speed));
		pos.y = Math.Max (Background.bounds.min.y + c_h  , Math.Min (Background.bounds.max.y - c_h + (10*Camera.main.orthographicSize/88), pos.y - diff.y * speed));
		Camera.main.transform.position = pos;

		OnSelectedGameObjectChanged += (GameObject obj) => {
			rightmenuoffset = (obj == null) ? 0 :30*Camera.main.orthographicSize* Camera.main.aspect/88 ;
		};
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
				line = new CombinedLine<Edge>(changedTrain.Path.AsEnumerable());
				LineMaster.ShowLine(line, LineOpts);
				RemoveKnots();
				AddKnots(train, train.WayPoints);
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

		RemoveKnots ();
		
		if (train != null) {
			LineMaster.RemoveAll ();
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

	public void ShowMenu(IIncident inc){
		ClickedIncident = inc;
		if(OnIncidentMenuOpen != null)
			OnIncidentMenuOpen (inc);
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
				}
			}
		}

//		Test ();

		// List all Traffic Light dependencies
		IDictionary<TrafficLight,TrafficLight> slaveToMaster = new Dictionary<TrafficLight, TrafficLight> ();
		foreach (TrafficLight tl in GameObject.FindObjectsOfType<TrafficLight>()) {
			if(slaveToMaster.ContainsKey(tl))
				continue;

			// Take until recursion
			var list = tl.SelectMany((l, i) => l.OrderBy(o => o == tl ? 1 : 0)).TakeWhile(o => o != tl);
			if(!list.Any()){
				// Should never happen, fix Scene when occurs!!
				Debug.LogError (tl + " controls nothing ("+tl.SelectMany(l=>l).Take(10).Select(t => t.name).Aggregate("", (a,b) => a+b)+")");
				continue;
			}

			foreach(var o in list){
				if(slaveToMaster.ContainsKey(o))
					slaveToMaster[slaveToMaster[o]] = tl;
				slaveToMaster[o] = tl;
			}
		}

		// Start master traffic lights
		foreach (var g in slaveToMaster.GroupBy(p => p.Value)) {
//			Debug.Log (g.Count()+" Traffic Lights under control by "+g.Key);
			g.Key.StartAsMaster();
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
			if(train.WayPoints.Contains(train.Path[i].From)){
				to = train.Path[i].From;
				break;
			}
		}

		ILine reroute = null;
		Edge lastEdge = currentEdge;

		evt.OnDrag += (Vector3 obj) => {
			// If we can Snap
			if(LightRailGame.EdgeRaycaster.CurrentHover != null){
				if(lastEdge == LightRailGame.EdgeRaycaster.CurrentHover.Edge)
					return;

				lastEdge = LightRailGame.EdgeRaycaster.CurrentHover.Edge;

				IEnumerable<Edge> a = graph.Dijkstra.PlanRoute(from, lastEdge.To);
				IEnumerable<Edge> b = graph.Dijkstra.PlanRoute(lastEdge.To, to);

				// Clear previous
				if(reroute != null) LineMaster.HideLine(reroute);
				reroute = new CombinedLine<Edge>(a.Concat(b));
				LineMaster.ShowLine(reroute, LineOptsReRoute);
			} 
			// Cannot Snap
			else {
				lastEdge = currentEdge;
				Knot.transform.position = obj;
				var c = Camera.main.ScreenToWorldPoint(obj).FixZ(currentEdge.To.position.z);
				// Clear previous
				if(reroute != null) LineMaster.HideLine(reroute);
				reroute = new CombinedLine<StraightLine>(new [] {
					new StraightLine(from.position, c),
					new StraightLine(c, to.position)
				});
				LineMaster.ShowLine(reroute, LineOptsReRoute);
			}
		};
		evt.OnRelease += (Vector3 obj) => {
			Debug.LogWarning("Released Re-route");
			if(reroute != null) LineMaster.HideLine(reroute);
			if(LightRailGame.EdgeRaycaster.CurrentHover != null && (lastEdge == LightRailGame.EdgeRaycaster.CurrentHover.Edge) && lastEdge != currentEdge){
				Debug.LogWarning("Re-route is in finished state");
				var newWP = train.WayPoints.Where(n=>true).ToList();
				if(LightRailGame.EdgeRaycaster.CurrentHover.t < 0.5)
					newWP.Insert (newWP.IndexOf(from), lastEdge.From);
				else 
					newWP.Insert (newWP.IndexOf(from), lastEdge.To);
				train.UpdatePath(newWP);
			}
		};
	}

	void AddKnots (Train train, IList<Node> wayPoints)
	{
		var controls = GameObject.Find ("LRG_Controls");
		// TODO cleanup this ugly mess
		WPKnots = wayPoints.ToList().Select ((wp, i) => {
			var knot = Instantiate(Knot, Camera.main.WorldToScreenPoint(wp.position), Quaternion.identity) as GameObject;
			knot.transform.SetParent(controls.transform, true);
			knot.SetActive (true);
			knot.GetComponent<Button>().onClick.AddListener(() => {
				if(train.WayPoints.Count() > 2){
					train.UpdatePath(train.WayPoints.Where(w => w != wp).ToList());
				}
			});
			return Tuple.Create(knot, wp);
		}).ToArray();
	}

	void RemoveKnots(){
		if(WPKnots != null)
		foreach (Tuple<GameObject,Node> p in WPKnots) {
			if(p.Item1 != null){
				p.Item1.SetActive(false);
				Destroy (p.Item1);
			}
		}
		WPKnots = new Tuple<GameObject, Node>[0];
	}

	void UpdateKnots (){
		if(WPKnots != null)
		foreach (Tuple<GameObject,Node> p in WPKnots) {
			if(p.Item1 != null)
				p.Item1.transform.position = Camera.main.WorldToScreenPoint(p.Item2.position);
		}
	}
}

