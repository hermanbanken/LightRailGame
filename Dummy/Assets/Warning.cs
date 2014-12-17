﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using System;

public class Warning : MonoBehaviour {

	public IIncident incident;

	private RectTransform panel;
	private RectTransform backdrop;
	private Text timer;
	private RectTransform icon;

	const string fail = "fail";
	private string text = "initial";

	void Start(){
		panel = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Panel");
		backdrop = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Backdrop");
		timer = GetComponentsInChildren<Text> ().First(r => r.gameObject.name == "Timer");
		icon = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Icon");
		GetComponent<Button> ().onClick.AddListener (OnClick);
	}

	public void OnClick(){
		Debug.Log("Clicked Button of incident " + incident);
		LightRailGame.GetInstance().ClickedIncident = incident;
	}

	void Update () {
		if (incident == null)
			return;
		
		var v = incident.CountDownValue ();

		var newText = v.HasValue ? (!v.Value.Equals(TimeSpan.Zero) ? v.Value.FormatMinSec() : fail) : "";

		if (!newText.Equals(text))
		{
			timer.enabled = v.HasValue;
			if (backdrop.gameObject.activeSelf != v.HasValue)
				backdrop.gameObject.SetActive (v.HasValue);
			panel.sizeDelta = new Vector2((v.HasValue ? 115f : 35f), 35f);
			icon.anchoredPosition = new Vector2 (v.HasValue ? -38f : 0f, 0);
			timer.text = newText;
			timer.color = newText.Equals(fail) ? Color.red : Color.white;

			text = newText;
		}
	}
}