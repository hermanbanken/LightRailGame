using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Mapchoose : MonoBehaviour {

	public void ChooseMap(string name){

		Application.LoadLevel (name);

	}

}
