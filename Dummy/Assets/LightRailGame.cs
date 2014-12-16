using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization;

public class LightRailGame : MonoBehaviour {

	[HideInInspector,NonSerialized]
	public bool paused = false;	
	[HideInInspector,NonSerialized]
	public IIncident ClickedIncident;
	
	private Train selected;
	private Action<Train> selectedTrainPathChangeAction;
	
	public readonly LineDrawMaster LineMaster = LineDrawMaster.getInstance();

	private LineRenderer selectionLine;

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
	
	// Use this for initialization
	void Start () {
		QualitySettings.antiAliasing = 4;

		if (LineRendererMaterial == null)
			Debug.LogWarning ("You did not set the Material of the LineRenderer. Please go to the Inspector of the LightRailGame object and set its material");
	
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
			ScoreManager.score++;
		});

		StartGame ();
	}

	/**
	 * Handle mouse/scrolling/events
	 */
	void FixedUpdate () {
		mouse.OnFrame ();

		// Do scrolling
		if(Input.mouseScrollDelta.magnitude > 0){
			Camera.main.orthographicSize -= Input.mouseScrollDelta.y;
			Camera.main.orthographicSize = Math.Max(3f, Camera.main.orthographicSize);
		}

		// Handle all mouse events
		while (mouse.Events.Any()) {
			var e = mouse.Events.Dequeue();

			// Handle panning
			var speed = 0.1f;
			var background = this.GetComponentAtScreen2DPosition<BoxCollider2D>(e.position);
			if(background != null && background.gameObject.name == "Quad"){
				var lastPos = e.position;
				e.OnDrag += (Vector3 newPos) => {
					// Pan background using the new mouse position
					var diff = newPos - lastPos;
					Camera.main.transform.Translate(-diff.x*speed, -diff.y*speed, 0, Space.World);
					lastPos = newPos;
				};
				return;
			}

			// Handle train clicks
			var train = GetComponentAtScreenPosition<Train>(e.position, true);
			if(train != null){
				// Select
				if(selected == null){
					selected = train;
					ILine line = null;
					selectedTrainPathChangeAction = changedTrain => {
						if(line != null) 
							LineMaster.HideLine(line);
						line = new CombinedLine(changedTrain.Path.AsEnumerable().Cast<ILine>());
						LineMaster.ShowLine(line, new LineOptions {
							materials = new [] { LineRendererMaterial },
							widths = new [] { .6f, .6f },
							colors = new [] { Color.blue, Color.red },
							offset = Vector3.back
						});
					};
					train.OnPathChange += selectedTrainPathChangeAction;
					selectedTrainPathChangeAction(train);
				}
				// Deselect
				else if(selected == train){
					OnDeselect();
				}
			}

			// Handle GUI element clicks
			var gui = GetComponentAtScreenPosition<GUIElement>(e.position, true);
			if(gui != null){
				Debug.Log("GUI ELEMENT CLICKED!");
			}

			paused = selected != null;
		}
	}

	// Allows remote request of deselection
	public void RequestDeselect ()
	{
		this.OnDeselect ();
	}
	
	private void OnDeselect(){
		LineMaster.RemoveAll ();
		this.CancelReroute(selected);
		selected.OnPathChange -= selectedTrainPathChangeAction;
		selected = null;
		paused = selected != null;
	}

	// Draw menu's
	void OnGUI(){
		if (selected != null)
			this.TrainGUI (selected);
		// Handle Obstacle clicks
		else if (ClickedIncident != null) {
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
					GameObject go = new GameObject();
					var model = Instantiate(Train, Vector3.zero, Quaternion.LookRotation(Vector3.down)) as Transform;
					model.localScale = new Vector3(2, 2, 2);
					model.parent = go.transform;
					Train train = go.AddComponent<Train>();
					train.Init (line, path, segment * i);
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
		Ray ray = Camera.main.ScreenPointToRay( position );
		RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);

		if (hit!=null && hit.collider != null)
		{
			return hit.collider.GetComponent<T>() ?? hit.collider.GetComponentInParent<T>();
		}     
		return null;
	}

	public static LightRailGame GetInstance(){
		return GameObject.FindObjectOfType<LightRailGame>();
	}

}

