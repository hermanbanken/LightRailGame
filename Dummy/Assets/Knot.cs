using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using System.Threading;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Reactive.Concurrency;

public class Knot : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IInitializePotentialDragHandler {

	public bool isDragging { get; private set; }

	LightRailGame game;
	Train train;
	Node origin; 

	event Action<PointerEventData> _OnDrag;
	event Action<PointerEventData> _OnEndDrag;

	// Use this for initialization
	void Start () {
		game = LightRailGame.GetInstance ();
	}

	Vector3 trackingPosition;
	void Update () {
		if (tracking && !isDragging) {
			if(
			//	!EventSystem.current.IsPointerOverGameObject() && 
			//	!ReferenceEquals(EventSystem.current.currentSelectedGameObject, null) &&
				LightRailGame.EdgeRaycaster.CurrentHover != null
			){
				trackingPosition = LightRailGame.EdgeRaycaster.CurrentHover.pos;
			}
			this.transform.position = Camera.main.WorldToScreenPoint(trackingPosition);
		}
	}

	bool IsRayCastHit(){
		return Physics2D.Raycast (Input.mousePosition, -Vector2.up).collider == this.collider2D;
	}

	#region IBeginDragHandler implementation
	
	public void OnBeginDrag (PointerEventData eventData)
	{
		isDragging = true;
		train = game.SelectedGameObject.GetComponent<Train> ();
		EventSystem.current.SetSelectedGameObject (this.gameObject);

		Node origin = null;
		CombinedLine<Edge> reroute = null;
		ComputationResult result = null;

		Observable.FromEvent<Edge> (a => LightRailGame.EdgeRaycaster.OnEdgeChange += a, a => LightRailGame.EdgeRaycaster.OnEdgeChange -= a)
			.Select (e => LightRailGame.EdgeRaycaster.CurrentHover)
			.StartWith (LightRailGame.EdgeRaycaster.CurrentHover)
			.SkipWhile (h => h == null)
			.Do(h => {
				if(origin == null){
					origin = h.t > 0.5f ? h.Edge.To : h.Edge.From;	
				}
			})
			.Do (n => Debug.Log ("Hovering over "+n.Edge, n.Edge))
			.CombineLatest(
				Observable.FromEvent<Train>(a => train.OnPathChange += a, a => train.OnPathChange -= a).StartWith(train).Select(t => t.Path[t.currentTrack]),
				(p, activeEdge) => Eppy.Tuple.Create<EdgeRaycaster.Hover,Edge>(p, activeEdge)
			)
			.TakeUntil (Observable.FromEvent<PointerEventData> (a => _OnEndDrag += a, a => _OnEndDrag -= a))
			.TakeUntil (Observable.FromEvent<GameObject> (a => game.OnSelectedGameObjectChanged += a, a => game.OnSelectedGameObjectChanged -= a))
			//.SubscribeOn(Scheduler.ThreadPool)
			.Select(t => ComputeRoute(t, train, origin))
			//.ObserveOn(Scheduler.CurrentThread)
			.Subscribe (pair => 
	            {
					if(reroute != null) game.LineMaster.HideLine(reroute);
					if(pair == null){ reroute = null; result = null; return; }
					result = pair;
					reroute = new CombinedLine<Edge>(pair.NewPiece);
					game.LineMaster.ShowLine(reroute, game.LineOptsReRoute);
				}, 
				e => Debug.LogException(e),
				() => {
					if(reroute != null){
						game.LineMaster.HideLine(reroute);
						Debug.Log ("[O] Reroute completed, old length was "+train.Path.Count+" new length is "+result.WayPoints.Count);
						Debug.Log ("[O] Old WayPoints: "+train.WayPoints.Count+" new count is "+result.WayPoints.Count);
						train.UpdatePath(result.WayPoints, result.Route);
					} else 
						Debug.Log ("[O] Reroute cancelled");
				});

		Debug.Log ("Begin Drag");
	}

	#endregion

