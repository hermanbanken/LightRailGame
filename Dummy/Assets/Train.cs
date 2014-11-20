﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Train : MonoBehaviour {

	private LightRailGame lightRailGame;

	public GameObject StartingNode;
	private Node currentNode;
	public bool forward = true;
	public float speed = 10f;
	private float position = 0f;

	public List<Edge> Path = new List<Edge>();
	public int currentStation;

	// Use this for initialization
	void Start () {
		lightRailGame = GameObject.Find("LightRailGame").GetComponent<LightRailGame> ();

		// Add collider script to TrainModel
		Collider c = GetComponentInChildren<Collider> ();
		(this.GetComponentInChildren<TrainCollisionDetector> () ?? c.gameObject.AddComponent<TrainCollisionDetector> ()).ReportTo(this);

		if (StartingNode == null && Path.Count == 0) {
			Debug.LogWarning("Either define a StartingNode or a Path for this train.");
			return;		
		}

		Update ();
	}

	public void SetPath(IList<GameObject> path){
		Debug.Log("Current station was "+this.Path[currentStation].name);
//		var p = path.ToList ();
//		var whereToStart = p.Where ((go, i) => {
//			var next = path [(i + 1) % path.Count];
//			if (go != this.Path [currentStation] && next != this.Path [currentStation])
//					return false;
//			var direction = (go.transform.position - next.transform.position).normalized;
//			return true;
//		}).First ();
//
//		this.Path = path.ToList();
//		this.currentStation = path.IndexOf (whereToStart);
//		Debug.Log("Current station is now "+Path[currentStation].name);
	}

	// Update is called once per frame
	void Update () {
		if (lightRailGame.paused)
			return;

		// New way of moving: move along Path defined in Train class
		if (Path.Count > 0) {
			UpdateToNextPosition(position + speed * Time.deltaTime);
		}

		if (this.speed < 10f)
			this.speed = Math.Min(10f, this.speed+Time.deltaTime);
	}

	public void UpdateToNextPosition(float unitsFromStation){
		Edge current = Path [currentStation];

		while (current.GetLength () < unitsFromStation) {
			Debug.Log ("Train went onto new Edge");
			unitsFromStation -= current.GetLength();
			currentStation = (currentStation + 1) % Path.Count;
			current = Path[currentStation];
		}

		float t = current.GetPositionOfUnitPoint (unitsFromStation);
		Vector3 pos = current.GetPoint (t);
		Vector3 rot = current.GetDirection (t);

		var parentTransform = this.gameObject.transform;
		pos.z -= 1;
		this.transform.position = pos;
		this.transform.rotation = Quaternion.LookRotation(rot);

	    this.position = unitsFromStation;
	}

	public void Deselect(){
		this.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
	}
	public void Select(){
		this.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
	}
}
