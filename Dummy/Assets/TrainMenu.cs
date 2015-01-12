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

		slider = GetComponentInChildren<Slider> ();
		slider.minValue = 0f;
		slider.maxValue = 10f;
		slider.onValueChanged.RemoveAllListeners ();
		slider.onValueChanged.AddListener((float val) => Selected.desiredSpeed = val);
		slider.fillRect.GetComponent<Image> ().color = Color.blue;
	
		game.OnSelectedGameObjectChanged += (GameObject obj) => {
			if(IsOpen && obj == null)
				this.gameObject.transform.position += hidePosition;
			if(!IsOpen && obj != null)
				this.gameObject.transform.position -= hidePosition;
		};

		gameObject.transform.position += hidePosition;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Selected != null) {
			slider.value = Selected.desiredSpeed;
			slider.fillRect.anchoredPosition = new Vector2(-5,0);
			slider.fillRect.sizeDelta = new Vector2(Selected.speed / Selected.desiredSpeed * 110f - 100f, slider.fillRect.sizeDelta.y);
			// TODO fix size of fillRect while dragging

			stopText.text = Selected.desiredSpeed == 0f ? "Start vehicle" : "Stop vehicle";
		}
	}
}
