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
        softJointLimit.limit = -10;
        _joint.lowAngularXLimit = softJointLimit;
    }

    protected override void OnPowerOff()
    {
        base.OnPowerOff();
        SoftJointLimit softJointLimit = new();
        softJointLimit.limit = -90;
        _joint.lowAngularXLimit = softJointLimit;
    }
}
