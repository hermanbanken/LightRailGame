using UnityEngine;
using System.Collections;
using System.Linq;

public class EdgeRaycaster : MonoBehaviour {

	public Graph graph;
	
	const float loose = 4f;
	const float strict = 1f;

	private float z = 0;
	private bool hit = false;
	private Hover hover = new Hover {};
	public Hover CurrentHover { 
		get {
			return hit ? hover : null;
		}
	}

	// Prevent allocations
	private float outT;
	private Vector3 outP;
	private Vector3 mp;

	// Use this for initialization
	void Start () {
		z = graph.edges.First().GetPoint(0f).z;
	}
	
	// Update is called once per frame
	void Update () {
		if (mp == MP || (mp - MP).magnitude < 1f)
			return;

		// Tight loop: only check current Edge
		if (hit && (hit = hover.Edge.TryGetClosestPoint (MP, loose, out outT, out outP))) {
			hover.t = outT;
			hover.pos = outP;
		}

		// Limit more expensive computation to 1/4-th the frame-rate
		if (Time.frameCount % 4 != 0) return;

		mp = MP;
		hover.Edge = graph.edges.MinBy (l => l.TryGetClosestPoint (mp, strict, out outT, out outP) ? (mp - outP).magnitude : float.MaxValue);
		hit = hover.Edge.TryGetClosestPoint (mp, strict, out outT, out outP);
		if (hit) {
			hover.t = outT;
			hover.pos = outP;
		}
	}

	private Vector3 MP {
		get {
			return EdgeSpace (Input.mousePosition);
		}
	}
	private Vector3 EdgeSpace(Vector3 cameraSpace){
		return Camera.main.ScreenToWorldPoint(cameraSpace).FixZ(z);
	}

	public class Hover {
		public Edge Edge { get; internal set; }
		public float t { get; internal set; }
		public Vector3 pos { get; internal set; }
	}
}
