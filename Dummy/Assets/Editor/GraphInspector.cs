using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Graph))]
public class GraphInspector : Editor {
	
	private Graph graph;
	
	public override void OnInspectorGUI () {
		graph = target as Graph;

		graph.CleanUp ();

		if (graph.Decoration = EditorGUILayout.ObjectField ("Path Decoration", graph.Decoration, typeof(Transform), true) as Transform) {
			Undo.RecordObject (graph, "Set Decoration");
			EditorUtility.SetDirty (graph);
		}

		GUILayout.TextField ("Node Count: " + graph.nodes.Count());
		GUILayout.TextField ("Edge Count: " + graph.edges.Count());

		if (GUILayout.Button ("Add Node")) {
			Undo.RecordObject (graph, "Add Node");
			Node n = graph.AddNode ();
			EditorUtility.SetDirty (graph);
			EditorUtility.SetDirty (n);
		}
	}

	public void OnSceneGUI () {
		graph = target as Graph;
		graph.CleanUp ();
		foreach (var n in graph.nodes) NodeInspector.OnSceneGUI (n);
		foreach (var e in graph.edges) EdgeInspector.OnSceneGUI (e);
	}
}
