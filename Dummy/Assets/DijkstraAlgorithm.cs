using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Dijkstra<E,N> where E : IEdge<N> where N : class {
	private IList<E> edges;

	public Dijkstra(IList<E> edges){
		this.edges = edges;
	}

	public IEnumerable<N> Plan(N from, N to){
		EqualityComparer<N> c = EqualityComparer<N>.Default;

		// Initialize
		var dist = new Dictionary<N,float> (c);
		var prev = new Dictionary<N,N> (c);
		var nodes = edges.SelectMany (e => new N[] { e.From, e.To }).Distinct ();
		var remaining = new List<N> (nodes);

		foreach (N n in nodes) {
			dist[n] = from.Equals(n) ? 0 : float.MaxValue;
		}

		// Run
		while (remaining.Count() > 0) {
			N u = remaining.MinBy(n => dist[n]);
			remaining.Remove(u);

			foreach(E e in edges.Where (e => e.From.Equals(u)).Where (e => remaining.Contains(e.To, c))){
				var v = e.To;
				var alt = dist[u] + e.Cost;
				if(alt < dist[v]){
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		var S = new List<N>();
		var t = to;
		while (prev.ContainsKey(t)) {
			S.Add (t);
			t = prev[t];
		}

		S.Reverse ();
		return S;
	}

	public IEnumerable<E> PlanRoute(N from, N to){
		var points = Plan (from, to).GetEnumerator();
		points.Reset ();
		var last = from;
		while (points.MoveNext()) {
			var edge = edges.FirstOrDefault(e => e.From.Equals(last) && e.To.Equals(points.Current));
			if(edge == null)
				break;
			else
				yield return edge;
			last = points.Current;
		}
	}
}

public static class LinqExt {
	public static TResult MinBy<TResult,T>(this IEnumerable<TResult> self, Func<TResult,T> selector) where TResult : class where T : IComparable {
		T min = default(T);
		TResult least = null;
		foreach (TResult t in self) {
			if(least == null || selector(t).CompareTo(min) < 0){
				least = t;
				min = selector(t);
			}
		}
		return least;
	}
}