using UnityEngine;

public abstract class BaseRalphAnimator : MonoBehaviour
{
    public LayerMask GroundLayers;
    public abstract void ManualInit();
    public abstract void ManualUpdate();
}
