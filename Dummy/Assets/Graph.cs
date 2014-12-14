using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Graph : MonoBehaviour {

	private List<Edge> _edges;
	private List<Node> _nodes;
	public Dijkstra<Edge,Node> Dijkstra { get { return new Dijkstra<Edge, Node> (_edges); } }
	
	public IEnumerable<Edge> edges {
		get {
			if(_edges == null || _edges.Any (e => e == null) || _edges.Count == 0){
				_edges = new List<Edge>(this.gameObject.GetComponentsInChildren<Edge>());
			}
			return _edges;
		}
	}

	public IEnumerable<Node> nodes {
		get {
			if(_nodes == null || _nodes.Any (n => n == null) || _nodes.Count == 0){
				_nodes = new List<Node>(this.gameObject.GetComponentsInChildren<Node>());
			}
			return _nodes;
		}
	}

	public void Reset () {
		_edges = null;
		_nodes = null;
	}

	public void CleanUp() {
		var removed = nodes.Where (n => n == null).Select((n, i) => i).ToArray();
		foreach(int r in removed)
			_nodes.RemoveAt(r);

		removed = edges.Where (e => e == null).Select((e, i) => i).ToArray();
		foreach(int r in removed)
			_edges.RemoveAt(r);
	}

	public Node AddNode ()
	{
		GameObject go = new GameObject ();
		go.transform.parent = this.gameObject.transform;
		Node node = go.AddComponent<Node> ();
		_nodes.Add (node);
		go.name = "Node "+nodes.Count();
		node.graph = this;
		return node;
	}

	public void RemoveNode (Node node)
	{
		var removed = _edges.Where(e => e.To == node || e.From == node).ToArray();
		foreach (Edge e in removed) {
			_edges.Remove(e);
		}
		_nodes.Remove (node);
	}

	public Edge AddEdge (Node from, Node to)
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
		return edge;
	}

	public void RemoveEdge (Edge edge)
	{
		_edges.Remove (edge);
	}
}