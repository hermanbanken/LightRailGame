using UnityEngine;
using System.Collections;

public class ReturnToStart : MonoBehaviour {
		public GameObject start1;
		public GameObject map1;
		public GameObject map2;
		public GameObject map3;
		public GameObject scrollbar;
		public GameObject text;
		public GameObject Exist;
		public GameObject train;
		public GameObject back;
		public GameObject banner;
		
	void OnMouseDown()
	{
		banner.SetActive (true);
		start1.SetActive (true);
		map1.SetActive (false);
		map2.SetActive (false);
		train.SetActive (true);
		map3.SetActive (false);
		scrollbar.SetActive (false);
		text.SetActive (true);
		back.SetActive (false);
		Exist.SetActive (true);
		gameObject.SetActive (false);
	}		
}