using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IncidentVisualizer : MonoBehaviour
{
	static Dictionary<IIncident,Warning> incidents = new Dictionary<IIncident,Warning> ();
	public GameObject HandlePrefab;
	static GameObject _HandlePrefab;

	void Start(){
		incidents = new Dictionary<IIncident,Warning> ();
		_HandlePrefab = HandlePrefab;
	}

	public static void Add(IIncident incident){
		var w = Instantiate(_HandlePrefab, Position (incident), Quaternion.identity) as GameObject;

		w.transform.SetParent (GameObject.FindObjectsOfType<Canvas> ().First(c => c.gameObject.name == "LRG_Controls").transform, false);
		var warning = w.GetComponent<Warning> ();
		warning.incident = incident;
		incidents [incident] = warning;
	}

	void Update(){
		foreach (var pair in incidents) {
			var warning = pair.Value;
			warning.gameObject.transform.position = Position(pair.Key) + (15f + warning.Width()/2f) * Vector3.right + 30f * Vector3.up;
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
		Destroy(w.gameObject);
	}
}