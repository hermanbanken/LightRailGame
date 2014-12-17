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
		var path = new List<Edge>() { };
		// Dijkstra WayPoints together
		for(int i = 1; i <= waypoints.Count; i++){
			try {
				var seg = new Dijkstra<Edge,Node>(allEdges).PlanRoute(waypoints[i-1], waypoints[i % waypoints.Count]);
				if(!seg.Any ())
					throw new ArgumentException("No route exists between nodes "+waypoints[i-1]+" and "+waypoints[i % waypoints.Count]+" in these "+allEdges.Count+" edges");
				path.AddRange(seg);
			} catch(Exception e){
				Debug.LogException(e);
				break;
			}
		}
		return path;
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
