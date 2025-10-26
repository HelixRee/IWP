using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class RalphHeadlightBehaviour : MonoBehaviour
{
    [SerializeField] private Material _baseMaterial;
    private MeshRenderer _meshRenderer;
    private Material _activeMaterial;

    // ID References
    private int _matVisibilityID;
    private int _matNormalisedLengthID;
    private int _matRandSeedID;

    private float _originalVisibility;
    private float _originalNormalisedLength;

    [Header("Public Members")]
    public bool isActive = true;
    public float animationTimer = 0f;
    public AnimationCurve IntensityCurve;
    [Space(10)]
    [Range(0, 2f)] public float IntensityMultiplier = 1f;
    [Range(0, 2f)] public float NormalisedIntensity = 1f;
    [Space(10)]
    [Range(0, 1f)] public float Visibility;
    [Range(0, 1f)] public float NormalisedLength;
    private void OnValidate()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            Debug.LogWarning("Headlight script has no attached renderer.");
            enabled = false;
            return;
        }

        _matVisibilityID = Shader.PropertyToID("_Visibility");
        _matNormalisedLengthID = Shader.PropertyToID("_NormalisedLength");
        _matRandSeedID = Shader.PropertyToID("_RandomSeed");


        _originalVisibility = _baseMaterial.GetFloat(_matVisibilityID);
        _originalNormalisedLength = _baseMaterial.GetFloat(_matNormalisedLengthID);

        _activeMaterial = new Material(_baseMaterial);
        _activeMaterial.name = _activeMaterial.name + " (" + name + ")";
        _activeMaterial.SetFloat(_matRandSeedID, Random.Range(0f, 10000f));
        _meshRenderer.material = _activeMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if (IntensityCurve.length == 0) return;
        if (isActive)
        {
            if (animationTimer > IntensityCurve.keys.First().time)
                animationTimer -= Time.deltaTime;
            else
                animationTimer = IntensityCurve.keys.First().time;
        }
        else
        {
            if (animationTimer < IntensityCurve.keys.Last().time)
                animationTimer += Time.deltaTime;
            else
                animationTimer = IntensityCurve.keys.Last().time;
        }

        NormalisedIntensity = IntensityCurve.Evaluate(animationTimer);

        Visibility = _originalVisibility * NormalisedIntensity * IntensityMultiplier;
        NormalisedLength = _originalNormalisedLength * NormalisedIntensity * IntensityMultiplier;


        UpdateShaderVariables();
    }

    private void UpdateShaderVariables()
    {
        _activeMaterial.SetFloat(_matVisibilityID, Visibility);
        _activeMaterial.SetFloat(_matNormalisedLengthID, NormalisedLength);
    }

    public float GetNormalisedIntensity()
    {
        return NormalisedIntensity;
    }
}
