using System.Collections.Generic;
using UnityEngine;

public class BatterySocket : MonoBehaviour
{
    [SerializeField] private Transform _attachTransform;
    [SerializeField] private List<CircuitComponent> _circuitGroup = new();
    [SerializeField] private GameObject _UI;

    private Battery _attachedBattery;
    private bool _isPowered = false;
    private bool _socketActive = true;
    private void Start()
    {
        _UI.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!_socketActive) return;
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
    public void DetachBattery()
    {
        if (_attachedBattery == null) return;

        _attachedBattery.tag = "Litter";
        _attachedBattery.GetComponent<Collider>().enabled = true;
        _attachedBattery.GetComponent<Rigidbody>().isKinematic = false;

        _attachedBattery = null;
        _isPowered = false;
        _socketActive = false;
        StartCoroutine(WaitAndReenable());
    }

    // Change to blacklist system eventually
    // minecraft nether portal logic
    private System.Collections.IEnumerator WaitAndReenable()
    {
        yield return new WaitForSeconds(1f);
        _socketActive = true;
    }

    public void RefreshUI()
    {
        if (_attachedBattery == null)
            _UI.SetActive(false);
        else
            _UI.SetActive(true);
    }

    public void DisableUI()
    {
        _UI.SetActive(false);
    }
    private void Update()
    {
        _circuitGroup.ForEach(comp => comp.isPowered = _isPowered);
        if (_attachedBattery == null) return;

        _attachedBattery.transform.position = Vector3.Lerp(_attachedBattery.transform.position, _attachTransform.position, 12f * Time.deltaTime);
        _attachedBattery.transform.rotation = Quaternion.Slerp(_attachedBattery.transform.rotation, _attachTransform.rotation, 12f * Time.deltaTime);
    }
}
