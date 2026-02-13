using UnityEngine;

public class CircuitLightMaterial : CircuitComponent
{
    [SerializeField] private Material _material;
    [SerializeField] private Color _baseColor;
    private float _transitionStartTimestamp = 0f;
    private float _transition = 0f;
    [SerializeField] private AnimationCurve _poweredOnCurve;
    [SerializeField] private AnimationCurve _poweredOffCurve;

    float poweredOnIntensity, poweredOffIntensity;
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (isPowered)
        {
            _transition = Mathf.Lerp(_transition, 1, 12f * Time.deltaTime);
            poweredOnIntensity = _poweredOnCurve.Evaluate(Time.time - _transitionStartTimestamp);
        }
        else
        {
            _transition = Mathf.Lerp(_transition, 0, 12f * Time.deltaTime);
            poweredOffIntensity = _poweredOffCurve.Evaluate(Time.time - _transitionStartTimestamp);
        }

        float intensity = Mathf.Lerp(poweredOffIntensity, poweredOnIntensity, _transition);
        _material.SetColor("emissiveFactor", _baseColor * intensity);
    }
    protected override void OnPowerOn()
    {
        base.OnPowerOn();
    }

    protected override void OnPowerOff()
    {
        base.OnPowerOff();
    }

    protected override void OnPowerChange()
    {
        base.OnPowerChange();
        _transitionStartTimestamp = Time.time;
    }
}
