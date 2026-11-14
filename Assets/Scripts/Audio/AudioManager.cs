using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace NeonSwarm
{
    public class AudioManager : MonoBehaviour
    {
        public const string MasterParam = "MasterVolume";
        public const string MusicParam = "MusicVolume";
        public const string SfxParam = "SFXVolume";
        private const float MutedDb = -80f;

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioLibrary library;
        [SerializeField] private int sfxVoices = 10;

        private AudioSource musicSource;
        private AudioSource uiSource;
        private AudioSource[] voices;
        private int nextVoice;
        private float[] lastPlayed;
        private MusicId currentMusic = MusicId.None;
        private Coroutine musicFade;
        private bool mixerReady;

        public static AudioManager Instance { get; private set; }

        public static void Play(SfxId id, float volumeScale = 1f)
        {
            if (Instance != null)
            {
                Instance.PlaySfx(id, volumeScale);
            }
        }

        public static void PlayMusic(MusicId id)
        {
            if (Instance != null)
            {
                Instance.SwitchMusic(id);
            }
        }

        public static void SetGameplayPaused(bool paused)
        {
            AudioListener.pause = paused;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var musicGroup = FindGroup("Music");
            var sfxGroup = FindGroup("SFX");

            musicSource = CreateSource("Music", musicGroup);
            musicSource.loop = true;
            musicSource.ignoreListenerPause = true;

            uiSource = CreateSource("UI", sfxGroup);
            uiSource.ignoreListenerPause = true;

            voices = new AudioSource[sfxVoices];
            for (var i = 0; i < voices.Length; i++)
            {
                voices[i] = CreateSource("Voice " + i, sfxGroup);
            }
            lastPlayed = new float[System.Enum.GetValues(typeof(SfxId)).Length];
        }

        private void Start()
        {
            mixerReady = true;
            ApplySettings(SaveSystem.Data.settings);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private bool UsesMixer => false;
#else
        private bool UsesMixer => mixer != null;
#endif

        public void ApplySettings(SettingsData settings)
        {
            var masterDb = LinearToDb(settings.volume);
            if (UsesMixer && mixerReady)
            {
                mixer.SetFloat(MasterParam, masterDb);
                mixer.SetFloat(MusicParam, settings.music ? 0f : MutedDb);
                mixer.SetFloat(SfxParam, settings.sfx ? 0f : MutedDb);
            }
            if (!UsesMixer)
            {
                AudioListener.volume = settings.volume;
                musicSource.mute = !settings.music;
                uiSource.mute = !settings.sfx;
                foreach (var voice in voices)
                {
                    voice.mute = !settings.sfx;
                }
            }
        }

        public void PlaySfx(SfxId id, float volumeScale = 1f)
        {
            var entry = library != null ? library.Find(id) : null;
            if (entry == null || entry.clip == null)
            {
                return;
            }
            var index = (int)id;
            var now = Time.unscaledTime;
            if (now - lastPlayed[index] < entry.minInterval)
            {
                return;
            }
            lastPlayed[index] = now;

            var source = id == SfxId.Click ? uiSource : NextVoice();
            source.pitch = 1f + Random.Range(-entry.pitchVariance, entry.pitchVariance);
            source.PlayOneShot(entry.clip, entry.volume * volumeScale);
        }

        public void SwitchMusic(MusicId id)
        {
            if (id == currentMusic)
            {
                return;
            }
            currentMusic = id;
            if (musicFade != null)
            {
                StopCoroutine(musicFade);
            }
            musicFade = StartCoroutine(FadeTo(library != null ? library.Find(id) : null));
        }

        private IEnumerator FadeTo(AudioLibrary.MusicEntry entry)
        {
            const float duration = 0.4f;
            var start = musicSource.volume;
            for (var t = 0f; t < duration && musicSource.isPlaying; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(start, 0f, t / duration);
                yield return null;
            }
            musicSource.Stop();
            if (entry == null || entry.clip == null)
            {
                yield break;
            }
            musicSource.clip = entry.clip;
            musicSource.volume = 0f;
            musicSource.Play();
            for (var t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(0f, entry.volume, t / duration);
                yield return null;
            }
            musicSource.volume = entry.volume;
        }

        private AudioSource NextVoice()
        {
            for (var i = 0; i < voices.Length; i++)
            {
                var candidate = voices[(nextVoice + i) % voices.Length];
                if (!candidate.isPlaying)
                {
                    nextVoice = (nextVoice + i + 1) % voices.Length;
                    return candidate;
                }
            }
            var stolen = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            return stolen;
        }

        private AudioSource CreateSource(string sourceName, AudioMixerGroup group)
        {
            var child = new GameObject(sourceName);
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = group;
            return source;
        }

        private AudioMixerGroup FindGroup(string groupName)
        {
            if (!UsesMixer)
            {
                return null;
            }
            var groups = mixer.FindMatchingGroups(groupName);
            return groups.Length > 0 ? groups[0] : null;
        }

        public static float LinearToDb(float value)
        {
            return value <= 0.0001f ? MutedDb : Mathf.Log10(value) * 20f;
        }
    }
}
