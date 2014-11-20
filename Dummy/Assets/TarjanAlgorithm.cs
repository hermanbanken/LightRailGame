using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class TarjanAlgorithm {

	// http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
	public static IList<IList<Node2>> Cycles(this Graph g){
		var t = new TarjanInstance (g);
		foreach(Node2 n in g.nodes) {
			if(!t.indexes.ContainsKey(n)){
				t.strongConnect(n);
			}
		}
		return t.Cycles;
	}

	private class TarjanInstance {
		public Graph g;
		public int index = 1;
		public IDictionary<Node2,int> indexes;
		public IDictionary<Node2,int> lowlink;
		public List<Node2> Stack = new List<Node2>();
		public IList<IList<Node2>> Cycles = new List<IList<Node2>> ();

		public TarjanInstance(Graph g){
			this.g = g;
			indexes = new Dictionary<Node2, int>();
			lowlink = new Dictionary<Node2, int>();
		}
	}

	private static void strongConnect (this TarjanInstance s, Node2 n)
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
