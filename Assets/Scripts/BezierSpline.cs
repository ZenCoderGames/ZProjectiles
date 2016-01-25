using UnityEngine;
using System;

public class BezierSpline : MonoBehaviour {
	[SerializeField]
	private Vector3[] _points;
	public int numIterations = 10;
	public bool showVelocity;

	public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
	}

	[SerializeField]
	private BezierControlPointMode[] _modes;

	[SerializeField]
	private bool loop;

	public bool Loop {
		get {
			return loop;
		}
		set {
			loop = value;
			if (value == true) {
				_modes[_modes.Length - 1] = _modes[0];
				SetControlPoint(0, _points[0]);
			}
		}
	}

	#region CONTROL_POINTS
	public int ControlPointCount {
		get {
			return _points.Length;
		}
	}

	public Vector3 GetControlPoint(int index)
	{
		return _points[index];
	}

	public void SetControlPoint(int index, Vector3 pos)
	{
		// If the index point is on the curve, rather than a handle
		// make sure the handles also get the displacement
		if (index % 3 == 0) {
			Vector3 delta = pos - _points[index];
			if (loop) {
				if (index == 0) {
					_points[1] += delta;
					_points[_points.Length - 2] += delta;
					_points[_points.Length - 1] = pos;
				}
				else if (index == _points.Length - 1) {
					_points[0] = pos;
					_points[1] += delta;
					_points[index - 1] += delta;
				}
				else {
					_points[index - 1] += delta;
					_points[index + 1] += delta;
				}
			}
			else {
				if (index > 0) {
					_points[index - 1] += delta;
				}
				if (index + 1 < _points.Length) {
					_points[index + 1] += delta;
				}
			}
		}

		_points[index] = pos;
		EnforceMode(index);
	}
	#endregion

	#region CONTROL_POINT_MODES
	public BezierControlPointMode GetControlPointMode(int index)
	{
		return _modes[(index+1)/3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		int modeIndex = (index+1)/3;
		_modes[modeIndex] = mode;
		if (loop) {
			if (modeIndex == 0) {
				_modes[_modes.Length - 1] = mode;
			}
			else if (modeIndex == _modes.Length - 1) {
				_modes[0] = mode;
			}
		}
		EnforceMode(index);
	}
	#endregion

	#region CONSTRAINTS
	public void EnforceMode(int index)
	{
		int modeIndex = (index+1)/3;
		BezierControlPointMode mode = _modes[modeIndex];
		if(mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == _modes.Length-1))
			return;

		// Find the left (fixed) and right (enforced) points of the mid point
		// Note: the mid point is the actual point on the curve while the left and right
		//       are the handles of the curve
		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			// In case of loop
			if (fixedIndex < 0) {
				fixedIndex = _points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			// In case of loop
			if (enforcedIndex >= _points.Length) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			// In case of loop
			if (fixedIndex >= _points.Length) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			// In case of loop
			if (enforcedIndex < 0) {
				enforcedIndex = _points.Length - 2;
			}
		}

		Vector3 middlePointPos = _points[middleIndex];
		Vector3 enforcedTangentPointPos = middlePointPos - _points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangentPointPos = enforcedTangentPointPos.normalized * Vector3.Distance(middlePointPos, _points[enforcedIndex]);
		}
		_points[enforcedIndex] = middlePointPos + enforcedTangentPointPos;
	}
	#endregion

	// Called by Unity when a components is created or reset
	public void Reset()
	{
		_points = new Vector3[] {
			new Vector3(0,0,0),
			new Vector3(1,0,0),
			new Vector3(2,0,0),
			new Vector3(3,0,0)
		};

		_modes = new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		};
	}

	// 0 -> 1
	public Vector3 GetPoint(float t)
	{
		int i;
		// if t >=1, choose last curve
		if (t >= 1) {
			t = 1;
			i = _points.Length - 4;
		}
		// else if t < 1, find t with respect to number of curves
		// eg: t = 0.5 and CurveCount = 2
		else {
			// eg: t = 1
			t = Mathf.Clamp01(t) * CurveCount;
			// eg: i = 1
			i = (int)t;
			// eg: t = 0
			t -= i;
			// eg: i = 3, hence it will start at the beginning of curve 2
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetCubicPoint(_points[i], _points[i + 1], _points[i + 2], _points[i + 3], t));
	}

	// 0 -> 1
	public Vector3 GetVelocity(float t)
	{
		int i;
		if (t >= 1f) {
			t = 1f;
			i = _points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetCubicFirstDerivative(_points[i], _points[i+1], _points[i+2], _points[i+3], t)) - transform.position;
	}

	// 0 -> 1
	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}

	// Since a spline is made up of multiple bezier curves
	public int CurveCount {
		get {
			return (_points.Length - 1) / 3;
		}
	}

	public void AddCurve () {
		Vector3 point = _points[_points.Length - 1];
		Array.Resize(ref _points, _points.Length + 3);
		point.x += 1f;
		_points[_points.Length - 3] = point;
		point.x += 1f;
		_points[_points.Length - 2] = point;
		point.x += 1f;
		_points[_points.Length - 1] = point;

		Array.Resize(ref _modes, _modes.Length + 1);
		_modes[_modes.Length - 1] = _modes[_modes.Length - 2];

		SetControlPointMode(_points.Length - 4, BezierControlPointMode.Mirrored);
		EnforceMode(_points.Length - 4);

		if (loop) {
			_points[_points.Length - 1] = _points[0];
			_modes[_modes.Length - 1] = _modes[0];
			EnforceMode(0);
		}
	}

	public void ModifyCurve(Vector3 startPoint, Vector3 endPoint)
	{
		transform.localEulerAngles = Vector3.zero;

		// Rotate the curve to be similar to the new points
		Vector3 originalStartPoint = _points[0];
		Vector3 originalEndPoint = _points[_points.Length-1];

		Vector3 vecBetweenOriginalPoints = transform.TransformPoint(originalEndPoint)-transform.TransformPoint(originalStartPoint);
		float angleBetweenOriginal = Vector3.Angle(vecBetweenOriginalPoints, Vector3.right);
		Vector3 crossOriginal = Vector3.Cross(vecBetweenOriginalPoints, Vector3.right);

		//// negate the angle if it is below the x-axis
		if(crossOriginal.y<1)
			angleBetweenOriginal *= -1;

		Vector3 vecBetweenNewPoints = endPoint - startPoint;
		float angleBetweenNewPoints = Vector3.Angle(vecBetweenNewPoints, Vector3.right);
		Vector3 crossNew = Vector3.Cross(vecBetweenNewPoints, Vector3.right);

		//// negate the angle if it is below the x-axis
		if(crossNew.y<1)
			angleBetweenNewPoints *= -1;

		transform.localEulerAngles = Vector3.up * angleBetweenOriginal; 
		transform.localEulerAngles -= Vector3.up * angleBetweenNewPoints;

		// Now move the points with respect to the new points
		// Also scale up the control point handles to match it
		float distanceFromNewPoints = Vector3.Distance(startPoint, endPoint);
		float distanceFromCurrentPoints = Vector3.Distance(originalStartPoint, originalEndPoint);
		float diff = distanceFromNewPoints/distanceFromCurrentPoints;

		Vector3 localSpaceStart = transform.InverseTransformPoint(startPoint);
		Vector3 dirnOfMovtStart = localSpaceStart - originalStartPoint;

		Vector3 localSpaceEnd = transform.InverseTransformPoint(endPoint);
		Vector3 dirnOfMovtEnd = localSpaceEnd - originalEndPoint;

		float t;
		int totalPoints = _points.Length;
		int curveCount = CurveCount;
		for(int i=0; i<totalPoints; ++i)
		{
			if(i%3==0)
			{
				t = (float)(i/3)/(float)(curveCount);
				SetControlPoint(i, _points[i] + (dirnOfMovtStart * (1-t)) + (dirnOfMovtEnd * t));
			}
			else
			{
				Vector3 dirnFromControlPoint = Vector3.zero;	
				// Left
				if(i%3==2)
				{
					dirnFromControlPoint = (_points[i] - _points[i+1]).normalized;
					float newDist = Vector3.Distance(_points[i], _points[i+1]) * diff;
					SetControlPoint(i, _points[i+1] + dirnFromControlPoint * newDist);
				}
				// Right
				else if(i%3==1)
				{
					dirnFromControlPoint = (_points[i] - _points[i-1]).normalized;
					float newDist = Vector3.Distance(_points[i], _points[i-1]) * diff;
					SetControlPoint(i, _points[i-1] + dirnFromControlPoint * newDist);
				}
			}
		}
	}
}