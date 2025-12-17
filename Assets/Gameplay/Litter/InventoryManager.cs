using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Transform _realPackMount;
    [SerializeField] private Transform _simulatedPackMount;
    [SerializeField] private RotationConstraint _rotationConstraint;
    [SerializeField] private BoxCollider _boxCast;
    [SerializeField] private LayerMask _litterLayer;

    private float _prevPackMountY;
    private Dictionary<GameObject, LitterBehaviour> _litterBehaviours = new();
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Persist across scene loads
        }
    }
    private void Start()
    {
        ConstraintSource constraintSource = new();
        constraintSource.weight = 1f;
        constraintSource.sourceTransform = _realPackMount;

        _rotationConstraint.SetSource(0, constraintSource);
        _rotationConstraint.rotationOffset = Vector3.zero;
        _rotationConstraint.rotationAtRest = Vector3.zero;


        _prevPackMountY = _realPackMount.transform.position.y;
    }
    public GameObject CreateLitterObject(LitterBehaviour litterScript)
    {
        GameObject simObject = Instantiate(litterScript.gameObject, transform);
        _litterBehaviours.Add(simObject, litterScript);
        litterScript.simulatedObject = simObject;

        Rigidbody rb = simObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        Vector3 offset = litterScript.transform.position - _realPackMount.position;

        simObject.transform.position = _simulatedPackMount.position + offset;

        return simObject;
    }

    public void RemoveLitterSimObject(GameObject simObject, bool reeableObject = true)
    {
        LitterBehaviour litterScript = _litterBehaviours[simObject];
        _litterBehaviours.Remove(simObject);

        Destroy(simObject);
        litterScript.simulatedObject = null;

        Rigidbody rb = litterScript.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        litterScript.gameObject.tag = "Litter";
        litterScript.isAsleep = true;

        Collider collider = litterScript.GetComponent<Collider>();
        collider.enabled = true;

        if (reeableObject)
        {
            IEnumerator coroutine;
            coroutine = WaitAndEnable(litterScript);
            StartCoroutine(coroutine);
        }
    }

    public LitterBehaviour RemoveTopLitterObject()
    {
        RaycastHit hit;
        if (!Physics.BoxCast(_boxCast.center + _boxCast.transform.position, _boxCast.bounds.extents / 2f, _boxCast.transform.forward, out hit, _boxCast.transform.rotation, 10f, _litterLayer))
            return null;
        LitterBehaviour litterScript = _litterBehaviours[hit.collider.gameObject];
        RemoveLitterSimObject(hit.collider.gameObject, false);
        return litterScript;
    }

    public IEnumerator WaitAndEnable(LitterBehaviour litterScript)
    {
        yield return new WaitForSeconds(2);
        litterScript.isAsleep = false;
    }

    private void LateUpdate()
    {
        float verticalVel = (_realPackMount.transform.position.y - _prevPackMountY) / Time.deltaTime;
        foreach (var litterScript in _litterBehaviours)
        {
            litterScript.Value.simulatedObject.GetComponent<Rigidbody>().AddForce(Vector3.up * -verticalVel * 0.3f, ForceMode.Force);
        }


        _prevPackMountY = _realPackMount.transform.position.y;
        foreach (var litterScript in _litterBehaviours)
        {
            Vector3 offset = litterScript.Value.simulatedObject.transform.position - _simulatedPackMount.position;
            litterScript.Value.gameObject.transform.position = _realPackMount.position + offset;
        }
    }
}
