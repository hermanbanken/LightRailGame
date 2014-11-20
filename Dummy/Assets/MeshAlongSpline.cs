using UnityEngine;
using System.Collections;

public class MeshAlongSpline : MonoBehaviour {

	public BezierSpline spline;
	
	public int frequency;

	public MeshFilter Mesh;

	public void Reset(){
		frequency = 100;
	}

	public void Awake () {
		if (frequency <= 0) {
			return;
		}

		Mesh.mesh = new Mesh ();

		var width = new Vector3(0, 0, -1);

		var stepSize = 1f / frequency;

		var vertices = new Vector3[frequency*4+4];
		var tri = new int[3 * frequency * 4];
		var normals = new Vector3[frequency*4+4];
		var uv = new Vector2[frequency * 4 + 4];

		var half = frequency;

		for(int f = 0, t = 0; f <= frequency*2; f+=2, t+=6){
			Vector3 p = spline.GetPoint(stepSize * f);
			Vector3 d = spline.GetDirection(stepSize * f);
			vertices[f  ] 		= p + Vector3.Cross(d, width);
			vertices[f+1] 		= p - Vector3.Cross(d, width);;
			vertices[half+f]	= p + Vector3.Cross(d, width);
			vertices[half+f+1]	= p - Vector3.Cross(d, width);	
			normals [f  ]		= -Vector3.forward;
			normals [f+1]		= -Vector3.forward;
			normals [half+f] 	= Vector3.left;
			normals [half+f+1] 	= Vector3.left;
			uv[f  ] 		= new Vector2(0.5f, 0);
			uv[f+1] 		= new Vector2(0.5f, 1);
			uv[half+f] 		= new Vector2(0.5f, 0);
			uv[half+f+1] 	= new Vector2(0.5f, 1);

			if(f > 0){
				tri[t-6] = f-2;
				tri[t-5] = f;
				tri[t-4] = f-1;
				tri[t-3] = f;
				tri[t-2] = f+1;
				tri[t-1] = f-1;

				tri[t-6+3*half] = half + f-2;
				tri[t-5+3*half] = half + f;
				tri[t-4+3*half] = half + f-1;
				tri[t-3+3*half] = half + f;
				tri[t-2+3*half] = half + f+1;
				tri[t-1+3*half] = half + f-1;
			}
		}

		Mesh.mesh.vertices = vertices;
		Mesh.mesh.triangles = tri;
		Mesh.mesh.normals = normals;
		Mesh.mesh.uv = uv;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
