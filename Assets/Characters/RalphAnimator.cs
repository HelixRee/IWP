using UnityEngine;

public abstract class RalphAnimator : MonoBehaviour
{
    public LayerMask GroundLayers;
    public abstract void ManualInit();
    public abstract void ManualUpdate();
}
