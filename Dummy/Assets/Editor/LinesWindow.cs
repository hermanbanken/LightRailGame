using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Schedule = LineSchedule;
using Rotorz.ReorderableList;
using UnityEditorInternal; 

/**
 * This class uses ReorderableList which is documented extremely well here:
 * @see http://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/
 */
public class LinesWindow : EditorWindow
{
	private ReorderableList listLines;
	private ReorderableList listWayPoints;
	private int selectedLine = 0;

	LightRailGame game;
	List<Schedule> schedule;
	public static LinesWindow instance;

	void OnEnable(){
		instance = this;
		game = LightRailGame.GetInstance ();

		// Probably no scene loaded
		if (game == null) {
			instance.Close ();
			return;
		}

		schedule = game.Schedule;

		// Make sure we have at least one schedule
		if (schedule.Count == 0)
			schedule.Add (new Schedule ());

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
		listLines.onChangedCallback = list => {
			if(selectedLine >= schedule.Count){
				selectedLine--;
				this.Repaint();
				return;
			}
		};

		listWayPoints = new ReorderableList (schedule [selectedLine].WayPoints, typeof(Node));
		listWayPoints.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
			rect.y += 2;
			var wp = schedule[selectedLine].WayPoints;
			var ns = game.graph.nodes;
			var currentIndex = ns.TakeWhile((n, i) => n != wp[index]).Count();
			var labels = ns.Select(n => n.gameObject.name).ToArray();
			var newIndex = EditorGUI.Popup(rect, currentIndex, labels);
			if(currentIndex != newIndex){
				wp[index] = ns.ElementAtOrDefault(newIndex);
			}
		};
		listWayPoints.drawHeaderCallback = (Rect rect) => {
			EditorGUI.LabelField(rect, "WayPoints of "+schedule[selectedLine].Name);
		};
		listWayPoints.onAddCallback = (ReorderableList list) => {
			schedule [selectedLine].WayPoints.Add(game.graph.nodes.FirstOrDefault());
			GUI.changed = true;
		};

		this.Repaint ();
	}

	void OnGUI(){
		if(game == null) 
			OnEnable ();

		var old = game.Schedule;
		EditorGUI.BeginChangeCheck ();

		listLines.DoLayoutList();
		EditorGUILayout.Space ();
		listWayPoints.DoLayoutList();

		if (EditorGUI.EndChangeCheck ()) {
			var _new = game.Schedule;
			game.Schedule = old;
			Undo.RecordObject (game, "Change Tram Line");
			game.Schedule = _new;
			EditorUtility.SetDirty(game);
			GraphInspector.SelectedLine(schedule[selectedLine]);
		}
	}

	[MenuItem("Window/Tram Lines %#l",priority = 3000)]
	public static void Init(){
		EditorWindow.GetWindow<LinesWindow> ("Tram Lines", true).Show ();
	}

	public void OnHierarchyChange(){
//		Debug.Log ("Hierarchy Changed!");
	}

	public static Schedule SelectedLine(){
		return instance == null ? null : instance.schedule.ElementAtOrDefault (instance.selectedLine);
	}

	void OnFocus(){
		GraphInspector.SelectedLine (schedule [selectedLine % schedule.Count]);
	}

	void OnDestroy(){
		if (instance == this) {
			instance.Close();
			instance.game = null;
			instance.schedule = null;
			instance = null;
		}
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