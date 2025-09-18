using System;
using Unity.Mathematics;
using UnityEngine;

public class RalphLegAnimator : RalphAnimator
{
    [Serializable]
    public class GenericArmature
    {
        public Transform Hips;
        public Transform UpperLeg;
        public Transform LowerLeg;
        public Transform Foot;
        public Transform Toe;
        public Transform HeelBase; // IK Point

        // Distance toe is lifted above foot [-0.05, 0.05]
        [HideInInspector] public float ToeLift => Vector3.Dot(Foot.position - Toe.position, HeelBase.up) - _initialToeLift; 
        [HideInInspector] public float _initialToeLift = 0f;
        public void Init()
        {
            _initialToeLift = ToeLift;
        }
        public void DrawArmature()
        {
            if (UpperLeg && LowerLeg)
                DrawLine(UpperLeg, LowerLeg);
            if (LowerLeg && Foot)
                DrawLine(LowerLeg, Foot);
            if (Foot && Toe)
                DrawLine(Foot, Toe);
        }
        public void DrawConstructionLines()
        {
            DrawLine(UpperLeg, HeelBase);
            Vector3 pntOnLine = NearestPointOnLine(UpperLeg.position, HeelBase.position - UpperLeg.position, LowerLeg.position);
            Gizmos.DrawLine(pntOnLine, LowerLeg.position);
        }
        public Vector3 GetElbowDisplacement()
        {
            Vector3 pntOnLine = NearestPointOnLine(UpperLeg.position, HeelBase.position - UpperLeg.position, LowerLeg.position);
            return LowerLeg.position - pntOnLine;
        }
    }

    [Serializable]
    public class RalphArmature
    {
        public Transform Hips;
        public Transform Anchor;
        public Transform Connector;
        public Transform UpperLegPitch;
        public Transform UpperLegTilt;
        public Transform LowerLeg;
        public Transform Foot; // IK Point

        // Distance connector is extended below the anchor [-0.015, 0.015]
        public float _connectorExtension;
        public float ConnectorExtension
        {
            get { return _connectorExtension; }
            set
            {
                _connectorExtension = value;
                Connector.position = Anchor.position - Anchor.forward * value;
            }
        }

        public void DrawArmature()
        {
            if (Anchor && Connector)
            {
                Color originalColor = Gizmos.color;
                Gizmos.color = Color.Lerp(Color.green, Color.red, Mathf.Abs(_connectorExtension) / 0.015f);
                DrawLine(Anchor, Connector);
                Gizmos.color = originalColor;
            }
            if (Connector && UpperLegPitch)
                DrawLine(Connector, UpperLegPitch);
            if (UpperLegPitch && LowerLeg)
                DrawLine(UpperLegPitch, LowerLeg);
            if (LowerLeg && Foot)
                DrawLine(LowerLeg, Foot);
        }

        public void DrawConstructionLines()
        {
            if (UpperLegPitch && Foot)
                DrawLine(UpperLegPitch, Foot);
            if (UpperLegPitch && Foot && LowerLeg)
            {
                Vector3 pntOnLine = NearestPointOnLine(UpperLegPitch.position, Foot.position - UpperLegPitch.position, LowerLeg.position);
                Gizmos.DrawLine(pntOnLine, LowerLeg.position);
            }
        }
    }
    static void DrawLine(Transform pos1, Transform pos2)
    {
        Gizmos.DrawLine(pos1.position, pos2.position);
    }
    static float LengthBetween(Transform pos1, Transform pos2)
    {
        return Vector3.Distance(pos1.position, pos2.position);
    }
    static Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
    {
        lineDir.Normalize();//this needs to be a unit vector
        var v = pnt - linePnt;
        var d = Vector3.Dot(v, lineDir);
        return linePnt + lineDir * d;
    }

    [Header("Source")]
    public GenericArmature Source;
    [Header("Ralph")]
    public RalphArmature Ralph;

    private float _sourceLegLength;
    private float _ralphLegLength;
    private float _scaleRatio = 1f;
    public Vector3 TargetOffset = Vector3.zero;
    [HideInInspector] public Vector3 DirectTarget = Vector3.zero;
    [HideInInspector] public Vector3 AdjustedTarget = Vector3.zero;
    public Vector3 ActiveTarget = Vector3.zero;

