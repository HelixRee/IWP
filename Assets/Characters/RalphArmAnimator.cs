using System;
using UnityEngine;

public class RalphArmAnimator : RalphAnimator
{
    [Serializable]
    public class TransformGroup
    {
        public Transform Anchor;
        public Transform Elbow;
        public Transform End;

        public void DrawAnchorEndLine()
        {
            if (!Anchor || !End) return;
            Gizmos.DrawLine(Anchor.position, End.position);
        }
        public void DrawElbowLine()
        {
            if (!Anchor || !End || !Elbow) return;

            Vector3 pntOnLine = NearestPointOnLine(Anchor.position, End.position - Anchor.position, Elbow.position);
            Gizmos.DrawLine(pntOnLine, Elbow.position);
        }
        public Vector3 GetElbowDisplacement()
        {
            Vector3 pntOnLine = NearestPointOnLine(Anchor.position, End.position - Anchor.position, Elbow.position);
            return Elbow.position - pntOnLine;
        }
        public Vector3 GetElbowPtOnLine()
        {
            return NearestPointOnLine(Anchor.position, End.position - Anchor.position, Elbow.position);
        }
        Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
        {
            lineDir.Normalize();//this needs to be a unit vector
            var v = pnt - linePnt;
            var d = Vector3.Dot(v, lineDir);
            return linePnt + lineDir * d;
        }
    }

    [Header("Source")]
    public TransformGroup Source;
    public Transform SourceHands;
    [Header("Ralph")]
    public TransformGroup RalphProxy;
    public TransformGroup Ralph;
    public Transform RalphHands;

    // Anchor to End
    private float _scaleRatio = 1.0f;
    private Vector3 _sourceAnchorToEndDir = Vector3.zero;
    private float _ralphAnchorToEndDist = 0f;

    // Anchor to Elbow
    private float _elbowNormalisedPosition = 0f;
    private Vector3 _elbowDisplacement = Vector3.zero;


    public override void ManualInit()
    {
        float ralphLength = Vector3.Magnitude(RalphProxy.Anchor.position - RalphProxy.End.position);
        float sourceLength = Vector3.Magnitude(Source.Anchor.position - Source.End.position);
        float sourceElbowLength = Vector3.Magnitude(Source.Anchor.position - Source.Elbow.position);

        _scaleRatio = ralphLength / sourceLength;

        _elbowNormalisedPosition = Vector3.Distance(Source.GetElbowPtOnLine(), Source.Anchor.position) / sourceLength;
    }

    public override void ManualUpdate()
    {
        // Calculate direction and position of end
        _sourceAnchorToEndDir = (Source.End.position - Source.Anchor.position).normalized;
        Vector3 sourceAnchorToElbowDir = (Source.Elbow.position - Source.Anchor.position).normalized;
        _ralphAnchorToEndDist = (Source.End.position - Source.Anchor.position).magnitude * _scaleRatio;

        // Set proxy end position to match animation
        RalphProxy.End.position = RalphProxy.Anchor.position + _sourceAnchorToEndDir * _ralphAnchorToEndDist;

        float upperArmLength = Vector3.Distance(Ralph.Anchor.position, Ralph.Elbow.position);
        float lowerArmLength = Vector3.Distance(Ralph.Elbow.position, Ralph.End.position);

        Vector3 armPlaneNormal = Vector3.Cross(_sourceAnchorToEndDir, sourceAnchorToElbowDir);
        Ralph.Anchor.forward = armPlaneNormal;
        SetZRotation(Ralph.Anchor, 0);

        Vector3 targetDisp = RalphProxy.End.position - RalphProxy.Anchor.position;
        float x = Vector3.Dot(targetDisp, Ralph.Anchor.up);
        float y = Vector3.Dot(targetDisp, Ralph.Anchor.right);

        IKHelper.CalcIK_2D_TwoBoneAnalytic(out float angle1, out float angle2, true, upperArmLength, lowerArmLength, x, y);

        // Abstract function
        SetZRotation(Ralph.Anchor, -angle1 * Mathf.Rad2Deg);
        SetZRotation(Ralph.Elbow, -angle2 * Mathf.Rad2Deg);

        Ralph.End.rotation = SourceHands.rotation;
        Vector3 angles = Ralph.End.localEulerAngles;
        angles.x = angles.z = 0;
        Ralph.End.localEulerAngles = angles;
    }
    private void SetYRotation(Transform transform, float rotation)
    {
        Vector3 angles = transform.localEulerAngles;
        angles.y = rotation;
        transform.localEulerAngles = angles;
    }
    private void SetZRotation(Transform transform, float rotation)
    {
        Vector3 angles = transform.localEulerAngles;
        angles.z = rotation;
        transform.localEulerAngles = angles;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Source.DrawAnchorEndLine();
        Source.DrawElbowLine();
        Ralph.DrawAnchorEndLine();
        Ralph.DrawElbowLine();
    }


}
