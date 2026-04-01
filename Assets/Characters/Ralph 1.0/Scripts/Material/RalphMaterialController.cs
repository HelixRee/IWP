using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.Rendering.Universal;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RalphMaterialController : MonoBehaviour
{
    private StarterAssetsInputs _input;
    private RalphRagdollController _ragdoll;

    public bool flagForRefresh = true;
    [Header("References")]
    [SerializeField] private Material _characterMaterial;
    [SerializeField] private List<RalphHeadlightBehaviour> _headLights = new();
    [SerializeField] private AnimationCurve _headlightIntensityCurve = new();
    [SerializeField] private AnimationCurve _mainLightPowerCurve = new();

    [Header("Public Members")]
    [InspectorName("Headlight Fill Amount")]
    [Range(0, 1f)] public float headlightFillAmt = 1f;


    // Main Lights
    [SerializeField] private List<Light> _mainLights = new();
    private List<float> _mainLightInitialIntensities = new();
    [Range(0, 2f)] public float mainLightIntensity = 0f;
    private float _mainLightEmission = 0f;
    [Range(0, 10f)] public float mainLightEmissionMult = 1f;
    [Range(0, 10f)] public float mainLightIntensityMult = 1f;
    private float _mainLightTimer = 0f;
    [Range(0, 360f)] public float mainLightAngle = 0f;
    [Range(1, 10f)] public float mainLightPower = 1f;

    [Header("Behaviour")]
    [SerializeField] private bool _randomiseHueOnStart = true;
    [SerializeField] private bool _cycleHue = true;

    [SerializeField] private Material _activeMaterial;
    // ID References
    private int _matHueOffsetID;
    private int _matHLIntensity1ID;
    private int _matHLIntensity2ID;
    private int _matHLIntensity3ID;
    private int _matHLIntensity4ID;
    private int _mainLightIntensityID;
    private int _mainLightAngleID;
    private int _mainLightPowerID;
    //_Main_Light_Intensity
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!Application.isPlaying) return;
        ResetMaterials();
        OnValidate();

        if (_randomiseHueOnStart)
            RandomiseHue();

        _input = GetComponent<StarterAssetsInputs>();
        _ragdoll = GetComponent<RalphRagdollController>();
        _mainLightInitialIntensities.Clear();
        _mainLights.ForEach(light => _mainLightInitialIntensities.Add(light.intensity));
        if (!_input.headlight)
            mainLightIntensity = 0;
    }
    private void OnValidate()
    {
        if (_characterMaterial == null)
        {
            Debug.LogWarning("Character material unassigned");
            enabled = false;
        }

        if (_activeMaterial == null || _activeMaterial.name != _characterMaterial.name + " (" + name + ")" || flagForRefresh)
        {
            flagForRefresh = false;
            _activeMaterial = new Material(_characterMaterial);
        }
        //_activeMaterial = new Material(_characterMaterial);
        _activeMaterial.name = _characterMaterial.name + " (" + name + ")";
        //Debug.Log("Refreshed");

        // Link ID references
        _matHueOffsetID = Shader.PropertyToID("_Hue_Offset");
        _matHLIntensity1ID = Shader.PropertyToID("_HLIntensity1");
        _matHLIntensity2ID = Shader.PropertyToID("_HLIntensity2");
        _matHLIntensity3ID = Shader.PropertyToID("_HLIntensity3");
        _matHLIntensity4ID = Shader.PropertyToID("_HLIntensity4");
        _mainLightIntensityID = Shader.PropertyToID("_Main_Light_Intensity");
        _mainLightAngleID = Shader.PropertyToID("_Main_Light_Angle");
        _mainLightPowerID = Shader.PropertyToID("_Main_Light_Power");


        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterial == null)
            {
                if (renderer.TryGetComponent(out ParticleSystem ps  )) continue;

                renderer.SetMaterials(new List<Material>() { _activeMaterial });
                continue;
            }
            if (renderer.sharedMaterial.shader == null) continue;
            if (renderer.sharedMaterial.shader != _characterMaterial.shader) continue;
            if (renderer.sharedMaterial == _activeMaterial) continue;
            if (renderer.TryGetComponent(out ParticleSystem ps2)) continue;
            //Debug.Log(renderer.name);
            renderer.SetMaterials(new List<Material>() { _activeMaterial });
        }
        
        InitHeadlights();
    }
    private void Update()
    {

        UpdateMaterialProperties();
        UpdateHeadlightObjects();

        // Gate editor functionality
        if (!Application.isPlaying) return;
        //if (headlightFillAmt > 0)
        //    headlightFillAmt -= Time.deltaTime / headlightDecayTime;
        //if (headlightFillAmt <= 0 && !_ragdollStarted)
        //{
        //    _ragdoll.StartRagdoll();
        //    _ragdollStarted = true;
        //}
        //if (Input.GetKeyDown(KeyCode.R))
        //    headlightFillAmt = 0f;
        mainLightPower = _mainLightPowerCurve.Evaluate(Time.time);
        if (_cycleHue)
            AdvanceHue(Time.deltaTime * 360f);
    }
    private void LateUpdate()
    {
        if (!Application.isPlaying) return;
        if (_input.headlight)
        {
            _mainLightTimer += Time.deltaTime * 2;
            float lightIntensity = _headlightIntensityCurve.Evaluate(1 - _mainLightTimer);
            mainLightIntensity = lightIntensity * mainLightIntensityMult;
            _mainLightEmission = lightIntensity * mainLightEmissionMult;
        }
        else
        {
            _mainLightTimer = 0;
            mainLightIntensity = Mathf.Lerp(mainLightIntensity, 0, Time.deltaTime * 12f);
            _mainLightEmission = Mathf.Lerp(_mainLightEmission, 0, Time.deltaTime * 12f);
        }
        _mainLightTimer = Mathf.Clamp01(_mainLightTimer);
        for (int i = 0; i < _mainLights.Count; i++)
        {
            _mainLights[i].intensity = mainLightIntensity * _mainLightInitialIntensities[i];
        }
        

    }
    private void InitHeadlights()
    {
        foreach (RalphHeadlightBehaviour headlight in _headLights)
            headlight.IntensityCurve = _headlightIntensityCurve;
    }
    private void UpdateHeadlightObjects()
    {
        for (int i = 0; i < _headLights.Count; i++)
        {
            float normalisedPos = (_headLights.Count - i - 1) / (float)_headLights.Count;
            normalisedPos = Mathf.Max(normalisedPos, 0.01f);
            if (headlightFillAmt >= normalisedPos)
            {
                _headLights[i].isActive = true;
            }
            else
            {
                _headLights[i].isActive = false;
            }
        }
    }
    private void UpdateMaterialProperties()
    {
        if (_activeMaterial == null) return;
        _activeMaterial.SetFloat(_matHLIntensity1ID, _headLights[0].GetNormalisedIntensity());
        _activeMaterial.SetFloat(_matHLIntensity2ID, _headLights[1].NormalisedIntensity);
        _activeMaterial.SetFloat(_matHLIntensity3ID, _headLights[2].NormalisedIntensity);
        _activeMaterial.SetFloat(_matHLIntensity4ID, _headLights[3].NormalisedIntensity);
        _activeMaterial.SetFloat(_mainLightIntensityID, _mainLightEmission);
        _activeMaterial.SetFloat(_mainLightPowerID, mainLightPower);
        _activeMaterial.SetFloat(_mainLightAngleID, mainLightAngle);

    }
    public void RandomiseHue()
    {
        //Debug.Log("Randomised");

        float randHue = Random.Range(0f, 360f);
        _activeMaterial.SetFloat(_matHueOffsetID, randHue);
    }

    public void AdvanceHue(float amount)
    {
        _activeMaterial.SetFloat(_matHueOffsetID, _activeMaterial.GetFloat(_matHueOffsetID) + amount);
    }

    private void OnDisable()
    {
        //Debug.Log("Disabled");

        if (_activeMaterial == null) return;
        // Reset hue for editor
        _activeMaterial.SetFloat(_matHueOffsetID, 0);
    }

    public void ResetMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterial == null) continue;
            if (renderer.sharedMaterial.shader == null) continue;
            if (renderer.sharedMaterial.shader != _characterMaterial.shader) continue;
            //Debug.Log(renderer.name);
            renderer.SetMaterials(new List<Material>() { _characterMaterial });
        }
        _activeMaterial = null;
    }

    public float GetActiveMaterialHueShift()
    {
        if (_activeMaterial != null)
            return _activeMaterial.GetFloat(_matHueOffsetID);
        else
            return Random.Range(0, 360f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RalphMaterialController))]
public class RalphMaterialControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        RalphMaterialController controller = (RalphMaterialController)target;
        if (GUILayout.Button("Reset")) {
            controller.ResetMaterials();
        }
    }
}
#endif