	public ComputationResult ComputeRoute(Eppy.Tuple<EdgeRaycaster.Hover, Edge> latest, Train train, Node origin)
	{
		if (latest.Item1 == null)
			return null;

		Node newWP = latest.Item1.t > 0.5f ? latest.Item1.Edge.To : latest.Item1.Edge.From;

		var beforeReRouteWP = train
			// Guard
			.Take (train.Path.Count*3)
			// All edges before the dragged Knot
			.Skip (1).TakeWhile (e => e.From != origin)
			// Get all waypoints in there
			.Select (e => e.From).Where (train.WayPoints.Contains);
			
		var beforeLastWP = beforeReRouteWP.LastOrDefault () ?? train.First().To;

		var afterReRouteWP = train
			// Guard
			.Take (train.Path.Count*3)
			// Ride on until the dragged Knot
			.SkipWhile(e => e.From != origin)
			// Until we loop again
			.TakeWhile(e => e.To != train.First().To)
			// Take the destination nodes and filter the Waypoints
			.Select (e => e.To).Where (train.WayPoints.Contains);

		var afterFirstWP = afterReRouteWP.FirstOrDefault () ?? train.First().To;

		IEnumerable<Edge> a = origin.graph.Dijkstra.PlanRoute(beforeLastWP, newWP);
		IEnumerable<Edge> b = origin.graph.Dijkstra.PlanRoute(newWP, afterFirstWP);

		// After route b, return back
		var back = train.SkipWhile (e => e.From != afterFirstWP).TakeWhile ((e, i) => i == 0 || e.From != train.First ().From);

		var route = train
			// Guard
			.Take (train.Path.Count*3)
			// Until we're off the original route
			.TakeWhile (e => e.From != beforeLastWP)
			// Add re-route
			.Concat (a).Concat (b)
			// Loop back
			.Concat (back);

		var wps = beforeReRouteWP.Concat(new Node[] { newWP }).Concat(afterReRouteWP);

//		/* Validate route: */
//		var context = "\nRouting train between " + train.First ().From + " and " + train.First ().To + " dest: "+newWP+
//			"\nWPs: "+train.WayPoints.Aggregate ("", (s,n) => s + n) +
//			"\nExisting: "+train.TakeWhile((e, i) => i == 0 || e != train.First()).Aggregate ("", (s, e) => s + e.From +"-"+ e.To + "=") + 
//			"\nBefore: "+beforeReRouteWP.Aggregate ("", (s,n) => s + n) +
//			"\nAfter: "+afterReRouteWP.Aggregate ("", (s,n) => s + n) +
//			"\nBack: "+back.Aggregate ("", (s, e) => s + e.From +"-"+ e.To + "=") + 
//			"\nRoute: " + route.Aggregate ("", (s, e) => s + e.From +"-"+ e.To + "=");
//
//		assert (route.First () == train.First (), "The edge we are on remains first in the route", context);
//		assert (route.Last ().To == train.First ().From, "The route loops back to start ("+route.Last ().To+"!="+train.First ().From+")", context);
//		route.Scan((_1, _2) => {
//			assert(_1 != _2, "Duplicate edge");
//			assert(_1.To == _2.From, "Edges must be connected");
//			return _2;
//		}, route.Last());
//		wps.Scan ((_1, _2) => {
//			assert(_1 != _2, "Duplicate waypoint");
//			return _2;
//		}, wps.Last());
//
//		Debug.Log (context);

		return new ComputationResult {
			Route = route.ToList(),
			WayPoints = wps.ToList(),
			NewPiece = a.Concat(b),
		};

//		Node from; 
//		Node to;
//
//		// Dragging WayPoint
//		if (train.WayPoints.Contains (origin)) {
//			from = train.WayPoints.TakeWhile (n => n != origin).LastOrDefault () ?? train.WayPoints.Last ();
//			to = train.WayPoints.SkipWhile (n => n != origin).Skip (1).FirstOrDefault () ?? train.WayPoints.First ();
//		} 
//		// Dragging in-between
//		else {
//			from = train.Skip (1).Select(e => e.From).TakeWhile(n => n != origin).LastOrDefault(n => train.WayPoints.Contains(n)) ?? origin;
//			to = train.Skip (1).Select(e => e.To).SkipWhile(n => n != origin).FirstOrDefault(n => train.WayPoints.Contains(n)) ?? origin;
//		}
//
//		IEnumerable<Edge> a = origin.graph.Dijkstra.PlanRoute(from, newWP);
//		IEnumerable<Edge> b = origin.graph.Dijkstra.PlanRoute(newWP, to);
//
//		return Eppy.Tuple.Create<IList<Node>,IList<Edge>>(
//			train.WayPoints.SelectMany (n => n == from ? new [] { from, newWP } : new [] { n }).ToList(),
//			train.TakeWhile(e => e.From != from).Concat(a).Concat(b).Concat(train.SkipWhile(e => e.From != to).TakeWhile(e => e.To != train.First().From)).ToList()
//		);
	}

