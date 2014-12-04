using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Node))]
public class NodeInspector : Editor {

	private const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	public override void OnInspectorGUI () {
		Node node = target as Node;
		Graph graph = node.graph;
		var existingEdges 	   = graph.edges.Where (e => e != null && e.From == node);
		var allreadyConnected  = existingEdges.Select (e => e.To);
		var possibleConnection = graph.nodes.Where (n => n != node).Where (n => !allreadyConnected.Contains (n));

		foreach (Node n in possibleConnection) {
			if (GUILayout.Button ("Connect to node "+n.gameObject.name)) {
				Undo.RecordObject (graph, "Connect nodes");
				Edge e = graph.AddEdge(node, n);
				EditorUtility.SetDirty (e);
				EditorUtility.SetDirty (graph);
			}
		}
	}
	
	public void OnSceneGUI () {
		NodeInspector.OnSceneGUI (target as Node);
	}

	public static void OnSceneGUI (Node node) {
		Transform handleTransform = node.graph.gameObject.transform;
		Vector3 point = handleTransform.TransformPoint(node.position);
		Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		EditorGUI.BeginChangeCheck();
		var p0 = Handles.DoPositionHandle(handleTransform.TransformPoint(node.position), handleRotation);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(node, "Move Node");
			EditorUtility.SetDirty(node);
			node.position = handleTransform.InverseTransformPoint(p0);
		}
		
		Handles.color = Color.green;
		float size = HandleUtility.GetHandleSize(node.position) * 2f;
		if (Handles.Button(handleTransform.TransformPoint(node.position), handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			// Show buttons to connect to other nodes
			// Select node?
		}
	}

}
