using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIBatteryCharge : MonoBehaviour
{
    [Range(0,1)] public float fillAmount = 1.0f;

    [Header("Materials")]
    [InspectorName("Battery UI Materials")]
    [SerializeField] private List<Image> _batteryUIs = new();
    private int _fillAmountID;

    private void OnValidate()
    {
        Init();
    }
    void Init()
    {
        _fillAmountID = Shader.PropertyToID("_Fill_Amount");
    }

    // Update is called once per frame
    void Update()
    {
        _batteryUIs.ForEach(img => img.materialForRendering.SetFloat(_fillAmountID, fillAmount));
    }
}
