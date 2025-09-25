using UnityEngine;

public class RalphHeadAnimator : RalphAnimator
{
    public Transform HeadProxy;
    [Range(0f, 1f)]
    public float Weight = 1f;

    [Range(0f, 10f)] public float Frequency = 3f;
    [Range(0f, 1f)] public float Damping = 0.5f;
    [Range(0f, 10f)] public float Readiness = 2f;

    private SODAngle _smoothedAngle;
    private float _nonSmoothedAngle;
    private Vector3 _initialAngles;

    public override void ManualInit()
    {
        _smoothedAngle = new SODAngle(HeadProxy.eulerAngles.y, 3, 0.5f, 2);
        _nonSmoothedAngle = HeadProxy.eulerAngles.y;
        _initialAngles = transform.localEulerAngles;
    }

    public override void ManualUpdate()
    {
        _nonSmoothedAngle = HeadProxy.eulerAngles.y;
        if (_nonSmoothedAngle < -180f) // -359f
            _nonSmoothedAngle = _nonSmoothedAngle + 360f;
        if (_nonSmoothedAngle > 180f) // 359f
            _nonSmoothedAngle = _nonSmoothedAngle - 360f;

        float smoothedAngle = _smoothedAngle.Update(Time.deltaTime, _nonSmoothedAngle);

        float disp = (smoothedAngle - _nonSmoothedAngle) % 360f;
        if (disp < -180f) // -359f
            disp = disp + 360f;
        if (disp > 180f) // 359f
            disp = disp - 360f;

        Vector3 angles = _initialAngles;
        angles.z += disp * Weight;
        transform.localEulerAngles = angles;
    }
}
