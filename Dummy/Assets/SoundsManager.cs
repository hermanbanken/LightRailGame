using UnityEngine;
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
	private GameObject seletedtram;


	void Awake () {	
	}

	void Start () {
		source = GetComponent<AudioSource>();
		Sprite mytram1 = Resources.Load("GhostTram", typeof(Sprite)) as Sprite;

		LightRailGame.GetInstance().OnSelectedGameObjectChanged += (GameObject obj) => {
			if(obj == null)
				return;
			if(obj.GetComponent<Train>() != null) {
				seletedtram=obj;
				GameObject hi1=obj.transform.FindChild("tramA").gameObject;
				GameObject hi2=obj.transform.FindChild("tramB").gameObject;
				hi1.GetComponent<SpriteRenderer>().sprite=mytram1;
				hi2.GetComponent<SpriteRenderer>().sprite=mytram1;
				source.PlayOneShot(Pickup,0.1f);
			}
		};
	

		LightRailGame.GetInstance().OnIncidentMenuOpen += (IIncident obj) => {
		
			source.PlayOneShot(warning,0.1f);
		};

		LightRailGame.ScoreManager.OnReroute += (object sender, ScoreManager.RerouteEventArgs e)=>
		{

			source.PlayOneShot(Reroutefinish,0.1f);
		};

		LightRailGame.ScoreManager.OnUserAction += (IIncident obj) => {
		
			source.PlayOneShot(action1,0.50f);
			int suit = obj.Suitability(obj.GetChosenSolution());
		};

		LightRailGame.ScoreManager.OnOccur += (IIncident obj) => {
	
			source.PlayOneShot(Occur,0.2f);
		};

		LightRailGame.ScoreManager.OnResolved += (IIncident obj) => {

			source.PlayOneShot(SuccRemoved,1.0f);
			Debug.Log ("hi,sound");
		};

		LightRailGame.ScoreManager.OnFailed += (IIncident obj) => {
		
			source.PlayOneShot(fail,0.3f);
		};
	
	}

	public void changeback()
	{
		Sprite mytram2 = Resources.Load("HTMTram", typeof(Sprite)) as Sprite;
		Debug.Log ("hi, i am back");
		if(seletedtram.GetComponent<Train>() != null) {
			GameObject hi3=seletedtram.transform.FindChild("tramA").gameObject;
			GameObject hi4=seletedtram.transform.FindChild("tramB").gameObject;
			hi3.GetComponent<SpriteRenderer>().sprite=mytram2;
			hi4.GetComponent<SpriteRenderer>().sprite=mytram2;
		}

	}
}
