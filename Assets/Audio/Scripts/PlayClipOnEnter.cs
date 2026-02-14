//Author: Small Hedge Games
//Updated: 13/06/2024
//Modified by: Helix

using UnityEngine;

namespace SmallHedge.AudioManager
{
    public class PlayClipOnEnter : StateMachineBehaviour
    {
        [SerializeField] private ClipType sound;
        [SerializeField, Range(0, 1)] private float volume = 1;
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AudioManager.PlaySound(sound, null, volume);
        }
    }
}