using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class TarjanAlgorithm {

	// http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
	public static IList<IList<Node>> Cycles(this Graph g){
		var t = new TarjanInstance (g);
		foreach(Node n in g.nodes) {
			if(!t.indexes.ContainsKey(n)){
				t.strongConnect(n);
			}
		}
		return t.Cycles;
	}

	private class TarjanInstance {
		public Graph g;
		public int index = 1;
		public IDictionary<Node,int> indexes;
		public IDictionary<Node,int> lowlink;
		public List<Node> Stack = new List<Node>();
		public IList<IList<Node>> Cycles = new List<IList<Node>> ();

		public TarjanInstance(Graph g){
			this.g = g;
			indexes = new Dictionary<Node, int>();
			lowlink = new Dictionary<Node, int>();
		}
	}

	private static void strongConnect (this TarjanInstance s, Node n)
	{
		s.indexes [n] = s.index;
		s.lowlink [n] = s.index;
		s.index++;
		s.Stack.Add (n);

		foreach (Edge e in s.g.edges.Where(e => e.From == n)) {
			if(!s.indexes.ContainsKey(e.To)){
				s.strongConnect(e.To);
				s.lowlink[n] = Math.Min(s.lowlink[n], s.lowlink[e.To]);
			} else if(s.Stack.Contains(e.To)){
				s.lowlink[n] = Math.Min(s.lowlink[n], s.indexes[e.To]);
			}
		}

		if (s.lowlink [n] == s.indexes [n]) {
			var cycle = s.Stack.SkipWhile(v => v != n).ToList();
			s.Stack.RemoveRange(s.Stack.Count - cycle.Count, cycle.Count);
			s.Cycles.Add(cycle);
		}
	}
}
