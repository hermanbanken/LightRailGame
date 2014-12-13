using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
	public static int score;        // The player's score.
	Text text;                      // Reference to the Text component.

	void Awake ()
	{
		// Set up the reference.
		text = GetComponent <Text> ();
		
		// Reset the score.
		score = 0;
	}
	
	void Update ()
	{
		// Set the displayed text to be the word "Score" followed by the score value.
		text.text = "Score:" +score.ToString();
	}

	/* Events */
	public event EventHandler<NextSegmentEventArgs> OnNextSegment;
	public event EventHandler<NodeVisitEventArgs> OnNodeVisit;
	public event EventHandler<StopVisitEventArgs> OnStopVisit;
	public event EventHandler<StationDueEventArgs> OnStationDue;
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