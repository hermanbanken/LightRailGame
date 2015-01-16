using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class Panner : MonoBehaviour, IDragHandler {

	LightRailGame game;
	
	[NonSerialized]
	private BoxCollider2D Background;

	void Start () {
		game = LightRailGame.GetInstance ();
		Background = GetComponent<BoxCollider2D> ();
	}

	void Update (){
		// Do scrolling
		Camera.main.orthographicSize -= Input.mouseScrollDelta.y;
		if(Input.mouseScrollDelta.y > 0){
			Camera.main.orthographicSize = Math.Max(Camera.main.orthographicSize,20f);
		}
		else{
			Camera.main.orthographicSize = Math.Min(Camera.main.orthographicSize, (float)Background.bounds.size.y/2/Camera.main.aspect);

		}
		if (Input.mouseScrollDelta.y != 0)
			FixCameraPosition (Vector3.zero, 0);

		var diff = new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0);
		if(diff.x != 0f || diff.y != 0f)
			FixCameraPosition(-diff, 1f);
	}

	#region IDragHandler implementation

	public void OnDrag (PointerEventData eventData)
	{
		Debug.Log ("PANNING!!");
		var speed = 0.5f * Camera.main.orthographicSize / 100f;
		FixCameraPosition (eventData.delta, speed);
	}

	#endregion
	
	private float rightmenuoffset;
	void FixCameraPosition (Vector3 diff, float speed)
	{
		var c_w = Camera.main.orthographicSize * Camera.main.aspect;
		var c_h = Camera.main.orthographicSize;
		var pos = Camera.main.transform.position;
		pos.x = Math.Max (Background.bounds.min.x  + c_w, Math.Min (Background.bounds.max.x - c_w + rightmenuoffset, pos.x - diff.x * speed));
		pos.y = Math.Max (Background.bounds.min.y + c_h  , Math.Min (Background.bounds.max.y - c_h + (10*Camera.main.orthographicSize/88), pos.y - diff.y * speed));
		Camera.main.transform.position = pos;
		
		game.OnSelectedGameObjectChanged += (GameObject obj) => {
			rightmenuoffset = (obj == null) ? 0 :30*Camera.main.orthographicSize* Camera.main.aspect/88 ;
		};
	}
}
