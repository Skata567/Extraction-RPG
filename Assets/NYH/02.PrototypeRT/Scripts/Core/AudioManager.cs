using UnityEngine;

namespace PrototypeRT
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        [SerializeField] private AudioCueData cueData;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.8f;

        private AudioSource _audioSource;

        public static AudioManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip == null || _audioSource == null) return;
            _audioSource.PlayOneShot(clip, sfxVolume);
        }

        public void PlaySfxByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || cueData == null) return;
            if (cueData.TryGetClip(key, out AudioClip clip))
                PlaySfx(clip);
        }
    }
}
