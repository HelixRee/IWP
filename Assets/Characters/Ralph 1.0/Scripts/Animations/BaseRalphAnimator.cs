using UnityEngine;

public abstract class BaseRalphAnimator : MonoBehaviour
{
    [HideInInspector]
    public LayerMask GroundLayers;
    public bool IsGrounded = false;
    public bool IsFalling = false;
    public bool UseGravity = false;
    public abstract void ManualInit();
    public abstract void ManualUpdate();
}
