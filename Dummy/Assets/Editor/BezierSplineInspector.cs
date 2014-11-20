using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor {

	private const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private static Color[] modeColors = {
		Color.white,
		Color.yellow,
		Color.cyan
	};

	private BezierSpline spline;
	private int selectedIndex = -1;

	public override void OnInspectorGUI () {
		spline = target as BezierSpline;
		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Toggle Loop");
			EditorUtility.SetDirty(spline);
			spline.Loop = loop;
		}
		if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount) {
			DrawSelectedPointInspector();
		}
		if (GUILayout.Button("Add Curve")) {
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}
		if (GUILayout.Button ("Set all Z to 0")) {
			Undo.RecordObject(spline, "Set all Z to 0");
			spline.SetAllZ(0);
			EditorUtility.SetDirty(spline);
		}
	}

	private void DrawSelectedPointInspector() {
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(selectedIndex, point);
		}
		EditorGUI.BeginChangeCheck();
		BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Change Point Mode");
			spline.SetControlPointMode(selectedIndex, mode);
			EditorUtility.SetDirty(spline);
		}
	}

	public virtual void OnSceneGUI () {
		OnSceneGUI(target as BezierSpline);
	}

	public static void OnSceneGUI(BezierSpline spline, BezierSplineInspector inspector = null){
		var handleTransform = spline.transform;
		var handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
		
		Vector3 p0 = ShowPoint(spline, 0, handleTransform, handleRotation, inspector);
		for (int i = 1; i < spline.ControlPointCount; i += 3) {
			Vector3 p1 = ShowPoint(spline, i, handleTransform, handleRotation, inspector);
			Vector3 p2 = ShowPoint(spline, i + 1, handleTransform, handleRotation, inspector);
			Vector3 p3 = ShowPoint(spline, i + 2, handleTransform, handleRotation, inspector);
			
			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);
			
			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
			p0 = p3;
		}
		//ShowDirections();
	}

	private void ShowDirections () {
		Handles.color = Color.green;
		Vector3 point = spline.GetPoint(0f);
		Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
		int steps = stepsPerCurve * spline.CurveCount;
		for (int i = 1; i <= steps; i++) {
			point = spline.GetPoint(i / (float)steps);
			Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
		}
	}

	public static Vector3 ShowPoint(BezierSpline spline, int index, Transform handleTransform, Quaternion handleRotation, BezierSplineInspector self){
		//Vector3 point = spline.GetControlPoint (index);
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0) {
			size *= 2f;
		}
		Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap)) {
			if(self != null){
				self.selectedIndex = index;
				self.Repaint();
			}
		}
		if (self != null && self.selectedIndex == index) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}
	
}