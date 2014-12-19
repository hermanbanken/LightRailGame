using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Tuturial : MonoBehaviour {
	public GameObject ObStacleOccure;
	public GameObject Quitgame;
	Text text;
	// Use this for initialization
	void Start () {
		LightRailGame.Difficulty = -1;
		text = GetComponent<Text>();
		text.text = "Welcome!\n\nIn order to help you understand how the game works, a small map was created with 2 loops: left and right and 3 stations.\n\nNotice how 2 trams move around and stop at stations (green square) and stop if the traffic light is red. ";
		//text.text="First, you see that two trams are run.There are some stations. \n\nFirstly,Try to click on a Tram.";
	}
	
	// Update is called once per frame
	void Update () {
		LightRailGame.GetInstance().OnSelectedGameObjectChanged += (GameObject obj) => {
			// The user selected something
			if(obj == null)
				return;
			if(obj.GetComponent<Train>() != null) {
				text.text="Congradurations! You pick a Tram now.Here you see the tram menu.\n\nYou can alter tram speed, route or stop it completely.\n\nSometimes, the accident may occur as a obstacke on the track.\n\nClick the button below see what happened";
				//LightRailGame.GetInstance().Obstacles.PlaceNewObstacle();
				ObStacleOccure.SetActive(true);
			
			}
			else 
			{
				text.text="You do not pick a Tram!!!";
			}
		};

		LightRailGame.GetInstance().OnIncidentMenuOpen += (IIncident obj) => {

			text.text="There are several options to choose from and they differ in success rate.\n Please choose one action and try to resolve the issue.\n\nNormally, you will be facing a trade-off between rerouting and waiting and need to decide whether you wish to alter the tram’s path.\n\nNow, Choose an action and see if it works out. If not, try again!";

		};
		//LightRailGame.GetInstance()

		LightRailGame.ScoreManager.OnUserAction += (IIncident obj) => {
			text.text="Now, you choose a action. See what happend next.\n\n If your action are successfull, the blockage will disappeared.\n\n But if you failed, choose an action again";
			int suit = obj.Suitability(obj.GetChosenSolution());
			// if suit<0, maybe a wrong action?;
			if(suit<0)
				text.text="No, you choose a wrong action.!!\nI belive you will fail. You need choose another actions.";;
		};

		LightRailGame.ScoreManager.OnResolved += (IIncident obj) => {
			text.text="Blockage removed!\n\nCongratulations, you now know the basics of the game. Good luck managing the network!";
			Quitgame.SetActive(true);
		};

	//	LightRailGame.ScoreManager.
		
	}

	public void next()
	{
		text.text = "But hey! Both of the trams have the same route and they are never reaching the station on the left. Soon it will turn yellow and then red and you will start losing points because your travellers are unhappy.\n\nPick one of the trams and let’s reroute it!";
	}
	public void createOb()
	{
		LightRailGame.GetInstance().Obstacles.PlaceNewObstacle();
		text.text="Oh no! A disturbance occurred.\n\nSee the exclamation mark on the yellow background?\n\nClick on it and let’s see how we can solve it…!";

	}




}