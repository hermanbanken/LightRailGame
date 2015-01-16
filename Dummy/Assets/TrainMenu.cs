using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class TrainMenu : MonoBehaviour {

	private Button close;
	private Button stop;
	private Text stopText;
	private Text reasonText;
	private Slider slider;
	private Vector3 visiblePosition;
	private Vector3 hidePosition = new Vector3(200,0,0);

	[NonSerialized]
	private LightRailGame _lrg;
	[NonSerialized]
	private bool _haslrg = false;

	public LightRailGame game { get {
		if(!_haslrg){
			_haslrg = true;
			_lrg = LightRailGame.GetInstance();
		}
		return _lrg;
	} }

	public Train Selected {
		get {
			return game.SelectedGameObject != null ? game.SelectedGameObject.GetComponent<Train>() : null;
		}
	}

	[NonSerialized]
	public bool IsOpen = false;
	
	// Use this for initialization
	void Start () 
	{
		close = this.GetComponentsInChildren<Button>().First(b=>b.gameObject.name == "Deselect");
		close.onClick.RemoveAllListeners ();
		close.onClick.AddListener(() => game.RequestDeselect());

		stop = this.GetComponentsInChildren<Button>().First(b=>b.gameObject.name == "Stop");
		stop.onClick.RemoveAllListeners ();
		stop.onClick.AddListener(() => {
			var prev = Selected.desiredSpeed;
			Selected.desiredSpeed = Selected.desiredSpeed == 0f ? 10f : 0f;
			LightRailGame.ScoreManager.DoDesiredSpeedChange(new ScoreManager.DesiredSpeedChangeEventArgs {
				Train = Selected,
				Previous = prev,
				Current = Selected.desiredSpeed
			});
		});

		stopText = stop.GetComponentInChildren<Text>();
		reasonText = this.GetComponentsInChildren<Text> ().Single (t => t.gameObject.name.Equals ("Reason"));

		slider = GetComponentInChildren<Slider> ();
		slider.minValue = 0f;
		slider.maxValue = 10f;
		slider.onValueChanged.RemoveAllListeners ();
		slider.onValueChanged.AddListener((float val) => { if(Selected != null) Selected.desiredSpeed = val; });
		slider.fillRect.GetComponent<Image> ().color = Color.blue;
	
		game.OnSelectedGameObjectChanged += (GameObject obj) => {
			if(IsOpen && obj == null){
				this.gameObject.transform.position += hidePosition;
				IsOpen = false;
			}
			if(!IsOpen && obj != null){
				this.gameObject.transform.position -= hidePosition;
				IsOpen = true;
			}
		};

		gameObject.transform.position += hidePosition;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Selected != null) {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				game.RequestDeselect();
				return;
			}

//			// Show which other train is blocking the traffic light 
//			if(Selected.Cause == Train.LimitingCause.WaitingForTrafficLight && Selected.stop as TrafficLight != null){
//				var tl = Selected.stop as TrafficLight;
//				tl = tl != null ? tl.Master ?? tl : null;
//				tl.Guard
//					.SelectMany(e => e.GetOccupants())
//					.Where (o => o != Selected).ToList ()
//					.ForEach(o => {
//						Debug.Log("Also at station next to this ("+Selected+") is "+o);
//					});
//			}

			if(Time.frameCount % 5 == 0)
			switch(Selected.Cause){
			case Train.LimitingCause.BehindTram:
				reasonText.text = "The tram is waiting behind another tram."; break;
			case Train.LimitingCause.AtStation:
				reasonText.text = "The tram is loading passengers at a station."; break;
			case Train.LimitingCause.WaitingForTrafficLight:
				reasonText.text = "The tram is waiting for a traffic light."; break;
			case Train.LimitingCause.Incident:
				reasonText.text = "An incident is preventing this tram from going full speed."; break;
			default:
				reasonText.text = ""; break;
			}

			slider.value = Selected.desiredSpeed;
			slider.fillRect.anchoredPosition = new Vector2(-5,0);
			slider.fillRect.sizeDelta = new Vector2(Selected.speed / Selected.desiredSpeed * 110f - 100f, slider.fillRect.sizeDelta.y);
			// TODO fix size of fillRect while dragging

			stopText.text = Selected.desiredSpeed == 0f ? "Start vehicle" : "Stop vehicle";
		}
	}
}
