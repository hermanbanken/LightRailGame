using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System;

[CustomEditor(typeof(Node))]
public class NodeInspector : Editor {

	private const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;
	protected static int nodePickerIndex = 0;

	public override void OnInspectorGUI () {
		Node node = target as Node;
		EditorGUILayout.HelpBox ("You selected a single Node gameObject. By selecting the complete Graph, you will get better overview and more controls", MessageType.Warning);
		OnInspectorGUI (this, node);
	}

	public static void OnInspectorGUI(Editor editor, Node node, Action onConnectTo = null){
		Graph graph = node.graph;
		var existingEdges 	   = graph.edges.Where (e => e != null && e.From == node);
		var allreadyConnected  = existingEdges.Select (e => e.To);
		var possibleConnection = graph.nodes.Where (n => n != node).Where (n => !allreadyConnected.Contains (n));

		// Station toggle
		var isStation = node.GetComponent<Station>() != null;
		if(isStation != GUILayout.Toggle(isStation, "Is Station?")){
			if(isStation){
				Undo.DestroyObjectImmediate(node.GetComponent<Station>());
			} else {
				Undo.AddComponent<Station>(node.gameObject);
			}
			editor.Repaint();
		}
		
		// Traffic Light toggle
		var isTraffic = node.GetComponent<TrafficLight>() != null;
		if(isTraffic != GUILayout.Toggle(isTraffic, "Is Traffic Light?")){
			if(isTraffic){
				Undo.DestroyObjectImmediate(node.GetComponent<TrafficLight>());
			} else {
				Undo.AddComponent<TrafficLight>(node.gameObject);
			}
			editor.Repaint();
		}
		
		EditorGUILayout.Space ();
		GUILayout.Label ("Create new edge by connecting to another node:");
		// Show buttons for each other node to connect to
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Connect to:")) {
			Edge e = graph.AddEdge (node, possibleConnection.ElementAt(nodePickerIndex));
			Undo.RegisterCreatedObjectUndo (e.gameObject, "Add Edge");
			EditorUtility.SetDirty (graph);
		}
		nodePickerIndex = EditorGUILayout.Popup(Math.Min(nodePickerIndex, possibleConnection.Count()-1), possibleConnection.Select(n => n.gameObject.name).ToArray());

		GUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		if(GUILayout.Button ("Connect to: click & then select node in scene") && onConnectTo != null){
			onConnectTo();
		}
		GUILayout.EndHorizontal ();

		EditorGUILayout.Space ();

		var bg = GUI.backgroundColor;
		var fg = GUI.contentColor;
		GUI.backgroundColor = Color.red;
		GUI.contentColor = Color.white;

		// Remove button, removes node and attached edges
		if(GUILayout.Button ("Remove Node and attached Edges")){
			Undo.IncrementCurrentGroup();
			var remove = graph.edges.Where(e => e.To == node || e.From == node).ToArray();
			foreach(Edge e in remove){
				Undo.DestroyObjectImmediate(e.gameObject);
			}
			Undo.DestroyObjectImmediate (node.gameObject);
			graph.RemoveNode(node);
			Undo.RecordObject(graph, "Remove Node");
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
			editor.Repaint();
		}

		GUI.backgroundColor = bg;
		GUI.contentColor = fg;

	}
	
	/**
	 * Draw Node GUI as part of the Graph GUI
	 * 
	 * Returns true if this node was clicked
	 */
	public static bool OnGraphSceneGUI(Node node, Color nodeColor, bool selected = true){
		Transform handleTransform = node.graph.gameObject.transform;
		Vector3 point = handleTransform.TransformPoint(node.position);
		Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		if (selected) {

			EditorGUI.BeginChangeCheck ();
			var p0 = Handles.DoPositionHandle (point, handleRotation);
			if (EditorGUI.EndChangeCheck ()) 
			{
				// Gather edges
				var affectedEdges = node.graph.edges.Where (e => e.From == node || e.To == node).ToList ();

				// Save state
				Undo.RecordObjects (new UnityEngine.Object[] { node }.Concat (affectedEdges.Cast<UnityEngine.Object> ()).ToArray (), "Move Node");

				// Update state
				node.position = handleTransform.InverseTransformPoint (p0);
				EditorUtility.SetDirty (node);
				affectedEdges.ForEach (e => {
					if(e.To == node)
						e.To = node;
					if(e.From == node)
						e.From = node;
					EditorUtility.SetDirty (e);
				});

				point = p0;
			}

		}
		
		Handles.color = nodeColor;
		float size = HandleUtility.GetHandleSize(node.position) * 2f;
		return Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap);
	}
	
	public void OnSceneGUI () {
		Tools.current = Tool.None;
		NodeInspector.OnGraphSceneGUI (target as Node, Color.green);
	}

}
