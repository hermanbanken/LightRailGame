using System;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour, IStop {
	private IList<Train> Presence = new List<Train> ();
	// The TrafficLight that might be green after this one turns red
	public TrafficLight Next;

	// Last time we changed colors
	public float lastChanged = 0;
	// Current color
	public TrafficLightState State = TrafficLightState.Green;

	void Reset() {
		Next = null;
	}

	void Start() {
		if (lastChanged == 0)
			SetGreen (this);
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