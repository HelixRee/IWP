using UnityEngine;

public class CircuitLightMaterial : CircuitComponent
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _material;
    [SerializeField] private Color _baseColor;
    private float _transitionStartTimestamp = 0f;
    private float _transition = 0f;
    [SerializeField] private AnimationCurve _poweredOnCurve;
    [SerializeField] private AnimationCurve _poweredOffCurve;

    float poweredOnIntensity, poweredOffIntensity;

    private void Start()
    {
        if (_renderer == null) return;
        //_renderer.material = new Material(_renderer.sharedMaterial);
        _material = _renderer.material;
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (_material == null) return;
        if (_renderer == null) return; 
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
        _material.SetFloat("_Emission", intensity);
        
        _material.SetColor("_EmissionColor", _baseColor * intensity);

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
