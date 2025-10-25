using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RalphMaterialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material _characterMaterial;
    [SerializeField] private List<GameObject> _headLights = new();

    [Header("Public Members")]
    [Range(0, 1f)] public float headlightFillAmt = 1f;

    [Header("Behaviour")]
    [SerializeField] private bool _randomiseHueOnStart = true;
    [SerializeField] private bool _cycleHue = true;

    // ID References
    private int _matHueOffsetID;
    private int _matHeadlightFillID;
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
        _matHeadlightFillID = Shader.PropertyToID("_HeadlightFill");
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
    private void UpdateHeadlightObjects()
    {
        for (int i = 0; i < _headLights.Count; i++)
        {
            float normalisedPos = (_headLights.Count - i - 1) / (float)_headLights.Count;
            normalisedPos = Mathf.Max(normalisedPos, 0.01f);
            if (headlightFillAmt >= normalisedPos)
            {
                _headLights[i].SetActive(true);
            }
            else
            {
                _headLights[i].SetActive(false);
            }
        }
    }
    private void UpdateMaterialProperties()
    {
        _characterMaterial.SetFloat(_matHeadlightFillID, headlightFillAmt);
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
