using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System;

public interface IEdge<TNode> where TNode : class {
	TNode From { get; }
	TNode To { get; }
	float Cost { get; }
}

public class Edge : BezierSpline, IEdge<Node2>
{
	[SerializeField]
	private Node2 _from;
	[SerializeField]
	private Node2 _to;

	private bool highlighted;
	private LineRenderer highlight;

	public float Cost { get { return this.GetLength (); } }

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
		EditorUtility.SetDirty (this);
	}

	public void SetHighlighted(bool highlighted){
		if (!highlighted) {
			highlight.enabled = false;
		} else {
			if(this.highlight == null){
				var go = new GameObject();
				go.transform.parent = this.transform;
				this.highlight = go.AddComponent<LineRenderer>();
				this.highlight.material.color = Color.red;
				this.highlight.SetVertexCount(100);
				for(int i = 0; i < 100; i++){
					this.highlight.SetPosition(i, this.GetPoint(i/100f)+Vector3.back);
				}
			}
			this.highlight.enabled = true;
		}
		this.highlighted = highlighted;
	}
}