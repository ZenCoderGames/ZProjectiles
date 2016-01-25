using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor {
	BezierCurve _curve;
	Transform _curveT;
	Quaternion _curveR;

	void OnSceneGUI()
	{
		_curve = target as BezierCurve;
		_curveT = _curve.transform;
		_curveR = Tools.pivotRotation == PivotRotation.Local ? _curveT.rotation : Quaternion.identity;

		Vector3 p0 = ShowPoint(0);
		Vector3 p1 = ShowPoint(1);
		Vector3 p2 = ShowPoint(2);
		Vector3 p3 = ShowPoint(3);

		// Show original lines
		Handles.color = Color.grey;
		Handles.DrawLine(p0, p1);
		Handles.DrawLine(p1, p2);
		Handles.DrawLine(p2, p3);

		// Show curve
		Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);

		// Show velocity
		if(_curve.showVelocity)
		{
			Vector3 lineStart = _curve.GetPoint(0f);
			Handles.color = Color.green;
			Handles.DrawLine(lineStart, lineStart + _curve.GetDirection(0f));
		
			for (int i = 1; i <= _curve.numIterations; i++) {
				Vector3 lineEnd = _curve.GetPoint(i / (float)_curve.numIterations);
				Handles.DrawLine(lineEnd, lineEnd + _curve.GetDirection(i / (float)_curve.numIterations));
			}
		}
	}

	Vector3 ShowPoint(int idx)
	{
		// convert to world space since handles are in world space
		Vector3 point = _curveT.TransformPoint(_curve.points[idx]);
		EditorGUI.BeginChangeCheck();
		point = Handles.DoPositionHandle(point, _curveR);
		if(EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(_curve, "Move Point");
			EditorUtility.SetDirty(_curve);
			// convert back to local space
			_curve.points[idx] = _curveT.InverseTransformPoint(point);
		}

		return point;
	}
}