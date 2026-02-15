using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class CircuitLockedDoor : CircuitComponent
{
    public bool closeOnLock = false;
    private ConfigurableJoint _joint;

    private void Start()
    {
        _joint = GetComponent<ConfigurableJoint>();

        if (isPowered)
            OnPowerOn();
        else
            OnPowerOff();
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
        float _limit = -transform.localEulerAngles.z;
        if (!closeOnLock)
        {
            _limit = Mathf.Min(-10, -transform.localEulerAngles.z);


            SoftJointLimit softJointLimit = new();
            softJointLimit.limit = _limit;
            _joint.lowAngularXLimit = softJointLimit;

            softJointLimit.limit = _limit + 0.01f;
            _joint.highAngularXLimit = softJointLimit;
        }
    }

    private float _limit = 0f;
    protected override void Update()
    {
        base.Update();
        
        if (closeOnLock && !isPowered)
        {
            _limit = Mathf.Min(-transform.localEulerAngles.z, _limit);
            _limit = Mathf.Lerp(_limit, 0, 12f * Time.deltaTime);

            SoftJointLimit softJointLimit = new();
            softJointLimit.limit = _limit;
            _joint.lowAngularXLimit = softJointLimit;

            softJointLimit.limit = 0f;
            _joint.highAngularXLimit = softJointLimit;
        }
    }
}
