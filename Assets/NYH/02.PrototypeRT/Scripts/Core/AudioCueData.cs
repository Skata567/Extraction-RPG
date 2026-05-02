using System;
using UnityEngine;

namespace PrototypeRT
{
    [CreateAssetMenu(fileName = "AudioCueData", menuName = "Prototype RT/Audio Cue Data")]
    public class AudioCueData : ScriptableObject
    {
        [SerializeField] private AudioCue[] cues = Array.Empty<AudioCue>();

        public bool TryGetClip(string key, out AudioClip clip)
        {
            foreach (AudioCue cue in cues)
            {
                if (cue.Key == key && cue.Clip != null)
                {
                    clip = cue.Clip;
                    return true;
                }
            }

            clip = null;
            return false;
        }
    }

    [Serializable]
    public class AudioCue
    {
        [SerializeField] private string key;
        [SerializeField] private AudioClip clip;

        public string Key => key;
        public AudioClip Clip => clip;
    }
}
