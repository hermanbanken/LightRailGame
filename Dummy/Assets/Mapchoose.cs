using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Mapchoose : MonoBehaviour {

	public Scrollbar diff_level;
	public static float diffcultno;

	void Start()
	{
		DontDestroyOnLoad (this);
	}

// Update is called once per frame
	void Update () {
		diffcultno = diff_level.value * 8;
		
	}
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
