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
		// Initialize
		var dist = new Dictionary<N,float> ();
		var prev = new Dictionary<N,N> ();
		var queue = new PriorityQueue<N> (new NodeComparer<N>(dist));

		var nodes = edges.SelectMany (e => new N[] { e.From, e.To }).Distinct ();
		foreach (N n in nodes) {
			dist[n] = from == n ? 0 : float.MaxValue;
			queue.Enqueue(n);
		}

		// Run
		while (queue.Count() > 0) {
			N u = queue.Dequeue();

			foreach(E e in edges.Where (e => e.From == u).Where (e => queue.Contains(e.To))){
				var v = e.To;
				var alt = dist[u] + e.Cost;
				Debug.Log ("Checking if "+alt+" is closer than "+dist[v]);
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

		return S;
	}
}

public class NodeComparer<N> : Comparer<N> where N : class {
	private IDictionary<N,float> distances;

	public NodeComparer(IDictionary<N,float> distances){
		this.distances = distances;
	}

	public override int Compare (N x, N y)
	{
		return (int) (distances [y] - distances [x]);
	}
}

public class PriorityQueue<T> : IEnumerable<T>
{
	private List<T> data;
	private IComparer<T> comparator;
	
	public PriorityQueue(IComparer<T> comparator = null)
	{
		this.comparator = comparator ?? Comparer<T>.Default;
		this.data = new List<T>();
	}
	
	public void Enqueue(T item)
	{
		data.Add(item);
		int ci = data.Count - 1; // child index; start at end
		while (ci > 0)
		{
			int pi = (ci - 1) / 2; // parent index
			if (comparator.Compare(data[ci], data[pi]) >= 0) break; // child item is larger than (or equal) parent so we're done
			T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
			ci = pi;
		}
	}

	public T Dequeue()
	{
		// assumes pq is not empty; up to calling code
		int li = data.Count - 1; // last index (before removal)
		T frontItem = data[0];   // fetch the front
		data[0] = data[li];
		data.RemoveAt(li);
		
		--li; // last index (after removal)
		int pi = 0; // parent index. start at front of pq
		while (true)
		{
			int ci = pi * 2 + 1; // left child index of parent
			if (ci > li) break;  // no children so done
			int rc = ci + 1;     // right child
			if (rc <= li && comparator.Compare(data[rc], data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
				ci = rc;
			if (comparator.Compare(data[pi], data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
			T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
			pi = ci;
		}
		return frontItem;
	}
	
	public T Peek()
	{
		T frontItem = data[0];
		return frontItem;
	}
	
	public int Count()
	{
		return data.Count;
	}

	#region IEnumerable implementation

	public IEnumerator<T> GetEnumerator ()
	{
		return this.data.GetEnumerator ();
	}

	#endregion

	#region IEnumerable implementation

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return this.data.GetEnumerator ();
	}

	#endregion
	
	public override string ToString()
	{
		string s = "";
		for (int i = 0; i < data.Count; ++i)
			s += data[i].ToString() + " ";
		s += "count = " + data.Count;
		return s;
	}
	
	public bool IsConsistent()
	{
		// is the heap property true for all data?
		if (data.Count == 0) return true;
		int li = data.Count - 1; // last index
		for (int pi = 0; pi < data.Count; ++pi) // each parent index
		{
			int lci = 2 * pi + 1; // left child index
			int rci = 2 * pi + 2; // right child index
			
			if (lci <= li && comparator.Compare(data[pi], data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
			if (rci <= li && comparator.Compare(data[pi], data[rci]) > 0) return false; // check the right child too.
		}
		return true; // passed all checks
	} // IsConsistent
} // PriorityQueue