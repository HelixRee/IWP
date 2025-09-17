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
        public Transform Toe; // IK Point

        // Distance toe is lifted above foot [-0.05, 0.05]
        [HideInInspector] public float _toeLift => Vector3.Dot(Foot.position - Toe.position, Hips.up) - _initialToeLift; 
        [HideInInspector] public float _initialToeLift = 0f;
        public void Init()
        {
            _initialToeLift = Vector3.Dot(Foot.position - Toe.position, Hips.up);
        }
        public void DrawGizmos()
        {
            if (UpperLeg && LowerLeg)
                DrawLine(UpperLeg, LowerLeg);
            if (LowerLeg && Foot)
                DrawLine(LowerLeg, Foot);
            if (Foot && Toe)
                DrawLine(Foot, Toe);
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

        public void DrawGizmos()
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


    }
    static void DrawLine(Transform pos1, Transform pos2)
    {
        Gizmos.DrawLine(pos1.position, pos2.position);
    }

    [Header("Source")]
    public GenericArmature Source;
    [Header("Ralph")]
    public RalphArmature Ralph;

    public override void ManualInit()
    {
        Source.Init();
    }

    public override void ManualUpdate()
    {
        float remappedToeLift = math.remap(-0.05f, 0.05f, -0.015f, 0.015f, Source._toeLift);
        Ralph.ConnectorExtension = remappedToeLift;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Ralph.DrawGizmos();
        Source.DrawGizmos();
    }
}
