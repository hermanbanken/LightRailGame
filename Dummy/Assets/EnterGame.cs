using UnityEngine;
using System.Collections;

public class EnterGame : MonoBehaviour {

	public GameObject map1;
	public GameObject map2;
	public GameObject map3;
	public GameObject scrollbar;
	public GameObject text;
	public GameObject Exist;
	public GameObject train;
	public GameObject back;
	public GameObject returnButton;
	public GameObject banner;
	//public AudioClip click;
	//private AudioSource source;
	void Start()
	{
		map1.SetActive (false);
		map2.SetActive (false);
		map3.SetActive (false);
		returnButton.SetActive (false);
	}



	void OnMouseDown()
	{
		//source.PlayOneShot(click,1);
		banner.SetActive (false);
		map1.SetActive (true);
		map2.SetActive (true);
		train.SetActive (false);
		map3.SetActive (true);
		scrollbar.SetActive (true);
		text.SetActive (true);
		back.SetActive (true);
		Exist.SetActive (false);
		gameObject.SetActive (false);
		returnButton.SetActive (true);

		//Application.LoadLevel ("Scene");

		//Time.timeScale = 1;
	
	
	
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
