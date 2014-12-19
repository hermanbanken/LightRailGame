using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Mapchoose : MonoBehaviour {

	public void ChooseMap(string name){
		Debug.Log ("Loading " + name);
		if (name.Equals ("TutorialLevel")) {
			Debug.Log ("Difficulty = 0");
			LightRailGame.Difficulty = -1;
		}
		Application.LoadLevel (name);

	}

}
