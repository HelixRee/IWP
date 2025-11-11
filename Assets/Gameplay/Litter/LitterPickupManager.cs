using UnityEngine;

public class LitterPickupManager : MonoBehaviour
{
    private SphereCollider _sphereCollider;
    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }
    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(_sphereCollider.center + transform.position, _sphereCollider.radius);
        foreach (Collider collider in colliders)
        {
            CreateLitter(collider);
        }
    }
    private void CreateLitter(Collider other)
    {
        Debug.Log(other.name);
        Debug.Log(other.tag);
        if (!other.CompareTag("Litter")) return;

        GameObject go = other.gameObject;
        go.tag = "Untagged";

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        //rb.useGravity = false;
        //rb.isKinematic = true;

        LitterFlightBehaviour flightScript = go.AddComponent<LitterFlightBehaviour>();
        flightScript.target = transform;
    }

}
