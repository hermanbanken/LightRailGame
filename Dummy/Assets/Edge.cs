using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public interface IEdge<TNode> where TNode : class {
	TNode From { get; }
	TNode To { get; }
	float Cost { get; }
}

public interface ILine {
	float GetUnitLength();
	Vector3 GetUnitPosition(float t);
}

public class Edge : BezierSpline, IEdge<Node>, ILine
{
	[SerializeField]
	private Node _from;
	[SerializeField]
	private Node _to;

	public float Cost { get { return this.GetLength (); } }

	public Node From {
		get { return _from; }
		set {
			// Convert Node position to World-relative-space and then to Spline-relative-space (this)
			Vector3 pos = this.transform.InverseTransformPoint(value.graph.transform.TransformPoint(value.position));

			if(value == null || this.points[0] == pos)
				return;
			this.points[0] = pos;
			_from = value;
		}
	}
	public Node To  {
		get { return _to; }
		set { 
			// Convert Node position to World-relative-space and then to Spline-relative-space (this)
			Vector3 pos = this.transform.InverseTransformPoint(value.graph.transform.TransformPoint(value.position));

			if(value == null || this.points[this.points.Length-1] == pos)
				return;
			this.points[this.points.Length-1] = pos;
			_to = value;
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
			}
		}
	}
	
	public void Awake(){
		if (this.points == null)
			this.Reset ();
		this.GetLength ();
	}

	public void Start(){
		var go = new GameObject ();
		go.transform.parent = this.transform;
		var deco = go.AddComponent<MeshAlongSpline> ();
		deco.spline = this;
		deco.frequency = (int)Math.Round(4 * this.GetLength ());
		MeshRenderer r = go.AddComponent<MeshRenderer> ();
		deco.Mesh = go.AddComponent<MeshFilter> ();
		//r.material.color = Color.yellow;
		r.material.mainTexture = Resources.Load<Texture2D>("rail");
		deco.Awake ();
	}
	
	public void Straighten ()
	{
		var step = (points[points.Length-1] - points[0]) / (this.points.Length - 1);
		for (int i = 1; i < this.points.Length - 1; i++) {
			this.points[i] = this.points[0] + i * step;
		}
	}

	#region ILine implementation
	public float GetUnitLength ()
	{
		return this.GetLength ();
	}
	Vector3 ILine.GetUnitPosition (float t)
	{
		return this.GetPoint(this.GetPositionOfUnitPoint(t));
	}
	#endregion

	public void Reverse ()
	{
		this.points = this.points.Reverse<Vector3> ().ToArray ();
		var f = this._from;
		this._from = this._to;
		this._to = f;
	}
	
	public override bool Equals (object o)
	{
		Edge a = o as Edge;
		return a != null && this.GetInstanceID () == a.GetInstanceID ();
	}

	public static bool operator ==(Edge a, Edge b){
		if (System.Object.ReferenceEquals(a, b))
		{
			return true;
		}
		
		// If one is null, but not both, return false.
		if (((object)a == null) || ((object)b == null))
		{
			return false;
		}
		
		// Return true if the fields match:
		return a.Equals(b);
	}
}