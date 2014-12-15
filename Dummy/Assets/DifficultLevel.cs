using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DifficultLevel : MonoBehaviour {

	public Scrollbar diff_level;
	Text text;
	// Use this for initialization
	void Awake ()
	{
		// Set up the reference.
		text = GetComponent <Text> ();
	
	}
	
	// Update is called once per frame
	void Update () {
		text.text = "Diffuculty:" + diff_level.value*8;
	
	}
}
