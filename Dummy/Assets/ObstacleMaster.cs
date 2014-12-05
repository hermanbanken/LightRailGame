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
		float randT = (float)rnd.NextDouble ();
		Vector3 pos = edge.GetPoint (randT);
		Vector3 dir = edge.GetDirection (randT);
		Vector3 buttonPosition = getButtonPosition (pos, dir, 7);

		Obstacle obstacle = new GameObject ().AddComponent<Obstacle> ();
	
		obstacle.init(pos, ObstacleType.Car, onUserActioned);
		obstacle.button.transform.localScale = new Vector3 (9, 3, 1);
		obstacle.button.transform.position = buttonPosition;
		obstacles.Add (obstacle);
		obstaclesPos.Add (obstacle.block.transform.position);
		if(onOccur != null) onOccur (obstacle);
	}
	

	Vector3 getButtonPosition(Vector3 pos, Vector3 dir, float distance){
		Vector3 a = new Vector3 ();
		Vector3 b = new Vector3 ();
		a.x = pos.x + distance * dir.y / (float)Math.Sqrt (dir.x * dir.x + dir.y * dir.y);
		a.y = pos.y - distance * dir.x / (float)Math.Sqrt (dir.x * dir.x + dir.y * dir.y);
		b.x = pos.x - distance * dir.y / (float)Math.Sqrt (dir.x * dir.x + dir.y * dir.y);
		b.y = pos.y + distance * dir.x / (float)Math.Sqrt (dir.x * dir.x + dir.y * dir.y);
		a.z = b.z = -4;
		return (a.magnitude < b.magnitude) ? a : b;
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

