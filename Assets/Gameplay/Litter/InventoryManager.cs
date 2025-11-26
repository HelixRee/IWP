using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Transform _realPackMount;
    [SerializeField] private Transform _simulatedPackMount;

    private float _prevPackMountY;

    private List<(GameObject simulatedObject, GameObject realObject)> objectPairs = new();
    private void Start()
    {
        _prevPackMountY = _realPackMount.transform.position.y;
    }
    public GameObject CreateObject(GameObject litterObject)
    {
        LitterFlightBehaviour flightScript = litterObject.GetComponent<LitterFlightBehaviour>();

        GameObject simObject = Instantiate(litterObject, transform);
        objectPairs.Add((simObject, litterObject));

        Rigidbody rb = simObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        Vector3 offset = litterObject.transform.position - _realPackMount.position;

        simObject.transform.position = _simulatedPackMount.position + offset;

        return simObject;
    }

    private void LateUpdate()
    {
        float verticalVel = (_realPackMount.transform.position.y - _prevPackMountY) / Time.deltaTime;
        foreach (var objectPair in objectPairs) 
        {
            objectPair.simulatedObject.GetComponent<Rigidbody>().AddForce(Vector3.up *  -verticalVel * 0.1f, ForceMode.Force);
        }


        _prevPackMountY = _realPackMount.transform.position.y;
        foreach (var objectPair in objectPairs)
        {
            Vector3 offset = objectPair.simulatedObject.transform.position - _simulatedPackMount.position;
            objectPair.realObject.transform.position = _realPackMount.position + offset;
        }
    }
}
