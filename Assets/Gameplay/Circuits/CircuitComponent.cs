using UnityEngine;

public class CircuitComponent : MonoBehaviour
{
    public bool isInverted = false;
    private bool _wasInverted = false;
    public bool isPowered 
    {
        get
        {
            return _isPowered != isInverted;
        }
        set
        {
            _isPowered = value;
        } 
    }
    private bool _isPowered = false;
    public bool _wasPowered = false;

    protected virtual void Update()
    {
        if (_isPowered != _wasPowered || isInverted != _wasInverted)
        {
            if (_isPowered != isInverted)
                OnPowerOn();
            else
                OnPowerOff();

            OnPowerChange();

            _wasPowered = _isPowered;
            _wasInverted = isInverted;
        }
    }

    protected virtual void OnPowerOn() {}

    protected virtual void OnPowerOff() {}

    protected virtual void OnPowerChange() {}
}
