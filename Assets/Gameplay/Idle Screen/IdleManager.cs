using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class IdleManager : MonoBehaviour
{
    public bool idleState = true;
    public bool prevIdleState = true;
    public float idleTimeout = 60f;
    public float idleTimeoutTimer = 0f;

    CanvasGroup group;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        group = GetComponent<CanvasGroup>();
        InputSystem.onEvent += OnInputEvent;
    }
    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (prevIdleState)
            OnStopIdle();
        idleState = false;
        prevIdleState = false;
        idleTimeoutTimer = 0f;
    }
    // Update is called once per frame
    void Update()
    {
        if (idleState)
        {
            group.alpha = Mathf.Lerp(group.alpha, 1f, 12f * Time.deltaTime);
        }
        else
        {
            group.alpha = Mathf.Lerp(group.alpha, 0f, 12f * Time.deltaTime);
        }

        idleTimeoutTimer += Time.deltaTime;

        if (idleTimeoutTimer > idleTimeout)
        {
            if (!prevIdleState)
                OnBecomeIdle();
            idleState = true;
            prevIdleState = true;
        }
    }

    private void OnBecomeIdle()
    {
        if (!RespawnManager.Instance._ragdollStarted)
        {
            RespawnManager.Instance.ForceRagdoll();
        }
        RespawnManager.Instance.enabled = false;
    }

    private void OnStopIdle()
    {
        RespawnManager.Instance.enabled = true;

    }
}
