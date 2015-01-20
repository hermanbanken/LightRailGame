using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;

public class TrainCollisionDetector : MonoBehaviour
{
	Train reportTo;
	Subject<Collision> _onEnter = new Subject<Collision>();
	Subject<Collision> _onExit  = new Subject<Collision>();
	IObservable<Collision> OnEnter { get { return _onEnter; } }
	IObservable<Collision> OnExit { get { return _onExit; } }

	void Start(){
		OnEnter.Subscribe (col => {
			var obs = col.gameObject.GetComponentInParent<Obstacle> ();
			if (obs != null) {
				// Debug.Log("Collision with obstacle with "+gameObject.name + " with "+col.gameObject.name);
				reportTo.Incident(obs.Incident);
			}
			
			var other = col.gameObject.GetComponentInParent<Train> ();
			if (other != null) {
				// Debug.Log("Collision on "+gameObject.name + " with "+col.gameObject.name);
				var incident = new TrainCollisionBlockage(reportTo, other, col);
				OnExit.Where(c => c.gameObject == col.gameObject).Take(1).Subscribe(c => incident.CollisionEnded());
				reportTo.Incident(incident);
			}
		});
	}

	void OnCollisionEnter(Collision col){
		if(!LightRailGame.DemoKey())
		_onEnter.OnNext (col);
	}

	void OnCollisionExit(Collision col){
		_onExit.OnNext (col);
	}

	public void ReportTo (Train train)
	{
		reportTo = train;
	}
}

