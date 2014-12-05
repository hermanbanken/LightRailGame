using UnityEngine;
using System.Collections;
using System;

public class Obstacle {
	public GameObject block;
	public GameObject button;
	public GameObject timerDisplay;
	public ObstacleType type;
	public TimeSpan timeToResolve;
	public DateTime? userActionedAt = null;

	Action<Obstacle> onUserActioned;
	
	public Obstacle(ObstacleType type, Action<Obstacle> onUserActioned){
		this.onUserActioned = onUserActioned;

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

		this.timerDisplay = new GameObject ("timer");
		this.timerDisplay.AddComponent<GUIText> ();
		this.timerDisplay.guiText.font =  (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		this.timerDisplay.guiText.enabled = false;
		this.block.transform.position = new Vector3 (UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 1);
		this.button = GameObject.CreatePrimitive (PrimitiveType.Cube);
		this.button.transform.position = new Vector3 (this.block.transform.position.x + 2, this.block.transform.position.y, 1);
		this.timerDisplay.guiText.transform.position = Camera.main.WorldToViewportPoint (this.block.transform.position);
		UnityEngine.Object.Destroy(this.button.GetComponent<Collider>());
	}

	public void Tick(){
		// Not yet actioned || resolved
		if (this.userActionedAt == null || this.timerDisplay == null || this.timerDisplay.guiText == null)
			return;
		// Else update timer
		var sinceDestroy = (DateTime.Now - this.userActionedAt);
		var remaining = timeToResolve - sinceDestroy.Value;
		this.timerDisplay.guiText.text = remaining.Minutes.ToString("D2") + ":" + remaining.Seconds.ToString("D2") + "." + remaining.Milliseconds.ToString("D3");
	}

	public void DoUserAction(){
		this.onUserActioned (this);
		this.userActionedAt = DateTime.Now;
		this.timerDisplay.guiText.enabled = true;
		GameObject.Destroy (this.button);
		GameObject.Destroy (this.block, (float) this.timeToResolve.TotalSeconds);
		GameObject.Destroy (this.timerDisplay, (float) this.timeToResolve.TotalSeconds);
	}
}