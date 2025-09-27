using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RalphWiggleBoneChain : BaseRalphAnimator
{
    public List<Transform> Bones = new();
    public float CurveAmount = 1f;
    private List<Quaternion> _startingRotations = new();
    public override void ManualInit()
    {
        foreach (Transform t in Bones)
        {
            _startingRotations.Add(t.localRotation);
        }
    }

    public override void ManualUpdate()
    {
        Transform rootBone = Bones.First();
        float weightDelta = 1f / Bones.Count;
        for (int i = 1; i < Bones.Count; i++)
        {
            float weight = 1 - weightDelta * i;

            Bones[i].localRotation = _startingRotations[i] * Quaternion.Lerp(Quaternion.identity, rootBone.localRotation, weight * CurveAmount);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < Bones.Count - 1; i++)
        {
            Gizmos.DrawLine(Bones[i].position, Bones[i + 1].position);
        }
    }
}
