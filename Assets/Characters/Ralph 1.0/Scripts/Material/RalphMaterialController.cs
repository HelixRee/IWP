using System.Collections.Generic;
using UnityEngine;

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

    // ID References
    private int _matHueOffsetID;
    private int _matHLIntensity1ID;
    private int _matHLIntensity2ID;
    private int _matHLIntensity3ID;
    private int _matHLIntensity4ID;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        // Link ID references
        _matHueOffsetID = Shader.PropertyToID("_Hue_Offset");
        _matHLIntensity1ID = Shader.PropertyToID("_HLIntensity1");
        _matHLIntensity2ID = Shader.PropertyToID("_HLIntensity2");
        _matHLIntensity3ID = Shader.PropertyToID("_HLIntensity3");
        _matHLIntensity4ID = Shader.PropertyToID("_HLIntensity4");

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
        _characterMaterial.SetFloat(_matHLIntensity1ID, _headLights[0].GetNormalisedIntensity());
        _characterMaterial.SetFloat(_matHLIntensity2ID, _headLights[1].NormalisedIntensity);
        _characterMaterial.SetFloat(_matHLIntensity3ID, _headLights[2].NormalisedIntensity);
        _characterMaterial.SetFloat(_matHLIntensity4ID, _headLights[3].NormalisedIntensity);
    }
    public void RandomiseHue()
    {
        float randHue = Random.Range(0f, 360f);
        _characterMaterial.SetFloat(_matHueOffsetID, randHue);
    }

    public void AdvanceHue(float amount)
    {
        _characterMaterial.SetFloat(_matHueOffsetID, _characterMaterial.GetFloat(_matHueOffsetID) + amount);
    }

    private void OnDisable()
    {
        // Reset hue for editor
        _characterMaterial.SetFloat(_matHueOffsetID, 0);
    }
}
