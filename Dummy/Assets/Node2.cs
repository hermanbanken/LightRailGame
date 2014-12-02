using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Node2 : MonoBehaviour {
	
	private Graph _graph;
	private GUIElement button;

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
			// Update positions of connected Edges
			foreach(Edge e in graph.edges.Where (e => e != null).Where (e => e.From == this || e.To == this))
				e.UpdatePositions();
		}
	}
	
	public bool SelectableGUI(){
		var w = 20f;
		var sp = Camera.main.WorldToScreenPoint(this.position + this.transform.parent.position);
		return GUI.Button (new Rect (sp.x - w/2, Screen.height - (sp.y + w/2), w, w), "");
	}
}