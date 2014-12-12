using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrafficLight : MonoBehaviour, IStop {
	private IList<Train> Presence = new List<Train> ();
	// The TrafficLight that might be green after this one turns red
	public TrafficLight Next;

	// Last time we changed colors
	public float lastChanged = 0;
	// Current color
	public TrafficLightState State = TrafficLightState.Green;
	public GameObject quad;
	public GameObject sphere;
	public Color haha = new Color(1f, 0.7f, 0f);
	void Reset() {
		Next = null;
	}

	void Start() {
		if (lastChanged == 0)    
			SetGreen (this);
	

		var temp = this.gameObject.GetComponent<Node> ();
		var directi = temp.graph.edges.FirstOrDefault (e => e.From == temp).GetDirection(0f);
		var posi = gameObject.transform.position  + 2.5f * Vector3.Cross ( directi, Vector3.forward);

		quad = GameObject.CreatePrimitive (PrimitiveType.Quad);

		quad.transform.parent = gameObject.transform;
		quad.transform.position = posi + 4f * Vector3.back;
		quad.transform.localScale = 2f * Vector3.one;
		quad.renderer.material.color = Color.black;
		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.parent = quad.transform;
		sphere.transform.localScale = 0.6f * Vector3.one;
		sphere.transform.localPosition = Vector3.zero + 2.4f * Vector3.back;;
		Destroy (sphere.collider);
		
	}
	void Update(){
		switch (State){
		case TrafficLightState.Red:
			sphere.renderer.material.color = Color.red;
			break;
		case TrafficLightState.Orange:
			sphere.renderer.material.color = haha;
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