using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization;

[System.Serializable]
public class LineSchedule : System.Object, ISerializable
{
	public string Name;
	public int TramCount;
	public List<Node> WayPoints;
	
	public LineSchedule(){
		WayPoints = new List<Node>();
	}
	
	protected LineSchedule(SerializationInfo info, StreamingContext context) {
		Name = info.GetString("name");
		TramCount = info.GetInt32("tc");
		var wc = info.GetInt32("wc");
		var ns = GameObject.FindObjectsOfType<Node>();
		WayPoints = new List<Node>();
		for(int i = 0; i < wc; i++){
			var node = ns.FirstOrDefault(n => n.GetInstanceID() == info.GetInt32("w-"+i));
			if(node == null)
				Debug.LogWarning("Node not found when deserializing");
			else
				WayPoints.Add(node);
		}
	}
	
	#region ISerializable implementation
	
	public void GetObjectData (SerializationInfo info, StreamingContext context)
	{
		info.AddValue ("name", Name);
		info.AddValue ("tc", TramCount);
		info.AddValue ("wc", WayPoints.Count);
		for (int i = 0; i < WayPoints.Count; i++) {
			info.AddValue("w-"+i, WayPoints[i].GetInstanceID());
		}
	}
	
	#endregion
}