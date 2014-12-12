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
	public IStop stop;

	public IList<Edge> Path = new List<Edge>();
	public int currentTrack;

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
		var prev = this.Path;
		this.currentTrack = path.IndexOf (this.Path [currentTrack]);
		this.Path = path;

		// Send events
		LightRailGame.ScoreManager.DoReroute (new ScoreManager.RerouteEventArgs { 
			Train = this,
			Route = path,
			PreviousRoute = prev 
		});
		if (OnPathChange != null)
			OnPathChange (this);
	}

	// Update is called once per frame
	void FixedUpdate () {
		// Clean resolved incidents
		if (incident != null && incident.IsResolved ()) {
			incident = null;
		}

		// Leave stops, if possible
		if (stop != null && stop.TryLeave(this)) {
			stop = null;
		}
	
		// Don't move tram if the game is paused or an incident exists
		if (lightRailGame.paused || incident != null || Path.Count == 0 || stop != null) {
			return;
		}

		// Stop at stops
		Edge e = Path [currentTrack];
		if (position + speed * Time.deltaTime > e.GetLength()){
			stop = (IStop) e.To.GetComponent<Station>() ?? (IStop) e.To.GetComponent<TrafficLight>();
			if(stop != null){
				stop.Arrive(this);
			}
		}

		// New way of moving: move along Path defined in Train class
		UpdateToNextPosition(position + speed * Time.deltaTime);
		this.speed = this.speed + acceleration () * Time.deltaTime;
	}

	/**
	 * Get next track segment, if there is any
	 */
	public bool TryGetNextTrack(out int nextTrack, out Edge track) {
		var nI = (currentTrack + 1) % Path.Count;
		if (Path [currentTrack].To == Path [nI].From) {
			nextTrack = nI;
			track = Path[nI];
			return true;
		}
		track = Path [currentTrack];
		nextTrack = currentTrack;
		return false;
	}

	/**
	 * Update position of Train
	 */
	public void UpdateToNextPosition(float unitsFromStation){
		Edge current = Path [currentTrack];

		while (current.GetLength () < unitsFromStation) {
			unitsFromStation -= current.GetLength();
			var previous = current;
			if(!TryGetNextTrack(out this.currentTrack, out current)){
				// At end of defined Path
				return;
			}

			// Send Event
			LightRailGame.ScoreManager.DoNextSegment(new ScoreManager.NextSegmentEventArgs { Train = this, PreviousSegment = previous, Segment = current });
		}

		// Update transform
		float t = current.GetPositionOfUnitPoint (unitsFromStation);
		Vector3 pos = current.GetPoint (t);
		Vector3 rot = current.GetDirection (t);
		pos.z -= 1;
		this.transform.position = pos;
		this.transform.rotation = Quaternion.LookRotation(rot);

		// Store new unit position
	    this.position = unitsFromStation;
	}

	private float maxAcc = 3;

	/**
	 * Get the desired Acceleration,
	 * keeping Stations, Traffic Lights and Trains ahead in mind
	 */
	private float acceleration(){
		Edge current = Path [currentTrack];
		var cL = current.GetLength ();

		var desiredSpeed = this.desiredSpeed;

		/* check for stations/traffic lights/end of track here */
		Edge _track; int _t;
		var isEndOfPath = !TryGetNextTrack (out _t, out _track);
		var station = current.To.gameObject.GetComponent<Station> ();
		var traffic = current.To.gameObject.GetComponent<TrafficLight> ();

		if (isEndOfPath && NeedBreak (0, cL - position)) {
			desiredSpeed = 0;
		} else
		if (station != null && NeedBreak (station.MaxSpeed (this), cL - position)) {
			desiredSpeed = station.MaxSpeed (this);
		} else
		if (traffic != null && NeedBreak (traffic.MaxSpeed (this), cL - position)) {
			desiredSpeed = traffic.MaxSpeed (this);
		}

		var diff = desiredSpeed - this.speed;
		return diff > 0 ? Math.Min (diff, maxAcc) : Math.Max (diff, -maxAcc);
	}

	private bool NeedBreak(float speed, float distanceUntilSpeed){
		var accTime = (this.speed - speed) / maxAcc;
		var avgSpeed = (this.speed + speed) / 2;
		return distanceUntilSpeed < avgSpeed * accTime;
	}

	public void Incident (IIncident incident)
	{
		this.incident = incident;
	}
}
