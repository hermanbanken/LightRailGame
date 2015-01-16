using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System.Linq;
using System;

public class ObstacleMaster : MonoBehaviour {
	[NonSerialized]
	List<Obstacle> obstacles = new List<Obstacle>();
	[NonSerialized]
	List<IIncident> incidents = new List<IIncident>();
	List<Vector3> obstaclesPos = new List<Vector3>();
	System.Random rnd = new System.Random ();

	LightRailGame game;
	List <ObstacleType> incidentTypeList = new List<ObstacleType>{ 
		ObstacleType.AngryMob, ObstacleType.DrunkenPassenger, ObstacleType.StenchOnBoard, ObstacleType.WomenInLabour };

	Action<Obstacle> onOccur;
	Action<Obstacle> onUserActioned;
	Action<Obstacle> onResolved;

	float? LastObstacle = null;
	float LastResolved;

	void Start (){
		game = GetComponent<LightRailGame> ();
		obstacles = new List<Obstacle>();
		incidents = new List<IIncident>();

		LastResolved = Time.time;
	}

	public void init(Action<Obstacle> onOccur, Action<Obstacle> onUserActioned, Action<Obstacle> onResolved) {
		this.onResolved = onResolved;
		this.onUserActioned = onUserActioned;
		this.onOccur = onOccur;
	}
	
	// TODO Rogier: call this from ScoreManager
	public void PlaceNewObstacle (){
		// Get random position
		Edge edge = game.graph.edges.ElementAt(rnd.Next(0, game.graph.edges.Count ()-1));
		float randU = (float)rnd.NextDouble () * edge.GetUnitLength ();
		float randT = edge.GetPositionOfUnitPoint (randU);
		Vector3 pos = edge.GetPoint(randT);
		Vector3 dir = edge.GetDirection (randT);
		Vector3 buttonPosition = getButtonPosition (pos, dir, 12);

		Obstacle obstacle = new GameObject ().AddComponent<Obstacle> ();
		System.Random gen = new System.Random ();
		int prob = gen.Next (0, 10);
		if (prob < 5)
			obstacle.init(pos, Eppy.Tuple.Create<ILine,float>(edge, randU), ObstacleType.Car, onUserActioned);
		else
			obstacle.init(pos, Eppy.Tuple.Create<ILine,float>(edge, randU), ObstacleType.Tree, onUserActioned);
		obstacle.buttonPosition = buttonPosition;
		obstacles.Add (obstacle);
		incidents.Add (obstacle.Incident);
		obstaclesPos.Add (obstacle.block.transform.position);

		if(onOccur != null) onOccur (obstacle);
	}

	public void CreateNewInsideTramIncident(){

		var trains = GameObject.FindObjectsOfType<Train> ();
		Train train = trains [rnd.Next(trains.Length)%trains.Length];
		var inc = new TramCarIncident (train.gameObject, incidentTypeList[rnd.Next (incidentTypeList.Count)]);
		train.Incident (inc);
		incidents.Add (inc);
	}

	public static Vector3 getButtonPosition(Vector3 pos, Vector3 dir, float distance){
		Vector3 a = new Vector3 ();
		Vector3 b = new Vector3 ();
		a.x = pos.x + distance * dir.y / (float)Math.Sqrt (dir.x * dir.x + dir.y * dir.y);
		a.y = pos.y - distance * dir.x / (float)Math.Sqrt (dir.x * dir.x + dir.y * dir.y);
		b.x = pos.x - distance * dir.y / (float)Math.Sqrt (dir.x * dir.x + dir.y * dir.y);
		b.y = pos.y + distance * dir.x / (float)Math.Sqrt (dir.x * dir.x + dir.y * dir.y);
		a.z = b.z = -4.5f;
		return (a.magnitude < b.magnitude) ? a : b;
	}

	void Update (){
		// Introduce obstacles
		if (incidents.Count < LightRailGame.Difficulty && (LastResolved + 2 + 10 * UnityEngine.Random.value < Time.time)) {
			// TODO Rogier: move this to ScoreManager
			if (!LastObstacle.HasValue || LastObstacle.Value + 10 < Time.time) {
				LastObstacle = Time.time;

				if(obstacles.Count * 2 - 2f * (.5f + UnityEngine.Random.value) > incidents.Count)
					CreateNewInsideTramIncident();
				else
					PlaceNewObstacle ();
			}
		}
		// Resolve obstacles
		var resolved = obstacles.Where (p => p.Incident.IsResolved()).ToList ();
		resolved.ForEach((ob) => { 
			obstacles.Remove(ob);
			LastResolved = Time.time;
			if(onResolved != null)
				onResolved(ob); 
		});

		incidents.RemoveAll (i => i.IsResolved ());
	}
	
	void OnDisable(){
		obstacles = new List<Obstacle>();
	}
}

public enum ObstacleType {
	Car,
	Tree,
	Barrel,
	Derailment,
	Defect,
	SwitchDefect,
	DrunkenPassenger,
	AngryMob, 
	WomenInLabour,
	StenchOnBoard
}

