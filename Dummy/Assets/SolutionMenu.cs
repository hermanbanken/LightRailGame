using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public class SolutionMenu : MonoBehaviour {

	IIncident inc;
	Action<ISolution> onChoose;
	List<Transform> Solutions;
	int activeSolutions = 0;
	Text IncidentText;

	List<Text> SolutionTexts;
	List<Text> TimeTexts;
	List<Text> ChanceTexts;
	List<Image> Icons;
	List<Button> Buttons;

	public Sprite IconWarning;
	public Sprite IconCrane;
	public Sprite IconRepairCrew;
	public Sprite IconCallPolice;
	public Sprite IconDoktor;
	public Sprite IconPushCar;
	public Sprite IconSuperman;
	public Sprite IconTowTruck;
	public Sprite IconHorn;

	Button Cancel;

	// Use this for initialization
	void Start () 
	{
		SolutionTexts = new List<Text> ();
		TimeTexts = new List<Text> ();
		ChanceTexts = new List<Text> ();
		Buttons = new List<Button> ();
		Icons = new List<Image> ();

		var canvas = this.gameObject.transform.GetChild (0);

		Solutions = new List<Transform> ();
		AddNewSolutionTile (canvas.GetChild(1));
		Solutions [0].gameObject.SetActive (false);
		
		IncidentText = canvas.GetChild (0).GetComponentInChildren<Text> ();

		Cancel = canvas.GetChild (1).GetComponent<Button> ();
		Cancel.onClick.AddListener (() => {
			this.gameObject.SetActive(false);
		});

		activeSolutions = 0;
	}

	void AddNewSolutionTile(Transform instance){
		if (instance == null)
			return;
		instance.transform.SetParent (this.transform.GetChild (0));
		Solutions.Add (instance);
		SolutionTexts.Add(instance.GetChild (1).GetComponent<Text> ());
		TimeTexts.Add(instance.GetChild (2).GetComponent<Text> ());
		Icons.Add(instance.GetChild (3).GetComponent<Image> ());
		ChanceTexts.Add(instance.GetChild (4).GetComponent<Text> ());
		Buttons.Add (instance.GetComponent<Button> ());
	}

	IEnumerable<IIncident> Incidents(){
		var incidents = new [] { inc }.ToList();
		if (inc.Subject ().GetComponent<Train> () != null) {
			incidents = inc.Subject ().GetComponent<Train> ().incident;
		}
		return incidents;
	}
	IEnumerable<ISolution> Actions(){
		var incidents = Incidents ();
		var actions = incidents.SelectMany(i => i.PossibleActions()).Distinct();
		return actions;
	}

	void Choose(int index){
		var action = Actions ().ElementAt (index);
		foreach (var i in Incidents ())
			i.SetChosenSolution (action);
		onChoose(action);
		this.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update () {
		if (inc == null)
			return;

		if (inc.IsResolved ())
			this.gameObject.SetActive (false);

		var actions = inc.PossibleActions ();
		var incidents = Incidents();
		if (inc.Subject ().GetComponent<Train> () != null) {
			incidents = inc.Subject ().GetComponent<Train> ().incident;
			actions = incidents.SelectMany(i => i.PossibleActions()).Distinct();
		}

		if(incidents.Distinct().Count () == 1)
			IncidentText.text = inc.Description()+" How would you like to resolve this issue?";
		else
			IncidentText.text = incidents.Select(i => i.Description()).Distinct().Aggregate("Several incidents occured. ", (a,b) => a + b + " ") + "How would you like to resolve these issues?";

		// Spawn new tiles
		actions.Select((s, i) => {
			if(activeSolutions <= i){
				if(Solutions.Count <= i){
					var transform = Instantiate(Solutions[0], Solutions[0].position, Quaternion.identity);
					AddNewSolutionTile(transform  as Transform);
				}
				Solutions[i].position = IncidentText.gameObject.transform.position + (90 + i * 70) * Vector3.down;
				Buttons[i].onClick.RemoveAllListeners();
				Buttons[i].onClick.AddListener(() => Choose(i));
				Solutions[i].gameObject.SetActive(true);
				activeSolutions++;
			}
			SolutionTexts[i].text = s.ProposalText;
			ChanceTexts[i].text = ((int)(s.SuccessRatio*100)) + "%";
			TimeTexts[i].text = ((int)(s.ResolveTime.TotalSeconds)) + " sec.";

			if(s == SolutionBlockages.Crane)
				Icons[i].sprite = IconCrane;
			else if(s == SolutionBlockages.Maintenance)
				Icons[i].sprite = IconRepairCrew;
			else if(s == SolutionIncidents.Police)
				Icons[i].sprite = IconCallPolice;
			else if(s == SolutionIncidents.DeliverBaby)
				Icons[i].sprite = IconDoktor;
			else if(s == SolutionBlockages.PushAside)
				Icons[i].sprite = IconPushCar;
			else if(s == PowerUps.Magic)
				Icons[i].sprite = IconSuperman;
			else if(s == SolutionBlockages.Tow)
				Icons[i].sprite = IconTowTruck;
			else if(s == SolutionBlockages.Horn)
				Icons[i].sprite = IconHorn;
			else
				Icons[i].sprite = IconWarning;

			return true;
		}).All (a => a);

		// Hide old tiles
		for(int i = actions.Count (); i < activeSolutions; i++){
			Solutions[i].gameObject.SetActive(false);
		}

		activeSolutions = actions.Count ();
	}

	public void Show (IIncident inc, Action<ISolution> onChoose)
	{
		this.inc = inc;
		this.onChoose = onChoose;
	}
}
