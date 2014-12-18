using UnityEngine;
using System.Collections;
using System;

public class Obstacle : MonoBehaviour, IOccupy {
	//  Initialized
	public GameObject Wrapper;
	public GameObject block;
	//public GameObject button;
	public Vector3 buttonPosition;
	
	// Set by init
	public ObstacleType type;
	[SerializeField]
	public IIncident Incident;
	Action<Obstacle> onUserActioned;
	
	public void init(Vector3 position, Eppy.Tuple<ILine,float> location, ObstacleType type, Action<Obstacle> onUserActioned){
		this.onUserActioned = onUserActioned;
		Incident = new ObstacleBlockage (this);
		Location = location;

		this.type = type;
		switch (type) {
		case ObstacleType.Car:
			this.block = GameObject.CreatePrimitive (PrimitiveType.Capsule);
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
		Wrapper.transform.position = position;

		block.transform.parent = Wrapper.transform;
		block.transform.localPosition = Vector3.zero;
	}

	void Update(){
		if (Incident != null && Incident.IsResolved() && block != null) {
			GameObject.Destroy (gameObject);
			return;
		}
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

	#region IOccupy implementation

	public Eppy.Tuple<ILine,float> Location { get; private set; }
	
	public float Speed {
		get {
			return 0f;
		}
	}

	#endregion
}