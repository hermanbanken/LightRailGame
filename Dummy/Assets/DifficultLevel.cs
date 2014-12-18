using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DifficultLevel : MonoBehaviour {

	public Slider slider;
	public float difficulty;
	Text text;
	// Use this for initialization
	void Awake ()
	{
		DontDestroyOnLoad (this);
		// Set up the reference.
		text = GetComponent <Text> ();
		slider.onValueChanged.RemoveAllListeners ();
		slider.onValueChanged.AddListener ((float difficulty) => LightRailGame.Difficulty = (int)difficulty);	
	}
	
	// Update is called once per frame
	void Update () {
		difficulty = slider.value;
		text.text = "Diffuculty: " + difficulty;
	}
}
