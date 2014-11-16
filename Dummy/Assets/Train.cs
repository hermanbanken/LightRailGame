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
	public float speed = 10f;
	private float position = 0f;

	public List<GameObject> Path = new List<GameObject>();
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

		// Store starting node
		currentNode = StartingNode == null ? null : StartingNode.GetComponent<Node>();
	
		// Read implicit Path that follows from WayPoint directions
		if (Path.Count == 0 && currentNode != null) {
			Path.Add (StartingNode);
			while(currentNode.nextNode != null && currentNode.nextNode.GetComponent<Node>() != null && currentNode.nextNode != StartingNode){
				Path.Add(currentNode.nextNode);
				currentNode = currentNode.nextNode.GetComponent<Node>();
			}	
		}

		Update ();
	}

	public void SetPath(IList<GameObject> path){
		Debug.Log("Current station was "+this.Path[currentStation].name);
		var p = path.ToList ();
		var whereToStart = p.Where ((go, i) => {
			var next = path [(i + 1) % path.Count];
			if (go != this.Path [currentStation] && next != this.Path [currentStation])
					return false;
			var direction = (go.transform.position - next.transform.position).normalized;
			return true;
		}).First ();

		this.Path = path.ToList();
		this.currentStation = path.IndexOf (whereToStart);
		Debug.Log("Current station is now "+Path[currentStation].name);
	}

	// Update is called once per frame
	void Update () {
		if (lightRailGame.paused)
			return;

		// New way of moving: move along Path defined in Train class
		if (Path.Count > 0) {
			UpdateToNextPosition(position + speed * Time.deltaTime);
		}
		// Old way of moving: move along waypoints
		else {
			if (currentNode == null)
				return;

			position += speed * Time.deltaTime;

			Vector3 pos;
			Vector3 dest;
			currentNode = currentNode.PositionOnLineReturningOverflow (position, out position, out pos, out dest);
			transform.position = pos;
			transform.rotation = Quaternion.LookRotation (dest - pos); ///Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(dest - pos));

		}
		
		if (this.speed < 10f)
			this.speed = Math.Min(10f, this.speed+Time.deltaTime);
	}

	public void UpdateToNextPosition(float unitsFromStation){
		int nextStation = (currentStation + 1) % Path.Count;

		Vector3 from = Path [currentStation].transform.position;
		Vector3 to = Path [nextStation].transform.position;
				
		float overflow = unitsFromStation - (to - from).magnitude;

		if (overflow > 0) {
			currentStation = nextStation;
			UpdateToNextPosition (overflow);
		} else {
			position = unitsFromStation;
			this.transform.position = from + (to - from).normalized * unitsFromStation;
			this.transform.rotation = Quaternion.LookRotation (to - from);
		}
	}

	public void Deselect(){
		this.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.gray;
	}
	public void Select(){
		this.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
	}
}
