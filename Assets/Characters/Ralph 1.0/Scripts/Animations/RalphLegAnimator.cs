using System;
using Unity.Mathematics;
using UnityEngine;

public class RalphLegAnimator : BaseRalphAnimator
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
        public float initialUpperPitch;
        public Transform UpperLegTilt;
        public Transform LowerLeg;
        public float initialLowerPitch;
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
        public void Init()
        {
            initialUpperPitch = UpperLegPitch.localEulerAngles.z;
            initialLowerPitch = LowerLeg.localEulerAngles.z;
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

    private float _yaw = 0f;


    private float _upperLegLength = 0f;
    private float _lowerLegLength = 0f;
    public override void ManualInit()
    {
        Source.Init();

        _sourceLegLength = LengthBetween(Source.UpperLeg, Source.LowerLeg) + LengthBetween(Source.LowerLeg, Source.HeelBase);

        _upperLegLength = LengthBetween(Ralph.UpperLegPitch, Ralph.LowerLeg);
        _lowerLegLength = LengthBetween(Ralph.LowerLeg, Ralph.Foot);
        _ralphLegLength = _upperLegLength + _lowerLegLength;

        _scaleRatio = _ralphLegLength / _sourceLegLength;
        //Debug.Log(_scaleRatio);

        _smoothedRaycastDistance = _ralphLegLength / 2f;
    }

    private float _smoothedRaycastDistance = 0f;
    private bool _validTarget = true;
    public override void ManualUpdate()
    {
        //Debug.DrawRay(Ralph.Connector.position, Ralph.UpperLegTilt.up * 100, Color.red);

        // Connector movement
        float remappedToeLift = math.remap(-0.05f, 0.05f, -0.015f, 0.015f, Source.ToeLift);
        Ralph.ConnectorExtension = remappedToeLift;

        // Calculate Yaw with added offset, max at rest
        //CalculateYaw();

        // Ground targeting
        Vector3 sourceDisp = Source.HeelBase.position - Source.UpperLeg.position;

        // Scale displacement in the correct direction
        Vector3 targetDisp = Vector3.zero;
        targetDisp += _scaleRatio * Vector3.Dot(sourceDisp, Ralph.Anchor.forward) * Ralph.Anchor.forward;
        targetDisp += footBaseScalar * Vector3.Dot(sourceDisp, Ralph.Anchor.right) * Ralph.Anchor.right;
        targetDisp += stepDepthScalar * Vector3.Dot(sourceDisp, Ralph.Anchor.up) * Ralph.Anchor.up;

        DirectTarget = targetDisp + Ralph.Connector.position + Source.UpperLeg.rotation * TargetOffset;
        _raycastStartPos = DirectTarget + Ralph.Anchor.forward * _ralphLegLength / 2;
        _groundDetected = Physics.Raycast(_raycastStartPos, -Ralph.Anchor.forward, out RaycastHit hitInfo, _ralphLegLength / 2, GroundLayers);

        if (_groundDetected) _smoothedRaycastDistance = Mathf.Lerp(_smoothedRaycastDistance, hitInfo.distance, 12f * Time.deltaTime);
        else _smoothedRaycastDistance = Mathf.Lerp(_smoothedRaycastDistance, _ralphLegLength / 2, 12f * Time.deltaTime);
        AdjustedTarget = _raycastStartPos - Ralph.Anchor.forward * _smoothedRaycastDistance + Ralph.Anchor.forward * 0.01f;


        ActiveTarget = _groundDetected ? AdjustedTarget : DirectTarget;
        SetZRotation(Ralph.UpperLegPitch, 0);

        // IK Logic
        Vector3 adjustedTargetDisp = ActiveTarget - Ralph.Anchor.position;
        float x = Vector3.Dot(adjustedTargetDisp, Ralph.UpperLegPitch.up);
        float y = Vector3.Dot(adjustedTargetDisp, Ralph.UpperLegPitch.right);

        _validTarget = IKHelper.CalcIK_2D_TwoBoneAnalytic(out float angle1, out float angle2, true, _upperLegLength, _lowerLegLength, x, y);

        // Set joint rotations on plane
        SetZRotation(Ralph.UpperLegPitch, -angle1 * Mathf.Rad2Deg + Ralph.initialUpperPitch);
        SetZRotation(Ralph.LowerLeg, -angle2 * Mathf.Rad2Deg + Ralph.initialLowerPitch);

        // Calculate Tilt
        //CalculateTilt();
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


        //Something is fucked here
        if (TiltRange.y > TiltRange.x)
            tiltAngle = Mathf.Clamp(tiltAngle, TiltRange.x, TiltRange.y);
        else
            tiltAngle = Mathf.Clamp(tiltAngle, TiltRange.y, TiltRange.x);

        SetXRotation(Ralph.UpperLegTilt, tiltAngle);

        //float zOffset = Vector3.Dot(tiltToTarget, Ralph.UpperLegTilt.forward);
        //Debug.Log(name + ", " + (Mathf.Abs(zOffset) < 0.0001f ? 0 : zOffset));
        //Debug.Log(zOffset);
        //Debug.Log(tiltAngle);
    }
    private float CalculateZOffset()
    {
        Vector3 tiltToTarget = Ralph.UpperLegTilt.position - ActiveTarget;
        float zOffset = Vector3.Dot(tiltToTarget, Ralph.UpperLegTilt.forward);
        return Math.Abs(zOffset);
    }
    private void CalculateYaw()
    {
        //Vector3 hipToKnee = Source.LowerLeg.position - Source.UpperLeg.position + Source.UpperLeg.forward;
        //float x = Vector3.Dot(hipToKnee, Ralph.Hips.forward);
        //float y = Mathf.Abs(Vector3.Dot(hipToKnee, Ralph.Hips.right));
        //float rawYaw = Vector2.SignedAngle(Vector2.up, new Vector2(x, y));
        float rawYaw = (Source.Hips.InverseTransformDirection(Source.UpperLeg.up)).y >= 0 ? Source.UpperLeg.localEulerAngles.y - 180f : Source.UpperLeg.localEulerAngles.y;
        //float rawYaw = Source.UpperLeg.localEulerAngles.y;

        float yaw = rawYaw > 180f ? rawYaw - 360f: rawYaw;
        _yaw = Mathf.Lerp(_yaw, yaw, Time.deltaTime * 24f);
        // Arbitrary range
        float clampedYaw = Mathf.Clamp(_yaw, -20, 20);
        float weightedYaw = clampedYaw == _yaw ? 0 : _yaw - clampedYaw;

            
        SetZRotation(Ralph.Connector, weightedYaw);
        //Debug.Log("Name: " + name + ", " + rawYaw + ", " + new Vector2(x, y));

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
        Gizmos.color = _validTarget ? Color.green : Color.red;
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
