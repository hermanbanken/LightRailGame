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
			if(value == null || value == _from)
				return;
			this.points[0] = value.transform.position;
			_from = value;
			EditorUtility.SetDirty(this);
		}
	}
	public Node2 To  {
		get { return _to; }
		set { 
			if(value == null || value == _to)
				return;
			Debug.Log ("Setting <To> Node2 on Edge");
			this.points[this.points.Length-1] = value.transform.position;
			_to = value;
			EditorUtility.SetDirty(this);
		}
	}

	public void UpdatePositions ()
	{
		if (this.points [0] != _from.transform.position || this.points [this.points.Length - 1] != _to.transform.position) {
			Debug.Log ("Updating positions");
			this.points [0] = _from.transform.position;
			this.points [this.points.Length - 1] = _to.transform.position;
			EditorUtility.SetDirty(this);
		}
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