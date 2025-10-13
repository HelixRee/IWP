using UnityEngine;
using UnityEngine.Events;

public class RalphAnimationEventHolder : MonoBehaviour
{
    [SerializeField] private UnityEvent _onJumpStart;
    public void TriggerJump()
    {
        _onJumpStart.Invoke();
    }
}
