using System.Collections;
using UnityEngine;

namespace NordicWilds.World
{
    [RequireComponent(typeof(AudioSource))]
    public class RegionMusicController : MonoBehaviour
    {
        public static RegionMusicController Instance { get; private set; }

        public AudioClip forestClip;
        public AudioClip yamatoClip;
        public float volume = 0.48f;
        public float fadeDuration = 1.6f;

        private AudioSource source;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0f;
        }

        private void Start()
        {
            PlayForest();
        }

        public void PlayForest()
        {
            CrossfadeTo(forestClip);
        }

        public void PlayYamato()
        {
            CrossfadeTo(yamatoClip);
        }

        private void CrossfadeTo(AudioClip clip)
        {
            if (clip == null)
                return;

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(CrossfadeRoutine(clip));
        }

        private IEnumerator CrossfadeRoutine(AudioClip nextClip)
        {
            if (source.clip == nextClip && source.isPlaying)
            {
                source.volume = volume;
                yield break;
            }

            float startVolume = source.volume;
            float t = 0f;
            while (source.isPlaying && t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, t / Mathf.Max(0.01f, fadeDuration));
                yield return null;
            }

            source.clip = nextClip;
            source.Play();

            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(0f, volume, t / Mathf.Max(0.01f, fadeDuration));
                yield return null;
            }

            source.volume = volume;
            fadeRoutine = null;
        }
    }
}
