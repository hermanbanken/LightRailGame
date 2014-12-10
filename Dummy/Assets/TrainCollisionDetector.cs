using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TrainCollisionDetector : MonoBehaviour
{
	Train reportTo;
	
	void OnCollisionEnter(Collision col){
		var obs = col.gameObject.GetComponentInParent<Obstacle> ();
		if (obs != null) {
			Debug.Log("Collision with obstacle with "+gameObject.name + " with "+col.gameObject.name);
			reportTo.speed = 0;
			reportTo.Incident(obs.Incident);
		}

		var other = col.gameObject.GetComponentInParent<Train> ();
		if (other != null) {
			Debug.Log("Collision on "+gameObject.name + " with "+col.gameObject.name);
			reportTo.speed = 0f;
			reportTo.Incident(new TrainCollisionBlockage(reportTo, other));
		}
	}

	public void ReportTo (Train train)
	{
		reportTo = train;
	}
}

