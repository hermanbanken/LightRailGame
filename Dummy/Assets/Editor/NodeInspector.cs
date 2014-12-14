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
		OnInspectorGUI (this, node);
	}

	public static void OnInspectorGUI(Editor editor, Node node){
		Graph graph = node.graph;
		var existingEdges 	   = graph.edges.Where (e => e != null && e.From == node);
		var allreadyConnected  = existingEdges.Select (e => e.To);
		var possibleConnection = graph.nodes.Where (n => n != node).Where (n => !allreadyConnected.Contains (n));

		if(editor as NodeInspector != null)
		foreach (Node n in possibleConnection) {
			if (GUILayout.Button ("Connect to node "+n.gameObject.name)) {
				Edge e = graph.AddEdge(node, n);
				Undo.RegisterCreatedObjectUndo(e.gameObject, "Add Edge");
				EditorUtility.SetDirty (graph);
			}
		}

		if(GUILayout.Button ("Remove Node")){
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
	}
	
	/**
	 * Draw Node GUI as part of the Graph GUI
	 * 
	 * Returns true if this node was clicked
	 */
	public static bool OnGraphSceneGUI(Node node, bool selected = true){
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
				Undo.RecordObjects (new Object[] { node }.Concat (affectedEdges.Cast<Object> ()).ToArray (), "Move Node");

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
		
		Handles.color = Color.green;
		float size = HandleUtility.GetHandleSize(node.position) * 2f;
		return Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap);
	}
	
	public void OnSceneGUI () {
		Tools.current = Tool.None;
		NodeInspector.OnGraphSceneGUI (target as Node);
	}

}
