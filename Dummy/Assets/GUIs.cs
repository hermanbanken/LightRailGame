using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class Extensions {
	public static string ToStr(this IEnumerable<Node> self){
		return "Path = " + self.Select(n => n.ToString()).Aggregate("", (s,n) => s+"|"+n) + ";";
	}
	public static string ToStr(this IEnumerable<Edge> self){
		return "Path = " + self.Select(e => e.From.ToString()+"->"+e.To.ToString()).Aggregate("", (s,n) => s+"|"+n) + ";";
	}

	public static IEnumerable<TResult> EachPair<TSource, TResult>(this IEnumerable<TSource> source,
	                                                        Func<TSource, TSource, TResult> transformation)
	{
		if (source == null) throw new ArgumentNullException("source");
		if (transformation == null) throw new ArgumentNullException("transformation");
		return EachPairImpl(source, transformation);
	}

private static IEnumerable<TResult> EachPairImpl<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TSource, TResult> f)
	{
		using (var i = source.GetEnumerator())
		{
			TSource prev = default(TSource);
			bool first = true;

			while(i.MoveNext()){
				if(first){
					first = false;
				} else {
					yield return f(prev, i.Current);
				}
				prev = i.Current;
			}
		}
	}
}

