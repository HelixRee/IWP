using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RalphRagdollController : MonoBehaviour
{
    [Header("Death Parameters")]
    [SerializeField] private float _deathDelay = 0.5f;
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
        name = name + " (Dead)";
        foreach (Rigidbody rb in _rigidbodies)
            rb.isKinematic = false;

        foreach (Collider collider in _colliders)
        {
            collider.enabled = true;
            collider.gameObject.layer = 6;
        }

        _mainBody.AddForce(_characterController.velocity * _launchPower, ForceMode.Impulse);
        InventoryManager.Instance.RemoveAllLitterObjects();

        onBecomeRagdoll.Invoke();
        StartCoroutine(WaitAndRespawn());
    }
    private IEnumerator WaitAndRespawn()
    {
        yield return new WaitForSeconds(_deathDelay);
        RespawnManager.Instance.Respawn();
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.R))
        //    StartRagdoll();
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