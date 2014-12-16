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
			if(reportTo.incident == null){
				var incident = new TrainCollisionBlockage(reportTo, other);
				reportTo.Incident(incident);
			}
		}
	}

	void OnCollisionStay(Collision col){
		reportTo.speed = 0;
	}

	public void ReportTo (Train train)
	{
		reportTo = train;
	}
}

