using System;
using UnityEngine;

public class RalphArmAnimator : MonoBehaviour
{
    [Serializable]
    public class TransformGroup
    {
        public Transform Anchor;
        public Transform Elbow;
        public Transform End;

        public void DrawAnchorEndLine()
        {
            Gizmos.DrawLine(Anchor.position, End.position);
        }
        public void DrawElbowLine()
        {
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
    public TransformGroup RalphProxy;
    public TransformGroup Ralph;

    // Anchor to End
    private float _scaleRatio = 1.0f;
    private Vector3 _anchorEndDirection = Vector3.zero;
    private float _anchorEndDistance = 0f;

    // Anchor to Elbow
    private float _elbowNormalisedPosition = 0f;
    private Vector3 _elbowDisplacement = Vector3.zero;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float ralphLength = Vector3.Magnitude(RalphProxy.Anchor.position - RalphProxy.End.position);
        float sourceLength = Vector3.Magnitude(Source.Anchor.position - Source.End.position);
        float sourceElbowLength = Vector3.Magnitude(Source.Anchor.position - Source.Elbow.position);

        _scaleRatio = ralphLength / sourceLength;

        _elbowNormalisedPosition = Vector3.Distance(Source.GetElbowPtOnLine(), Source.Anchor.position) / sourceLength;
    }

    // Update is called once per frame
    void Update()
    {

        // Calculate direction and position of end
        _anchorEndDirection = (Source.End.position - Source.Anchor.position).normalized;
        Vector3 anchorElbowDirection = (Source.Elbow.position - Source.Anchor.position).normalized;
        _anchorEndDistance = (Source.End.position - Source.Anchor.position).magnitude * _scaleRatio;

        RalphProxy.End.position = RalphProxy.Anchor.position + _anchorEndDirection * _anchorEndDistance;

        // Calculate direction and position of elbow
        _elbowNormalisedPosition = Vector3.Distance(Source.GetElbowPtOnLine(), Source.Anchor.position) / (Source.End.position - Source.Anchor.position).magnitude;

        _elbowDisplacement = Source.GetElbowDisplacement() * _scaleRatio;

        RalphProxy.Elbow.position = RalphProxy.Anchor.position + _anchorEndDirection * _anchorEndDistance * _elbowNormalisedPosition + _elbowDisplacement;

        float upperArmLength = Vector3.Distance(Ralph.Anchor.position, Ralph.Elbow.position);
        float lowerArmLength = Vector3.Distance(Ralph.Elbow.position, Ralph.End.position);

        Vector3 armPlaneNormal = Vector3.Cross(_anchorEndDirection, anchorElbowDirection);
        Ralph.Anchor.forward = armPlaneNormal;
        Vector3 angles = Ralph.Anchor.localEulerAngles;
        angles.z = 0;
        Ralph.Anchor.localEulerAngles = angles;
        Vector3 targetDisp = RalphProxy.End.position - RalphProxy.Anchor.position;
        float x = Vector3.Dot(targetDisp, Ralph.Anchor.right);
        float y = Vector3.Dot(targetDisp, Ralph.Anchor.up);
        //Debug.Log("X: " + x + ", Y:" + y);
        IKHelper.CalcIK_2D_TwoBoneAnalytic(out float angle1, out float angle2, true, upperArmLength, lowerArmLength, y, x);
        Debug.Log(angle1 * Mathf.Rad2Deg + ", " + angle2 * Mathf.Rad2Deg);

        angles = Ralph.Anchor.localEulerAngles;
        angles.z = -angle1 * Mathf.Rad2Deg;
        Ralph.Anchor.localEulerAngles = angles;

        angles = Ralph.Elbow.localEulerAngles;
        angles.z = -angle2 * Mathf.Rad2Deg + 90;
        Ralph.Elbow.localEulerAngles = angles;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Source.DrawAnchorEndLine();
        Source.DrawElbowLine();
        RalphProxy.DrawAnchorEndLine();
        //RalphProxy.DrawElbowLine();
        Ralph.DrawElbowLine();

    }


}
