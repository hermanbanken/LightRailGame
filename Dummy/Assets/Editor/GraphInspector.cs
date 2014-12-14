using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(Graph))]
public class GraphInspector : Editor {

	private Graph graph;
	private Edge selectedEdge;
	private int? selectedIndex;
	private Node selectedNode;
	private bool selectionIsNode = false;
	private bool subFieldFold = true;

	private LineSchedule selectedLineSchedule;
	private IList<Edge> selectedLineScheduleEdges;

	private bool nodeConnectionMode = false;
	
	void OnEnable(){
		graph = target as Graph;
		graph.CleanUp ();
		OnSelectLine += OnTramLineSelection;
		OnSelectLine(LinesWindow.SelectedLine ());
	}
	void OnDisable(){
		OnSelectLine -= OnTramLineSelection;
	}

	public override void OnInspectorGUI () {
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label("Elements");
		EditorGUI.BeginDisabledGroup (true);
		EditorGUILayout.BeginVertical ();
		EditorGUILayout.IntField ("Nodes", graph.nodes.Count());
		EditorGUILayout.IntField ("Edges", graph.edges.Count());
		EditorGUILayout.EndVertical ();
		EditorGUI.EndDisabledGroup();

		if (GUILayout.Button ("+")) {
			Node n = graph.AddNode ();
			Undo.RegisterCreatedObjectUndo(n.gameObject, "Add Node");
			EditorUtility.SetDirty (graph);
		}

		EditorGUILayout.EndHorizontal ();

		var indent = EditorGUI.indentLevel;
		//EditorGUI.indentLevel--;

		// Add selected Node buttons
		if (selectionIsNode && selectedNode != null && (subFieldFold = EditorGUILayout.InspectorTitlebar (subFieldFold, selectedNode))) {
			GUILayout.Label ("Selected Node:");
			NodeInspector.OnInspectorGUI (this, selectedNode, () => {
				nodeConnectionMode = true;
				this.Repaint ();
			});
		} else
		// Add selected Edge buttons
		if (!selectionIsNode && selectedEdge != null && (subFieldFold = EditorGUILayout.InspectorTitlebar (subFieldFold, selectedEdge))) {
			GUILayout.Label ("Selected Edge:");
			EdgeInspector.OnInspectorGUI (this, selectedEdge);
		} else if(subFieldFold) {
			EditorGUILayout.HelpBox ("Click a Node or Edge in the Scene to manage properties, while still seeing the other Nodes and Edges in the Graph.", MessageType.Info);
		}

		EditorGUI.indentLevel = indent;
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
		var clickedNode = graph.nodes.FirstOrDefault (n => NodeInspector.OnGraphSceneGUI (n, nodeColor (n), selectionIsNode && n == this.selectedNode));
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
		var clickedEdgeIndex = graph.edges.Select (e => new { 
				edge = e, 
				index = EdgeInspector.OnGraphSceneGUI (e, edgeColor (e), selectionIsNode || selectedEdge != e ? null : selectedIndex) 
			}).ToList ()
			.FirstOrDefault (t => t.index.HasValue);
		if (clickedEdgeIndex != null) {
			selectionIsNode = false;
			selectedEdge = clickedEdgeIndex.edge;
			selectedIndex = clickedEdgeIndex.index;
			this.Repaint ();
		}
	}
	
	public static event Action<LineSchedule> OnSelectLine;
	public static void SelectedLine(LineSchedule line){
		var evt = OnSelectLine;
		if (evt != null) evt (line);
	}

	private void OnTramLineSelection(LineSchedule line){
		selectedLineSchedule = line;
		selectedLineScheduleEdges = line == null ? null : line.RouteFromWayPoints(graph.edges.ToList());
		SceneView.RepaintAll();
	}

	private Color nodeColor(Node n){
		return selectedLineSchedule != null && selectedLineSchedule.WayPoints.Any (no => no.GetInstanceID() == n.GetInstanceID()) ? Color.red : Color.green;
	}
	private Color edgeColor(Edge e){
		return selectedLineScheduleEdges != null && selectedLineScheduleEdges.Any (ed => ed.GetInstanceID() == e.GetInstanceID()) ? Color.red : Color.white;
	}
}
