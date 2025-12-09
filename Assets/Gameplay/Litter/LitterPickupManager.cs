using UnityEngine;

public class LitterPickupManager : MonoBehaviour
{
    [SerializeField] private InventoryManager _inventorySystemReference;
    private SphereCollider _sphereCollider;
    private float cooldownTimer = 0f;
    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }
    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(_sphereCollider.center + transform.position, _sphereCollider.radius);
        //foreach (Collider collider in colliders)
        //{
        //    CreateLitter(collider);
        //}
        if (colliders.Length > 0 && cooldownTimer <= 0)
        {
            foreach (Collider collider in colliders)
            {
                if (CreateLitter(collider))
                {
                    //Debug.Log("Picked");
                    break;
                }
            }
        }
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        //Debug.Log(cooldownTimer);
    }
    private bool CreateLitter(Collider other)
    {
        //Debug.Log(other.name);
        //Debug.Log(other.tag);
        if (!other.CompareTag("Litter")) return false;

        GameObject go = other.gameObject;
        LitterBehaviour flightScript = go.GetComponent<LitterBehaviour>();
        if (flightScript.isAsleep) return false;
        go.tag = "Untagged";
    
        Rigidbody rb = go.GetComponent<Rigidbody>();
        rb.mass = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.useGravity = false;
        rb.isKinematic = true;

        flightScript.target = transform;
        flightScript.enabled = true;

        flightScript.inventoryManager = _inventorySystemReference;

        return true;
    }

}
