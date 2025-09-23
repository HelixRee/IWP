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
    public List<RalphAnimator> updateOrder = new();

    public Armature Source;
    public Armature Ralph;

    private float _scaleRatio = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _scaleRatio = Ralph.GetPelvisOffset().magnitude / Source.GetPelvisOffset().magnitude;
        Ralph.CaptureInitialOffset();

        // Initalise child scripts
        updateOrder.ForEach(item => item.GroundLayers = GroundLayers);
        updateOrder.ForEach(item => item.ManualInit());
    }

    void LateUpdate()
    {
        UpdateRootMotion();


        // Update child scripts
        updateOrder.ForEach(item => { if (item.enabled) item.ManualUpdate(); });
    }

    void UpdateRootMotion()
    {
        Ralph.SetPelvisOffset(Source.GetPelvisOffset() * _scaleRatio);
        //Ralph.Pelvis.rotation = Source.Pelvis.rotation * Ralph._pelvisRotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        updateOrder.ForEach(item => Gizmos.DrawLine(item.transform.position, Ralph.Pelvis.position));
    }
}
