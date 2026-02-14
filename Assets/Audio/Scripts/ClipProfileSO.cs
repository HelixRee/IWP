using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmallHedge.AudioManager
{
    [CreateAssetMenu(menuName = "SFX/Clip Profile", fileName = "Clip Profile")]
    public class ClipProfileSO : ScriptableObject
    {
        public ClipLibrarySO activeClipLibrary;
        public int persistentSources = 4;
    }
}

