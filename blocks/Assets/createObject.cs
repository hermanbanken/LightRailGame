using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using System.Linq;

public class createObject : MonoBehaviour {
	List<Pairs> obstacles = new List<Pairs>();
	List<Vector3> obstaclesPos = new List<Vector3>();
	void running (){
		Pairs pair = new Pairs ("car");
		obstacles.Add (pair);
		obstaclesPos.Add (pair.block.transform.position);
	}
	void Start() {
		InvokeRepeating ("running", 5F,5F);
	}
	void Update (){
		obstacles.ForEach(p => { if (p != null) p.Tick(); });

		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray =  Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit)){
				Pairs pair = obstacles.FirstOrDefault(p => p.button == hit.collider.gameObject);
				if(pair == null){
					// Button not found
					// Ignore, whatever...
				} else {
					pair.Destroy();
				}
			}
		}
	}
}

