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

    }

    [Serializable]
    public class RalphArmature
    {
        public Transform Anchor;
        public Transform Connector;
        public Transform UpperLeg;
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
            if (Connector && UpperLeg)
                DrawLine(Connector, UpperLeg);
            if (UpperLeg && LowerLeg)
                DrawLine(UpperLeg, LowerLeg);
            if (LowerLeg && Foot)
                DrawLine(LowerLeg, Foot);
        }

        public void DrawConstructionLines()
        {
            DrawLine(UpperLeg, Foot);
            Vector3 pntOnLine = NearestPointOnLine(UpperLeg.position, Foot.position - UpperLeg.position, LowerLeg.position);
            Gizmos.DrawLine(pntOnLine, LowerLeg.position);
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
    private float _scaleRatio = 1.0f;
    public Vector3 TargetOffset = Vector3.zero;
    public Vector3 DirectTarget = Vector3.zero;
    public Vector3 AdjustedTarget = Vector3.zero;

    private Vector3 _raycastStartPos = Vector3.zero;
    private bool _groundDetected = true;

    public override void ManualInit()
    {
        Source.Init();

        _sourceLegLength = LengthBetween(Source.UpperLeg, Source.LowerLeg) + LengthBetween(Source.LowerLeg, Source.HeelBase);
        _ralphLegLength = LengthBetween(Ralph.UpperLeg, Ralph.LowerLeg) + LengthBetween(Ralph.LowerLeg, Ralph.Foot);

        _scaleRatio = _ralphLegLength / _sourceLegLength;

    }

    public override void ManualUpdate()
    {
        // Connector movement
        float remappedToeLift = math.remap(-0.05f, 0.05f, -0.015f, 0.015f, Source.ToeLift);
        Ralph.ConnectorExtension = remappedToeLift;

        // Ground targeting
        Vector3 sourceDisp = Source.HeelBase.position - Source.UpperLeg.position;
        Vector3 targetDisp = sourceDisp * _scaleRatio;

        DirectTarget = targetDisp + Ralph.UpperLeg.position + Ralph.Foot.rotation * TargetOffset;
        _raycastStartPos = DirectTarget + Ralph.Anchor.forward * _ralphLegLength / 2;
        _groundDetected = Physics.Raycast(_raycastStartPos, -Ralph.Anchor.forward, out RaycastHit hitInfo, _ralphLegLength / 2);
        AdjustedTarget = hitInfo.point;


        // IK Logic
        
        //Debug.Log(yaw);
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
