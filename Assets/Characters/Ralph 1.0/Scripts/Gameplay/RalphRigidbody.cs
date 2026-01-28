using UnityEngine;

public class RalphRigidbody : MonoBehaviour
{
    private CharacterController _controller;
    private RalphMovementController _movement;
    private Collider _collider;
    [SerializeField] private float _mass = 1.0f;
    [SerializeField] private float _strength = 1.0f;
    [SerializeField] private float _auxStrength = 1.0f;
    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _movement = GetComponent<RalphMovementController>();
        _collider = GetComponent<Collider>();
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y < -0.5f)
        {
            if (hit.rigidbody != null)
            {
                Vector3 resultantForce = Vector3.zero;
                resultantForce += _mass * Physics.gravity;
                resultantForce += (_mass * 0.5f) * (hit.controller.velocity.normalized * hit.controller.velocity.sqrMagnitude);
               
            }
        }

        PushRigidBodies(hit);
    }
    Vector3 vel = Vector3.one;
    private void OnTriggerStay(Collider other)
    {
        if (!_movement.Grounded) return;
        Rigidbody parentRigidbody = null;

        if (other.TryGetComponent(out Rigidbody rigidbody) || other.transform.parent.TryGetComponent(out parentRigidbody))
        {
            if (rigidbody == null) rigidbody = parentRigidbody;
            Vector3 resultantForce = Vector3.zero;
            resultantForce += _auxStrength * transform.forward;
            Vector3 forcePoint = new Vector3(other.bounds.center.x, _collider.bounds.center.y, other.bounds.center.z);
            rigidbody.AddForceAtPosition(resultantForce, forcePoint);

            //Debug.Log("Pushing " + other.name);
        }
    }
    private void PushRigidBodies(ControllerColliderHit hit)
    {
        // https://docs.unity3d.com/ScriptReference/CharacterController.OnControllerColliderHit.html

        // make sure we hit a non kinematic rigidbody
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        //// make sure we only push desired layer(s)
        //var bodyLayerMask = 1 << body.gameObject.layer;
        //if ((bodyLayerMask & pushLayers.value) == 0) return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f) return;

        // Calculate push direction from move direction, horizontal motion only
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);
        Vector3 forcePoint = new Vector3(hit.collider.bounds.center.x, _collider.bounds.center.y, hit.collider.bounds.center.z);
        // Apply the push and take strength into account
        body.AddForceAtPosition(pushDir * _strength, forcePoint, ForceMode.Impulse);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, vel);
    }
}
