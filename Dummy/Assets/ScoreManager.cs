using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
	// Values
	public static int Score;        		// The player's score.
	public static TimeSpan ProblemTime = TimeSpan.Zero;
	public static TimeSpan DelayTime = TimeSpan.Zero;
	public static int ImpactCount;

	public static int PTcount;
	public static int DTcount;
	public static int BalancePDcount;   
	public static int treshholdPT;

	public static IDictionary<IIncident,float> StartTime = new Dictionary<IIncident, float>();

	// Text fields
	public InputField ScoreText;         
	public InputField ProblemTimerText; 
	public InputField DelayTimerText;       
	public InputField ImpactCounterText;
	
	void Awake ()
	{
		// Reset the score.
		Score = 0;
		ImpactCount = 0;
		PTcount = 0;
		DTcount = 0;
		BalancePDcount = 2; // DT counts half the time, value [0,5] High is less importance off DT
		treshholdPT = 4; // start timer after the 4th incident

	}
	
	void Start()
	{
		ScoreText = ScoreText ?? GameObject.Find ("SM_Score").GetComponent<InputField>();
		ImpactCounterText = ImpactCounterText ?? GameObject.Find ("SM_Impact").GetComponent<InputField>();
		ProblemTimerText = ProblemTimerText ?? GameObject.Find ("SM_ProblemTimer").GetComponent<InputField>();
		DelayTimerText = DelayTimerText ?? GameObject.Find ("SM_DelayTimer").GetComponent<InputField>();

		OnResolved += (IIncident obj) => { 
			PTcount -= 1; 
			Score += 10;
			StartTime.Remove(obj);
		};
		OnOccur += (IIncident obj) => {
			PTcount += 1;
			StartTime [obj] = Time.time;
		};
		OnStationDue += (object sender, StationDueEventArgs e) => {
						DTcount += 1;
						ImpactCount += 1;};
		OnStationOk  += (object sender, StationDueEventArgs e) => DTcount -= 1;
		OnReroute += (object sender, RerouteEventArgs e) => ImpactCount += 34;

		// Reroute should influence PT or DT since DT is influenced by stations PT is used. 
		// PTcount += 1;
		// onDEroute ....
		// PTcount -= 1;

		OnUserAction += (IIncident obj) => {
			// Time since occur/last action
			var timeSince = Time.time - StartTime[obj];
	
			// DUmmy
			var suitability = obj.Suitability(obj.GetChosenSolution());
			if (suitability <= 0) Score -= 100000;

			// Insert smartness here
			if (timeSince <= 10) Score += 1000; else Score -= 1000;

			// Prepare for possible fail
			StartTime[obj] = Time.time;
		};

		OnDesiredSpeedChange += (object sender, DesiredSpeedChangeEventArgs e) => {
			// als de previous > 0 en current == 0 dan net gestopt.
			if (e.Previous > 0 && e.Current == 0) ImpactCount += 100;
			if (e.Previous == 0 && e.Current > 0) ImpactCount -= 50;
		};
	}

	void FixedUpdate(){
		// test values used to increase.
		// TODO tweak the values to be sure to only increase appropriate
		if (PTcount > treshholdPT) ProblemTime += TimeSpan.FromSeconds (Time.fixedDeltaTime);
		if (DTcount > 0) DelayTime += TimeSpan.FromSeconds (Time.fixedDeltaTime/BalancePDcount);
		//ImpactCount += 1;
		//Score += 1;
	}

	void Update ()
	{
		// Limit changing framerate
		if(Time.frameCount%4 == 0){
			ProblemTimerText.text = ProblemTime.FormatMinSec();
			DelayTimerText.text = DelayTime.FormatMinSec();
			ImpactCounterText.text = ImpactCount.ToString();
			ScoreText.text = Score.ToString();
		}
	}

	/* Events */
	public event EventHandler<NextSegmentEventArgs> OnNextSegment;
	public event EventHandler<NodeVisitEventArgs> OnNodeVisit;
	public event EventHandler<StopVisitEventArgs> OnStopVisit;
	public event EventHandler<StationDueEventArgs> OnStationDue;
	public event EventHandler<StationDueEventArgs> OnStationOk;
	public event Action<IIncident> OnOccur;
	public event Action<IIncident> OnUserAction;
	public event Action<IIncident> OnResolved;
	public event Action<IIncident> OnFailed;
	public event EventHandler<RerouteEventArgs> OnReroute;
	public event EventHandler<DesiredSpeedChangeEventArgs> OnDesiredSpeedChange;
	
	/* Event invocators */
	public virtual void DoNextSegment (NextSegmentEventArgs e)
	{
		var handler = OnNextSegment;
		if (handler != null)
			handler (this, e);
	}

	public virtual void DoNodeVisit (NodeVisitEventArgs e)
	{
		var handler = OnNodeVisit;
		if (handler != null)
			handler (this, e);
	}

	public virtual void DoStopVisit (StopVisitEventArgs e)
	{
		var handler = OnStopVisit;
		if (handler != null)
			handler (this, e);
	}

	public virtual void DoStationDue (StationDueEventArgs e)
	{
		var handler = OnStationDue;
		if (handler != null)
			handler (this, e);
	}

	public virtual void DoStationOk (StationDueEventArgs e)
	{
		var handler = OnStationOk;
		if (handler != null)
			handler (this, e);
	}

	public virtual void DoOccur (IIncident obj)
	{
		var handler = OnOccur;
		if (handler != null)
			handler (obj);
	}

	public virtual void DoUserAction (IIncident obj)
	{
		var handler = OnUserAction;
		if (handler != null)
			handler (obj);
	}

	public virtual void DoResolved (IIncident obj)
	{
		var handler = OnResolved;
		if (handler != null)
			handler (obj);
	}
	
	public virtual void DoFailed (IIncident obj)
	{
		var handler = OnFailed;
		if (handler != null)
			handler (obj);
	}

	public virtual void DoReroute (RerouteEventArgs e)
	{
		var handler = OnReroute;
		if (handler != null)
			handler (this, e);
	}
	
	public virtual void DoDesiredSpeedChange (DesiredSpeedChangeEventArgs e)
	{
		var handler = OnDesiredSpeedChange;
		if (handler != null)
			handler (this, e);
	}

	/* Events */
	public class NextSegmentEventArgs : EventArgs {
		public Train Train { get; set; }
		public Edge Segment { get; set; }
		public Edge PreviousSegment { get; set; }
	}

	public class NodeVisitEventArgs : EventArgs {
		public Train Train { get; set; }
		public Node Node { get; set; }
	}
	
	public class StopVisitEventArgs : EventArgs {
		public Train Train { get; set; }
		public IStop Stop { get; set; }
	}
	
	public class StationDueEventArgs : EventArgs {
		public Station Station { get; set; }
	}

	public class RerouteEventArgs : EventArgs {
		public Train Train { get; set; }
		public IEnumerable<Edge> Route { get; set; }
		public IEnumerable<Edge> PreviousRoute { get; set; }
	}

	public class DesiredSpeedChangeEventArgs : EventArgs {
		public Train Train { get; set; }
		public float Previous { get; set; }
		public float Current { get; set; }
	}

}