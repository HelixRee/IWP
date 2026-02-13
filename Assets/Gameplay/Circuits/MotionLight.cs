using UnityEngine;

[RequireComponent(typeof(Light))]
public class MotionLight : MonoBehaviour
{
    [SerializeField] private float _rampdownSpeed = 6f;
    [SerializeField] private float _baseIntensity = 0.1f;
    public float smoothedSpeed = 0f;
    public float test = 0f;
    private Vector3 _prevPos;
    private Light _light;
    private void Start()
    {
        _light = GetComponent<Light>();
        _prevPos = transform.position;

    }
    private void LateUpdate()
    {
        Vector3 delta = transform.position - _prevPos;
        test = delta.magnitude;
        if (delta.magnitude > 1)
        {
            _prevPos = transform.position;
            smoothedSpeed = 0f;
            _light.intensity = _baseIntensity * smoothedSpeed;

            return;
        }

        smoothedSpeed = Mathf.Lerp(smoothedSpeed, delta.magnitude, _rampdownSpeed * Time.deltaTime);
        _light.intensity = _baseIntensity * smoothedSpeed;

        _prevPos = transform.position;
    }
}
