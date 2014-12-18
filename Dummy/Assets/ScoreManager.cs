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
	public static int ImpactCount = 0;

	// Text fields
	public InputField ScoreText;         
	public InputField ProblemTimerText;     
	public InputField DelayTimerText;       
	public InputField ImpactCounterText;
	
	void Awake ()
	{
		// Reset the score.
		Score = 0;
	}

	void FixedUpdate(){
		// Dummy value increase
		// TODO make sure to only increase when this is appropriate
		ProblemTime += TimeSpan.FromSeconds (Time.fixedDeltaTime);
		DelayTime += TimeSpan.FromSeconds (Time.fixedDeltaTime/2);
		ImpactCount += 1;
		Score += 1;
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
	public event EventHandler<RerouteEventArgs> OnReroute;
	
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

	public virtual void DoReroute (RerouteEventArgs e)
	{
		var handler = OnReroute;
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
}