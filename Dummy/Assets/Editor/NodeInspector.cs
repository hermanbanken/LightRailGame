using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Node2))]
public class NodeInspector : Editor {

	private const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	public override void OnInspectorGUI () {
		Node2 node = target as Node2;
		Graph graph = node.GetGraph ();
		var existingEdges 	   = graph.edges.Where (e => e != null && e.From == node);
		var allreadyConnected  = existingEdges.Select (e => e.To);
		var possibleConnection = graph.nodes.Where (n => n != node).Where (n => !allreadyConnected.Contains (n));

		foreach (Node2 n in possibleConnection) {
			if (GUILayout.Button ("Connect to node "+n.gameObject.name)) {
				Undo.RecordObject (graph, "Connect nodes");
				graph.AddEdge(node, n);
				EditorUtility.SetDirty (graph);
			}
		}
	}
	
	public void OnSceneGUI () {
		NodeInspector.OnSceneGUI (target as Node2);
	}

	public static void OnSceneGUI (Node2 node) {
		Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? node.gameObject.transform.rotation : Quaternion.identity;

		EditorGUI.BeginChangeCheck();
		var p0 = Handles.DoPositionHandle(node.position, handleRotation);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(node, "Move Node");
			EditorUtility.SetDirty(node);
			node.position = p0;
		}
		
		Handles.color = Color.green;
		float size = HandleUtility.GetHandleSize(node.position) * 2f;
		if (Handles.Button(node.position, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			// Show buttons to connect to other nodes
			// Select node?
		}
	}

}
