using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Node2 : MonoBehaviour {
	
	private Graph _graph;
	public Graph graph {
		get { 
			if(this._graph == null){
				this._graph = GameObject.FindObjectOfType<Graph>();
			}
			return this._graph;
		}
		set {
			this._graph = value;
		}
	}

	public Vector3 position {
		get {
			return gameObject.transform.position;
		}
		set {
			gameObject.transform.position = value;
			var m = graph.edges.Where (e => e != null).Where (e => e.From == this || e.To == this).ToList ();
			m.ForEach(e => e.UpdatePositions());
			var h = graph.edges.Select (e => e == null ? 5 : (e.From == null ? 1 : 0) + (e.To == null ? 1 : 0)).Sum (a => a);
			//Debug.Log ("Found "+m.Count+" edges to update, and "+(h / graph.edges.Count())+" positions per edge are null....");
		}
	}

}