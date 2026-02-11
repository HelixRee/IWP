using System.Collections.Generic;
using UnityEngine;

public class BatterySocket : MonoBehaviour
{
    [SerializeField] private Transform _attachTransform;
    [SerializeField] private List<CircuitComponent> _circuitGroup = new();
    private Battery _attachedBattery;
    private bool _isPowered = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Battery battery))
            return;

        AttachBattery(battery);
    }

    private void AttachBattery(Battery battery)
    {
        battery.tag = "Untagged";
        _attachedBattery = battery;
        battery.GetComponent<Collider>().enabled = false;
        battery.GetComponent<Rigidbody>().isKinematic = true;

        _isPowered = true;
    }
    private void DetachBattery()
    {
        _attachedBattery.tag = "Litter";
        _attachedBattery.GetComponent<Collider>().enabled = true;
        _attachedBattery.GetComponent<Rigidbody>().isKinematic = false;

        _attachedBattery = null;
        _isPowered = false;
    }
    private void Update()
    {
        _circuitGroup.ForEach(comp => comp.isPowered = _isPowered);
        if (_attachedBattery == null) return;

        _attachedBattery.transform.position = Vector3.Lerp(_attachedBattery.transform.position, _attachTransform.position, 12f * Time.deltaTime);
        _attachedBattery.transform.rotation = Quaternion.Slerp(_attachedBattery.transform.rotation, _attachTransform.rotation, 12f * Time.deltaTime);
    }
}
