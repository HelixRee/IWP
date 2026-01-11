using StarterAssets;
using UnityEngine;

public class RalphHeadlightController : MonoBehaviour
{
    private StarterAssetsInputs _input;
    private RalphMaterialController _materialController;

    [Range(0,360)] public float yaw = 0f;
    public bool IsDeployed = false;
    [SerializeField] private Transform _brainTransform;
    [SerializeField] private Transform _rotationAnchorTransform;
    [SerializeField] private Transform _lightAnchorTransform;
    [SerializeField] private float _yawOffset = 90f;
    private float _initialYawOffset;
    [SerializeField] private float _deploymentOffset = 0.2f;
    private SODFloat _currentOffset;
    private float _initialOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        _materialController = GetComponent<RalphMaterialController>();

        if (_brainTransform == null)
        {
            enabled = false;
            Debug.Log("No assigned brain transform, disabling component.");
            return;
        }

        _initialOffset = _brainTransform.localPosition.z;
        _initialYawOffset = _rotationAnchorTransform.localEulerAngles.z;
        _currentOffset = new SODFloat(_initialOffset, 3, 0.9f, 1);
    }
    private void Update()
    {
        yaw = Camera.main.transform.localEulerAngles.y + _yawOffset;



        _materialController.mainLightAngle = yaw;
        Vector3 angles = _rotationAnchorTransform.localEulerAngles;
        angles.z = yaw + _initialYawOffset;
        _rotationAnchorTransform.localEulerAngles = angles;
    }
    void LateUpdate()
    {
        IsDeployed = _input.headlight;

        if (IsDeployed)
        {
            _currentOffset.Update(Time.deltaTime, _deploymentOffset);
        }
        else
        {
            _currentOffset.Update(Time.deltaTime, _initialOffset);
        }
        Vector3 localPos = _brainTransform.localPosition;
        localPos.z = _currentOffset.Value;
        _brainTransform.localPosition = localPos;
    }
}
