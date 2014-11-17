using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Node2 : MonoBehaviour {

	public Graph graph;

	public Vector3 position {
		get {
			return gameObject.transform.position;
		}
		set {
			gameObject.transform.position = value;
			var m = graph.edges.Where (e => e != null).Where (e => e.From == this || e.To == this).ToList ();
			var h = graph.edges.Select (e => e == null ? 5 : (e.From == null ? 1 : 0) + (e.To == null ? 1 : 0)).Sum (a => a);
			m.ForEach(e => e.UpdatePositions());
			Debug.Log ("Found "+m.Count+" edges to update, and "+(h / graph.edges.Count())+" positions per edge are null....");
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