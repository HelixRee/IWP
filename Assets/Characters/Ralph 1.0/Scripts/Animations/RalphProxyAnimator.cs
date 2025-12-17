using System;
using System.Collections.Generic;
using UnityEngine;

public class RalphProxyAnimator : MonoBehaviour
{
    [Serializable]
    public class Armature
    {
        public Transform Root;
        public Transform Pelvis;

        [HideInInspector]
        public Quaternion _pelvisRotation;
        public void CaptureInitialOffset()
        {
            _pelvisRotation = Pelvis.rotation;
        }
        public Vector3 GetPelvisOffset()
        {
            return Pelvis.position - Root.position;
        }

        public void SetPelvisOffset(Vector3 offset)
        {
            Pelvis.position = Root.position + offset;
        }
    }
    public LayerMask GroundLayers;
    public bool IsGrounded = true;
    public bool IsFalling = true;
    public bool IsAiming = false;
    public List<BaseRalphAnimator> updateOrder = new();

    public Armature Source;
    public Armature Ralph;

    [Header("Hip Animation")]
    public FollowObject HipFollower;
    public Transform RealHips;

    [Header("Leg Animators")]
    public List<RalphLegAnimator> LegAnimators = new();
    [Header("Arm Animators")]
    public List<RalphArmAnimator> ArmAnimators = new();
    private float _scaleRatio = 1f;
    private Vector3 _aimDirection = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HipFollower.ManualInit();
        _scaleRatio = Ralph.GetPelvisOffset().magnitude / Source.GetPelvisOffset().magnitude;
        Ralph.CaptureInitialOffset();

        // Initalise child scripts
        updateOrder.ForEach(item => item.GroundLayers = GroundLayers);
        updateOrder.ForEach(item => item.ManualInit());
    }
    
    void LateUpdate()
    {
        HipFollower.ManualUpdate();
        UpdateRootMotion();


        // Update child scripts
        ArmAnimators.ForEach(item => item.throwDirection = _aimDirection);
        ArmAnimators.ForEach(item => item.isAiming = IsAiming);
        updateOrder.ForEach(item => { item.IsGrounded = IsGrounded; item.IsFalling = IsFalling; });
        updateOrder.ForEach(item => { if (item.enabled) item.ManualUpdate(); });
    }

    void UpdateRootMotion()
    {
        Source.Pelvis.localPosition = Vector3.ClampMagnitude(Source.Pelvis.localPosition, 1f);
        Ralph.SetPelvisOffset(Source.GetPelvisOffset() * _scaleRatio);

        Vector3 clampedPosition = HipFollower.Target.position;
        Vector3 disp = HipFollower.transform.position - HipFollower.Target.position;
        clampedPosition += Vector3.Dot(Vector3.up, disp) * Vector3.up;

        RealHips.position = clampedPosition;
        //Ralph.Pelvis.rotation = Source.Pelvis.rotation * Ralph._pelvisRotation;
    }
    public void SetAimDirection(Vector3 aimDir)
    {
        _aimDirection = aimDir;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var item in updateOrder)
        {
            if (!item.enabled) continue;

            if (item.GetType() == typeof(FollowObject))
            {
                FollowObject followObject = item as FollowObject;
                DrawGizmoToParent(followObject.Target);
            }
            else
            {
                DrawGizmoToParent(item.transform);
            }

        }
    }

    private void DrawGizmoToParent(Transform child)
    {
        if (child.name == "Main") return;
        if (child == null) return;
        if (child.parent == null) return;
        Gizmos.DrawLine(child.position, child.parent.position);
        DrawGizmoToParent(child.parent);
    }
}
