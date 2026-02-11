using StarterAssets;
using UnityEngine;

public class LitterPickupManager : MonoBehaviour
{
    [SerializeField] private StarterAssetsInputs _input;
    [SerializeField] private InventoryManager _inventorySystemReference;
    [SerializeField] private Transform _flightTarget;
    private SphereCollider _sphereCollider;
    private float cooldownTimer = 0f;
    private void Start()
    {
        _sphereCollider = GetComponent<SphereCollider>();
    }
    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(_sphereCollider.transform.rotation * _sphereCollider.center + _sphereCollider.transform.position, _sphereCollider.radius);
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
        
        if (_input.interact)
        {
            foreach (Collider collider in colliders)
            {
                if (!collider.TryGetComponent(out BatterySocket batterySocket)) continue;
                batterySocket.DetachBattery();
                break;
            }
        }
    }
    private bool CreateLitter(Collider other)
    {
        //Debug.Log(other.name);
        //Debug.Log(other.tag);
        if (!other.CompareTag("Litter")) return false;

        GameObject go = other.gameObject;
        LitterFlightBehaviour flightScript = go.GetComponent<LitterFlightBehaviour>();
        if (flightScript.isAsleep) return false;
        go.tag = "Untagged";
    
        Rigidbody rb = go.GetComponent<Rigidbody>();
        rb.mass = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.useGravity = false;
        rb.isKinematic = true;

        flightScript.target = _flightTarget;
        flightScript.enabled = true;

        flightScript.inventoryManager = _inventorySystemReference;

        return true;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent(out BatterySocket batterySocket)) return;
        batterySocket.RefreshUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out BatterySocket batterySocket)) return;
        batterySocket.DisableUI();
    }
}
