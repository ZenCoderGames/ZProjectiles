using UnityEngine;

public class BezierCurve : MonoBehaviour {
	public Vector3[] points;
	public int numIterations = 10;
	public bool showVelocity;

	// Called by Unity when a components is created or reset
	public void Reset()
	{
		points = new Vector3[] {
			new Vector3(0,0,0),
			new Vector3(1,0,0),
			new Vector3(2,0,0),
			new Vector3(3,0,0)
		};
	}

	// 0 -> 1
	public Vector3 GetPoint(float t)
	{
		return transform.TransformPoint(Bezier.GetCubicPoint(points[0], points[1], points[2], points[3], t));
	}

	// 0 -> 1
	public Vector3 GetVelocity(float t)
	{
		return transform.TransformPoint(Bezier.GetCubicFirstDerivative(points[0], points[1], points[2], points[3], t)) - transform.position;
	}

	// 0 -> 1
	public Vector3 GetDirection(float t)
	{
		return GetVelocity(t).normalized;
	}
}