using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Graph))]
public class GraphInspector : Editor {
	
	private Graph graph;
	private Edge selectedEdge;
	private int? selectedIndex;
	private Node selectedNode;
	private bool selectionIsNode = false;
	
	public override void OnInspectorGUI () {
		graph = target as Graph;

		graph.CleanUp ();

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

		// Draw Node GUI and detect clicks
		var clickedNode = graph.nodes.FirstOrDefault (n => NodeInspector.OnGraphSceneGUI (n, selectionIsNode && n == this.selectedNode));
		if (clickedNode != null) {
			selectionIsNode = true;
			selectedNode = clickedNode;
		}
		
		// Draw Edge GUI and detect clicks
		var clickedEdgeIndex = graph.edges.Select (e => new { edge = e, index = EdgeInspector.OnGraphSceneGUI (e, selectionIsNode || selectedEdge != e ? null : selectedIndex) }).ToList().FirstOrDefault (t => t.index.HasValue);
		if (clickedEdgeIndex != null) {
			selectionIsNode = false;
			selectedEdge = clickedEdgeIndex.edge;
			selectedIndex = clickedEdgeIndex.index;
		}
	}
}
