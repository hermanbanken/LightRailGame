using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Graph))]
public class GraphInspector : Editor {
	
	private Graph graph;
	
	public override void OnInspectorGUI () {
		graph = target as Graph;

		graph.CleanUp ();

		GUILayout.TextField ("Node Count: " + graph.nodes.Count);
		GUILayout.TextField ("Edge Count: " + graph.edges.Count);

		if (GUILayout.Button ("Add Node")) {
			Undo.RecordObject (graph, "Add Node");
			graph.AddNode ();
			EditorUtility.SetDirty (graph);
		}
	}

}
