using UnityEngine;

public class RalphRigidbody : MonoBehaviour
{
    private CharacterController _controller;
    private RalphMovementController _movement;
    private Collider _collider;
    [SerializeField] private float _mass = 1.0f;
    [SerializeField] private float _temp = 1.0f;
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
                

                //hit.rigidbody.AddForceAtPosition(resultantForce, hit.point);


                //Debug.Log(hit.rigidbody.GetPointVelocity(hit.point));
                //Debug.Log(hit.controller.velocity);
                //hit.controller.Move(hit.rigidbody.GetPointVelocity(hit.point));
            }
        }
        else
        {
            if (hit.rigidbody != null)
            {
                //Vector3 resultantForce = Vector3.zero;
                ////resultantForce += (_mass * 0.5f) * (hit.moveDirection * hit.moveLength * hit.moveLength);
                //resultantForce += (_mass * 0.5f) * (hit.moveDirection);

                //hit.rigidbody.AddForceAtPosition(resultantForce, hit.point);
                //Debug.Log(hit.moveDirection + ", " + hit.moveLength + ", " + resultantForce);
            }
        }
        //hit.rigidbody.AddForce()
    }
    Vector3 vel = Vector3.one;
    private void OnTriggerStay(Collider other)
    {
        //Debug.Log(other.name);
        if (other.TryGetComponent(out Rigidbody rigidbody)) 
        {
            Vector3 resultantForce = Vector3.zero;
            //resultantForce += (_mass * 0.5f) * (_controller.velocity.normalized * _controller.velocity.sqrMagnitude);
            resultantForce += (_mass * 0.5f) * transform.forward;
            rigidbody.AddForceAtPosition(resultantForce, _collider.bounds.center);
            //Debug.Log("Pushing " + other.name);
            ////rigidbody.AddForce(resultantForce);
            ////rigidbody.AddForceAtPosition(resultantForce, other.transform.position);
            ////rigidbody.AddForce(resultantForce, ForceMode.Force);
            //rigidbody.linearVelocity = resultantForce;
            //_controller.Move(rigidbody.GetPointVelocity(transform.position) * Time.fixedDeltaTime);
            //_controller.Move(Vector3.zero);
            //vel = rigidbody.GetPointVelocity(transform.position);
            ////vel = rigidbody.GetPointVelocity(transform.position) * 0.15f;

            //Vector3 tempVel = new Vector3(-vel.x, vel.y, -vel.z);
            ////_movement.AddVel(tempVel * Time.deltaTime * _temp);
            ////if (vel.magnitude < 0.1) return;
            ////_movement.AddVel(vel);
            ////_controller.mo += vel * Time.deltaTime;
            //Debug.Log(vel);
            //hit.controller.Move(hit.rigidbody.GetPointVelocity(hit.point));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, vel);
    }
}
