using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Node : MonoBehaviour {

	public GameObject nextNode;
	private LineRenderer line;
	private Vector3 from;
	private Vector3 to;

	// Use this for initialization
	void Start () {
		from = this.transform.position;
		if (nextNode != null)
			to = nextNode.transform.position;
		else
			to = from;

		CreateLine ();
	}

	void CreateLine(){
		from = Vector3.zero;
		to = Vector3.zero;

		line = gameObject.GetComponent<LineRenderer> ();
		if (line == null) {
			line = gameObject.AddComponent("LineRenderer") as LineRenderer;
		}
		line.SetVertexCount (nextNode == null ? 1 : 2);

		line.SetColors (new Color(255,0,0,100), new Color(0,255,0,100));
		line.SetVertexCount (2);
		line.SetWidth(0.3f, 0.3f);
	}
	
	// Update is called once per frame
	void Update () {
		if (line == null || !line.isVisible) {
			CreateLine ();
		}

		if (from != this.transform.position) {
			from = this.transform.position;
			line.SetPosition(0, from);
		}
		if (nextNode != null && to != nextNode.transform.position) {
			to = nextNode.transform.position;
			line.SetPosition(1, to);
		}
	}

	public Node PositionOnLineReturningOverflow(float unitsFromStart, out float newUnitsFromStart, out Vector3 position, out Vector3 destination){
		position = to;
		destination = to;

		if (to == from) {
			newUnitsFromStart = 0;
			return this;
		}

		float overflow = unitsFromStart - (to - from).magnitude;

		if (overflow > 0 && nextNode != null && nextNode.GetComponent<Node> () != null) {
			return nextNode.GetComponent<Node> ().PositionOnLineReturningOverflow (overflow, out newUnitsFromStart, out position, out destination);
		} else if (overflow > 0) {
			position = to;
			newUnitsFromStart = (to - from).magnitude;
			return this;
		}

		newUnitsFromStart = unitsFromStart;
		position = from + (to - from).normalized * unitsFromStart;
		return this;
	}
	
}