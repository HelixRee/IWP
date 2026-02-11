using UnityEngine;

public class CircuitComponent : MonoBehaviour
{
    public bool isPowered = false;
    public bool _wasPowered = false;

    protected virtual void Update()
    {
        if (isPowered != _wasPowered)
        {
            if (isPowered)
                OnPowerOn();
            else
                OnPowerOff();

            OnPowerChange();
            _wasPowered = isPowered;
        }
    }

    protected virtual void OnPowerOn() {}

    protected virtual void OnPowerOff() {}

    protected virtual void OnPowerChange() {}
}