    // Foot base
    public float footBaseScalar = 1f;
    public float stepDepthScalar = 1f;
    public Vector2 TiltRange = Vector2.zero;

    private Vector3 _raycastStartPos = Vector3.zero;
    private bool _groundDetected = true;

    public override void ManualInit()
    {
        Source.Init();

        _sourceLegLength = LengthBetween(Source.UpperLeg, Source.LowerLeg) + LengthBetween(Source.LowerLeg, Source.HeelBase);
        _ralphLegLength = LengthBetween(Ralph.UpperLegPitch, Ralph.LowerLeg) + LengthBetween(Ralph.LowerLeg, Ralph.Foot);

        _scaleRatio = _ralphLegLength / _sourceLegLength;
    }

    public override void ManualUpdate()
    {
        // Connector movement
        float remappedToeLift = math.remap(-0.05f, 0.05f, -0.015f, 0.015f, Source.ToeLift);
        Ralph.ConnectorExtension = remappedToeLift;

        // Ground targeting
        Vector3 sourceDisp = Source.HeelBase.position - Source.UpperLeg.position;
        
        // Scale displacement in the correct direction
        Vector3 targetDisp = Vector3.zero;
        targetDisp += _scaleRatio * Vector3.Dot(sourceDisp, Ralph.Anchor.up) * Ralph.Anchor.up;
        targetDisp += footBaseScalar * Vector3.Dot(sourceDisp, Ralph.Anchor.forward) * Ralph.Anchor.forward;
        targetDisp += stepDepthScalar * Vector3.Dot(sourceDisp, Ralph.Anchor.right) * Ralph.Anchor.right;

        DirectTarget = targetDisp + Ralph.Anchor.position + Ralph.Anchor.rotation * TargetOffset;
        _raycastStartPos = DirectTarget + Ralph.Anchor.forward * _ralphLegLength / 2;
        _groundDetected = Physics.Raycast(_raycastStartPos, -Ralph.Anchor.forward, out RaycastHit hitInfo, _ralphLegLength / 2);
        AdjustedTarget = hitInfo.point;


        ActiveTarget = _groundDetected ? AdjustedTarget : DirectTarget;

        // IK Logic
        CalculateTilt();
        //Debug.Log(CalculateZOffset());
        // Yaw
        CalculateYaw();
        // Find components in local space
        //SetZRotation(Ralph.Connector, 0);
        //float xOffset = Vector3.Dot(Ralph.UpperLegTilt.up, -Ralph.Connector.up);
        //float yOffset = Vector3.Dot(Ralph.UpperLegTilt.up, Ralph.Connector.right);

        //float yawOffset = Vector2.SignedAngle(Vector2.up, new Vector2(yOffset, xOffset));

        //// (- Ralph.Anchor.rotation * Vector3.up)
        //// change to 
        //{
        //    Vector3 connectorToTarget = Ralph.Connector.position - ActiveTarget;
        //    float x = Vector3.Dot(connectorToTarget, -Ralph.Connector.up);
        //    float y = Vector3.Dot(connectorToTarget, Ralph.Connector.right);
        //    float yaw = -Vector2.SignedAngle(Vector2.down, new Vector2(y, x));

        //    //yaw += yawOffset;

        //    if (yaw > 90f) yaw = yaw - 180f;
        //    if (yaw < -90f) yaw = 180f + yaw;
        //    //SetZRotation(Ralph.Connector, yaw);
        //    //Debug.Log(yaw);

        //}
        // Calculate yaw


        // Bone Solver
        //float weebus = GetRotationAroundAxis(Quaternion.AngleAxis(excessTilt, Ralph.UpperLegTilt.right), Ralph.Connector.forward);
        //Debug.Log(weebus);

        //Debug.Log(name + ", Disp: " + disp + ", Tilt: " + tiltAngle);
        //Debug.Log(yaw);
    }
    private float excessTilt = 0f;
    float GetRotationAroundAxis(Quaternion q, Vector3 axis)
    {
        // Rotate the axis by the quaternion
        Vector3 rotated = q * axis;

        // Get the angle between original and rotated
        float angle = Vector3.SignedAngle(axis, rotated, axis);

        return angle;
    }
    private void CalculateTilt()
    {
        // Tilt
        // Reset frame of reference
        SetXRotation(Ralph.UpperLegTilt, 0);

        Vector3 tiltToTarget = Ralph.UpperLegTilt.position - ActiveTarget;
        float x = Vector3.Dot(tiltToTarget, Ralph.UpperLegTilt.forward);
        float y = Vector3.Dot(tiltToTarget, -Ralph.UpperLegTilt.up);
        float tiltAngle = Vector2.SignedAngle(Vector2.up, new Vector2(x, y));
        excessTilt = tiltAngle;
        if (TiltRange.y > TiltRange.x)
            tiltAngle = Mathf.Clamp(tiltAngle, TiltRange.x, TiltRange.y);
        else
            tiltAngle = Mathf.Clamp(tiltAngle, TiltRange.y, TiltRange.x);
        excessTilt -= tiltAngle;
        SetXRotation(Ralph.UpperLegTilt, tiltAngle);

        //float zOffset = Vector3.Dot(tiltToTarget, Ralph.UpperLegTilt.forward);
        //Debug.Log(name + ", " + (Mathf.Abs(zOffset) < 0.0001f ? 0 : zOffset));
        //Debug.Log(zOffset);
        //Debug.Log(tiltAngle);
        Debug.DrawRay(Ralph.Connector.position, Ralph.UpperLegTilt.up * 100, Color.red);
    }
    private float CalculateZOffset()
    {
        Vector3 tiltToTarget = Ralph.UpperLegTilt.position - ActiveTarget;
        float zOffset = Vector3.Dot(tiltToTarget, Ralph.UpperLegTilt.forward);
        return Math.Abs(zOffset);
    }
    private void CalculateYaw()
    {
        SetZRotation(Ralph.Connector, 0);

        int iterationCount = 15;

        int initialIntervalCount = 2;
        float intervalDistance = 360f / initialIntervalCount;
        float bestAngle = 0f;
        float bestResult = CalculateZOffset();

        for (int i = 0; i < initialIntervalCount; i++)
        {
            float testAngle = 0f;
            testAngle *= i * intervalDistance;

            float result = TestYaw(testAngle);
            if (result < bestAngle)
            {
                bestAngle = testAngle;
                bestResult = result;
            }
        }
        for (int iteration = 1; iteration <= iterationCount; iteration++)
        {
            float angleOffset = intervalDistance / Mathf.Pow(2, iteration + 1);

            float positiveResult = TestYaw(bestAngle + angleOffset);
            float negativeResult = TestYaw(bestAngle - angleOffset);
            // if positive result is better
            if (positiveResult < negativeResult)
            {
                bestAngle = bestAngle + angleOffset;
                bestResult = positiveResult;
            }
            else
            {
                bestAngle = bestAngle - angleOffset;
                bestResult = negativeResult;
            }
        }

        SetZRotation(Ralph.Connector, bestAngle);

    }

    private float TestYaw(float testAngle)
    {
        SetZRotation(Ralph.Connector, testAngle);

        return CalculateZOffset();
    }

    private void SetXRotation(Transform transform, float rotation)
    {
        Vector3 angles = transform.localEulerAngles;
        angles.x = rotation;
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
        Source.DrawArmature();
        Ralph.DrawArmature();

        Gizmos.color = Color.yellow;
        Source.DrawConstructionLines();
        Ralph.DrawConstructionLines();

        if (!Application.isPlaying && Ralph.Anchor && Ralph.Foot)
        {
            Vector3 target = Ralph.Foot.position;
            Vector3 raycastStartPos = target + Ralph.Anchor.forward * 0.1f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(raycastStartPos, target);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_raycastStartPos, DirectTarget);
            if (_groundDetected)
            {
                Gizmos.color = new Color(0f,1f,0f,0.5f);
                Gizmos.DrawSphere(AdjustedTarget, 0.0125f * _ralphLegLength);
            }
        }

        Gizmos.color = _groundDetected ? new Color(1f,0f,0f,0.5f) : new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(DirectTarget, 0.0125f * _ralphLegLength);
    }
}
