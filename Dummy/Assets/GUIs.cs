using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class GUIs
{
	private static bool stateSelectPath = false;
	private static List<Edge> newPath = new List<Edge>();
	private static ILine selectedPath = null;

	public static void CancelReroute(this LightRailGame game, Train train){
		stateSelectPath = false;
		newPath = new List<Edge> ();
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
					newPath = new List<Edge>(new [] { train.Path[train.currentStation] });
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

			var node = game.graph.edges.Select((edge) => edge.To).FirstOrDefault((to) => to.SelectableGUI());

			// Add node
			if(node != null){
				// Find last station
				var from = (newPath.LastOrDefault() ?? train.Path.Skip(train.currentStation).First()).To;
				// Do Dijkstra route plan
				var subpath = game.graph.Dijkstra.Plan(from, node);

				// No path was found
				if(subpath.Count() == 0){
					// TODO Play beeper sound
					Debug.LogWarning("No route exists; Now play Beeper sound");
				} else {
					// Subpath contianed only intermediate/end nodes, not the start, so: add start
					subpath = new [] { from }.Concat(subpath);

					// Add new edges
					newPath.AddRange(subpath.EachPair((a, b) => {
						return game.graph.edges.FirstOrDefault(e => e.From == a && e.To == b);
					}).Where (e => e != null));

					// Redraw line
					game.LineMaster.HideLine(selectedPath);
					selectedPath = null;
				}

				// Completed path
				if(newPath.Count > 1 && node == newPath.First().From){			
					Debug.Log ("Old "+train.Path.ToStr());
					Debug.Log ("New "+newPath.ToStr());
					// TODO If completed, then update train.Path
					train.UpdatePath(newPath.ToList());
					stateSelectPath = false;
				}
			}
		}
		
		if (selectedPath == null && stateSelectPath) {
			// TODO Show nice visualisation of selected waypoints
			selectedPath = new CombinedLine (newPath.Cast<ILine> ());				
			game.LineMaster.ShowLine (selectedPath, new LineOptions {
				widths = new [] { 1f, 1f },
				colors = new [] { Color.blue, Color.green },
				offset = Vector3.back
			});
		} else if (!stateSelectPath && selectedPath != null) {
			game.LineMaster.HideLine(selectedPath);
			selectedPath = null;
		} 
	}
}

public static class Extensions {
	public static string ToStr(this IEnumerable<Node> self){
		return "Path = " + self.Select(n => n.ToString()).Aggregate("", (s,n) => s+"|"+n) + ";";
	}
	public static string ToStr(this IEnumerable<Edge> self){
		return "Path = " + self.Select(e => e.From.ToString()+"->"+e.To.ToString()).Aggregate("", (s,n) => s+"|"+n) + ";";
	}

	public static IEnumerable<TResult> EachPair<TSource, TResult>(this IEnumerable<TSource> source,
	                                                        Func<TSource, TSource, TResult> transformation)
	{
		if (source == null) throw new ArgumentNullException("source");
		if (transformation == null) throw new ArgumentNullException("transformation");
		return EachPairImpl(source, transformation);
	}

	private static IEnumerable<TResult> EachPairImpl<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TSource, TResult> f)
	{
		using (var i = source.GetEnumerator())
		{
			TSource prev = default(TSource);
			bool first = true;

			while(i.MoveNext()){
				if(first){
					first = false;
				} else {
					yield return f(prev, i.Current);
				}
				prev = i.Current;
			}
		}
	}
}

