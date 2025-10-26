using UnityEngine;
using System.Collections.Generic;

public class RalphIdleAnimator : MonoBehaviour
{
    public List<BaseRalphAnimator> updateOrder = new();
    private void Start()
    {
        updateOrder.ForEach(item => item.ManualInit());
        updateOrder.ForEach(item => item.UseGravity = true);
    }
    void LateUpdate()
    {
        // Update child scripts
        updateOrder.ForEach(item => { if (item.enabled) item.ManualUpdate(); });
    }
}
