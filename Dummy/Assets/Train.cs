using UnityEngine;
using System.Collections;

public class Train : MonoBehaviour {

	public GameObject StartingNode;
	private Node currentNode;
	public bool forward = true;
	public float speed = 10f;
	private float position = 0f;

	// Use this for initialization
	void Start () {
		if (StartingNode == null)
			return;

		currentNode = StartingNode.GetComponent<Node>();
	}
	
	// Update is called once per frame
	void Update () {
		if (currentNode == null)
			return;
		
		position += speed * Time.deltaTime;

		Vector3 pos;
		Vector3 dest;
		currentNode = currentNode.PositionOnLineReturningOverflow (position, out position, out pos, out dest);
		transform.position = pos;
		transform.rotation = Quaternion.LookRotation (dest - pos); ///Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(dest - pos));
	}
}
