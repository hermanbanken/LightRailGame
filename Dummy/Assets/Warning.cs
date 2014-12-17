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
	private LineRenderer line;
	private Vector3[] corners = new Vector3[4];
	private Transform subjectTransform;

	const string fail = "fail";
	private string text = "initial";


	void Start(){
		panel = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Panel");
		backdrop = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Backdrop");
		timer = GetComponentsInChildren<Text> ().First(r => r.gameObject.name == "Timer");
		icon = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Icon");
//		line = GetComponentsInChildren<RectTransform> ().First(r => r.gameObject.name == "Line");
		GetComponent<Button> ().onClick.AddListener (OnClick);
		line = gameObject.GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer> ();
		line.SetVertexCount (2);
		line.SetColors (Color.red, Color.black);
		line.SetWidth (2f, 0.4f);
	}

	public Transform FindSubjectTransform(Transform initial){
		for (int i = 0; i < 5; i++) {
			if(initial.position != Vector3.zero)
				return initial;
			initial = initial.parent ?? initial;
		}
		return initial;
	}

	public void OnClick(){
		Debug.Log("Clicked Button of incident " + incident);
		LightRailGame.GetInstance().ClickedIncident = incident;
	}

	void Update () {
		if (incident == null)
			return;

		if (subjectTransform == null)
			subjectTransform = FindSubjectTransform (incident.Subject ().transform);

		panel.GetWorldCorners(corners);
		line.SetPosition (0, subjectTransform.position);
		line.SetPosition (1, Camera.main.ScreenToWorldPoint((corners[0] + corners[1])/2f).FixZ(-10f));

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