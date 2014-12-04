using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Train : MonoBehaviour {

	private LightRailGame lightRailGame;

	public GameObject StartingNode;
	private Node currentNode;
	public bool forward = true;
	public float speed = 0;
	public float desiredSpeed = 10f;
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

		FixedUpdate ();
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
	void FixedUpdate () {
		if (lightRailGame.paused)
			return;

		// New way of moving: move along Path defined in Train class
		if (Path.Count > 0) {
			UpdateToNextPosition(position + speed * Time.deltaTime);
		}

		if (Math.Abs (this.speed - this.desiredSpeed) > 0.0001)
			this.speed = this.speed + (this.desiredSpeed - this.speed) * Time.deltaTime / 2;
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
		var q = Quaternion.LookRotation(rot);

	//	q.w = 90;

		//q.x = 90;
		//q.w = 0; 
		//q.z = 90;

		pos.z -= 1;
		this.transform.position = pos;
		this.transform.rotation = q;

	    this.position = unitsFromStation;
	}

	public void Deselect(){
		this.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
	}
	public void Select(){
		this.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
	}
}
