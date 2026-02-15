using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Transform _realPackMount;
    [SerializeField] private Transform _simulatedPackMount;
    [SerializeField] private Transform _kinematicProxy;
    [SerializeField] private BoxCollider _boxCast;
    [SerializeField] private LayerMask _litterLayer;

    private float _prevPackMountY;
    private Dictionary<GameObject, LitterFlightBehaviour> _litterBehaviours = new();
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
            //DontDestroyOnLoad(gameObject); // Optional: Persist across scene loads
        }
    }
    private void Start()
    {
        _prevPackMountY = _realPackMount.transform.position.y;
    }
    private void Update()
    {
        _kinematicProxy.transform.rotation = _realPackMount.transform.rotation;
    }

    public void SetRefPoint(Transform refPoint)
    {
        _realPackMount = refPoint;
    }
    public GameObject CreateLitterObject(LitterFlightBehaviour litterScript)
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
        if (!_litterBehaviours.ContainsKey(simObject)) return;
        LitterFlightBehaviour litterScript = _litterBehaviours[simObject];
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

    public void RemoveAllLitterObjects()
    {
        foreach (var litterBehaviour in _litterBehaviours)
        {
            StartCoroutine(WaitAndRemove(litterBehaviour.Key));
        }
    }

    private IEnumerator WaitAndRemove(GameObject simObject)
    {
        yield return null;
        RemoveLitterSimObject(simObject);
    }
    
    public LitterFlightBehaviour RemoveTopLitterObject()
    {
        RaycastHit hit;
        if (!Physics.BoxCast(_boxCast.center + _boxCast.transform.position, _boxCast.bounds.extents / 2f, _boxCast.transform.forward, out hit, _boxCast.transform.rotation, 10f, _litterLayer))
            return null;
        LitterFlightBehaviour litterScript = _litterBehaviours[hit.collider.gameObject];
        RemoveLitterSimObject(hit.collider.gameObject, false);
        return litterScript;
    }

    public IEnumerator WaitAndEnable(LitterFlightBehaviour litterScript)
    {
        yield return new WaitForSeconds(0.7f);
        litterScript.isAsleep = false;
    }

    private void LateUpdate()
    {
        float verticalVel = (_realPackMount.transform.position.y - _prevPackMountY) / Time.deltaTime;
        foreach (var litterScript in _litterBehaviours)
        {
            litterScript.Value.simulatedObject.GetComponent<Rigidbody>().AddForce(Vector3.up * -verticalVel * 0.2f, ForceMode.Force);
        }


        _prevPackMountY = _realPackMount.transform.position.y;
        foreach (var litterScript in _litterBehaviours)
        {
            Vector3 offset = litterScript.Value.simulatedObject.transform.position - _simulatedPackMount.position;
            litterScript.Value.gameObject.transform.position = _realPackMount.position + offset;
            litterScript.Value.gameObject.transform.rotation = litterScript.Value.simulatedObject.transform.rotation;
        }
    }
}
