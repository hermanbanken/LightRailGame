using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

public class Warning : MonoBehaviour {

	public IIncident incident;

	private RectTransform panel;
	private RectTransform backdrop;
	private Text timer;
	private Button button;
	private RectTransform icon;

	private string text = "initial";

	void Start(){
		panel = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Panel");
		backdrop = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Backdrop");
		timer = GetComponentsInChildren<Text> ().First(r => r.gameObject.name == "Timer");
		icon = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Icon");
		(button = GetComponent<Button> ()).onClick.AddListener (OnClick);
	}

	public void OnClick(){
		Debug.Log("Clicked Button of incident " + incident);
		LightRailGame.GetInstance().ClickedIncident = incident;
	}

	void Update () {
		if (incident == null)
			return;
		
		var v = incident.CountDownValue ();

		var newText = v.HasValue ? v.Value.Minutes.ToString ("D2") + ":" + v.Value.Seconds.ToString ("D2") : "";

		if (!newText.Equals(text)) 
		{
			timer.enabled = v.HasValue;
			if (backdrop.gameObject.activeSelf != v.HasValue)
				backdrop.gameObject.SetActive (v.HasValue);
			panel.sizeDelta = new Vector2((v.HasValue ? 115f : 35f), 35f);
			icon.anchoredPosition = new Vector2 (v.HasValue ? -38f : 0f, 0);
			timer.text = newText;

			text = newText;
		}
	}
}
