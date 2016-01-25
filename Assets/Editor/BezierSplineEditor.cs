using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineEditor : Editor {
	BezierSpline _spline;
	Transform _splineT;
	Quaternion _splineR;

	private const float HANDLE_SIZE = 0.04f;
	private const float PICK_SIZE = 0.06f;

	private int _selectedIndex = -1;

	Tool LastTool = Tool.None;

	void OnEnable()
	{
		LastTool = Tools.current;
		Tools.current = Tool.None;
	}

	void OnDisable()
	{
		Tools.current = LastTool;
	}

	void OnSceneGUI()
	{
		_spline = target as BezierSpline;
		_splineT = _spline.transform;
		_splineR = Tools.pivotRotation == PivotRotation.Local ? _splineT.rotation : Quaternion.identity;

		// Draw curves
		Vector3 p0 = ShowPoint(0);
		for (int i = 1; i < _spline.ControlPointCount; i += 3) {
			Vector3 p1 = ShowPoint(i);
			Vector3 p2 = ShowPoint(i + 1);
			Vector3 p3 = ShowPoint(i + 2);

			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);

			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
			p0 = p3;
		}

		// Show velocity
		if(_spline.showVelocity)
		{
			Vector3 lineStart = _spline.GetPoint(0f);
			Handles.color = Color.green;
			Handles.DrawLine(lineStart, lineStart + _spline.GetDirection(0f));

			int steps = _spline.numIterations * _spline.CurveCount;
			for (int i = 1; i <= steps; i++) {
				Vector3 lineEnd = _spline.GetPoint(i / (float)steps);
				Handles.DrawLine(lineEnd, lineEnd + _spline.GetDirection(i / (float)steps));
			}
		}
	}

	private static Color[] modeColors = {
		Color.white,
		Color.yellow,
		Color.green
	};

	Vector3 ShowPoint(int idx)
	{
		// convert to world space since handles are in local space
		Vector3 point = _splineT.TransformPoint(_spline.GetControlPoint(idx));

		Handles.color = Color.white;
		float size = HandleUtility.GetHandleSize(point);
		if (idx == 0) {
			size *= 2f;
		}

		Handles.color = modeColors[(int)_spline.GetControlPointMode(idx)];

		// If this point is selected
		if (Handles.Button(point, _splineR, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotCap)) {
			_selectedIndex = idx;
			Repaint();
		}

		// If point is selected and position is changed
		if (_selectedIndex == idx) {
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle(point, _splineR);
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(_spline, "Move Point");
				EditorUtility.SetDirty(_spline);
				// convert back to local space
				_spline.SetControlPoint(idx, _splineT.InverseTransformPoint(point));
			}
		}

		return point;
	}

	public override void OnInspectorGUI()
	{
		_spline = target as BezierSpline;

		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop", _spline.Loop);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(_spline, "Toggle Loop");
			EditorUtility.SetDirty(_spline);
			_spline.Loop = loop;
		}

		if(_selectedIndex>=0 && _selectedIndex<=_spline.ControlPointCount)
			DrawSelectedPointInspector();

		if (GUILayout.Button("Add Curve")) {
			Undo.RecordObject(_spline, "Add Curve");
			_spline.AddCurve();
			EditorUtility.SetDirty(_spline);
		}
	}

	void DrawSelectedPointInspector()
	{
		GUILayout.Label("Selected Point (" + _selectedIndex.ToString() + ")");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", _spline.GetControlPoint(_selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(_spline, "Move Point");
			EditorUtility.SetDirty(_spline);
			_spline.SetControlPoint(_selectedIndex, point);
		}

		EditorGUI.BeginChangeCheck();
		BezierSpline.BezierControlPointMode mode = (BezierSpline.BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", _spline.GetControlPointMode(_selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(_spline, "Change Point Mode");
			_spline.SetControlPointMode(_selectedIndex, mode);
			EditorUtility.SetDirty(_spline);
		}
	}
}