	public class ComputationResult
	{
		public IList<Node> WayPoints { get; set; }
		public IList<Edge> Route { get; set; }
		public IEnumerable<Edge> NewPiece { get; set; }
	}

	private void assert(bool condition, string message, string context = null){
		if(!condition)
			throw new Exception(message + context);
	}

	#region IDragHandler implementation

	public void OnDrag (PointerEventData eventData)
	{
		this.transform.position = eventData.position;

		var handler = _OnDrag;
		if (handler != null)
			handler (eventData);

//		if(OnDrag)
//		Thread
//		Task UITask= task.ContinueWith(() =>
//		                               {
//			this.TextBlock1.Text = "Complete"; 
//		}, TaskScheduler.FromCurrentSynchronizationContext());
//		ILine reroute = null;
//		Edge lastEdge = currentEdge;
//		
//		evt.OnDrag += (Vector3 obj) => {
//			// If we can Snap
//			if(LightRailGame.EdgeRaycaster.CurrentHover != null){
//				if(lastEdge == LightRailGame.EdgeRaycaster.CurrentHover.Edge)
//					return;
//				
//				lastEdge = LightRailGame.EdgeRaycaster.CurrentHover.Edge;
//				
//				IEnumerable<Edge> a = graph.Dijkstra.PlanRoute(from, lastEdge.To);
//				IEnumerable<Edge> b = graph.Dijkstra.PlanRoute(lastEdge.To, to);
//				
//				// Clear previous
//				if(reroute != null) LineMaster.HideLine(reroute);
//				reroute = new CombinedLine<Edge>(a.Concat(b));
//				LineMaster.ShowLine(reroute, LineOptsReRoute);
//			} 
//			// Cannot Snap
//			else {
//				lastEdge = currentEdge;
//				Knot.transform.position = obj;
//				var c = Camera.main.ScreenToWorldPoint(obj).FixZ(currentEdge.To.position.z);
//				// Clear previous
//				if(reroute != null) LineMaster.HideLine(reroute);
//				reroute = new CombinedLine<StraightLine>(new [] {
//					new StraightLine(from.position, c),
//					new StraightLine(c, to.position)
//				});
//				LineMaster.ShowLine(reroute, LineOptsReRoute);
//			}
//		};
//		evt.OnRelease += (Vector3 obj) => {
//			Debug.LogWarning("Released Re-route");
//			if(reroute != null) LineMaster.HideLine(reroute);
//			if(LightRailGame.EdgeRaycaster.CurrentHover != null && (lastEdge == LightRailGame.EdgeRaycaster.CurrentHover.Edge) && lastEdge != currentEdge){
//				Debug.LogWarning("Re-route is in finished state");
//				var newWP = train.WayPoints.Where(n=>true).ToList();
//				if(LightRailGame.EdgeRaycaster.CurrentHover.t < 0.5)
//					newWP.Insert (newWP.IndexOf(from), lastEdge.From);
//				else 
//					newWP.Insert (newWP.IndexOf(from), lastEdge.To);
//				train.UpdatePath(newWP);
//			}
//		};
	}

	#endregion
	
	#region IEndDragHandler implementation
	
	public void OnEndDrag (PointerEventData eventData)
	{
		isDragging = false;

		var handler = _OnEndDrag;
		if (handler != null)
			handler (eventData);

		EventSystem.current.SetSelectedGameObject (train.gameObject);
	}
	
	#endregion

	#region IInitializePotentialDragHandler implementation

	public void OnInitializePotentialDrag (PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject (this.gameObject);
	}

	#endregion

	bool tracking;
	public void SetTracking (bool b)
	{
		this.tracking = b;
	}
}
