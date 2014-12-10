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
}