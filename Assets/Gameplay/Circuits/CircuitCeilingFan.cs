using UnityEngine;

public class CircuitCeilingFan : CircuitComponent
{
    [SerializeField] private float _poweredOnSpeed = 360; 
    [SerializeField] private float _dampingSpeed = 3f;
    [SerializeField] private Transform _ceilingFanBlades;
    private float _currentSpeed = 0f;

    protected override void Update()
    {
        base.Update();
        if (isPowered)
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, _poweredOnSpeed, _dampingSpeed * Time.deltaTime);
        }
        else
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0, _dampingSpeed * Time.deltaTime);
        }

        Vector3 angles = _ceilingFanBlades.transform.localEulerAngles;
        angles.z += _currentSpeed * Time.deltaTime;
        _ceilingFanBlades.transform.localEulerAngles = angles;
    }
}
