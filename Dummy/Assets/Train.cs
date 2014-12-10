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
	public IIncident incident;

	public IList<Edge> Path = new List<Edge>();
	public int currentStation;

	public event Action<Train> OnPathChange;

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

	public void UpdatePath(IList<Edge> path){
		this.currentStation = path.IndexOf (this.Path [currentStation]);
		this.Path = path;

		if (OnPathChange != null)
			OnPathChange (this);
	}

	// Update is called once per frame
	void FixedUpdate () {
		// Clean resolved incidents
		if (incident != null && incident.IsResolved ()) {
			incident = null;
		}
	
		// Don't move tram if the game is paused or an incident exists
		if (lightRailGame.paused || incident != null) {
			return;
		}

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

	public void Incident (IIncident incident)
	{
		this.incident = incident;
	}
}
