using UnityEngine;

public class RalphRigidbody : MonoBehaviour
{
    [SerializeField] private float _mass = 1.0f;
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Vector3.Dot((hit.point - hit.collider.bounds.center), Vector3.down) < 0.9f)
        {
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(Vector3.down * _mass, hit.point);
                Debug.Log("test");
            }
        }
        //hit.rigidbody.AddForce()
    }
}
