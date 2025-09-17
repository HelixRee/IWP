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
    public Vector3 DirectTarget = Vector3.zero;
    public Vector3 AdjustedTarget = Vector3.zero;

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
        Vector3 sourceDisp = Source.HeelBase.position - Source.Hips.position;
        
        // Scale displacement in the correct direction
        Vector3 targetDisp = Vector3.zero;
        targetDisp += _scaleRatio * Vector3.Dot(sourceDisp, Ralph.Hips.up) * Ralph.Hips.up;
        targetDisp += footBaseScalar * Vector3.Dot(sourceDisp, Ralph.Hips.forward) * Ralph.Hips.forward;
        targetDisp += stepDepthScalar * Vector3.Dot(sourceDisp, Ralph.Hips.right) * Ralph.Hips.right;

        DirectTarget = targetDisp + Ralph.Hips.position + Ralph.Hips.rotation * TargetOffset;
        _raycastStartPos = DirectTarget + Ralph.Anchor.forward * _ralphLegLength / 2;
        _groundDetected = Physics.Raycast(_raycastStartPos, -Ralph.Anchor.forward, out RaycastHit hitInfo, _ralphLegLength / 2);
        AdjustedTarget = hitInfo.point;

        // IK Logic
        // Tilt
        Vector3 activeTarget = _groundDetected ? AdjustedTarget : DirectTarget;
        float disp = Mathf.Clamp(Vector3.Dot((activeTarget - Ralph.UpperLegPitch.position), Ralph.Hips.forward), -0.125f, 0.125f);
        float tiltAngle = 0f;
        if (disp > 0f)
            tiltAngle = math.remap(0f, 0.125f, 0f, TiltRange.y, disp);
        else
            tiltAngle = math.remap(-0.125f, 0f, TiltRange.x, 0f, disp);
        SetXRotation(Ralph.UpperLegTilt, tiltAngle);

        // Yaw


        // Bone Solver



        //Debug.Log(name + ", Disp: " + disp + ", Tilt: " + tiltAngle);
        //Debug.Log(yaw);
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
