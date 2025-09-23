using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WiggleBoneChain : RalphAnimator
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
}
