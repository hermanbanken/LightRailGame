using UnityEngine;
using System.Collections;
using System;

public class Pairs {
	public GameObject block;
	public GameObject button;
	public GameObject timerDisplay;
	public string typeName;
	public float lifetime;
	public DateTime? destroyedAt = null;

	public Pairs(string typeName){
		this.typeName = typeName;
		switch (typeName) {
		case "car":
			this.block = GameObject.CreatePrimitive (PrimitiveType.Capsule);
			this.lifetime = 2f;
			break;
		case "tree":
			this.block = GameObject.CreatePrimitive (PrimitiveType.Cube);
			this.lifetime = 3f;
			break;
		case "barrel":
			this.block = GameObject.CreatePrimitive (PrimitiveType.Cylinder);
			this.lifetime = 4f;
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
		UnityEngine.Object.Destroy(this.block.GetComponent<Collider>());
	}

	public void Tick(){
		if (this.destroyedAt == null || this.timerDisplay == null || this.timerDisplay.guiText == null)
			return;
		var sinceDestroy = (DateTime.Now - this.destroyedAt);
		var remaining = TimeSpan.FromSeconds (lifetime) - sinceDestroy.Value;
		this.timerDisplay.guiText.text = remaining.Minutes.ToString("D2") + ":" + remaining.Seconds.ToString("D2") + "." + remaining.Milliseconds.ToString("D3");

	}

	public void Destroy(){

		this.destroyedAt = DateTime.Now;
		this.timerDisplay.guiText.enabled = true;
		GameObject.Destroy (this.button);
		GameObject.Destroy (this.block, this.lifetime);
		GameObject.Destroy (this.timerDisplay,this.lifetime);
	}
}