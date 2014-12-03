using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class GUIs
{
	private static bool stateSelectPath = false;
	private static List<Node> newPath = new List<Node>();

	public static void CancelReroute(this LightRailGame game, Train train){
		stateSelectPath = false;
		newPath = new List<Node> ();
	}

	public static void TrainGUI(this LightRailGame game, Train train)
	{
		if (!stateSelectPath) {
			var w = 120;
			var h = 200;
			var x = Screen.width / 2 - w / 2;
			var y = Screen.height / 2 - h / 2;
			var maxSpeed = 10;
			GUI.Box (new Rect (x - 5, y - 5, w + 10, h + 10), "");
			GUILayout.BeginArea (new Rect (x, y, w, h));
			// Make a background box
			GUILayout.Label ("Tram options");
			// Speed slider
			GUILayout.Label ("Desired speed (0-30 km/h):");
			train.desiredSpeed = GUILayout.HorizontalSlider (train.desiredSpeed, 0, maxSpeed);
			// Pause button
			if (GUILayout.Button (train.desiredSpeed == 0 ? "Start tram" : "Stop tram"))
					train.desiredSpeed = train.desiredSpeed == 0 ? maxSpeed : 0;

			if (GUILayout.Button (!stateSelectPath ? "Re-route" : "Cancel re-route")) {
				if(stateSelectPath)
					game.CancelReroute(train);
				else  {
					stateSelectPath = true;
					// As we cannot deviate from current Edge, at this edge as starting Path
					newPath = new List<Node>(new [] { train.Path[train.currentStation].From, train.Path[train.currentStation].To });
				}
			}

			if (GUILayout.Button ("Close dialog")) {
				game.RequestDeselect();
			}

			GUILayout.EndArea ();
		}

		if (stateSelectPath) {
			GUILayout.BeginArea(new Rect (Screen.width - 130, 10, 120, 25));
			if (GUILayout.Button ("Cancel re-routing"))
				game.CancelReroute(train);
			GUILayout.EndArea ();

			var node = train.Path.Select((edge) => edge.To).FirstOrDefault((to) => to.SelectableGUI());
			// Completed path
			if(newPath.Count > 0 && node == newPath[0]){
				train.SetPath(newPath.Cast<GameObject>().ToList());
				stateSelectPath = false;
			}
			// Add node
			else if(node != null){
				var from = newPath.LastOrDefault() ?? train.Path.Select(e => e.From).Skip(train.currentStation).First();
				var subpath = game.graph.Dijkstra.Plan(from, node);
				if(subpath.Count() == 0){
					// TODO Play beeper sound
					Debug.LogWarning("No route exists; Now play Beeper sound");
					// No path
				} else {
					newPath.AddRange(subpath.Skip(subpath.First() == newPath.LastOrDefault() ? 1 : 0));
					// TODO If completed, then update train.Path
					// TODO Show nice visualisation of selected waypoints
				}
			}
			// here: Extend path to make it feasible
			// here: Draw partial new path
		}
	}
}

