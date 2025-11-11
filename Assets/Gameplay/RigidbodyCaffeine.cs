using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyCaffeine : MonoBehaviour
{
    private Rigidbody _rb;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (_rb.IsSleeping())
        {
            Debug.Log("WAKE UP");
            _rb.WakeUp();
        }
    }
}
