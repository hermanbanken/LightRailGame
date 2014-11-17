using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Edge : BezierSpline
{
	public Node2 From;
	public Node2 To;

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