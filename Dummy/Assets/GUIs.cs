using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class GUIs
{
	private static void CenterInScreen(int w, int h, out int top_x, out int left_y){
		top_x = Screen.width / 2 - w / 2;
		left_y = Screen.height / 2 - h / 2;
	}

	/**
	 * Show incident GUI, returns true only if the user clicked the button
	 */
	public static bool IncidentGUI(this IIncident incident){
		int w = 600, h = 200;
		int x, y;

		var actions = incident.PossibleActions ();
		var incidents = new [] { incident }.ToList();
		if (incident.Subject ().GetComponent<Train> () != null) {
			incidents = incident.Subject ().GetComponent<Train> ().incident;
			actions = incidents.SelectMany(i => i.PossibleActions()).Distinct();
		}

		h = 100 + actions.Count () * 30;

		CenterInScreen(w, h, out x, out y);
		GUI.Box (new Rect (x - 5, y - 5, w + 10, h + 10), "");
		GUILayout.BeginArea (new Rect (x, y, w, h));
		GUI.skin.button.alignment = TextAnchor.MiddleLeft;
		if(incidents.Distinct().Count () == 1)
			GUILayout.Label (incident.Description()+" How would you like to resolve this issue?");
		else
			GUILayout.Label (incidents.Select(i => i.Description()).Distinct().Aggregate("Several incidents occured. ", (a,b) => a + b + " ") + "How would you like to resolve these issues?");

		GUILayout.BeginHorizontal();
		GUILayout.Label ("Action");
		GUILayout.Label ("Duration", GUILayout.Width(50));
		GUILayout.Label ("Succes%", GUILayout.Width(60));
		GUILayout.EndHorizontal();

		GUILayout.BeginScrollView (Vector2.zero);
		foreach (ISolution s in actions) {
			GUILayout.BeginHorizontal();
			GUI.enabled = s as IPowerUp == null || (s as IPowerUp).IsAvailable();
			if(GUILayout.Button (s.ProposalText)){
				if(s as IPowerUp != null) (s as IPowerUp).Use();
				incidents.ForEach(i => i.SetChosenSolution(s));
				return true;
			}
			GUI.enabled = true;
			GUILayout.Label (((int) s.ResolveTime.TotalSeconds) + " sec.", GUILayout.Width(50));
			GUILayout.Label (((int)(s.SuccessRatio*100)+"%"), GUILayout.Width(60));
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();

		if (GUILayout.Button ("Cancel", GUILayout.ExpandWidth(false), GUILayout.Width(100)))
			return true;

		GUILayout.EndArea ();
		return false;
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

