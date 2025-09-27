using System.Collections.Generic;
using UnityEngine;

public class RalphPackMountAnimator : BaseRalphAnimator
{
    public List<BaseRalphAnimator> ChildAnimations = new();
    [Header("Pack Mount")]
    public Transform PackMount;
    public Transform PackMountTarget;
    private Vector3 _initialPackMountRot;
    public override void ManualInit()
    {
        ChildAnimations.ForEach(Anim => Anim.ManualInit());

        _initialPackMountRot = PackMount.localEulerAngles;
    }

    public override void ManualUpdate()
    {
        ChildAnimations.ForEach(Anim => Anim.ManualUpdate());

        AimPackMount();
    }
    private void AimPackMount()
    {
        PackMount.LookAt(PackMountTarget, Vector3.up);
        PackMount.Rotate(Vector3.right, 90);
        Vector3 angles = PackMount.localEulerAngles;
        angles.y = _initialPackMountRot.y;
        PackMount.localEulerAngles = angles;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (PackMount && PackMountTarget)
            Gizmos.DrawLine(PackMount.position, PackMountTarget.position);
    }


}
