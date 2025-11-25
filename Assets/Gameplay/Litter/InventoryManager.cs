using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Transform _realPackMount;
    [SerializeField] private Transform _simulatedPackMount;

    private List<(GameObject simulatedObject, GameObject realObject)> objectPairs = new();
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
        foreach (var objectPair in objectPairs)
        {
            Vector3 offset = objectPair.simulatedObject.transform.position - _simulatedPackMount.position;
            objectPair.realObject.transform.position = _realPackMount.position + offset;
        }
    }
}
