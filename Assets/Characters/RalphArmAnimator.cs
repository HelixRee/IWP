using System;
using UnityEngine;

public class RalphArmAnimator : BaseRalphAnimator
{
    [Serializable]
    public class TransformGroup
    {
        public Transform Anchor;
        public Transform Elbow;
        public Transform End;

        public void DrawArmature()
        {
            if (Anchor && Elbow)
                Gizmos.DrawLine(Anchor.position, Elbow.position);
            if (Elbow && End)
                Gizmos.DrawLine(Elbow.position, End.position);
        }
        public void DrawConstructionLines()
        {
            DrawAnchorEndLine();
            DrawElbowLine();
        }
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
    [Header("Ralph")]
    public Vector3 handTarget;
    public TransformGroup Ralph;

    // Anchor to End
    private float _scaleRatio = 1.0f;
    private Vector3 _sourceAnchorToEndDir = Vector3.zero;
    private float _ralphAnchorToEndDist = 0f;

    // Arm lengths
    float _upperArmLength = 0f;
    float _lowerArmLength = 0f;

    // Target within range
    bool _validTarget = true;
    public override void ManualInit()
    {
        float ralphLength = Vector3.Magnitude(Ralph.Anchor.position - Ralph.End.position);
        float sourceLength = Vector3.Magnitude(Source.Anchor.position - Source.End.position);

        // Measure arm lengths
        _upperArmLength = Vector3.Distance(Ralph.Anchor.position, Ralph.Elbow.position);
        _lowerArmLength = Vector3.Distance(Ralph.Elbow.position, Ralph.End.position);

        _scaleRatio = ralphLength / sourceLength;
    }

    public override void ManualUpdate()
    {
        // Calculate direction and position of end
        _sourceAnchorToEndDir = (Source.End.position - Source.Anchor.position).normalized;
        Vector3 sourceAnchorToElbowDir = (Source.Elbow.position - Source.Anchor.position).normalized;
        _ralphAnchorToEndDist = (Source.End.position - Source.Anchor.position).magnitude * _scaleRatio;

        // Set proxy end position to match animation
        handTarget = Ralph.Anchor.position + _sourceAnchorToEndDir * _ralphAnchorToEndDist;

        // Find arm rotation plane
        Vector3 armPlaneNormal = Vector3.Cross(_sourceAnchorToEndDir, sourceAnchorToElbowDir);
        if (armPlaneNormal != Vector3.zero)
            Ralph.Anchor.forward = armPlaneNormal;
        SetZRotation(Ralph.Anchor, 0);

        // Measure displacements on plane
        Vector3 targetDisp = handTarget - Ralph.Anchor.position;
        float x = Vector3.Dot(targetDisp, Ralph.Anchor.up);
        float y = Vector3.Dot(targetDisp, Ralph.Anchor.right);

        _validTarget = IKHelper.CalcIK_2D_TwoBoneAnalytic(out float angle1, out float angle2, true, _upperArmLength, _lowerArmLength, x, y);

        // Set joint rotations on plane
        SetZRotation(Ralph.Anchor, -angle1 * Mathf.Rad2Deg);
        SetZRotation(Ralph.Elbow, -angle2 * Mathf.Rad2Deg);

        // Set hand rotation in Y axis only
        Ralph.End.rotation = Source.End.rotation;
        Vector3 angles = Ralph.End.localEulerAngles;
        angles.x = angles.z = 0;
        Ralph.End.localEulerAngles = angles;
    }

    private void SetZRotation(Transform transform, float rotation)
    {
        Vector3 angles = transform.localEulerAngles;
        angles.z = rotation;
        transform.localEulerAngles = angles;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _validTarget ? Color.yellow : Color.red;
        Source.DrawConstructionLines();
        Ralph.DrawConstructionLines();

        Gizmos.color = Color.green;
        Source.DrawArmature();
        Ralph.DrawArmature();

        Gizmos.color = _validTarget ? new Color(0f,1f,0f,0.5f) : new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(handTarget, _lowerArmLength * 0.1f);
    }


}
