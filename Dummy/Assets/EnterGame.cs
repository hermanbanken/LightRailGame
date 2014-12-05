using UnityEngine;
using System.Collections;

public class EnterGame : MonoBehaviour {

	void OnMouseDown()
	{
		Application.LoadLevel ("Scene");
		Time.timeScale = 1;
		
	
	}

	// Use this for initialization
	/*void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if ( Input.GetMouseButtonDown (0)) {
		{
			Application.LoadLevel ("Scene");
		}
	}
}*/
}
