using UnityEngine;

public class SplineProjectile : MonoBehaviour {
	public BezierSpline spline;
	public float speed;
	public bool loop;
	Transform _transform;
	float _t;
	public Transform start, end;

	void Awake() {
		_transform = transform;
		spline.ModifyCurve(start.position, end.position);
	}

	void Update () {
		_t += Time.deltaTime * speed;
		_t = Mathf.Clamp(_t, 0, 1);

		if(loop && _t>=1)
			_t = 0;

		_transform.localPosition = spline.GetPoint(_t);
	}
}