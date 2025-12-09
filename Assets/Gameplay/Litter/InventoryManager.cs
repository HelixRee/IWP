using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Transform _realPackMount;
    [SerializeField] private Transform _simulatedPackMount;

    private float _prevPackMountY;
    private Dictionary<GameObject, LitterBehaviour> litterBehaviours = new();

    private void Start()
    {
        _prevPackMountY = _realPackMount.transform.position.y;
    }
    public GameObject CreateLitterObject(LitterBehaviour litterScript)
    {
        GameObject simObject = Instantiate(litterScript.gameObject, transform);
        litterBehaviours.Add(simObject, litterScript);
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

    public void RemoveLitterSimObject(GameObject simObject)
    {
        LitterBehaviour litterScript = litterBehaviours[simObject];
        litterBehaviours.Remove(simObject);

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
        IEnumerator coroutine;
        coroutine = WaitAndEnable(litterScript);
        StartCoroutine(coroutine);
    }

    private IEnumerator WaitAndEnable(LitterBehaviour litterScript)
    {
        yield return new WaitForSeconds(2);
        litterScript.isAsleep = false;
    }

    private void LateUpdate()
    {
        float verticalVel = (_realPackMount.transform.position.y - _prevPackMountY) / Time.deltaTime;
        foreach (var litterScript in litterBehaviours)
        {
            litterScript.Value.simulatedObject.GetComponent<Rigidbody>().AddForce(Vector3.up * -verticalVel * 0.1f, ForceMode.Force);
        }


        _prevPackMountY = _realPackMount.transform.position.y;
        foreach (var litterScript in litterBehaviours)
        {
            Vector3 offset = litterScript.Value.simulatedObject.transform.position - _simulatedPackMount.position;
            litterScript.Value.gameObject.transform.position = _realPackMount.position + offset;
        }
    }
}
