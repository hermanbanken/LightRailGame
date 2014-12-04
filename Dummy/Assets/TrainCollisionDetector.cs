using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TrainCollisionDetector : MonoBehaviour
{
	Train reportTo;
	
	void OnCollisionEnter(Collision col){
		if (col.gameObject.GetComponentInParent<Train> () != null) {
			Debug.Log("Collision on "+gameObject.name + " with "+col.gameObject.name);
			col.gameObject.GetComponentInParent<Train>().speed = 0.0f;
			reportTo.speed = -0.1f;
		}
	}

	public void ReportTo (Train train)
	{
		reportTo = train;
	}
}

