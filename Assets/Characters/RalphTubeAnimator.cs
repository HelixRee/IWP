using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class RalphTubeAnimator : BaseRalphAnimator
{
    public Transform ControlPointL;
    public Transform ControlPointR;

    public List<BaseRalphAnimator> childAnimations = new();

    private BezierKnot _leftKnot;
    private BezierKnot _rightKnot;

    private SplineContainer _splineContainer;
    private Spline _spline;

    [Serializable]
    public class ReferencePoint
    {
        [Range(0f, 1f)]
        public float normalisedDistance = 0f;
        public Transform point;

        [HideInInspector] public Transform wrapper;
        [HideInInspector] public Vector3 positionOffset = Vector3.zero;
        [HideInInspector] public Vector3 rotationOffset = Vector3.zero;
    }

    public List<ReferencePoint> ReferencePoints = new();

    private void OnValidate()
    {
        _splineContainer = GetComponent<SplineContainer>();
        _spline = _splineContainer.Splines[0];
    }

    public override void ManualInit()
    {
        _rightKnot = _spline[1];
        _leftKnot = _spline[2];

        foreach (var anim in childAnimations)
            anim.ManualInit();

        foreach (var rp in ReferencePoints)
        {
            GameObject wrapper = new GameObject(rp.point.name + " Wrapper");
            rp.wrapper = wrapper.transform;
            wrapper.transform.parent = rp.point.parent;
            wrapper.transform.SetSiblingIndex(rp.point.GetSiblingIndex());


            rp.wrapper.position = transform.TransformPoint(_spline.EvaluatePosition(rp.normalisedDistance));
        }
        foreach (var rp in ReferencePoints)
        {


            float offset = rp.normalisedDistance + 0.1f >= 1f ? -0.1f : 0.1f;
            //rp.wrapper.up = transform.TransformPoint(_spline.EvaluatePosition(rp.normalisedDistance + offset)) - rp.wrapper.position;
            rp.wrapper.LookAt(transform.TransformPoint(_spline.EvaluatePosition(rp.normalisedDistance + offset)), transform.right);

            rp.point.SetParent(rp.wrapper, true);
            rp.point.localPosition = Vector3.zero;

        }
    }

    public bool test = false;
    public override void ManualUpdate()
    {
        foreach (var anim in childAnimations)
            anim.ManualUpdate();

        _rightKnot.Position = transform.InverseTransformPoint(ControlPointR.position);
        _leftKnot.Position = transform.InverseTransformPoint(ControlPointL.position);

        _spline.SetKnot(1, _rightKnot);
        _spline.SetKnot(2, _leftKnot);

        foreach (var rp in ReferencePoints)
        {
            rp.wrapper.position = transform.TransformPoint(_spline.EvaluatePosition(rp.normalisedDistance));
            rp.point.localPosition = Vector3.zero;

        }
        if (Input.GetKeyDown(KeyCode.F1)) test = !test;

        for (int i = 0; i < ReferencePoints.Count; i++)
        {
            ReferencePoint rp = ReferencePoints[i];
            float offset = rp.normalisedDistance + 0.1f >= 1f ? -0.1f : 0.1f;
            //rp.wrapper.up = transform.TransformPoint(_spline.EvaluatePosition(rp.normalisedDistance + offset)) - rp.wrapper.position;
            rp.wrapper.LookAt(transform.TransformPoint(_spline.EvaluatePosition(rp.normalisedDistance + offset)), transform.right);

            //transform.Rotate(Vector3.up, 90);

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        foreach (var item in ReferencePoints)
        {
            Gizmos.DrawSphere(transform.TransformPoint(_spline.EvaluatePosition(item.normalisedDistance)), 0.01f);
        }
    }
}
