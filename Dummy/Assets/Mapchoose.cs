using UnityEngine;
using System.Collections;

public class Mapchoose : MonoBehaviour {

	// Use this for initialization
	public void chooseMap1()
	{
		Application.LoadLevel ("Scene");
	}

	public void chooseMap2()
	{
		Application.LoadLevel ("SceneB");
	}
}
