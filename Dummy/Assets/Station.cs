using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class Station : MonoBehaviour, IStop {
	// Mechanics
	private IDictionary<Train,float> Presence = new Dictionary<Train, float> ();
	private float stopTime = 5f;
	private float lastVisited = 0;

	public float dueTime = 60f;
	public float dangerTime = 40f;

	private State _state;
	private State ActiveState { 
		get { return _state; }
		set {
			if(value != _state){
				_state = value;
				if(value == State.Due){
					LightRailGame.ScoreManager.DoStationDue(new ScoreManager.StationDueEventArgs { Station = this });
				} else if(value == State.OK){
					LightRailGame.ScoreManager.DoStationOk(new ScoreManager.StationDueEventArgs { Station = this });
				}
			}
		}
	}

	public enum State { Due, Danger, OK }

	// Visuals
	private GameObject quad;

	void Reset(){
		dueTime = 60f;
		dangerTime = 40f;
	}

	Edge edge;
	void Start(){
		_state = State.OK;

		var node = this.gameObject.GetComponent<Node> ();
		quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
		TrafficLight.SetOffsetPositionAndDirection (node, quad.transform);
		quad.transform.parent = gameObject.transform;
		quad.transform.localScale = 2f * Vector3.one;
		quad.AddComponent<GUIText> ();
		
		quad.renderer.material.color = Color.yellow;
	}

	void OnGUI(){
		if(edge != null)
		Handles.ArrowCap(0, edge.GetPoint (0f), Quaternion.LookRotation (edge.GetDirection (0f)), 100f);
	}

	void Update(){
		//OnGUI ();

		if (ActiveState == State.OK) {
			this.quad.renderer.material.color = Color.Lerp (Color.white, Color.green, .7f);
		} else {
			var severity = (Time.time - lastVisited - dangerTime) / (dueTime - dangerTime);
			this.quad.renderer.material.color = Color.Lerp (Color.yellow, Color.red, Math.Min(1f, Math.Max(0f, severity)));
			var overdue = TimeSpan.FromSeconds(Time.time - lastVisited);
			this.quad.guiText.text = overdue.Minutes.ToString("D2") + ":" + overdue.Seconds.ToString("D2");
		}
	}

	void FixedUpdate(){
		ActiveState = Presence.Count == 0 && lastVisited + dueTime < Time.time ? 
			State.Due : 
			Presence.Count == 0 && lastVisited + dangerTime < Time.time ? 
				State.Danger : 
				State.OK;
	}

	#region IStop implementation

	public void Arrive (Train train)
	{
		if (!IsPresent (train)) {
			Presence [train] = Time.time;
		}
	}

	public bool TryLeave (Train train)
	{
		if(!Presence.ContainsKey(train))
			throw new InvalidOperationException ("The train can't leave a station he is not at");

		if (Presence [train] + stopTime < Time.time) {
			Presence.Remove (train);
			lastVisited = Time.time;
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
