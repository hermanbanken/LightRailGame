using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseExit : MonoBehaviour {

	//public bool paused;

	public void Resumegame()
	{
		Time.timeScale = 1;
	}
	public void ExitGame () 
	{ 
		Time.timeScale = 1;
		Application.LoadLevel ("Start");

	}
	public void OnMouseDown()
	{
		Time.timeScale = 0;
		//Application.LoadLevel ("Start");		
	}
	
}
