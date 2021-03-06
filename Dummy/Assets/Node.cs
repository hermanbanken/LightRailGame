﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Node : MonoBehaviour {
	
	private Graph _graph;
	private GUIElement button;
	
	// Cache traffic light
	private TrafficLight _tl;
	public TrafficLight TrafficLight {
		get { return _tl; }
	}
	// Cache station
	private Station _station;
	public Station Station {
		get { return _station; }
	}

	private IEnumerable<Edge> _outgoing;

	public void Start() {
		_tl = this.GetComponent<TrafficLight> ();
		_station = this.GetComponent<Station> ();
		_outgoing = graph.edges.Where (e => e.From == this).ToArray();
	}

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

	public IEnumerable<Edge> OutGoing(){
		return _outgoing ?? (_outgoing = graph.edges.Where (e => e.From == this).ToArray());
	}

	public override bool Equals (object o)
	{
		Node a = o as Node;
		return null != a && this.GetInstanceID () == a.GetInstanceID ();
	}

	public static bool operator ==(Node a, Node b){
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

	public static bool operator !=(Node a, Node b){
		return !(a == b);
	}

	public override int GetHashCode ()
	{
		return GetInstanceID ();
	}
}