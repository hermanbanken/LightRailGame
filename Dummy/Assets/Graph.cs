using UnityEngine;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System;
using Eppy;

public class Graph : MonoBehaviour {

	public GameObject ToAddPrefab;

	// Use this for initialization
	void Start () {
		var gos = GameObject.FindGameObjectsWithTag("Rails");
		var points = gos.Select (g => getMaxPoints(g));
		foreach(Tuple<Vector3,Vector3> p in points){
			GameObject.Instantiate(ToAddPrefab, p.Item1, Quaternion.identity);
			GameObject.Instantiate(ToAddPrefab, p.Item2, Quaternion.identity);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	Tuple<Vector3,Vector3> getMaxPoints(GameObject obj){
		Vector3 p = obj.transform.position;
		Vector3 s = obj.transform.localScale;
		Vector3 start = new Vector3 (
			p.x + s.x / 2,
			p.y + s.y / 2,
			p.z + s.z / 2
				);
		Vector3 end = new Vector3 (
			p.x - s.x / 2,
			p.y - s.y / 2,
			p.z - s.z / 2
			);
		return Tuple.Create(start,end);
	}

}