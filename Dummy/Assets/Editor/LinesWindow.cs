using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Schedule = LineSchedule;
//using Rotorz.ReorderableList;
//using UnityEditorInternal; 

public class LinesWindow : EditorWindow
{
	private List<Schedule> RemovedLines = new List<Schedule> ();
	private Dictionary<int,bool> visibleLines = new Dictionary<int, bool>();

	public void OnGUI(){
		var game = LightRailGame.GetInstance ();
		var schedule = game.Schedule;

		/* Draw Line Counter */
		var newCount = EditorGUILayout.IntField ("Number of lines", schedule.Count);

		// Remove if Count is decreased (but cache, as this might be temporary)
		if (newCount < schedule.Count) {
			RemovedLines.AddRange(schedule.GetRange(newCount, schedule.Count - newCount).Reverse<Schedule>());
			schedule.RemoveRange (newCount, schedule.Count - newCount);
		} else 
		// Add if Count is increased (but reuse cache, as decrease might have been unintended)
		if (newCount > schedule.Count) {
			var reused = Math.Min (RemovedLines.Count, newCount - schedule.Count);
			// First add from cache, then add new lines if neccessary
			schedule.AddRange(
				RemovedLines.Reverse<Schedule>().Take(reused).Concat(
					Enumerable.Range(0, Math.Max(0, newCount - schedule.Count - reused))
						.Select(i => new Schedule()
		        )
			));
			RemovedLines.RemoveRange(RemovedLines.Count - reused, reused);
		}

		/* Draw inspectors for Lines */
		for (int i = 0; i < schedule.Count; i++) {
			visibleLines[i] = EditorGUILayout.Foldout(visibleLines.GetOrElse(i, i == 0), "Line "+(i+1));
			if(visibleLines[i]){
				EditorGUI.indentLevel++;
				schedule[i].Name = EditorGUILayout.TextField("Name", schedule[i].Name);
				schedule[i].TramCount = EditorGUILayout.IntSlider("# of trams", schedule[i].TramCount, 1, 20);

				//ReorderableListGUI.ListField(chedule[i].WayPoints, WayPointListItem, DrawEmpty);
				for(int j = 0; j < schedule[i].WayPoints.Count; j++){
					//schedule[i].WayPoints[j].
				}

				EditorGUI.indentLevel--;
			}
		}
	}

	public string WayPointListItem(Rect position, string itemValue){
		
	}

	[MenuItem("Window/Tram Lines %#l",priority = 3000)]
	public static void Init(){
		LinesWindow w = EditorWindow.GetWindow<LinesWindow> ("Tram Lines", true);
		w.Show ();
	}


	public void OnHierarchyChange(){
		Debug.Log ("Hierarchy Changed!");
	}
}

public static class DictExt {

	public static T GetOrElse<K,T>(this IDictionary<K,T> self, K key, T orElse){
		if (self.ContainsKey (key))
			return self [key];
		else
			return orElse;
	}

}