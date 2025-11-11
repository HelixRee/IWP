using UnityEngine;

public class LitterFlightBehaviour : MonoBehaviour
{
    private Rigidbody _rb;
    public Transform target;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.AddForce(Vector3.up * 2 * _rb.mass , ForceMode.Impulse);
    }
    private void Update()
    {
        if (target == null) return;

        Vector3 steeringVelocity = (target.position - (_rb.position + _rb.linearVelocity));
        //_rb.linearVelocity += steeringVelocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_rb.position, _rb.linearVelocity);
        Gizmos.DrawLine(target.position, _rb.position + _rb.linearVelocity);
    }
}
