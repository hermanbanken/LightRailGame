using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IncidentVisualizer : MonoBehaviour
{
	static Dictionary<IIncident,GameObject> incidents = new Dictionary<IIncident,GameObject> ();
	public GameObject HandlePrefab;
	static GameObject _HandlePrefab;
	static IncidentVisualizer instance;

	void Start(){
		incidents = new Dictionary<IIncident,GameObject> ();
		_HandlePrefab = HandlePrefab;
		instance = this;
	}

	public static void Add(IIncident incident){
		var w = Instantiate(_HandlePrefab, Position (incident), Quaternion.identity) as GameObject;

		w.transform.SetParent (GameObject.FindObjectsOfType<Canvas> ().First(c => c.gameObject.name == "LRG_Controls").transform, false);
//		w.transform.parent = GameObject.FindObjectOfType<Canvas> ().transform;
		var script = w.GetComponent<Warning> ();
		if (script != null)
			script.incident = incident;

		incidents [incident] = w;
	}

	void Update(){
		foreach (var pair in incidents) {
			var handle = pair.Value;
			handle.transform.position = Position(pair.Key);
		}
	}

	static Vector3 Position(IIncident incident){
		var subject  = incident.Subject();

		if(subject == null)
			return Vector3.zero;
		
		return Camera.main.WorldToScreenPoint(subject.transform.position - 3f*Vector3.back);
	}
	
	public static void Remove(IIncident incident){
		var w = incidents [incident];
		incidents.Remove (incident);
		Destroy(w);
	}
}