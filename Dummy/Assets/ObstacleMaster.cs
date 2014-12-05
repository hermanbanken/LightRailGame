using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System.Linq;
using System;

public class ObstacleMaster : MonoBehaviour {
	List<Obstacle> obstacles = new List<Obstacle>();
	List<Vector3> obstaclesPos = new List<Vector3>();
	System.Random rnd = new System.Random ();
	LightRailGame game;

	Action<Obstacle> onOccur;
	Action<Obstacle> onUserActioned;
	Action<Obstacle> onResolved;

	void Start (){
		game = GetComponent<LightRailGame> ();
	}

	public void init(Action<Obstacle> onOccur, Action<Obstacle> onUserActioned, Action<Obstacle> onResolved) {
		this.onResolved = onResolved;
		this.onUserActioned = onUserActioned;
		this.onOccur = onOccur;
		InvokeRepeating ("running", 5F,5F);
	}

	void running (){
		// Get random position
		Edge edge = game.graph.edges.ElementAt(rnd.Next(0, game.graph.edges.Count ()-1));
		Vector3 pos = edge.GetPoint ((float)rnd.NextDouble ());

		Obstacle obstacle = new GameObject ().AddComponent<Obstacle> ();
		obstacle.init(pos, ObstacleType.Car, onUserActioned);
		obstacles.Add (obstacle);
		obstaclesPos.Add (obstacle.block.transform.position);
		if(onOccur != null) onOccur (obstacle);
	}

	void Update (){
		// Tick
		obstacles.ForEach(p => { if (p != null) p.Tick(); });

		// Resolve obstacles
		var resolved = obstacles.Where (p => p.userActionedAt + p.timeToResolve < DateTime.Now).ToList ();
		resolved.ForEach((ob) => { 
			obstacles.Remove(ob);
			if(onResolved != null)
				onResolved(ob); 
		});

		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray =  Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit)){
				Obstacle obstacle = obstacles.FirstOrDefault(p => p.button == hit.collider.gameObject);
				// Button was hit
				if(obstacle != null){
					obstacle.DoUserAction();
				}
			}
		}
	}
}

public enum ObstacleType {
	Car,
	Tree,
	Barrel
}

