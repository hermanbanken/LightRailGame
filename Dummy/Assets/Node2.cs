using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Node2 : MonoBehaviour {

	private Graph graph;

	public Vector3 position {
		get {
			return gameObject.transform.position;
		}
		set {
			gameObject.transform.position = value;
		}
	}

	public void SetGraph (Graph g)
	{
		this.graph = g;
	}

	public Graph GetGraph (){
		if (this.graph == null)
			this.graph = this.gameObject.GetComponentInParent<Graph> ();
		return this.graph;
	}

}