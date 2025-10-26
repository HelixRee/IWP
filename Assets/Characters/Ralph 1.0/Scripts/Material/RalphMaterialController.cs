using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RalphMaterialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material _characterMaterial;
    [SerializeField] private List<RalphHeadlightBehaviour> _headLights = new();
    [SerializeField] private AnimationCurve _headlightIntensityCurve = new();

    [Header("Public Members")]
    [Range(0, 1f)] public float headlightFillAmt = 1f;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!Application.isPlaying) return;
        if (_randomiseHueOnStart)
            RandomiseHue();
    }
    private void OnValidate()
    {
        if (_characterMaterial == null)
        {
            Debug.LogWarning("Character material unassigned");
            enabled = false;
        }

        if (_activeMaterial == null)
            _activeMaterial = new Material(_characterMaterial);
        _activeMaterial.name = _characterMaterial.name + " (" + name + ")";
        //Debug.Log("Refreshed");

        // Link ID references
        _matHueOffsetID = Shader.PropertyToID("_Hue_Offset");
        _matHLIntensity1ID = Shader.PropertyToID("_HLIntensity1");
        _matHLIntensity2ID = Shader.PropertyToID("_HLIntensity2");
        _matHLIntensity3ID = Shader.PropertyToID("_HLIntensity3");
        _matHLIntensity4ID = Shader.PropertyToID("_HLIntensity4");


        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterial == null) continue;
            if (renderer.sharedMaterial.shader == null) continue;
            if (renderer.sharedMaterial.shader != _characterMaterial.shader) continue;
            if (renderer.sharedMaterial == _activeMaterial) continue;
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
        if (_cycleHue)
            AdvanceHue(Time.deltaTime * 360f);
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

            renderer.SetMaterials(new List<Material>() { _characterMaterial });
        }
        _activeMaterial = null;
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