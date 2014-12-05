using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class LightRailGame : MonoBehaviour {

	public bool paused = false;
	private Train selected;
	private Action<Train> selectedTrainPathChangeAction;

	public readonly LineDrawMaster LineMaster = LineDrawMaster.getInstance();

	private LineRenderer selectionLine;
	public Graph graph;
	public ObstacleMaster Obstacles;

	private Mouse mouse = new Mouse ();

	public Transform Train;

	// Set Line for Unity to package in Build
	public Material LineRendererMaterial;

	// Use this for initialization
	void Start () {
		if (LineRendererMaterial == null)
			Debug.LogWarning ("You did not set the Material of the LineRenderer. Please go to the Inspector of the LightRailGame object and set its material");
	
		// Do not show FPS in non-dev Build
		GameObject.Find ("FPS").SetActive (Debug.isDebugBuild);

		// Get Graph
		graph = GameObject.FindObjectOfType<Graph> ();

		// Initialize obstacle's
		Obstacles = gameObject.GetComponent<ObstacleMaster>() ?? gameObject.AddComponent<ObstacleMaster> ();
		Obstacles.init (obstacle => {
			Debug.Log("An obstacle was placed.");
		},obstacle => {
			Debug.Log("An obstacle was actioned by the user.");
		},obstacle => {
			Debug.Log("An obstacle was resolved.");
		});

		StartGame ();
	}

	// Update is called once per frame
	void FixedUpdate () {
		mouse.OnFrame ();
		while (mouse.Events.Any()) {
			var e = mouse.Events.Dequeue();

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
	}

	private void StartGame(){
		var routes = graph.Cycles ();
		foreach (IList<Node> route in routes) {
			Debug.Log ("Cycle of length "+route.Count+": "+route.Select (n => n.name).Aggregate ("", (f, n) => f.Length == 0 ? n : f + "," + n));
			var edges = route.Concat(route.Take(1)).EachPair((a, b) => {
				var edge = graph.edges.FirstOrDefault(e => e.From == a && e.To == b);
				if(edge == null) Debug.Log ("No edge from "+a+ " to " + b);
				return edge;
			}).ToList();
			Debug.Log ("Of the cycle "+edges.Count(e => e == null)+" edges were not defined");

			if(edges.Count(e => e == null) > 0) 
				continue;

			GameObject go = new GameObject();
			var model = Instantiate(Train, Vector3.zero, Quaternion.LookRotation(Vector3.down)) as Transform;
			model.localScale = new Vector3(3, 3, 3);
			model.parent = go.transform;
			Train train = go.AddComponent<Train>();
			train.Path = edges.ToList();
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

//	private void DragStart(Vector3 position){
//		Ray ray = Camera.main.ScreenPointToRay( position );
//		RaycastHit hit;
//		if(Physics.Raycast(ray, out hit)){
//			Train select = hit.collider.GetComponentInParent<Train>();
//			Debug.Log ("Hit @ " + position + " : " + (select == null ? "no train" : select.name));
//			if(select != null){
//				if(selected != null)
//					DragEnd(position);
//				paused = true;
//				selected = select;
//				selectionIsRound = false;
//				updateDragPath();
//			}
//		}
//	}
//
//	public void DragMove(Vector3 position){
//		Ray ray = Camera.main.ScreenPointToRay( position );
//		RaycastHit hit;
//		if(Physics.Raycast(ray, out hit)){
//			Node node = hit.collider.gameObject.GetComponent<Node>();
//			if(node != null && (selectedWaypoints.Count == 0 || selectedWaypoints[selectedWaypoints.Count-1] != node)){
//				selectedWaypoints.Add(node);
//				selectionIsRound = false;
//				updateDragPath();
//			}
//			Train train = hit.collider.GetComponentInParent<Train>();
//			if(train != null && selected == train && !selectionIsRound){
//				selectionIsRound = true;
//				updateDragPath();
//			}
//		}
//	}
//
//	private void DragEnd(Vector3 position){
//		if (selectionIsRound) {
//			selected.SetPath(selectedWaypoints.Select(wp => wp.gameObject).ToList());
//			Debug.Log ("Changed path!!!!" + selectedWaypoints);
//		}
//
//		if(selected != null)
//			selected.Deselect();
//		selected = null;
//		paused = false;
//		selectedWaypoints = new List<Node> ();
//		selectionIsRound = false;
//		updateDragPath ();
//	}
//
//	private void updateDragPath ()
//	{
//		var up = new Vector3 (0, 0, -0.25f);
//		selectionLine.SetVertexCount (selected == null ? 0 : selectedWaypoints.Count + 1 + (selectionIsRound ? 1 : 0));
//		if (selected != null) {
//			int i = 0;
//			selectionLine.SetPosition(i++, selected.gameObject.transform.position + up);
//			foreach(Node p in selectedWaypoints){
//				selectionLine.SetPosition(i++, p.gameObject.transform.position + up);
//			}
//			if(selectionIsRound){
//				selectionLine.SetPosition(i, selected.transform.position + up);
//			}
//		}
//	}
}
