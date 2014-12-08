using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Mouse {

	private bool down = false;
	public Queue<MouseEvent> Events = new Queue<MouseEvent>();
	private MouseEvent Last;

	public Mouse(){
	}

	public void OnFrame(){
		// Still down
		if (down && Input.GetMouseButton (0)) {
			Last.HandleDrag(Input.mousePosition);
		}

		// New click
		if (!down && Input.GetMouseButtonDown (0)) {
			Last = new MouseClick(Input.mousePosition);
			Events.Enqueue(Last);
			down = true;
			//ScoreManager.score++;
		}

		// End of click
		if (down && Input.GetMouseButtonUp (0)) {
			down = false;
		}
	}

}

public abstract class MouseEvent {
	public readonly Vector3 position;
	public MouseEvent(Vector3 position){
		this.position = position;
	}

	public event Action<Vector3> OnDrag;
	public void HandleDrag(Vector3 newPosition){
		OnDrag (newPosition);
	}
}

public class MouseClick : MouseEvent {
	public MouseClick(Vector3 position) : base(position){}
}