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
	
	// Set by init
	public ObstacleType type;
	[SerializeField]
	public IIncident Incident;
	Action<Obstacle> onUserActioned;

	public void init(Vector3 position, ObstacleType type, Action<Obstacle> onUserActioned){
		this.onUserActioned = onUserActioned;
		Incident = new ObstacleBlockage (this);

		this.type = type;
		switch (type) {
		case ObstacleType.Car:
			//this.block = GameObject.CreatePrimitive (PrimitiveType.Capsule);
			this.block= (GameObject)Instantiate(Resources.Load("prefab/tree"));
		//	this.block = Instantiate(Resources.Load("prefab/tree"), new Vector3(0,0,-4),Quaternion.identity);
			break;
		case ObstacleType.Tree:
			this.block = GameObject.CreatePrimitive (PrimitiveType.Cube);
			break;
		case ObstacleType.Barrel:
			this.block = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
			break;
		}

		Wrapper = gameObject;
		gameObject.name = "Obstacle";
		timerDisplay = new GameObject ("timer");
		Wrapper.transform.position = position;

		block.transform.parent = Wrapper.transform;
		timerDisplay.transform.parent = Wrapper.transform;
		timerDisplay.transform.localPosition = Vector3.zero;
		block.transform.localPosition = Vector3.zero+Vector3.back;
		timerDisplay.AddComponent<GUIText> ();
		timerDisplay.guiText.enabled = false;
	}

	void Update(){
		if (Incident != null && Incident.IsResolved() && block != null) {
			GameObject.Destroy (gameObject);
			return;
		}

		if(timerDisplay != null)
			this.timerDisplay.guiText.text = ButtonText ();
	}
	
	public string ButtonText(){
		if (Incident == null)
			return "";

		// Not yet actioned || resolved
		if (!Incident.CountDownValue().HasValue)
			return "handle";
		
		// Else update timer
		var remaining = Incident.CountDownValue ().Value;
		return remaining.Minutes.ToString("D2") + ":" + remaining.Seconds.ToString("D2") + "." + (remaining.Milliseconds/100).ToString("D1");
	}

	public void DoUserAction(){
		if(onUserActioned != null)
			onUserActioned (this);

		userActionedAt = Time.time;
		timerDisplay.guiText.enabled = true;
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
		var w = 60f;
		var sp = screenPosition(buttonPosition);
		return GUI.Button (new Rect (sp.x - w/2, Screen.height - (sp.y + w/2), w, w/2), ButtonText());
	}
}