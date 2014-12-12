using System;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour, IStop {
	private IDictionary<Train,float> Presence = new Dictionary<Train, float> ();
	private float stopTime = 5f;

	#region IStop implementation

	public void Arrive (Train train)
	{
		if(!IsPresent(train))
			Presence [train] = Time.time;
	}

	public bool TryLeave (Train train)
	{
		if(!Presence.ContainsKey(train))
			throw new InvalidOperationException ("The train can't leave a station he is not at");

		if (Presence [train] + stopTime < Time.time) {
			Presence.Remove (train);
			return true;
		} else {
			return false;
		}
	}

	public bool IsPresent (Train train)
	{
		return Presence.ContainsKey (train);
	}

	public float MaxSpeed (Train train)
	{
		//if (IsPresent (train))
			return 0f;
		//return 0.5f;
	}

	#endregion

}
