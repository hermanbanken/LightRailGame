﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Node : MonoBehaviour {
	
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

	public void Start(){
		// TODO remove this random assingment if Pawel add new complex Map's
		if(UnityEngine.Random.value > 0.5)
			gameObject.AddComponent<Station> ();
		else 
			gameObject.AddComponent<TrafficLight> ();
	}

	public override String ToString(){
		return this.gameObject.name;
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

	public Vector2 screenPosition(){
		var sp = Camera.main.WorldToScreenPoint (this.position + this.transform.parent.position);
		return new Vector2 (sp.x, sp.y);
	}
	
	public bool SelectableGUI(){
		var w = 20f;
		var sp = screenPosition();
		GUI.Label(new Rect (sp.x - w/2, Screen.height - (sp.y + w/2), w, w), this.ToString().Replace("(Node)", "").Replace("Node ", ""));
		return GUI.Button (new Rect (sp.x - w/2, Screen.height - (sp.y + w/2), w, w), "");
	}
}