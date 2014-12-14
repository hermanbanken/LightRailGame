using System;
using UnityEditor;
using UnityEngine;

public class LinesWindow : EditorWindow
{
	public void OnGUI(){
//		EditorGUILayout.
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