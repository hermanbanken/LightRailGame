using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Eppy;

public interface IEdge<TNode> where TNode : class {
	TNode From { get; }
	TNode To { get; }
	float Cost { get; }
}

public interface ILine {
	float GetUnitLength();
	Vector3 GetUnitPosition(float t);
	bool TryGetClosestPoint(Vector3 other, float maxDistance, out float t, out Vector3 pos);
}

public interface IKnowWhoIsHere : ILine {
	IEnumerable<T> GetOccupants<T>() where T : IOccupy;
	IEnumerable<IOccupy> GetOccupants();
	void Arrive<T>(T who) where T : IOccupy;
	void Leave<T>(T who) where T : IOccupy;
}

public interface IOccupy {
	Tuple<ILine,float> Location { get; }
	float Speed { get; }
}

public class Edge : BezierSpline, IEdge<Node>, ILine, IKnowWhoIsHere
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
		/* Create Visuals */
		var go = new GameObject ();
		go.transform.parent = this.transform;
		var deco = go.AddComponent<MeshAlongSpline> ();
		deco.spline = this;
		deco.frequency = (int)Math.Round(4 * this.GetLength ());
		MeshRenderer r = go.AddComponent<MeshRenderer> ();
		deco.Mesh = go.AddComponent<MeshFilter> ();
		r.material.mainTexture = LightRailGame.GetInstance ().RailTexture;//Resources.Load<Texture2D>("rail");
		r.material.shader = LightRailGame.GetInstance ().RailShader;//Shader.Find ("Transparent/Cutout/Diffuse");
		deco.Awake ();

		Occupants = new Dictionary<Type, IList<IOccupy>>();
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
		return null != a && this.GetInstanceID () == a.GetInstanceID ();
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

	public static bool operator !=(Edge a, Edge b){
		return !(a == b);
	}
	
	public override int GetHashCode ()
	{
		return GetInstanceID ();
	}

	#region IKnowWhoIsHere implementation
	private IDictionary<Type,IList<IOccupy>> Occupants = new Dictionary<Type, IList<IOccupy>>();

	public IEnumerable<T> GetOccupants<T> () where T : IOccupy
	{
		IList<IOccupy> objs;
		if (Occupants != null && Occupants.TryGetValue (typeof(T), out objs)) {
			return objs.Cast<T>();
		}
		return new T[0];
	}

	public IEnumerable<IOccupy> GetOccupants ()
	{
		if (Occupants == null)
			Debug.LogError ("Occupants is null");
		return Occupants.SelectMany (p => p.Value).ToArray ();
	}

	public void Arrive<T> (T who) where T : IOccupy
	{
		IList<IOccupy> objs;
		if (Occupants == null) {
			Occupants = new Dictionary<Type, IList<IOccupy>>();
		}

		if (!Occupants.TryGetValue (typeof(T), out objs)) {
			Occupants [typeof(T)] = new List<IOccupy> () { who };
		} else {
			objs.Add(who);
		}
	}

	public void Leave<T> (T who) where T : IOccupy
	{
		if(Occupants != null && Occupants.ContainsKey(typeof(T)) && Occupants[typeof(T)].Contains(who))
			Occupants [typeof(T)].Remove (who);
	}

	#endregion

	#region ILine implementation

	public bool TryGetClosestPoint (Vector3 other, float maxDistance, out float t, out Vector3 pos)
	{
		// TODO pre-calculate Edge bounding square and pre-check if that matches the point 
		return base.TryGetClosestPoint (other, maxDistance, out t, out pos);
	}

	#endregion
}