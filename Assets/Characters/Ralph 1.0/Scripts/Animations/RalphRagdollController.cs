using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RalphRagdollController : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> _rigidbodies = new();
    [SerializeField] private List<Collider> _colliders = new();

    [SerializeField] private UnityEvent onBecomeRagdoll;

    public void StartRagdoll()
    {
        foreach (Rigidbody rb in _rigidbodies)
            rb.isKinematic = false;

        foreach (Collider collider in _colliders)
            collider.enabled = true;

        onBecomeRagdoll.Invoke();
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