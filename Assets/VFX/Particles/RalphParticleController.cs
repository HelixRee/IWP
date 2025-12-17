using System.Collections.Generic;
using UnityEngine;

public class RalphParticleController : MonoBehaviour
{
    private bool _landPrimed = false;
    [SerializeField] private List<ParticleSystem> _sparkParticles = new();
    void OnLand(float verticalVelocity)
    {
        Debug.Log("Landed with a velocity of " + verticalVelocity);
        if (verticalVelocity > -4) return;
        float mult = Mathf.Clamp01(Mathf.Abs((verticalVelocity + 4) / 2f));
        Debug.Log(mult);
        foreach (var particle in _sparkParticles)
        {
            var emissionMod = particle.emission;
            ParticleSystem.Burst burst = new();
            burst.probability = 1;
            burst.count = 25f * mult;

            emissionMod.SetBurst(0, burst);
        }

    
        _landPrimed = true;
    }

    void OnLandRecover()
    {
        if (!_landPrimed) return;
        _landPrimed = false;
        foreach (var particle in _sparkParticles)
        {
            particle.Play();
        }
    }
}
