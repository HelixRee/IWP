using SmallHedge.AudioManager;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayClipOnEnable : MonoBehaviour
{
    [SerializeField] private ClipType audioType;

    private void OnEnable()
    {
        AudioManager.PlaySound(audioType, GetComponent<AudioSource>());
    }

    private void OnDisable()
    {
        GetComponent<AudioSource>().Stop();
    }
}
