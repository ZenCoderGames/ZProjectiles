using UnityEngine;

public static class Bezier
{
	// Quadratic Lerp = (1-t)^2*P0 + 2*(1-t)*t*P1 + t^2*P2
	public static Vector3 GetQuadraticPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		t = Mathf.Clamp01(t);
		float invT = 1f-t;
		return invT * invT * p0 +
			2 * invT * t * p1 + 
			t * t * p2;
	}

	// Derivative of Quadratic Lerp above: 2*(1-t)*(p1-p0) + 2*t*(p2-p1)
	public static Vector3 GetQuadraticFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, float t) {
		return 2f * (1f - t) * (p1 - p0) +	2f * t * (p2 - p1);
	}

	// Cubic Lerp = (1 - t)3 P0 + 3 (1 - t)2 t P1 + 3 (1 - t) t2 P2 + t3 P3 
	public static Vector3 GetCubicPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float invT = 1f - t;
		return
			invT * invT * invT * p0 +
			3f * invT * invT * t * p1 +
			3f * invT * t * t * p2 +
			t * t * t * p3;
	}

	// Derivative of Cubic Lerp above: 3 (1 - t)2 (P1 - P0) + 6 (1 - t) t (P2 - P1) + 3 t2 (P3 - P2)
	public static Vector3 GetCubicFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float invT = 1f - t;
		return
			3f * invT * invT * (p1 - p0) +
			6f * invT * t * (p2 - p1) +
			3f * t * t * (p3 - p2);
	}
}