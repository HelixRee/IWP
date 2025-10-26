using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RalphRagdollController : MonoBehaviour
{
    [Header("Launch Parameters")]
    [SerializeField] private Rigidbody _mainBody;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float _launchPower = 10f;
    private Vector3 _launchVelocity = Vector3.zero;
    private Vector3 _prevPos = Vector3.zero;

    [Space(10)]
    [SerializeField] private List<Rigidbody> _rigidbodies = new();
    [SerializeField] private List<Collider> _colliders = new();

    [SerializeField] private UnityEvent onBecomeRagdoll;

    private void Start()
    {
        _prevPos = _characterController.center;
    }
    public void StartRagdoll()
    {
        foreach (Rigidbody rb in _rigidbodies)
            rb.isKinematic = false;

        foreach (Collider collider in _colliders)
            collider.enabled = true;

        _mainBody.AddForce(_characterController.velocity * _launchPower, ForceMode.Impulse);

        onBecomeRagdoll.Invoke();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            StartRagdoll();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RalphRagdollController))]
public class RalphRagdollControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        RalphRagdollController controller = (RalphRagdollController)target;
        if (GUILayout.Button("Start Ragdoll"))
        {
            controller.StartRagdoll();
        }
    }
}
#endif