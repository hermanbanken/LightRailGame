using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Station : MonoBehaviour, IStop {
	private IDictionary<Train,float> Presence = new Dictionary<Train, float> ();
	private float stopTime = 5f;
	public GameObject quad;

	void Start(){

		var temp = this.gameObject.GetComponent<Node> ();
		var directi = temp.graph.edges.FirstOrDefault (e => e.From == temp).GetDirection(0f);
		var posi = gameObject.transform.position  + 2.5f * Vector3.Cross ( directi, Vector3.forward);
		quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
		quad.transform.parent = gameObject.transform;
		quad.transform.position = posi + 4f * Vector3.back;
		quad.transform.localScale = 2f * Vector3.one;
		quad.transform.rotation = Quaternion.Euler(0, 0, Vector3.Angle(Vector3.right,directi));
		
		quad.renderer.material.color = Color.yellow;
	}
	#region IStop implementation


	public void Arrive (Train train)
	{
		if(!IsPresent(train))
			Presence [train] = Time.time;
	}

	public bool TryLeave (Train train)
	{
		if(!Presence.ContainsKey(train))
			throw new InvalidOperationException ("The train can't leave a station he is not at");

		if (Presence [train] + stopTime < Time.time) {
			Presence.Remove (train);
			return true;
		} else {
			return false;
		}
	}

	public bool IsPresent (Train train)
	{
		return Presence.ContainsKey (train);
	}

	public float MaxSpeed (Train train)
	{
		//if (IsPresent (train))
			return 0f;
		//return 0.5f;
	}

	#endregion

}
