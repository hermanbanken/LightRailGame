using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Tuturial : MonoBehaviour {
	public GameObject ObStacleOccure;
	public GameObject Quitgame;
	//public AudioClip warning;
	//public AudioClip Pickup;
	//private AudioSource source;
	Text text;
	// Use this for initialization


	void Start () {
		LightRailGame.Difficulty = -1;
		text = GetComponent<Text>();
		text.text="Have a look at the map, you should see two trams moving. There are some stations that the trams visit.\n\nFirstly, try to click on a tram.";

		LightRailGame.GetInstance().OnSelectedGameObjectChanged += (GameObject obj) => {
			// The user selected something
			if(obj == null)
				return;
			if(obj.GetComponent<Train>() != null) {
				text.text="Congratulations! You picked a Tram. The tram menu appeared.\n\n You can alter the tram's speed, route or stop it completely.\n\n"+
					"The current route of the tram is also highlighted.\n\nTo change a route you can drag the blue line. Change its route to the left side and notice how it changes";
				//LightRailGame.GetInstance().Obstacles.PlaceNewObstacle();
				//source.PlayOneShot(Pickup,0.5f);
			
			}
			else 
			{
				text.text="Please pick a tram!";
			}
		};

		LightRailGame.ScoreManager.OnReroute += (object sender, ScoreManager.RerouteEventArgs e)=>
		{
			text.text= "Success! You just made sure that the tram will reach omitted station. Once it reaches it, it will make sure the station is green and the travellers transported."+
				"\n\nSometimes, an accident may occur which blocks the tracks.\n\nClick the button below to manually add one";;
			ObStacleOccure.SetActive(true);
		};


		LightRailGame.GetInstance().OnIncidentMenuOpen += (IIncident obj) => {

			text.text="There are several options to choose from and they differ in success rate.\n Please choose an action and try to resolve the issue.\n\n" +
				"Normally, you will be facing a trade-off between rerouting and waiting and will need to decide whether you wish to alter the tram’s path.\n\n" +
				"Now, Choose an action and see if it works out. If not, try again!";
			//source.PlayOneShot(warning,0.50f);


		};
		//LightRailGame.GetInstance()


		LightRailGame.ScoreManager.OnUserAction += (IIncident obj) => {
			ObStacleOccure.SetActive(false);
			text.text="You have picked an action, let's see what happens next";
			int suit = obj.Suitability(obj.GetChosenSolution());
			// if suit<0, maybe a wrong action?;
			if(suit<0)
				text.text="Hmm, that might not have been a good idea!!\nYou might need to choose another one...";;

		};

	
	}

	void Update()
	{
		LightRailGame.ScoreManager.OnResolved += (IIncident obj) => {
			text.text="Blockage removed!\n\nCongratulations, you now know the basics of the game. Good luck managing the network!";
			Quitgame.SetActive(true);
			ObStacleOccure.SetActive(false);
		};

		}

	public void next()
	{
		text.text = "But hey! Both of the trams have the same route and they are never reaching the station on the left. Soon it will turn yellow and then red and you will start losing points because your travellers are unhappy.\n\nPick one of the trams and let’s reroute it!";
	}
	public void createOb()
	{
		LightRailGame.GetInstance().Obstacles.PlaceNewObstacle();
		text.text="Oh no! A disturbance occurred.\n\nNotice an exclamation mark with the yellow background?\n\n" +
			"Click on it and let’s see how we can solve it…!";


	}




}