using UnityEngine;
using System.Collections;
using System;

public class Obstacle : MonoBehaviour {
	// Set at mouse event
	public float? userActionedAt = null; // Seconds since userActioned

	//  Initialized
	public GameObject Wrapper;
	public GameObject block;
	public GameObject button;
	public GameObject timerDisplay;
	public TimeSpan timeToResolve;
	
	// Set by init
	public ObstacleType type;
	public IIncident Incident;
	Action<Obstacle> onUserActioned;
	
	public void init(Vector3 position, ObstacleType type, Action<Obstacle> onUserActioned){
		this.onUserActioned = onUserActioned;
		Incident = new ObstacleBlockage (this);

		this.type = type;
		switch (type) {
		case ObstacleType.Car:
			this.block = GameObject.CreatePrimitive (PrimitiveType.Capsule);
			this.timeToResolve = TimeSpan.FromSeconds(2f);
			break;
		case ObstacleType.Tree:
			this.block = GameObject.CreatePrimitive (PrimitiveType.Cube);
			this.timeToResolve = TimeSpan.FromSeconds(3f);
			break;
		case ObstacleType.Barrel:
			this.block = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
			this.timeToResolve = TimeSpan.FromSeconds(4f);
			break;
		}

		Wrapper = gameObject;
		gameObject.name = "Obstacle";
		timerDisplay = new GameObject ("timer");
		button = GameObject.CreatePrimitive (PrimitiveType.Cube);

		Wrapper.transform.position = position;

		block.transform.parent = Wrapper.transform;
		timerDisplay.transform.parent = Wrapper.transform;
		button.transform.parent = Wrapper.transform;

		button.transform.localPosition = Vector3.zero;
		block.transform.localPosition = Vector3.zero;
		timerDisplay.transform.localPosition = Vector3.zero;

		timerDisplay.AddComponent<GUIText> ();
		timerDisplay.transform.localPosition = new Vector3 (2f, 0f, -2f);
		timerDisplay.guiText.enabled = false;
		timerDisplay.guiText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		timerDisplay.guiText.transform.position = Camera.main.WorldToViewportPoint (Wrapper.transform.position);
	}

	public void Tick(){
		// Not yet actioned || resolved
		if (userActionedAt == null || timerDisplay == null || timerDisplay.guiText == null)
			return;

		// Else update timer
		var sinceDestroy = (Time.time - userActionedAt.Value);
		var remaining = timeToResolve - TimeSpan.FromSeconds(sinceDestroy);
		timerDisplay.guiText.text = remaining.Minutes.ToString("D2") + ":" + remaining.Seconds.ToString("D2") + "." + remaining.Milliseconds.ToString("D3");
	}

	public void DoUserAction(){
		if(onUserActioned != null)
			onUserActioned (this);

		userActionedAt = Time.time;
		timerDisplay.guiText.enabled = true;

		GameObject.Destroy (button);
		GameObject.Destroy (block, (float) timeToResolve.TotalSeconds);
		GameObject.Destroy (timerDisplay, (float) timeToResolve.TotalSeconds);
		GameObject.Destroy (Wrapper, (float) timeToResolve.TotalSeconds);
	}
}