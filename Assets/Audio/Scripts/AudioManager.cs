//Author: Small Hedge Games
//Updated: 13/06/2024
//Modified by: Helix

using System;
using UnityEngine;
using UnityEngine.Audio;
using GD.MinMaxSlider;
using Unity.VisualScripting;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

namespace SmallHedge.AudioManager
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private ClipLibrarySO SO;
        private static AudioManager instance = null;
        private AudioSource audioSource;
        private List<AudioSource> audioSources = new List<AudioSource>();
        private int persistentSources = 4;

        private void Awake()
        {
            if(!instance)
            {
                instance = this;
                audioSource = GetComponent<AudioSource>();
            }
        }

        public static AudioSource PlaySound(ClipType sound, AudioSource source = null, float volume = 1)
        {
            CheckForInstance();

            ClipList soundList = instance.SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            if (clips.Length == 0) { Debug.LogWarning("No clips found for this sound type. Check the active clip library."); return null; }
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if(source)
            {
                if (!source.outputAudioMixerGroup)
                    source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = volume * soundList.volume;
                source.Play();
                return source;
            }
            else
            {
                return instance.PlaySound(soundList, randomClip, volume);
            }
        }

        public AudioSource PlaySound(ClipList soundList, AudioClip clip, float volume = 1)
        {
            bool sourceFound = false;
            AudioSource audioSource = new AudioSource();
            foreach (AudioSource source in audioSources)
            {
                if (source == null) continue;
                if (source.isPlaying) continue;
                audioSource = source;
                sourceFound = true;
                break;
            }

            if (!sourceFound)
            {
                GameObject newAudioSource = new GameObject("Audio Source");
                audioSource = newAudioSource.AddComponent<AudioSource>();
                newAudioSource.transform.parent = transform;
                if (audioSources.Count >= 4) 
                    StartCoroutine(DestroyWhenInactive(audioSource, audioSources));
                //else
                    audioSources.Add(audioSource);
            }

            if (audioSource != null)
            {
                audioSource.outputAudioMixerGroup = soundList.mixer;
                audioSource.clip = clip;
                audioSource.pitch = UnityEngine.Random.Range(soundList.pitchVariance.x, soundList.pitchVariance.y);
                audioSource.volume = volume * soundList.volume;
                audioSource.Play();
            }

            return audioSource;
        }

        private IEnumerator DestroyWhenInactive(AudioSource audioSource, List<AudioSource> audioSources)
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                if (!audioSource.isPlaying)
                {
                    audioSources.Remove(audioSource);
                    Destroy(audioSource.gameObject);
                    break;
                }
            }
        }

        private static void CheckForInstance()
        {
            if (!instance)
            {
                GameObject audioManager = new GameObject("Audio Manager");
                audioManager.transform.SetAsLastSibling();
                instance = audioManager.AddComponent<AudioManager>();

                //Find clip profile asset
                ClipProfileSO profileSO = Resources.Load<ClipProfileSO>("Audio Profiles/Clip Profile");
                if (!profileSO) { Debug.LogWarning("Clip Profile not found at Resources/Audio Profiles/Clip Profile"); return; }
                if (!profileSO.activeClipLibrary) { Debug.LogWarning("Clip Profile has no active Clip Library"); return; }
                instance.SO = profileSO.activeClipLibrary;
                instance.persistentSources = profileSO.persistentSources;
            }
        }
    }

    [Serializable]
    public class ClipList
    {
        [HideInInspector] public string name;
        [Range(0, 1)] public float volume;
        [MinMaxSlider(0.75f, 1.25f), Tooltip("Pitch variance range, 1 is default.")]
        public Vector2 pitchVariance = new Vector2(1,1);
        public AudioMixerGroup mixer;
        public AudioClip[] sounds;
    }
}