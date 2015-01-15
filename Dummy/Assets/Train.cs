using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class Train : MonoBehaviour, IOccupy, IPointerClickHandler, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IEnumerable<Edge>  {

	private LightRailGame lightRailGame;

	private Node currentNode;

	[HideInInspector]
	public bool forward = true;
	public float speed = 0;
	public float desiredSpeed = 10f;
	private float position = 0f;

	public List<IIncident> incident { get; private set; }
	private bool hasIncident; // For performance : null checks are costly
	public IStop stop { get; private set; }
	private bool isAtStop;

	public IList<Node> WayPoints { get; private set; }
	public IList<Node> OriginalWayPoints { get; private set; }
	public IList<Edge> Path { get; private set; }
	public IList<Edge> OriginalPath { get; private set; }
	public int currentTrack;

	public event Action<Train> OnPathChange;

	void Awake (){
		if (Path == null)
			Path = new List<Edge> ();
	}

	// Use this for initialization
	void Start () {
		lightRailGame = LightRailGame.GetInstance();

		// Add collider script to TrainModel
		Collider c = GetComponentInChildren<Collider> ();
		(this.GetComponentInChildren<TrainCollisionDetector> () ?? c.gameObject.AddComponent<TrainCollisionDetector> ()).ReportTo(this);

		if (Path.Count == 0) {
			Debug.LogWarning("Either define a Path for this train.");
			return;		
		}

		incident = new List<IIncident> ();

		FixedUpdate ();
	}

	public void Init(LineSchedule line, IList<Edge> route, float initialUnitPositionOnLine = 0f){
		if (OriginalPath != null)
			throw new InvalidOperationException ("Initialization may only be done once");
		if (Path.Count > 0)
			throw new InvalidOperationException ("Initialize the path only by using the Init method, do not set the Path elsewere");
		Path = route;
		OriginalPath = route;

		WayPoints = line.WayPoints;
		OriginalWayPoints = line.WayPoints.ToList ();

		Edge current = Path [currentTrack];
		while (current.GetLength () < initialUnitPositionOnLine) {
			initialUnitPositionOnLine -= current.GetLength ();
			if (!TryGetNextTrack (out this.currentTrack, out current)) {
				// At end of defined Path
				break;
			}
		}		
		current.Arrive(this);

		position = initialUnitPositionOnLine;
	}

	public void UpdatePath(IList<Node> wayPoints, IList<Edge> preCalculatedPath = null){
		if (wayPoints.Count < 2 && preCalculatedPath == null)
			return;

		if (preCalculatedPath != null && preCalculatedPath.Count == 0)
			return;//throw new ArgumentException ("Empty path given");

		var path = preCalculatedPath ?? wayPoints.RouteFromWayPoints (wayPoints.First ().graph.edges.ToList());

		if (path != null && path.Count == 0)
			return;// throw new ArgumentException ("No route is plannable");

		var prev = this.Path;
		var prevCurrentTrack = currentTrack;
		this.currentTrack = path.IndexOf (this.Path [currentTrack]);
		if (this.currentTrack < 0) {
			path = this.Path.Concat(path).ToList();
			this.currentTrack = prevCurrentTrack;
			// TODO handle when on new track : then fix route to desired path
		}
		this.Path = path;
		this.WayPoints = wayPoints;

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
		if (hasIncident && incident.Any (i => i.IsResolved ())) {
			incident.RemoveAll(i => i.IsResolved());
			hasIncident = incident.Any();
		}

		// Leave stops, if possible
		if (isAtStop && stop.TryLeave(this)) {
			stop = null;
			isAtStop = false;
		}
	
		// Don't move tram if the game is paused or there is no path
		if (lightRailGame.paused || Path.Count == 0 || isAtStop) {
			return;
		}

		// Limit speed
		if (hasIncident) {
			this.speed = Math.Min(this.speed, incident.MinBy(i => Math.Abs(i.MaxSpeedOfSubject())).MaxSpeedOfSubject());
		}

		// Stop at stops
		Edge e = Path [currentTrack];
		if (position + speed * Time.deltaTime > e.GetLength()){
			stop = (IStop) e.To.GetComponent<Station>() ?? (IStop) e.To.GetComponent<TrafficLight>();
			isAtStop = stop != null;
			if(isAtStop){
				stop.Arrive(this);
				LightRailGame.ScoreManager.DoStopVisit(new ScoreManager.StopVisitEventArgs() {
					Train = this,
					Stop = stop
				});
			}
		}

		// Update position if speed != 0
		if (!nearlyEqual(this.speed, 0f, 0.01f)) {
			UpdateToNextPosition(position + speed * Time.deltaTime);
		}

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
		// When fail
		track = Path [currentTrack];
		nextTrack = currentTrack;
		return false;
	}

	/**
	 * Get prev track segment, if there is any
	 */
	public bool TryGetPrevTrack(out int prevTrack, out Edge track) {
		var nI = (currentTrack - 1) % Path.Count;
		if (Path [currentTrack].From == Path [nI].To) {
			prevTrack = nI;
			track = Path[nI];
			return true;
		}
		// When fail
		track = Path [currentTrack];
		prevTrack = currentTrack;
		return false;
	}

	/**
	 * Update position of Train
	 */
	public void UpdateToNextPosition(float unitsFromStation){
		Edge current = Path [currentTrack];

		// Moving forwards
		while (current.GetLength () < unitsFromStation) {
			unitsFromStation -= current.GetLength();
			var previous = current;
			if(!TryGetNextTrack(out this.currentTrack, out current)){
				// At end of defined Path
				return;
			}

			// Send Events
			current.Arrive(this);
			previous.Leave(this);
			LightRailGame.ScoreManager.DoNodeVisit(new ScoreManager.NodeVisitEventArgs { Train = this, Node = current.From });
			LightRailGame.ScoreManager.DoNextSegment(new ScoreManager.NextSegmentEventArgs { Train = this, PreviousSegment = previous, Segment = current });
		}

		// Moving backwards
		while (unitsFromStation < 0) {
			var previous = current;
			if(!TryGetPrevTrack(out this.currentTrack, out current)){
				// At beginning of defined Path
				return;
			}
			unitsFromStation += current.GetLength();
			current.Arrive(this);
			previous.Leave(this);
			LightRailGame.ScoreManager.DoNodeVisit(new ScoreManager.NodeVisitEventArgs { Train = this, Node = current.To });
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
		var station = current.To.Station;
		var traffic = current.To.TrafficLight;

		if (isEndOfPath && NeedBreak (0, cL - position)) {
			desiredSpeed = 0;
		} else
		if (station != null && NeedBreak (station.MaxSpeed (this), cL - position)) {
			desiredSpeed = station.MaxSpeed (this);
		} else
		if (traffic != null && NeedBreak (traffic.MaxSpeed (this), cL - position - this.collider.bounds.size.magnitude / 2.2f)) {
			desiredSpeed = traffic.MaxSpeed (this);
		}

		/* Stop for vehicles and obstacles ahead */
		var ahead = currentTrack;
		var accum = 0f;
		var forZero = DistanceUntilSpeed(0);
		var clearange = this.collider.bounds.size.magnitude * 3f;
		do 
		{
			if(desiredSpeed == 0) break;

			// TODO refine maths here; 
			var block = Path [ahead].GetOccupants ()
				.Where (o => accum + o.Location.Item2 > this.position)
				.Where (o => accum + o.Location.Item2 - this.position < forZero + clearange);
			if (block.Any ()) {
				var min = block.MinBy (o => accum + o.Location.Item2 - this.position);
				desiredSpeed = Math.Min(desiredSpeed, min.Speed);
			}

			accum += Path[ahead].GetUnitLength();
			ahead = (ahead + 1) % Path.Count;
		}
		// If we don't need to break for vehicles standing this far away, don't look further
		while(accum - position < forZero + clearange);

		// Limit de- or accelleration
		var diff = desiredSpeed - this.speed;
		return diff > 0 ? Math.Min (diff, maxAcc) : Math.Max (diff, -maxAcc);
	}

	private float DistanceUntilSpeed(float speed){
		var accTime = (this.speed - speed) / maxAcc;
		var avgSpeed = (this.speed + speed) / 2;
		return avgSpeed * accTime;
	}

	private bool NeedBreak(float speed, float distanceUntilSpeed){
		var accTime = (this.speed - speed) / maxAcc;
		var avgSpeed = (this.speed + speed) / 2;
		return distanceUntilSpeed < avgSpeed * accTime;
	}

	public void Incident (IIncident incident)
	{
		this.incident.Add(incident);
		this.hasIncident = true;
		this.speed = Math.Min(this.speed, incident.MaxSpeedOfSubject());
	}

	public static bool nearlyEqual(float a, float b, float epsilon) {
		float absA = Math.Abs(a);
		float absB = Math.Abs(b);
		float diff = Math.Abs(a - b);
		
		if (a == b) { // shortcut, handles infinities
			return true;
		} else if (a == 0 || b == 0 || diff < float.MinValue) {
			// a or b is zero or both are extremely close to it
			// relative error is less meaningful here
			return diff < (epsilon * float.MinValue);
		} else { // use relative error
			return diff / (absA + absB) < epsilon;
		}
	}

	#region IOccupy implementation

	public Eppy.Tuple<ILine, float> Location {
		get {
			return Eppy.Tuple.Create<ILine, float>(Path[currentTrack], position);
		}
	}

	public float Speed {
		get {
			return speed;
		}
	}

	#endregion

	#region IPointerClickHandler implementation

	void IPointerClickHandler.OnPointerClick (PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject (this.gameObject, eventData);
	}

	#endregion
	
	#region ISelectHandler implementation

	void ISelectHandler.OnSelect (BaseEventData eventData)
	{
		lightRailGame.DoSelect (gameObject);
		
		foreach (var renderer in this.GetComponentsInChildren<SpriteRenderer>())
			renderer.sprite = lightRailGame.GhostTram;

		lightRailGame.OnSelectedGameObjectChanged += ChangeSpriteBack;
	}
	
	void ChangeSpriteBack (GameObject _)
	{
		lightRailGame.OnSelectedGameObjectChanged -= this.ChangeSpriteBack;

		foreach (var renderer in this.GetComponentsInChildren<SpriteRenderer>())
			renderer.sprite = lightRailGame.NormalTram;
	}
	#endregion

	#region IPointerEnterHandler implementation

	public void OnPointerEnter (PointerEventData eventData)
	{
		foreach (var renderer in this.GetComponentsInChildren<SpriteRenderer>())
			renderer.sprite = lightRailGame.GhostTram;
	}

	#endregion

	#region IPointerExitHandler implementation

	public void OnPointerExit (PointerEventData eventData)
	{
		if(lightRailGame.SelectedGameObject != this.gameObject)
		foreach (var renderer in this.GetComponentsInChildren<SpriteRenderer>())
			renderer.sprite = lightRailGame.NormalTram;
	}

	#endregion

	#region IEnumerable implementation

	public IEnumerator<Edge> GetEnumerator ()
	{
		return new EdgeEnumerator (this, Path, currentTrack);
	}

	#endregion

	#region IEnumerable implementation

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return new EdgeEnumerator (this, Path, currentTrack);
	}

	#endregion

	public class EdgeEnumerator : IEnumerator<Edge> {
		IList<Edge> edges;
		int i;
		int start;
		Train self;
		int loops = 0;

		public EdgeEnumerator(Train self, IList<Edge> edges, int start = 0){
			this.self = self;
			this.edges = edges;
			this.i = start - 1;
			this.start = start;
		}

		#region IEnumerator implementation
		public bool MoveNext ()
		{
			if (edges.Count > i + 1) {
				i++;
				return true;
			} else
			if (edges[i].To == edges[0].From){
				loops++;
				if(loops > 4){
					Debug.LogWarning("Looping Train iterator", self);
					return false;
				}
				i = 0;
				return true;
			}
			return false;
		}
		public void Reset ()
		{
			i = start - 1;
		}
		public Edge Current {
			get {
				return edges[i];
			}
		}
		#endregion
		#region IDisposable implementation
		public void Dispose ()
		{
			edges = null;
		}
		#endregion

		#region IEnumerator implementation
		object IEnumerator.Current { get { return Current; } }
		#endregion
	}
}
