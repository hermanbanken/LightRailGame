using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Graph : MonoBehaviour {

	private List<Edge> _edges;
	private List<Node2> _nodes;
	public Dijkstra<Edge,Node2> Dijkstra;

	[SerializeField]
	public Transform Decoration;

	public IEnumerable<Edge> edges {
		get {
			if(_edges == null || _edges.Any (e => e == null) || _edges.Count == 0){
				_edges = new List<Edge>(this.gameObject.GetComponentsInChildren<Edge>());
				Dijkstra = new Dijkstra<Edge, Node2>(_edges);
			}
			return _edges;
		}
	}

	public IEnumerable<Node2> nodes {
		get {
			if(_nodes == null || _nodes.Any (n => n == null) || _nodes.Count == 0){
				_nodes = new List<Node2>(this.gameObject.GetComponentsInChildren<Node2>());
			}
			return _nodes;
		}
	}

	public void Reset () {
		_edges = null;
		_nodes = null;
		Dijkstra = new Dijkstra<Edge, Node2>(_edges);
	}

	public void CleanUp() {
		var removed = nodes.Where (n => n == null).Select((n, i) => i).ToArray();
		foreach(int r in removed)
			_nodes.RemoveAt(r);

		removed = edges.Where (e => e == null).Select((e, i) => i).ToArray();
		foreach(int r in removed)
			_edges.RemoveAt(r);
	}

	public Node2 AddNode ()
	{
		GameObject go = new GameObject ();
		go.transform.parent = this.gameObject.transform;
		Node2 node = go.AddComponent<Node2> ();
		_nodes.Add (node);
		go.name = "Node "+nodes.Count();
		node.graph = this;
		return node;
	}

	public Edge AddEdge (Node2 from, Node2 to)
	{
		GameObject go = new GameObject ();
		go.transform.parent = this.gameObject.transform;
		Edge edge = go.AddComponent<Edge> ();
		_edges.Add (edge);
		go.name = "Edge "+edges.Count();
		// Set positions here
		edge.From = from;
		edge.To = to;
		edge.Straighten ();
		edge.graph = this;
		Dijkstra = new Dijkstra<Edge, Node2>(_edges);
		return edge;
	}

}