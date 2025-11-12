using UnityEngine;

public class LitterFlightBehaviour : MonoBehaviour
{
    public PIDVec3 pid;
    public float correctionalSpeed = 2;
    private Rigidbody _rb;
    public Transform target;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.AddForce(Vector3.up * 2, ForceMode.VelocityChange);

        pid = new PIDVec3(1f, 1f, 1f);
        //pid.integral = -Physics.gravity;
    }
    private void FixedUpdate()
    {
        if (target == null) return;

        //Vector3 steeringVelocity = (target.position - (_rb.position + _rb.linearVelocity));
        Vector3 correctionalForce = pid.Update(target.position, _rb.position, Time.fixedDeltaTime) * correctionalSpeed;
        //_rb.linearVelocity += steeringVelocity;
        //_rb.AddForce(steeringVelocity * _rb.mass, ForceMode.Acceleration);
        _rb.AddForce(correctionalForce, ForceMode.Acceleration);
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_rb.position, _rb.linearVelocity);
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(target.position, _rb.position + _rb.linearVelocity);
    }
}
