using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Graph : MonoBehaviour {

	public List<Edge> edges;
	public List<Node2> nodes;
	
	public void Reset () {
		edges = new List<Edge> ();
		nodes = new List<Node2> ();
	}

	public void CleanUp() {
		var removed = nodes.Where (n => n == null).Select((n, i) => i).ToArray();
		foreach(int r in removed)
			nodes.RemoveAt(r);

		removed = edges.Where (e => e == null).Select((e, i) => i).ToArray();
		foreach(int r in removed)
			edges.RemoveAt(r);
	}

	public void AddNode ()
	{
		GameObject go = new GameObject ();
		go.transform.parent = this.gameObject.transform;
		Node2 node = go.AddComponent<Node2> ();
		nodes.Add (node);
		go.name = "Node "+nodes.Count;
		node.SetGraph (this);
	}

	public void AddEdge (Node2 from, Node2 to)
	{
		GameObject go = new GameObject ();
		go.transform.parent = this.gameObject.transform;
		Edge edge = go.AddComponent<Edge> ();
		edges.Add (edge);
		go.name = "Edge "+edges.Count;
		edge.From = from;
		edge.To = to;
		// Set positions here
		edge.SetGraph (this);
	}
}