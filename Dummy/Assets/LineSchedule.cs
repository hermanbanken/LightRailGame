using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization;

[System.Serializable]
public class LineSchedule : System.Object
{
	public string Name;
	public int TramCount;
	public List<Node> WayPoints;
	
	public LineSchedule(){
		WayPoints = new List<Node>();
	}
}

public static class LineExt {
	public static IList<Edge> RouteFromWayPoints(this IList<Node> waypoints, IList<Edge> allEdges){
		var dijk = new Dijkstra<Edge,Node> (allEdges);
		var path = new List<Edge>() { };
		// Only single waypoint:
		if (waypoints.Count == 1) {
			try {
				waypoints.Add(allEdges.Single(e => e.From == waypoints[0]).To);
			} catch (Exception) {
				throw new ArgumentException("The route is ambigous since no single outgoing edge from the one waypoint given exists");
			}
		}
		// Dijkstra WayPoints together
		for(int i = 1; i <= waypoints.Count; i++){
			if(waypoints[i-1] == waypoints[i % waypoints.Count])
				continue;
			try {
				var seg = dijk.PlanRoute(waypoints[i-1], waypoints[i % waypoints.Count]);
				if(!seg.Any ())
					throw new ArgumentException("No route exists between nodes "+waypoints[i-1]+" and "+waypoints[i % waypoints.Count]+" in these "+allEdges.Count+" edges");
				path.AddRange(seg);
			} catch(Exception e){
				Debug.LogException(e);
				break;
			}
		}

		/* Validate route: */
		assert (waypoints.All(wp => path.Any(e => e.To == wp || e.From == wp)), "All waypoints are covered");
		path.Scan((a, b) => { assert (a.To == b.From, "All edges are connected"); return b; }, path.Last());
		path.ForEach (e => assert (allEdges.Contains (e), "Edge "+e+" does not exist."));

		return path;
	}

	public static void assert(bool a, string message){
		if(!a)
			Debug.LogError(message);
	}

	public static IList<Edge> RouteFromWayPoints(this LineSchedule line, IList<Edge> allEdges){
		return line.WayPoints.RouteFromWayPoints (allEdges);
	}
	
	public static T GetCurrent<T>(this IEnumerable<T> self, float unitPosition, out float unitPositionOnCurrent) where T : ILine {
		unitPositionOnCurrent = unitPosition;
		var e = self.GetEnumerator ();
		while(e.MoveNext()){
			var length = e.Current.GetUnitLength();
			if(length > unitPositionOnCurrent){
				return e.Current;
			} else {
				unitPositionOnCurrent -= length;
			}
		}
		throw new ArgumentOutOfRangeException("unitPosition");
	}
}
