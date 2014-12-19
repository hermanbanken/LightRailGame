using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LineDrawMaster {
	private static LineDrawMaster Instance;

	private IDictionary<ILine,LineRenderer> renderers = new Dictionary<ILine, LineRenderer>();
	private IList<LineRenderer> unused = new List<LineRenderer>();

	public LineDrawMaster(){
		unused = new List<LineRenderer> ();
	}

	public static LineDrawMaster getInstance(){
		return (Instance = new LineDrawMaster()); 
	}

	public LineRenderer ShowLine(ILine e, LineOptions options = null){
		if (renderers.ContainsKey (e)) {
			Dispose(renderers[e], e);
		}
		options = options ?? new LineOptions();
		var line = GetFreeLine ();
		renderers [e] = line;
		var length = e.GetUnitLength ();
		var c = (int)length * 5;
		line.SetVertexCount (c);
		for (int i = 0; i < c; i++) {
			line.SetPosition(i, e.GetUnitPosition(i / 5f) + options.offset);
		}
		line.materials = options.materials;
		line.SetColors (options.colors [0], options.colors [1]);
		line.SetWidth (options.widths [0], options.widths [1]);
		return line;
	}

	public void HideLine (ILine e)
	{
		if (renderers.ContainsKey (e)) {
			Dispose(renderers[e], e);
		}
	}

	public void RemoveAll() {
		foreach (var p in renderers.ToList())
			this.Dispose (p.Value, p.Key);
	}
	
	public void Tick(){

	}

	private LineRenderer GetFreeLine(){
		if (unused.Count > 0) {
			var line = unused [0];
			unused.RemoveAt (0);
			line.enabled = true;
			return line;
		} else {
			var go = new GameObject();
			var lr = go.AddComponent<LineRenderer>();
			lr.name = "LineDrawMaster Line";
			return lr;
		}
	}

	private void Dispose(LineRenderer r, ILine key = null){
		if (key == null) {
			key = renderers.FirstOrDefault (p => p.Value == r).Key;
		}
		if (renderers [key] == r) {
			renderers.Remove (key);
			unused.Add (r);	
		}	
		r.enabled = false;
	}

	public enum LineMode {
		
	}
}

public class LineOptions {
	public Vector3 offset = Vector3.zero;
	public Material[] materials = new Material[] {};
	public Color[] colors = new [] { Color.red, Color.blue };
	public float[] widths = new [] { 1f, 1f };
}

public class StraightLine : ILine {
	Vector3 from;
	Vector3 to;

	public StraightLine(Vector3 from, Vector3 to){
		this.to = to;
		this.from = from;
	}

	#region ILine implementation

	public float GetUnitLength ()
	{
		return (from - to).magnitude;
	}

	public Vector3 GetUnitPosition (float t)
	{
		return Vector3.Lerp (from, to, t);
	}

	public bool TryGetClosestPoint (Vector3 other, float maxDistance, out float t, out Vector3 pos)
	{
		t = 0f;
		pos = Vector3.zero;
		return false;
	}

	#endregion


}

public class CombinedLine : ILine {
	private IEnumerable<ILine> lines;
	public CombinedLine(IEnumerable<ILine> lines){
		this.lines = lines;
	}
	
	#region ILine implementation
	public float GetUnitLength ()
	{
		return lines.Sum(l => l.GetUnitLength());
	}
	public Vector3 GetUnitPosition (float t)
	{
		var p = t;
		foreach (ILine l in lines) {
			var length = l.GetUnitLength();
			if(p > length){
				p -= length;
			} else {
				return l.GetUnitPosition(p);
			}
		}
		return lines.Last ().GetUnitPosition (lines.Last ().GetUnitLength ());
	}
	#endregion

	#region ILine implementation

	public bool TryGetClosestPoint (Vector3 other, float maxDistance, out float t, out Vector3 pos)
	{
		float _t; Vector3 _pos;
		return lines.MinBy (l => {
			return l.TryGetClosestPoint(other, maxDistance, out _t, out _pos) ? (other-_pos).magnitude : float.MaxValue;
		}).TryGetClosestPoint(other, maxDistance, out t, out pos);
	}

	#endregion
}