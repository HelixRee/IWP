using System;
using System.Linq;
using UnityEngine;

public class RalphArmAnimator : BaseRalphAnimator
{
    [SerializeField] private LayerMask _groundLayers;
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
    public SODVec3 terrainHandTarget;
    public Vector3 currentHandTarget;
    public bool terrainAnimation = false;
    public float terrainTransition = 0;
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

    [Header("Throw")]
    [SerializeField] private Transform _throwIdle;
    public Vector3 aimTargetOffset = Vector3.zero;
    public Vector3 throwDirection = Vector3.zero;
    public AnimationCurve throwMotion = new();
    public float throwStartTimestamp = 0;
    public float aimTransition = 0f;
    public bool isAiming = false;
    public bool wasAiming = false;
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
        terrainHandTarget = new(Vector3.zero, 2, 0.7f, 2);
        terrainHandTarget.AbsValue = new Vector3(1, 1, 1);
    }

    public override void ManualUpdate()
    {
        //Debug.Log(terrainHandTarget.Value);

        if (_throwIdle != null)
            UpdateThrowLogic();
        //if (!isAiming)
        UpdateHandLogic();

        UpdateHandIK();
    }
    private void UpdateThrowLogic()
    {
        if (isAiming)
        {
            if (!wasAiming) aimTargetOffset = Vector3.zero;
            aimTransition = Mathf.Lerp(aimTransition, 1, Time.deltaTime * 12f);
        }
        else
        {
            if (wasAiming)
            {
                //throwDirection = Camera.main.transform.forward;
                throwStartTimestamp = Time.time;
            }
            if (Time.time > throwStartTimestamp + throwMotion.keys.Last().time)
                aimTransition = Mathf.Lerp(aimTransition, 0, Time.deltaTime * 12f);
            aimTargetOffset = throwMotion.Evaluate(Time.time - throwStartTimestamp) * throwDirection;
        }
        wasAiming = isAiming;


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

    private void OnDrawGizmosSelected()
    {
        if (terrainAnimation)
        {
            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawSphere(terrainHandTarget.Value, _lowerArmLength * 0.1f);

        }
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawSphere(currentHandTarget, _lowerArmLength * 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -transform.parent.right * _totalArmLength);
        Vector3 dir = (-transform.parent.right + transform.parent.forward * offsetMult * 3).normalized;
        Gizmos.DrawRay(transform.position, dir * _totalArmLength);

        Gizmos.DrawRay(transform.position, _auxCastDir * _totalArmLength);
    }

    private Vector3 _auxCastDir = Vector3.zero;
    private Vector3 _targetAuxCastDir = Vector3.zero;
    private RaycastHit hitInfo;
    private void UpdateHandLogic()
    {
        if (!IsGrounded)
            return;
        int offsetMult = isLeft ? -1 : 1;
        hitInfo = new RaycastHit();
        PrimaryRaycast();
        bool invalidTarget = ShouldCancelOverride();
        if (invalidTarget) hitInfo = new RaycastHit();

        bool inRange = false;

        if (hitInfo.collider == null)
        {
            Vector3 dir = (-transform.parent.right + transform.parent.forward * offsetMult * 3).normalized;

            Raycast(dir, out hitInfo);
            if (hitInfo.collider != null)
            {
                inRange = true;
                _targetAuxCastDir = dir;
            }
        }

        if (hitInfo.collider == null)
        {
            Vector3 dir = -transform.parent.right;

            Raycast(dir, out hitInfo);
            if (hitInfo.collider != null)
            {
                inRange = true;
                _targetAuxCastDir = dir;
            }
        }

        if (inRange)
        {
            Vector3 startAuxCastDir = _auxCastDir;
            for (float i = 1; i < 16; i++)
            {
                _auxCastDir = Vector3.Lerp(startAuxCastDir, _targetAuxCastDir, i / 16f);
                PrimaryRaycast();
                if (!ShouldCancelOverride()) break;
            }
        }
        // If dot shows that position is no longer infront of player, cancel the connection
        if (!terrainAnimation) return;
        if (ShouldCancelOverride())
        {
            terrainAnimation = false;
        }
    }
    private void PrimaryRaycast()
    {
        Vector3 prevAuxCastDir = _auxCastDir;
        Raycast(_auxCastDir, out hitInfo);
        // If no more wall to follow keep hand position at previous position
        if (hitInfo.collider == null)
        {

            Vector3 dir = (terrainHandTarget.Value - transform.position);
            dir = (dir - dir.normalized * 0.05f).normalized;
            Raycast(dir, out hitInfo);
            if (hitInfo.collider != null)
                _auxCastDir = dir;
        }

        if (hitInfo.collider != null)
        {
            // Check if surface is perpendicular to arm
            float dot = Vector3.Dot(hitInfo.normal, -_auxCastDir);
            if (dot > 0.5f)
            {
                if (!terrainAnimation)
                    terrainHandTarget.AbsValue = hitInfo.point - _auxCastDir * 0.05f;
                else
                    terrainHandTarget.Update(Time.deltaTime, hitInfo.point - _auxCastDir * 0.05f);

                terrainAnimation = true;
            }
            else
                _auxCastDir = prevAuxCastDir;
        }

    }
    private bool Raycast(Vector3 direction, out RaycastHit hitInfo)
    {
        return Physics.Raycast(
            transform.position,
            direction,
            out hitInfo,
            _totalArmLength,
            _groundLayers.value,
            QueryTriggerInteraction.Ignore);
    }
    private bool ShouldCancelOverride()
    {
        Vector3 disp = terrainHandTarget.Value - transform.position;
        float fwdDot = Vector3.Dot(disp.normalized, -transform.parent.right);
        float rightDot = Vector3.Dot(disp.normalized, transform.parent.forward * offsetMult);
        //Debug.Log("Forward: " + fwdDot + ", Right: " + rightDot + ", " + name);

        return (fwdDot < 0.1f || rightDot < -0.15f || disp.magnitude > _totalArmLength || Mathf.Abs(disp.y) > 0.2f);
    }
    //public float debugAngle;
    private void UpdateHandIK()
    {
        float distanceMult = 1 + (handTarget - currentHandTarget).magnitude * 4;
        //Debug.Log(3f * distanceMult + " : " + name);
        terrainTransition = Mathf.Lerp(terrainTransition, (terrainAnimation && IsGrounded) ? 1 : 0, 3f * distanceMult * Time.deltaTime);

        // Calculate direction and position of end
        _sourceAnchorToEndDir = (Source.End.position - Source.Anchor.position).normalized;
        Vector3 sourceAnchorToElbowDir = (Source.Elbow.position - Source.Anchor.position).normalized;
        _ralphAnchorToEndDist = (Source.End.position - Source.Anchor.position).magnitude * _scaleRatio;

        // Set proxy end position to match animation
        handTarget = Ralph.Anchor.position + _sourceAnchorToEndDir * _ralphAnchorToEndDist;

        currentHandTarget = Vector3.Lerp(handTarget, terrainHandTarget.Value, terrainTransition);
        if (_throwIdle)
            currentHandTarget = Vector3.Lerp(currentHandTarget, _throwIdle.position + aimTargetOffset, aimTransition);

        Vector3 currentSourceAnchorToEndDir = (currentHandTarget - Ralph.Anchor.position).normalized;
        {
            Vector3 targetDisp = currentHandTarget - Ralph.Anchor.position;
            float x = Vector3.Dot(targetDisp, Ralph.Anchor.parent.forward);
            float y = Vector3.Dot(targetDisp, -Ralph.Anchor.parent.right);

            float angle = Vector2.SignedAngle(Vector2.up, new Vector2(x, y));
            sourceAnchorToElbowDir = Quaternion.Euler(0f, -angle, 0f) * sourceAnchorToElbowDir;
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
    }
}
