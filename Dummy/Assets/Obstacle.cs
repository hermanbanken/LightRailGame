using UnityEngine;
using System.Collections;
using System;

public class Obstacle : MonoBehaviour {
	// Set at mouse event
	public float? userActionedAt = null; // Seconds since userActioned

	//  Initialized
	public GameObject Wrapper;
	public GameObject block;
	//public GameObject button;
	public GameObject timerDisplay;
	public Vector3 buttonPosition;
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
		Wrapper.transform.position = position;

		block.transform.parent = Wrapper.transform;
		timerDisplay.transform.parent = Wrapper.transform;
		timerDisplay.transform.localPosition = Vector3.zero;
		block.transform.localPosition = Vector3.zero;
		timerDisplay.AddComponent<GUIText> ();
		timerDisplay.guiText.enabled = false;
	}

	// Deprecated, remove soon
	public void Tick(){
		// Not yet actioned || resolved
		if (userActionedAt == null || timerDisplay == null || timerDisplay.guiText == null)
			return;

		// Else update timer
		var sinceDestroy = (Time.time - userActionedAt.Value);
		var remaining = timeToResolve - TimeSpan.FromSeconds(sinceDestroy);
		timerDisplay.guiText.text = remaining.Minutes.ToString("D2") + ":" + remaining.Seconds.ToString("D2") + "." + remaining.Milliseconds.ToString("D3");
	}

	public string ButtonText(){
		// Not yet actioned || resolved
		if (userActionedAt == null || timerDisplay == null || timerDisplay.guiText == null)
			return "";
		
		// Else update timer
		var sinceDestroy = (DateTime.Now - userActionedAt);
		var remaining = timeToResolve - sinceDestroy.Value;
		return remaining.Minutes.ToString("D2") + ":" + remaining.Seconds.ToString("D2") + "." + (remaining.Milliseconds/100).ToString("D1");
	}

	public void DoUserAction(){
		if(onUserActioned != null)
			onUserActioned (this);

		userActionedAt = Time.time;
		timerDisplay.guiText.enabled = true;

		GameObject.Destroy (block, (float) timeToResolve.TotalSeconds);
		GameObject.Destroy (timerDisplay, (float) timeToResolve.TotalSeconds);
		GameObject.Destroy (Wrapper, (float) timeToResolve.TotalSeconds);
	}

	public Vector2 screenPosition(Vector3? pos = null){
		if (!pos.HasValue)
			pos = this.Wrapper.transform.position;
		var sp = Camera.main.WorldToScreenPoint (pos.Value);
		return new Vector2 (sp.x, sp.y);
	}

	public bool DrawGUI ()
	{
		// Draw button at correct position, look at Re-route GUIs.TrainGUI, where buttons are drawn at Node's position
		if (Wrapper != null){
		var obsPos = Wrapper.transform.position;
		var buttonPos = obsPos; // get from Master
		}
		var w = 60f;
		var sp = screenPosition(buttonPosition);
		return GUI.Button (new Rect (sp.x - w/2, Screen.height - (sp.y + w/2), w, w/2), ButtonText());
		
	}
}