using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class Edge : BezierSpline
{
	[SerializeField]
	private Node2 _from;
	[SerializeField]
	private Node2 _to;

	public Node2 From {
		get { return _from; }
		set {
			// Convert Node position to World-relative-space and then to Spline-relative-space (this)
			Vector3 pos = this.transform.InverseTransformPoint(value.graph.transform.TransformPoint(value.position));

			if(value == null || this.points[0] == pos)
				return;
			this.points[0] = pos;
			_from = value;
			EditorUtility.SetDirty(this);
		}
	}
	public Node2 To  {
		get { return _to; }
		set { 
			// Convert Node position to World-relative-space and then to Spline-relative-space (this)
			Vector3 pos = this.transform.InverseTransformPoint(value.graph.transform.TransformPoint(value.position));

			if(value == null || this.points[this.points.Length-1] == pos)
				return;
			this.points[this.points.Length-1] = pos;
			_to = value;
			EditorUtility.SetDirty(this);
		}
	}

	public void UpdatePositions ()
	{
		To = To;
		From = From;
	}

	private Graph _graph;
	public Graph graph {
		get { 
			if (this._graph == null) {
				this._graph = this.gameObject.GetComponentInParent<Graph> ();
			}
			return this._graph;
		}
		set {
			if(this._graph != value){
				this._graph = value;
				EditorUtility.SetDirty(this);
			}

		}
	}
	
}