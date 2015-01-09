﻿using UnityEngine;
using System.Collections;

public class SoundsManager : MonoBehaviour {
	public AudioClip warning;
	public AudioClip Pickup;
	public AudioClip Reroutefinish;
	public AudioClip action1;
	public AudioClip SuccRemoved;
	public AudioClip Occur;
	public AudioClip fail;
	private AudioSource source;



	void Awake () {	
		source = GetComponent<AudioSource>();

	}

	// Use this for initialization
	void Start () {
	
	}


	// Update is called once per frame
	void Update () {

		LightRailGame.GetInstance().OnSelectedGameObjectChanged += (GameObject obj) => {
			//source.Stop();
			if(obj == null)
				return;
			if(obj.GetComponent<Train>() != null) {
				source.PlayOneShot(Pickup,0.5f);			
			}
		};

		LightRailGame.GetInstance().OnIncidentMenuOpen += (IIncident obj) => {
			source.Stop();
			source.PlayOneShot(warning,0.1f);
		};

		LightRailGame.ScoreManager.OnReroute += (object sender, ScoreManager.RerouteEventArgs e)=>
		{
			source.Stop();
			source.PlayOneShot(Reroutefinish,0.1f);
		};

		LightRailGame.ScoreManager.OnUserAction += (IIncident obj) => {
			source.Stop();
			source.PlayOneShot(action1,0.50f);
			int suit = obj.Suitability(obj.GetChosenSolution());
		};

		LightRailGame.ScoreManager.OnOccur += (IIncident obj) => {
			source.Stop();
			source.PlayOneShot(Occur,0.2f);
		};

		LightRailGame.ScoreManager.OnResolved += (IIncident obj) => {
			source.Stop();
			source.PlayOneShot(SuccRemoved,0.2f);
		};

		LightRailGame.ScoreManager.OnFailed += (IIncident obj) => {
			source.Stop();
			source.PlayOneShot(fail,0.3f);
		};

	}
}
