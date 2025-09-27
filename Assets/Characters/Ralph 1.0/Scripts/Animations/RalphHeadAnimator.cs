using System.Collections.Generic;
using UnityEngine;

public class RalphHeadAnimator : BaseRalphAnimator
{
    [Range(0f, 1f)] public float Weight = 1f;
    public Transform HeadTarget;

    public List<BaseRalphAnimator> ChildAnimations = new();

    private float _angleOffset = 0f;
    private Vector3 _initialAngles;

    public override void ManualInit()
    {
        ChildAnimations.ForEach(anim => anim.ManualInit());
        _initialAngles = transform.localEulerAngles;
    }

    public override void ManualUpdate()
    {
        ChildAnimations.ForEach(anim => anim.ManualUpdate());

        transform.localEulerAngles = _initialAngles;
        Vector3 offset = HeadTarget.position - transform.position;
        float x = Vector3.Dot(offset, -transform.up);
        float y = Vector3.Dot(offset, -transform.right);
        Vector2 offset2D = new Vector2(x, y);
        _angleOffset = Vector2.SignedAngle(offset2D, Vector2.up);

        Vector3 angles = _initialAngles;
        angles.z += _angleOffset * Weight;
        transform.localEulerAngles = angles;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.Lerp(Color.green, Color.red, Mathf.Abs(_angleOffset) / 10f);
        Gizmos.DrawLine(transform.position, HeadTarget.position);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -transform.right * (HeadTarget.position - transform.position).magnitude);
    }
}
