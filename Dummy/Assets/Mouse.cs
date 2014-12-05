using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mouse {

	private bool down = false;
	public Queue<MouseEvent> Events = new Queue<MouseEvent>();

	public Mouse(){
	}

	public void OnFrame(){
		// New click
		if (!down && Input.GetMouseButtonDown (0)) {
			Events.Enqueue(new MouseClick(Input.mousePosition));
			down = true;
			ScoreManager.score++;
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
}

public class MouseClick : MouseEvent {
	public MouseClick(Vector3 position) : base(position){}
}