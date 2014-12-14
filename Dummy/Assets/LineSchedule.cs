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

public static class LineExt{
	public static IList<Edge> RouteFromWayPoints(this IList<Node> waypoints, IList<Edge> allEdges){
		var path = new List<Edge>() { };
		// Dijkstra WayPoints together
		for(int i = 1; i <= waypoints.Count; i++){
			try {
				path.AddRange(new Dijkstra<Edge,Node>(allEdges).PlanRoute(waypoints[i-1], waypoints[i % waypoints.Count]));
			} catch(Exception e){
				Debug.Log ("Exception while evaluating lines");
				Debug.LogException(e);
				break;
			}
		}
		return path;
	}

	public static IList<Edge> RouteFromWayPoints(this LineSchedule line, IList<Edge> allEdges){
		return line.WayPoints.RouteFromWayPoints (allEdges);
	}
}
