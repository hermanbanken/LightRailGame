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
		if (graph == null)
			Debug.Log ("Graph null");
		if (graph.edges.First().From == null)
			Debug.Log ("Edges From null");
		var existingEdges 	   = graph.edges.Where (e => e.From == node);
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

	private void OnSceneGUI () {
		Node2 node = target as Node2;
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
			Repaint();
		}

//		Transform handleTransform = node.transform;
//		Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
//
//		Vector3 p0 = handleTransform.TransformPoint(node.position);
//		
//		Handles.color = Color.green;
//		float size = HandleUtility.GetHandleSize(handleTransform.position) * 2f;
//
//		if (Handles.Button(handleTransform.position, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
//			// Show buttons to connect to other nodes
//			Repaint();
//		}
//
//		EditorGUI.BeginChangeCheck();
//		p0 = Handles.DoPositionHandle(p0, handleRotation);
//		if (EditorGUI.EndChangeCheck()) {
//			Undo.RecordObject(node, "Move Node");
//			EditorUtility.SetDirty(node);
//			node.position = handleTransform.InverseTransformPoint(p0);
//		}
	}

}
