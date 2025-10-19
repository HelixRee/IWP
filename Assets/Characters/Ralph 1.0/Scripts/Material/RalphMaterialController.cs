using UnityEngine;

public class RalphMaterialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material _characterMaterial;


    [Header("Behaviour")]
    [SerializeField] private bool _randomiseHueOnStart = true;
    [SerializeField] private bool _cycleHue = true;

    // ID References
    private int _matHueOffsetID;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_characterMaterial == null)
        {
            Debug.LogWarning("Character material unassigned");
            enabled = false;
        }

        // Link ID references
        _matHueOffsetID = Shader.PropertyToID("_Hue_Offset");

        if (_randomiseHueOnStart)
            RandomiseHue();
    }
    private void Update()
    {
        if (_cycleHue)
            AdvanceHue(Time.deltaTime * 360f);
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
