using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrafficLight : MonoBehaviour, IStop, IEnumerable<List<TrafficLight>> {
	[NonSerialized]
	private IList<Train> Presence = new List<Train> ();
	// The TrafficLight that might be green after this one turns red
	public TrafficLight Next;
	// The TrafficLight that mimmics the behavior of this one
	public TrafficLight Slave;
	[HideInInspector,NonSerialized]
	public TrafficLight Master;
	[HideInInspector,NonSerialized]
	public Enumerator MasterEnumerator;

	// Last time we changed colors
	[HideInInspector]
	public float lastChanged = 0;
	// Current color
	[HideInInspector]
	public TrafficLightState State = TrafficLightState.Red;

	// Visual stuff
	[NonSerialized]
	private GameObject quad;
	[NonSerialized]
	private GameObject sphere;
	public readonly static Color orange = new Color(1f, 0.7f, 0f);

	void Reset() {
		Next = null;
	}

	void Start() {
		lastChanged = Time.time;
		State = TrafficLightState.Red;

		var node = this.gameObject.GetComponent<Node> ();
		var edge = node.graph.edges.FirstOrDefault (e => e.From == node || e.To == node);
		Vector3 dir;
		if(edge != null){
			dir = edge.GetDirection(edge.From == node ? 0f : 1f);
		} else {
			dir = Vector3.up;
		}
		var pos = gameObject.transform.position  + 2.5f * Vector3.Cross (dir, Vector3.forward);
		quad = GameObject.CreatePrimitive (PrimitiveType.Quad);
		quad.transform.parent = gameObject.transform;
		quad.transform.position = pos + 4f * Vector3.back;
		quad.transform.localScale = 2f * Vector3.one;
		quad.renderer.material.color = Color.black;
		Destroy (quad.collider);

		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.parent = quad.transform;
		sphere.transform.localScale = 0.6f * Vector3.one;
		sphere.transform.localPosition = Vector3.zero;
		sphere.renderer.material.color = Color.red;
		Destroy (sphere.collider);

		Update ();
	}

	public void StartAsMaster ()
	{
		// Prepare main traffic light loop
		MasterEnumerator = new Enumerator (this);

		// Initialize first round greens
		bool started = false;
		MasterEnumerator.Current.ForEach (tl => tl.State = TrafficLightState.Green);
	}

	public void StartAsSlave(TrafficLight master)
	{
		Master = master;
	}

	void Update(){
		switch (State){
		case TrafficLightState.Red:
			sphere.renderer.material.color = Color.red;
			break;
		case TrafficLightState.Orange:
			sphere.renderer.material.color = orange;
			break;
		default:
			sphere.renderer.material.color = Color.green;
			break;
		}
	}

	/**
	 * Implement color changing
	 */
	void FixedUpdate() {
	
		// Time to go orange
		if (MasterEnumerator != null && MasterEnumerator.Current.First().State == TrafficLightState.Green && lastChanged + Duration (TrafficLightState.Green) < Time.time) {
			MasterEnumerator.Current.ForEach (tl => {
				tl.State = TrafficLightState.Orange;
			});
		}

		// Time to go red + make another green
		if (MasterEnumerator != null && MasterEnumerator.Current.First().State == TrafficLightState.Orange && lastChanged + Duration (TrafficLightState.Green) + Duration (TrafficLightState.Orange) < Time.time) {
			MasterEnumerator.Current.ForEach (tl => {
				tl.State = TrafficLightState.Red;
			});

			lastChanged = Time.time;
			MasterEnumerator.MoveNext ();
			MasterEnumerator.Current.ForEach (tl => {
				tl.State = TrafficLightState.Green;
			});
		}
	}
	
	/**
	 * Duration of the various states
	 */
	private float Duration(TrafficLightState s){
		switch (s) {
			case TrafficLightState.Green: return 4f;
			case TrafficLightState.Orange: return 1f;
			default: 
				return 3f;
		}
	}
	
	#region IStop implementation

	public void Arrive (Train train)
	{
		Presence.Add (train);
	}

	public bool IsPresent (Train train)
	{
		return Presence.Contains (train);
	}

	public bool TryLeave (Train train)
	{
		if (State != TrafficLightState.Red) {
			Presence.Remove (train);
			return true;
		} else
			return false;
	}

	public float MaxSpeed (Train train)
	{
		if (State == TrafficLightState.Green)
			return 10f;
		else
			return 0f;
	}

	#endregion

	#region IEnumerable implementation

	public IEnumerator<List<TrafficLight>> GetEnumerator () { return new Enumerator(this, false); }
	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator (){ return new Enumerator(this, false); }

	#endregion

	public enum TrafficLightState {
		Red, Green, Orange
	}

	public class Enumerator : IEnumerator<List<TrafficLight>> {
		private TrafficLight root;
		private List<TrafficLight> current;
		private List<TrafficLight> prev;
		private bool loop;

		public Enumerator(TrafficLight root, bool loop = true) {
			this.loop = loop;
			this.root = root;
			Reset ();
		}

		#region IEnumerator implementation
		object System.Collections.IEnumerator.Current {
			get {
				return current;
			}
		}
		#endregion

		#region IEnumerator implementation
		
		public bool MoveNext ()
		{
			prev = current;
			current = current.SelectMany (tl => new [] { tl.Next }.Concat(Slaves(tl.Next)).Where (t => t != null)).Distinct().ToList();
			if (current.Count == 0 && loop) {
				current = new [] { root }.Concat(Slaves (root)).ToList();
			}
			return current.Count > 0;
		}

		private IEnumerable<TrafficLight> Slaves(TrafficLight master){
			if (master != null) {
				TrafficLight slave = master.Slave;
				var done = new List<TrafficLight> (new [] {master});
				while (slave != null && !done.Contains(slave)) {
					yield return slave;
					done.Add (slave);
					slave = slave.Slave;
				}
			}
		}

		public void Reset ()
		{
			prev = new List<TrafficLight>();
			current = new List<TrafficLight>();
			current.Add(root);	
			current.AddRange (Slaves (root));
		}
		
		#endregion
		
		#region IDisposable implementation
		
		public void Dispose ()
		{
			this.root = null;
			this.current = null;
		}
		
		#endregion
		
		#region IEnumerator implementation
		
		public List<TrafficLight> Current {
			get {
				return current;
			}
		}

		public List<TrafficLight> Previous {
			get {
				return prev;
			}
		}

		#endregion
	}
}