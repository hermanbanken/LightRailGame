using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DifficultLevel : MonoBehaviour {

	public Scrollbar diff_level;
	public float diffcultno;
	Text text;
	// Use this for initialization
	void Awake ()
	{
		DontDestroyOnLoad (this);
		// Set up the reference.
		text = GetComponent <Text> ();
	
	}
	
	// Update is called once per frame
	void Update () {
		diffcultno = diff_level.value * 8;
		text.text = "Diffuculty:" + diffcultno;
	
	}
}
