using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrafficLight : MonoBehaviour, IStop {
	private IList<Train> Presence = new List<Train> ();
	// The TrafficLight that might be green after this one turns red
	public TrafficLight Next;
	public TrafficLight Slave;

	// Last time we changed colors
	[HideInInspector]
	public float lastChanged = 0;
	// Current color
	[HideInInspector]
	public TrafficLightState State = TrafficLightState.Green;

	// Visual stuff
	private GameObject quad;
	private GameObject sphere;
	public readonly static Color orange = new Color(1f, 0.7f, 0f);

	void Reset() {
		Next = null;
	}

	void Start() {
		if (lastChanged == 0)    
			SetGreen (this);

		var node = this.gameObject.GetComponent<Node> ();
		var edge = node.graph.edges.FirstOrDefault (e => e.From == node || e.To == node);
		Vector3 dir;
		if(edge != null){
			dir = edge.GetDirection(edge.From == node ? 0f : 1f);
		} else {
			dir = Vector3.up;
		}
		var pos = gameObject.transform.position  + 2.5f * Vector3.Cross (dir, Vector3.forward);
		quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
		quad.transform.parent = gameObject.transform;
		quad.transform.position = pos + 4f * Vector3.back;
		quad.transform.localScale = 2f * Vector3.one;
		quad.renderer.material.color = Color.black;
		Destroy (quad.collider);

		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.parent = quad.transform;
		sphere.transform.localScale = 0.6f * Vector3.one;
		sphere.transform.localPosition = Vector3.zero;
		Destroy (sphere.collider);
	}

	void Update(){
		switch (State){
		case TrafficLightState.Red:
			sphere.renderer.material.color = Color.red;
			break;
		case TrafficLightState.Orange:
			sphere.renderer.material.color = orange;
			break;
		default:
			sphere.renderer.material.color = Color.green;
			break;
		}
	}

	/**
	 * Detect if we can change from:
	 *   green -> orange, 
	 *   orange -> red, 
	 * and, if no Next is defined, 
	 *   red -> green
	 */
	void FixedUpdate() {
		if (lastChanged + Duration(State) < Time.time)
		{
			if(State == TrafficLightState.Green)
				State = TrafficLightState.Orange;
			else if(State == TrafficLightState.Orange){
				State = TrafficLightState.Red;
				if(Next != null)
					Next.SetGreen(Next);
			} else if(Next == null && State == TrafficLightState.Red){
				State = TrafficLightState.Green;
			}

			if (Slave != null) {
				Slave.State = State;
				Slave.lastChanged = Time.time;
			}
			lastChanged = Time.time;
		}
	}
	
	/**
	 * Duration of the various states
	 */
	private float Duration(TrafficLightState s){
		switch (s) {
			case TrafficLightState.Green: return 4f;
			case TrafficLightState.Orange: return 1f;
			default: 
				return 3f;
		}
	}

	/**
	 * Allows traffic lights to receive the trigger to start being green
	 */
	public void SetGreen(TrafficLight greenOne){
		// Bubble, (but prevent loop)
		if (Next != null && Next != greenOne) {
			Next.SetGreen (greenOne);
		}

		if (greenOne == this) {
			State = TrafficLightState.Green;
			if (Slave != null) 
				Slave.State = TrafficLightState.Green;
		}
		lastChanged = Time.time;
	}

	#region IStop implementation

	public void Arrive (Train train)
	{
		Presence.Add (train);
	}

	public bool IsPresent (Train train)
	{
		return Presence.Contains (train);
	}

	public bool TryLeave (Train train)
	{
		if (State != TrafficLightState.Red) {
			Presence.Remove (train);
			return true;
		} else
			return false;
	}

	public float MaxSpeed (Train train)
	{
		if (State == TrafficLightState.Green)
			return 10f;
		else
			return 0f;
	}

	#endregion

	public enum TrafficLightState {
		Red, Green, Orange
	}
}