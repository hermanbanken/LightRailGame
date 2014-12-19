using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Tuturial : MonoBehaviour {
	public GameObject ObStacleOccure;
	Text text;
	// Use this for initialization
	void Start () {
		LightRailGame.Difficulty = -1;
		text = GetComponent<Text>();
		text.text="Have a look at the map, you should see two trams moving. There are some stations that the trams visit. \n\n Firstly, try to click on a tram.";
	}
	
	// Update is called once per frame
	void Update () {
		LightRailGame.GetInstance().OnSelectedGameObjectChanged += (GameObject obj) => {
			// The user selected something
			if(obj == null)
				return;
			if(obj.GetComponent<Train>() != null) {
			text.text="Congratulations! You picked a Tram. The tram menu appeared.\n\n You can alter the tram's speed, route or stop it completely.\n\n" +
				"Sometimes, an accident may occur which blocks the tracks.\n\n Click the button below to manually add one";
				//LightRailGame.GetInstance().Obstacles.PlaceNewObstacle();
				ObStacleOccure.SetActive(true);
			
			}
			else 
			{
				text.text="Please pick a tram!";
			}
		};

		LightRailGame.GetInstance().OnIncidentMenuOpen += (IIncident obj) => {

			text.text="There are several options to choose from and they differ in success rate.\n Please choose an action and try to resolve the issue.\n\n" +
				"Normally, you will be facing a trade-off between rerouting and waiting and will need to decide whether you wish to alter the tram’s path.\n\n" +
				"Now, Choose an action and see if it works out. If not, try again!";

		};
		//LightRailGame.GetInstance()

		LightRailGame.ScoreManager.OnUserAction += (IIncident obj) => {
			text.text="You have picked an action, let's see what happens next";
			int suit = obj.Suitability(obj.GetChosenSolution());
			// if suit<0, maybe a wrong action?;
			if(suit<0)
				text.text="Hmm, that might not have been a good idea!!\n You might need to choose another one...";;
		};

						
	}


	public void createOb()
	{
		LightRailGame.GetInstance().Obstacles.PlaceNewObstacle();
		text.text="Oh no! A disturbance occurred.\n\nNotice an exclamation mark with the yellow background?\n\n" +
			"Click on it and let’s see how we can solve it…!";

	}


}