using UnityEngine;
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
	private RectTransform line;

	const string fail = "fail";
	private string text = "initial";


	void Start(){
		panel = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Panel");
		backdrop = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Backdrop");
		timer = GetComponentsInChildren<Text> ().First(r => r.gameObject.name == "Timer");
		icon = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Icon");
		line = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Line");
		GetComponent<Button> ().onClick.AddListener (OnClick);
	}

	public void OnClick(){
		LightRailGame.GetInstance().ShowMenu(incident);
	}

	void Update () {
		if (incident == null)
			return;

		var v = incident.CountDownValue ();

		var newText = incident.HandleText();

		if (!newText.Equals(text))
		{
			timer.enabled = v.HasValue;
			if (backdrop.gameObject.activeSelf != v.HasValue)
				backdrop.gameObject.SetActive (v.HasValue);
			panel.sizeDelta = new Vector2((v.HasValue ? 115f : 35f), 35f);
			icon.anchoredPosition = new Vector2 (v.HasValue ? -38f : 0f, 0);
			line.anchoredPosition = new Vector2 (v.HasValue ? -38f : 0f, 0) + new Vector2(-22f, -22f);
			timer.text = newText;

			if(incident.CountDownValue().HasValue)
				timer.color = incident.CountDownValue().Value == TimeSpan.Zero && !incident.IsResolved() ? Color.red : Color.white;

			text = newText;
		}
	}

	public float Width(){
		return incident == null ? 0f : incident.CountDownValue ().HasValue ? 115f : 35f;
	}
}

public static class Vector3Ext {
	public static Vector3 FixZ(this Vector3 self, float z){
		return new Vector3(self.x, self.y, z);
	}
	public static Vector3 FixY(this Vector3 self, float y){
		return new Vector3(self.x, y, self.z);
	}
	public static Vector3 FixX(this Vector3 self, float x){
		return new Vector3(x, self.y, self.z);
	}

}