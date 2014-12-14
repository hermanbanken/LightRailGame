using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Schedule = LineSchedule;
using Rotorz.ReorderableList;
using UnityEditorInternal; 

public class LinesWindow : EditorWindow
{
	private ReorderableList listLines;
	private ReorderableList listWayPoints;
	private int selectedLine = 0;

	LightRailGame game;
	List<Schedule> schedule;

	private void OnEnable(){
		game = LightRailGame.GetInstance ();
		schedule = game.Schedule;

		listLines = new ReorderableList (schedule, typeof(Schedule), false, true, true, true);
		listLines.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			rect.y += 2;
			var a = 60;
			var b = 120;
			var c = 80;
			var n = rect.width - a - b - c - 15;

			var rectName  = new Rect(rect.x, rect.y, n, EditorGUIUtility.singleLineHeight);
			var rectNrLab = new Rect(rect.x+n+5, rect.y, a, EditorGUIUtility.singleLineHeight);
			var rectCount = new Rect(rect.x+n+a+10, rect.y, b, EditorGUIUtility.singleLineHeight);
			var rectSel   = new Rect(rect.x+n+a+b+15, rect.y, c, EditorGUIUtility.singleLineHeight);

			schedule[index].Name = EditorGUI.TextField(rectName, String.IsNullOrEmpty(schedule[index].Name) ? "Line "+(index+1) : schedule[index].Name);
			EditorGUI.LabelField(rectNrLab, "# of trams");
			schedule[index].TramCount = EditorGUI.IntSlider(rectCount, schedule[index].TramCount, 1, 20);

			EditorGUI.BeginDisabledGroup(selectedLine == index);
			if(GUI.Button(rectSel, "Edit")){
				selectedLine = index;
				listWayPoints.list = schedule [selectedLine].WayPoints;
				GraphInspector.SelectedLine(schedule[selectedLine]);
			}
			EditorGUI.EndDisabledGroup();
		};
		listLines.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "Lines");
		};

		listWayPoints = new ReorderableList (schedule [selectedLine].WayPoints, typeof(Node));
		listWayPoints.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			rect.y += 2;
			var wp = schedule[selectedLine].WayPoints;
			var ns = game.graph.nodes;
			var currentIndex = ns.TakeWhile((n, i) => n != wp.ElementAtOrDefault(index)).Count();
			var labels = ns.Select(n => n.gameObject.name).ToArray();
			var newIndex = EditorGUI.Popup(rect, currentIndex, labels);
			wp[index] = ns.ElementAtOrDefault(newIndex);
		};
		listWayPoints.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "WayPoints of "+schedule[selectedLine].Name);
		};
		listWayPoints.onAddCallback = (ReorderableList list) => {
			schedule [selectedLine].WayPoints.Add(game.graph.nodes.FirstOrDefault());
		};
	}

	public void OnGUI(){

		listLines.DoLayoutList();
		EditorGUILayout.Space ();
		listWayPoints.DoLayoutList();
	}

	[MenuItem("Window/Tram Lines %#l",priority = 3000)]
	public static void Init(){
		LinesWindow w = EditorWindow.GetWindow<LinesWindow> ("Tram Lines", true);
		w.Show ();
	}


	public void OnHierarchyChange(){
//		Debug.Log ("Hierarchy Changed!");
	}
}

public static class OptionExt {

	public static T GetOrElse<K,T>(this IDictionary<K,T> self, K key, T orElse){
		if (self.ContainsKey (key))
			return self [key];
		else
			return orElse;
	}

	public static T GetOrElse<T>(this IEnumerable<T> self, int index, T orElse) where T : class {
		return self.ElementAtOrDefault(index) ?? orElse;
	}

}