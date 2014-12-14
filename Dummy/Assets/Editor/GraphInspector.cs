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
	private bool subFieldFold = true;

	private bool nodeConnectionMode = false;

	public override void OnInspectorGUI () {
		graph = target as Graph;

		graph.CleanUp ();

		GUILayout.Label("Graph:");
		GUILayout.TextField ("Node Count: " + graph.nodes.Count());
		GUILayout.TextField ("Edge Count: " + graph.edges.Count());

		if (GUILayout.Button ("Add Node")) {
			Node n = graph.AddNode ();
			Undo.RegisterCreatedObjectUndo(n.gameObject, "Add Node");
			EditorUtility.SetDirty (graph);
		}

		// Add selected Node buttons
		if (selectionIsNode && selectedNode != null) {
			subFieldFold = EditorGUILayout.InspectorTitlebar(subFieldFold, selectedNode);
			GUILayout.Label("Selected Node:");
			if(GUILayout.Button ("Connect to...")){
				nodeConnectionMode = true;
				this.Repaint();
			}
			NodeInspector.OnInspectorGUI(this, selectedNode);
		} else
		// Add selected Edge buttons
		if (!selectionIsNode && selectedEdge != null) {
			subFieldFold = EditorGUILayout.InspectorTitlebar(subFieldFold, selectedEdge);
			GUILayout.Label("Selected Edge:");
			EdgeInspector.OnInspectorGUI(this, selectedEdge);
		}
	}

	public void OnSceneGUI () {
		graph = target as Graph;

		// Reload edges and nodes after Undo operation
		if (Event.current.type == EventType.ValidateCommand) {
			switch (Event.current.commandName) {
			case "UndoRedoPerformed":
				graph.Reset();
				break;
			}
		}
				
		// Draw Node GUI and detect clicks
		var clickedNode = graph.nodes.FirstOrDefault (n => NodeInspector.OnGraphSceneGUI (n, selectionIsNode && n == this.selectedNode));
		if (clickedNode != null) {
			if(nodeConnectionMode){
				nodeConnectionMode = false;
				Undo.RegisterCreatedObjectUndo(graph.AddEdge(selectedNode, clickedNode).gameObject, "Add Edge");
			}
			selectionIsNode = true;
			selectedNode = clickedNode;
			this.Repaint();
		}

		// Don't show edges in Node Connection Mode
		if (nodeConnectionMode)
			return;

		// Draw Edge GUI and detect clicks
		var clickedEdgeIndex = graph.edges.Select (e => new { edge = e, index = EdgeInspector.OnGraphSceneGUI (e, selectionIsNode || selectedEdge != e ? null : selectedIndex) }).ToList ().FirstOrDefault (t => t.index.HasValue);
		if (clickedEdgeIndex != null) {
			selectionIsNode = false;
			selectedEdge = clickedEdgeIndex.edge;
			selectedIndex = clickedEdgeIndex.index;
			this.Repaint ();
		}
	}
}
