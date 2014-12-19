using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Edge))]
public class EdgeInspector : BezierSplineInspector {
	private const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	public override void OnSceneGUI () {
		Edge e = target as Edge;
		BezierSplineInspector.OnSceneGUI(e, this);
		e.From = e.From;
		e.To = e.To;
	}
	
	public override void OnInspectorGUI () {
		base.OnInspectorGUI ();
		Edge e = target as Edge;
		EditorGUILayout.HelpBox ("You selected a single Edge gameObject. By selecting the complete Graph, you will get better overview and more controls", MessageType.Warning);
		OnInspectorGUI (this, e);
	}

	public static void OnInspectorGUI(Editor editor, Edge edge){
		// Editor
		edge.From = EditorGUILayout.ObjectField("From", edge.From, typeof(Node), true) as Node ?? edge.From;
		edge.To = EditorGUILayout.ObjectField("To", edge.To, typeof(Node), true) as Node ?? edge.To;

		GUILayout.BeginHorizontal ();

		if (GUILayout.Button ("Reverse Edge")) {
			Undo.RecordObject (edge, "Reverse Edge");
			edge.Reverse();
			EditorUtility.SetDirty (edge);
		}

		if (GUILayout.Button ("Split Edge")) {
			Undo.RecordObject (edge, "Split Edge");
			var between = edge.graph.AddNode();
			between.position = Vector3.Slerp(edge.From.position, edge.To.position, 0.5f);
			edge.graph.AddEdge(edge.From, between);
			edge.graph.AddEdge(between, edge.To);
			edge.graph.RemoveEdge(edge);
		}

		var bg = GUI.backgroundColor;
		var fg = GUI.contentColor;
		GUI.backgroundColor = Color.red;
		GUI.contentColor = Color.white;
		if(GUILayout.Button ("Remove Edge")){
			var g = edge.graph;
			edge.graph.RemoveEdge(edge);
			Undo.DestroyObjectImmediate (edge.gameObject);
			EditorUtility.SetDirty(g);
			editor.Repaint();
		}
		GUI.backgroundColor = bg;
		GUI.contentColor = fg;

		GUILayout.EndHorizontal ();
	}

	/**
	 * Draw Edge GUI as part of the Graph GUI
	 * 
	 * Do not draw start & end points as those are Nodes, 
	 * and are therefore adjusted by the Node GUI
	 * 
	 * Returns non-null index value if Control Point of this edge was clicked
	 */
	public static int? OnGraphSceneGUI(Edge edge, Color edgeColor, int? selectedIndex = null){
		var handleTransform = edge.transform;
		var handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
		int? newSelection = selectedIndex;

		// Draw handles for all in-between points
		Vector3 p0 = handleTransform.TransformPoint(edge.GetControlPoint(0));
		for (int i = 1; i < edge.ControlPointCount; i += 3) {
			Vector3 p1 = ShowPoint(edge, i, handleTransform, handleRotation, newSelection, out newSelection);
			Vector3 p2 = ShowPoint(edge, i + 1, handleTransform, handleRotation, newSelection, out newSelection);
			Vector3 p3 = handleTransform.TransformPoint(edge.GetControlPoint(i+2));
			
			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);
			
			Handles.DrawBezier(p0, p3, p1, p2, edgeColor, null, 2f);
			p0 = p3;

			var d = HandleUtility.DistancePointBezier(Input.mousePosition, p0, p3, p1, p2);
			if(Input.GetMouseButtonDown(0) && d < 10f){
				Debug.Log ("D " + d);
			}
		}

		// Arrow for direction
		Handles.color = selectedIndex.HasValue ? Color.yellow : Color.blue;
		var halfWay = .5f;
		var halfWP = edge.GetPoint(halfWay);
		var size = HandleUtility.GetHandleSize (halfWP) * 0.2f;
		if (Handles.Button (halfWP, Quaternion.LookRotation (edge.GetDirection (halfWay)), size, size, Handles.ConeCap)) {
			Debug.Log ("Select edge");
			return -1;
		}

		//			Undo.RecordObject(edge, "Switch Edge Direction");
		//			//edge.Reverse();
		//			EditorUtility.SetDirty(edge);
		return newSelection;
	}

	/**
	 * Draw button and optionally a Handle for a Bezier control point 
	 */
	private static Vector3 ShowPoint(Edge edge, int index, Transform handleTransform, Quaternion handleRotation, int? selectedIndex, out int? newSelectedIndex){
		Vector3 point = handleTransform.TransformPoint(edge.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0) {
			size *= 2f;
		}

		Handles.color = Color.white;

		// Allow to change selection
		if (Handles.Button (point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			newSelectedIndex = index;
		} else if (selectedIndex.HasValue && selectedIndex.Value != index) {
			newSelectedIndex = selectedIndex;		
		} else {
			newSelectedIndex = null;		
		}

		// Draw Handle if selected
		if (selectedIndex.HasValue && index == selectedIndex.Value) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(edge, "Move Point");
				EditorUtility.SetDirty(edge);
				edge.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}
}

