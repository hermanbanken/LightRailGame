using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LightRailGame : MonoBehaviour {

	public bool paused = false;
	private Train selected;
	private IList<Node> selectedWaypoints = new List<Node>();

	private LineRenderer selectionLine;
	private bool selectionIsRound = false;
	private bool mouseDown = false;
	public Graph graph;

	private Mouse mouse = new Mouse ();

	public Transform Train;

	// Use this for initialization
	void Start () {
		selectionLine = gameObject.GetComponent<LineRenderer> ();
		if (selectionLine == null) {
			selectionLine = gameObject.AddComponent("LineRenderer") as LineRenderer;
		}
		selectionLine.SetVertexCount (0);
		selectionLine.SetColors(new Color(0,0,255,10), new Color(0,0,255,200));
		selectionLine.SetWidth(0.5f, 0.5f);

		graph = GameObject.FindObjectOfType<Graph> ();
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
					train.Path.ForEach((ed) => ed.SetHighlighted(true));
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
		selected.Path.ForEach((ed) => ed.SetHighlighted(false));
		this.CancelReroute(selected);
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
			IEnumerable<Edge> edges = route.Select((n, i) => graph.edges.First(e => e.From == n && e.To == route[(i+1)%route.Count]));

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
