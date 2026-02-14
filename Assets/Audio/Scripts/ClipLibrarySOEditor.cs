//Author: Small Hedge Games
//Updated: 13/06/2024
//Modified by: Helix

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.AudioManager
{
    [CustomEditor(typeof(ClipLibrarySO))]
    public class ClipLibrarySOEditor : Editor
    {
        private void OnEnable()
        {
            ref ClipList[] soundList = ref ((ClipLibrarySO)target).sounds;

            if (soundList == null)
                return;

            string[] names = Enum.GetNames(typeof(ClipType));
            bool differentSize = names.Length != soundList.Length;

            Dictionary<string, ClipList> sounds = new();

            if (differentSize)
            {
                for (int i = 0; i < soundList.Length; ++i)
                {
                    sounds.Add(soundList[i].name, soundList[i]);
                }
            }

            Array.Resize(ref soundList, names.Length);
            for (int i = 0; i < soundList.Length; i++)
            {
                string currentName = names[i];
                soundList[i].name = currentName;
                if (soundList[i].volume == 0) soundList[i].volume = 1;

                if (differentSize)
                {
                    if (sounds.ContainsKey(currentName))
                    {
                        ClipList current = sounds[currentName];
                        UpdateElement(ref soundList[i], current.volume, current.sounds, current.mixer);
                    }
                    else
                        UpdateElement(ref soundList[i], 1, new AudioClip[0], null);

                    static void UpdateElement(ref ClipList element, float volume, AudioClip[] sounds, AudioMixerGroup mixer)
                    {
                        element.volume = volume;
                        element.sounds = sounds;
                        element.mixer = mixer;
                    }
                }
            }
        }
    }
}
#endif
