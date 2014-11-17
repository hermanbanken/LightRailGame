using UnityEngine;
using System.Linq;

public class SplineDecorator : MonoBehaviour {

	public BezierSpline spline;

	public int frequency;

	public bool lookForward;

	public Transform[] items;

	private void Awake () {
		if (frequency <= 0 || items == null || items.Length == 0) {
			return;
		}
		float stepSize = frequency * items.Length;
		if (spline.Loop || stepSize == 1) {
			stepSize = 1f / stepSize;
		}
		else {
			stepSize = 1f / (stepSize - 1);
		}
		for (int p = 0, f = 0; f < frequency; f++) {
			for (int i = 0; i < items.Length; i++, p++) {
				Transform item = Instantiate(items[i]) as Transform;
				Vector3 position = spline.GetPoint(p * stepSize);
				Vector3 nextPosition = spline.GetPoint ((p+1) * stepSize);
				item.hideFlags = HideFlags.None | HideFlags.HideInHierarchy; 
				foreach(Renderer r in item.GetComponentsInChildren<Renderer>())
					r.enabled = true;
				item.transform.localPosition = position;
				item.transform.localScale.Scale(nextPosition - position);
				if (lookForward) {
					item.transform.LookAt(position + spline.GetDirection(p * stepSize));
				}
				item.transform.parent = transform;
			}
		}
	}
}