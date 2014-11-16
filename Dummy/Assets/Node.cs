using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class Node : MonoBehaviour {

	public GameObject nextNode;
	public List<GameObject> nextNodes = new List<GameObject> ();
	public Material RailWayMaterial;
	private IEnumerable<LineRenderer> lines = new List<LineRenderer>();
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

	void Reset(){
		nextNodes = new List<GameObject> ();
	}

	void CreateLine(){
		if (nextNodes.Count == 0)
			nextNodes.Add (nextNode);

		lines = nextNodes.Select ((go, i) => {
			var lineParent = i == 0 ? gameObject : new GameObject(gameObject.name + " extra line", new [] { typeof(LineRenderer) });
			var line = lineParent.GetComponent<LineRenderer> () ?? lineParent.AddComponent("LineRenderer") as LineRenderer;
			line.SetVertexCount(2);
			line.material = RailWayMaterial;
			line.SetWidth(0.3f, 0.3f);
			line.SetPosition(0, this.transform.position);
			line.SetPosition(1, go.transform.position);
			return line;
		}).ToList();
	}
	
	// Update is called once per frame
	void Update () {
		if (lines == null) {
			CreateLine ();
		}
		return;
//		if (from != this.transform.position) {
//			from = this.transform.position;
//			line.SetPosition(0, from);
//		}
//		if (nextNode != null && to != nextNode.transform.position) {
//			to = nextNode.transform.position;
//			line.SetPosition(1, to);
//		}
	}

	// Previous way of moving: using the defined line between a waypoint and the next waypoint
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