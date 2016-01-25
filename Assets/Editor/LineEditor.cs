using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Line))]
public class LineEditor : Editor {

	private void OnSceneGUI()
	{
		Line line = target as Line;

		Transform handleTransform = line.transform;

		Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		Vector3 startPoint = handleTransform.InverseTransformPoint(line.start);
		Vector3 endPoint = handleTransform.InverseTransformPoint(line.end);

		Handles.color = Color.white;

		// Handle
		EditorGUI.BeginChangeCheck();
		startPoint = Handles.DoPositionHandle(startPoint, handleRotation);
		if(EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(line, "Move Point");
			EditorUtility.SetDirty(line);
			line.start = handleTransform.InverseTransformPoint(startPoint);
		}

		EditorGUI.BeginChangeCheck();
		endPoint = Handles.DoPositionHandle(endPoint, handleRotation);
		if(EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(line, "Move Point");
			EditorUtility.SetDirty(line);
			line.end = handleTransform.InverseTransformPoint(endPoint);
		}
		Handles.DrawLine(startPoint, endPoint);
	}
}