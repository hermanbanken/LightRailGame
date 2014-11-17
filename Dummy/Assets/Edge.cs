using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Edge : BezierSpline
{
	private Node2 _from;
	private Node2 _to;
	public Node2 From {
		get { return _from; }
		set { 
			if(value == null)
				return;
			this.points[0] = value.transform.position;
			_from = value;
		}
	}
	public Node2 To  {
		get { return _to; }
		set { 
			if(value == null)
				return;
			this.points[this.points.Length-1] = value.transform.position;
			_to = value;
		}
	}

	public void UpdatePositions ()
	{
		Debug.Log ("Updating positions");
		this.points[0] = _from.transform.position;
		this.points[this.points.Length-1] = _to.transform.position;
	}

	private Graph graph;

	public void SetGraph (Graph g)
	{
		this.graph = g;
	}

	public Graph GetGraph ()
	{
		if (this.graph == null)
			this.graph = this.gameObject.GetComponentInParent<Graph> ();
		return this.graph;
	}

}