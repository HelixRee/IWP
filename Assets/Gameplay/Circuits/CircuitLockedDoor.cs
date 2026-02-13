using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class CircuitLockedDoor : CircuitComponent
{
    private ConfigurableJoint _joint;

    private void Start()
    {
        _joint = GetComponent<ConfigurableJoint>();
    }

    protected override void OnPowerOn()
    {
        base.OnPowerOn();
        SoftJointLimit softJointLimit = new();
        softJointLimit.limit = -90;
        _joint.lowAngularXLimit = softJointLimit;

        softJointLimit.limit = 0;
        _joint.highAngularXLimit = softJointLimit;

        //_joint.GetComponent<Rigidbody>().AddRelativeTorque(10000f,0,0,ForceMode.Impulse);
        _joint.GetComponent<Rigidbody>().AddForceAtPosition(-transform.up * 2f, transform.position - transform.right, ForceMode.Impulse);
    }

    protected override void OnPowerOff()
    {
        base.OnPowerOff();
        SoftJointLimit softJointLimit = new();
        softJointLimit.limit = Mathf.Min(-10, -transform.localEulerAngles.z);
        _joint.lowAngularXLimit = softJointLimit;

        softJointLimit.limit = Mathf.Min(-10, -transform.localEulerAngles.z) + 0.01f;
        _joint.highAngularXLimit = softJointLimit;
    }
}
