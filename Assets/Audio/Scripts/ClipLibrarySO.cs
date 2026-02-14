//Author: Small Hedge Games
//Updated: 13/06/2024
//Modified by: Helix

using UnityEngine;

namespace SmallHedge.AudioManager
{
    [CreateAssetMenu(menuName = "SFX/Clip Library", fileName = "Clip Library")]
    public class ClipLibrarySO : ScriptableObject
    {
        public ClipList[] sounds;
    }
}