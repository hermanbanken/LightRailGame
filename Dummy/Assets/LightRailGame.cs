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

	// Use this for initialization
	void Start () {
		selectionLine = gameObject.GetComponent<LineRenderer> ();
		if (selectionLine == null) {
			selectionLine = gameObject.AddComponent("LineRenderer") as LineRenderer;
		}
		selectionLine.SetVertexCount (0);
		selectionLine.SetColors(new Color(0,0,255,10), new Color(0,0,255,200));
		selectionLine.SetWidth(0.5f, 0.5f);
	}

	// Update is called once per frame
	void Update () {
		HandleTouches ();
	}

	private void HandleTouches(){
		if (Input.touchCount > 0){
			if(Input.GetTouch (0).phase == TouchPhase.Began) 
				DragStart (Input.GetTouch(0).position);
			if (Input.GetTouch (0).phase == TouchPhase.Moved)
				DragMove (Input.GetTouch(0).position);
			if (Input.GetTouch(0).phase == TouchPhase.Ended)
				DragEnd(Input.GetTouch(0).position);
		}

		if (mouseDown) {
			DragMove (Input.mousePosition);
		}

		if (Input.GetMouseButtonDown (0)) {
			mouseDown = true;
			if(selected == null)
				DragStart (Input.mousePosition);
		}

		if (selected != null && Input.mousePresent && Input.GetMouseButtonUp (0)) {
			mouseDown = false;
			DragEnd(Input.mousePosition);
		}

	}

	private void DragStart(Vector3 position){
		Ray ray = Camera.main.ScreenPointToRay( position );
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)){
			Train select = hit.collider.GetComponentInParent<Train>();
			Debug.Log ("Hit @ " + position + " : " + (select == null ? "no train" : select.name));
			if(select != null){
				if(selected != null)
					DragEnd(position);
				paused = true;
				selected = select;
				selectionIsRound = false;
				updateDragPath();
			}
		}
	}

	public void DragMove(Vector3 position){
		Ray ray = Camera.main.ScreenPointToRay( position );
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)){
			Node node = hit.collider.gameObject.GetComponent<Node>();
			if(node != null && (selectedWaypoints.Count == 0 || selectedWaypoints[selectedWaypoints.Count-1] != node)){
				selectedWaypoints.Add(node);
				selectionIsRound = false;
				updateDragPath();
			}
			Train train = hit.collider.GetComponentInParent<Train>();
			if(train != null && selected == train && !selectionIsRound){
				selectionIsRound = true;
				updateDragPath();
			}
		}
	}

	private void DragEnd(Vector3 position){
		if (selectionIsRound) {
			selected.SetPath(selectedWaypoints.Select(wp => wp.gameObject).ToList());
			Debug.Log ("Changed path!!!!" + selectedWaypoints);
		}

		if(selected != null)
			selected.Deselect();
		selected = null;
		paused = false;
		selectedWaypoints = new List<Node> ();
		selectionIsRound = false;
		updateDragPath ();
	}

	private void updateDragPath ()
	{
		var up = new Vector3 (0, 0, -0.25f);
		selectionLine.SetVertexCount (selected == null ? 0 : selectedWaypoints.Count + 1 + (selectionIsRound ? 1 : 0));
		if (selected != null) {
			int i = 0;
			selectionLine.SetPosition(i++, selected.gameObject.transform.position + up);
			foreach(Node p in selectedWaypoints){
				selectionLine.SetPosition(i++, p.gameObject.transform.position + up);
			}
			if(selectionIsRound){
				selectionLine.SetPosition(i, selected.transform.position + up);
			}
		}
	}
}
