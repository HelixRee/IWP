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
    public SODVec3 overrideHandTarget;
    public Vector3 currentHandTarget;
    public bool overrideAnimation = false;
    public float overrideTransition = 0;
    public bool isLeft = false;
    int offsetMult => isLeft ? -1 : 1;
    public TransformGroup Ralph;

    // Anchor to End
    private float _scaleRatio = 1.0f;
    private Vector3 _sourceAnchorToEndDir = Vector3.zero;
    private float _ralphAnchorToEndDist = 0f;

    // Arm lengths
    float _upperArmLength = 0f;
    float _lowerArmLength = 0f;
    float _totalArmLength = 0f;

    // Target within range
    bool _validTarget = true;

    // Arm world interaction
    private Vector3 _colliderSize;
    private void OnValidate()
    {
        ManualInit();
    }
    public override void ManualInit()
    {
        float ralphLength = Vector3.Magnitude(Ralph.Anchor.position - Ralph.End.position);
        float sourceLength = Vector3.Magnitude(Source.Anchor.position - Source.End.position);

        // Measure arm lengths
        _upperArmLength = Vector3.Distance(Ralph.Anchor.position, Ralph.Elbow.position);
        _lowerArmLength = Vector3.Distance(Ralph.Elbow.position, Ralph.End.position);
        _totalArmLength = _lowerArmLength + _upperArmLength;
        _colliderSize = new Vector3(_totalArmLength / 2, 0.2f, _totalArmLength / 2);

        _scaleRatio = ralphLength / sourceLength;
        overrideHandTarget = new(Vector3.zero);
    }

    public override void ManualUpdate()
    {
        UpdateHandLogic();
        UpdateHandIK();
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
        Gizmos.DrawSphere(currentHandTarget, _lowerArmLength * 0.1f);

        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawSphere(overrideHandTarget.Value, _lowerArmLength * 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -transform.parent.right * _totalArmLength);
        Vector3 dir = (-transform.parent.right + transform.parent.forward * offsetMult * 3).normalized;
        Gizmos.DrawRay(transform.position, dir * _totalArmLength);

        //Gizmos.DrawRay(transform.position, (overrideHandTarget - transform.position).normalized * _totalArmLength);
        Gizmos.DrawRay(transform.position, _prevAuxCastDir * _totalArmLength);
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = new Color(0, 1, 0, 0.5f);
        //Gizmos.DrawRay(transform.position, transform.parent.forward * _totalArmLength);
        //Vector3 cubeSize = new Vector3(armLength, 0.2f, armLength);

        //Matrix4x4 matrix = transform.localToWorldMatrix;
        //matrix.SetTRS(transform.position, transform.parent.rotation, Vector3.one);
        //Gizmos.matrix = matrix;
        //Gizmos.DrawCube(Vector3.forward * offsetMult * _totalArmLength / 4f - Vector3.right * _totalArmLength / 2f, _colliderSize);

        //Gizmos.matrix = Matrix4x4.identity;


        // Target logic
        //Gizmos.color = new Color(1, 0, 0, 0.5f);

    }
    //private bool _auxCast = false;
    //private Vector3 _auxCastDir = Vector3.zero;
    private Vector3 _prevAuxCastDir = Vector3.zero;
    private Vector3 _localAuxCastDir = Vector3.zero;
    private void UpdateHandLogic()
    {
        if (!IsGrounded)
            return;
        int offsetMult = isLeft ? -1 : 1;

        RaycastHit hitInfo;
        Raycast(_prevAuxCastDir, out hitInfo);
        // If no more wall to follow keep hand position at previous position
        if (hitInfo.collider == null)
        {

            Vector3 dir = (overrideHandTarget.Value - transform.position);
            dir = (dir - dir.normalized * 0.05f).normalized;
            _prevAuxCastDir = dir;
            _localAuxCastDir = dir;
            Raycast(_prevAuxCastDir, out hitInfo);
        }

        if (hitInfo.collider != null)
        {
            // Check if surface is perpendicular to arm
            float dot = Vector3.Dot(hitInfo.normal, -_prevAuxCastDir);
            if (dot > 0.5f)
            {
                overrideAnimation = true;
                overrideHandTarget.AbsValue = hitInfo.point - _prevAuxCastDir * 0.05f;
            }
        }
        bool invalidTarget = ShouldCancelOverride();
        if (invalidTarget) hitInfo = new RaycastHit();

        if (hitInfo.collider == null)
        {
            Vector3 dir = (-transform.parent.right + transform.parent.forward * offsetMult * 3).normalized;

            Raycast(dir, out hitInfo);
            if (hitInfo.collider != null)
                _prevAuxCastDir = dir;
                //_prevAuxCastDir = Vector3.Lerp(_prevAuxCastDir, dir, 6f * Time.deltaTime);
        }

        if (hitInfo.collider == null)
        {
            Vector3 dir = -transform.parent.right;

            Raycast(dir, out hitInfo);
            if (hitInfo.collider != null)
                _prevAuxCastDir = dir;
        }


        // If dot shows that position is no longer infront of player, cancel the connection
        if (!overrideAnimation) return;
        if (ShouldCancelOverride())
        {
            overrideAnimation = false;
        }
    }
    private bool Raycast(Vector3 direction, out RaycastHit hitInfo)
    {
        return Physics.Raycast(
            transform.position,
            direction,
            out hitInfo,
            _totalArmLength,
            GroundLayers.value,
            QueryTriggerInteraction.Ignore);
    }
    private bool ShouldCancelOverride()
    {
        Vector3 disp = overrideHandTarget.Value - transform.position;
        float fwdDot = Vector3.Dot(disp.normalized, -transform.parent.right);
        float rightDot = Vector3.Dot(disp.normalized, transform.parent.forward * offsetMult);
        //Debug.Log("Forward: " + fwdDot + ", Right: " + rightDot + ", " + name);

        return (fwdDot < 0.1f || rightDot < -0.15f || disp.magnitude > _totalArmLength);
    }
    //public float debugAngle;
    private void UpdateHandIK()
    {
        float distanceMult = 1 + (handTarget - currentHandTarget).magnitude * 8;
        //Debug.Log(3f * distanceMult + " : " + name);
        overrideTransition = Mathf.Lerp(overrideTransition, (overrideAnimation && IsGrounded) ? 1 : 0, 3f * distanceMult * Time.deltaTime);

        // Calculate direction and position of end
        _sourceAnchorToEndDir = (Source.End.position - Source.Anchor.position).normalized;
        Vector3 sourceAnchorToElbowDir = (Source.Elbow.position - Source.Anchor.position).normalized;
        _ralphAnchorToEndDist = (Source.End.position - Source.Anchor.position).magnitude * _scaleRatio;

        // Set proxy end position to match animation
        handTarget = Ralph.Anchor.position + _sourceAnchorToEndDir * _ralphAnchorToEndDist;

        currentHandTarget = Vector3.Lerp(handTarget, overrideHandTarget.Value, overrideTransition);

        Vector3 currentSourceAnchorToEndDir = (currentHandTarget - Ralph.Anchor.position).normalized;
        {
            Vector3 targetDisp = currentHandTarget - Ralph.Anchor.position;
            float x = Vector3.Dot(targetDisp, Ralph.Anchor.parent.forward);
            float y = Vector3.Dot(targetDisp, -Ralph.Anchor.parent.right);

            float angle = Vector2.SignedAngle(Vector2.up, new Vector2(x, y));
            sourceAnchorToElbowDir = Quaternion.Euler(0f, -angle, 0f) * sourceAnchorToElbowDir;
            //Ralph.Anchor.Rotate(Vector3.right, Mathf.Lerp(0, isLeft ? -90 : 90, overrideTransition));
            //Ralph.Anchor.Rotate(Vector3.right, Mathf.Lerp(0, -angle, overrideTransition));
            //debugAngle = angle;
            //Debug.Log(-angle + ", " + name);
        }

        // Find arm rotation plane
        Vector3 armPlaneNormal = Vector3.Cross(currentSourceAnchorToEndDir, sourceAnchorToElbowDir);
        if (armPlaneNormal != Vector3.zero)
            Ralph.Anchor.forward = armPlaneNormal;
        SetZRotation(Ralph.Anchor, 0);


        // Measure displacements on plane
        {
            Vector3 targetDisp = currentHandTarget - Ralph.Anchor.position;
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

        //{
        //    Vector3 targetDisp = currentHandTarget - Ralph.Anchor.position;
        //    float x = Vector3.Dot(targetDisp, Ralph.Anchor.parent.forward);
        //    float y = Vector3.Dot(targetDisp, -Ralph.Anchor.parent.right);

        //    float angle = Vector2.SignedAngle(Vector2.up, new Vector2(x, y));
        //    //Ralph.Anchor.Rotate(Vector3.right, Mathf.Lerp(0, isLeft ? -90 : 90, overrideTransition));
        //    //Ralph.Anchor.Rotate(Vector3.right, Mathf.Lerp(0, -angle, overrideTransition));
        //    //debugAngle = angle;
        //    //Debug.Log(-angle + ", " + name);
        //}
    }
}
