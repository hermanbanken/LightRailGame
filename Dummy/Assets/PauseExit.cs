using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PauseExit : MonoBehaviour {

	//public bool paused;
	List<GameObject> buttons;

	public void Start(){
		buttons = new List<GameObject>(GameObject.FindGameObjectsWithTag("whenPaused"));
		buttons.ForEach (b => b.SetActive (false));
	}

	public void Update(){
		if (Input.GetKeyDown(KeyCode.Space)) {
			if(Time.timeScale > 0)
				OnMouseDown();
			else
				Resumegame();
		}
	}

	public void Resumegame()
	{
		Time.timeScale = 1;
		buttons.ForEach (b => b.SetActive (false));
	}
	public void ExitGame () 
	{ 
		Time.timeScale = 1;
		Application.LoadLevel ("Start");

	}
	public void OnMouseDown()
	{
		Time.timeScale = 0;
		buttons.ForEach (b => b.SetActive (true));
	}
	
}
