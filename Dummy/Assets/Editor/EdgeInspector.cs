using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Edge))]
public class EdgeInspector : BezierSplineInspector {

	public override void OnSceneGUI () {
		Edge e = target as Edge;
		e.From = e.From;
		e.To = e.To;
		BezierSplineInspector.OnSceneGUI(e, this);
	}

	public static void OnSceneGUI(Edge e){
		e.From = e.From;
		e.To = e.To;
		BezierSplineInspector.OnSceneGUI(e);
	}

	
	public override void OnInspectorGUI () {
		base.OnInspectorGUI ();
		Edge e = target as Edge;
		
		//		Editor
		e.From = EditorGUILayout.ObjectField(e.From, typeof(Node), true) as Node;
		e.To = EditorGUILayout.ObjectField(e.To, typeof(Node), true) as Node;
	}

}

