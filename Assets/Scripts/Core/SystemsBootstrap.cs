using UnityEngine;

namespace NeonSwarm
{
    public static class SystemsBootstrap
    {
        public const string SystemsResource = "Systems";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            PhysicsSetup.Apply();
            SaveSystem.Load();
            ProgressTracker.Initialize();
            QualitySettings.vSyncCount = 0;
            Input.multiTouchEnabled = true;
            if (Object.FindObjectOfType<AudioManager>() == null)
            {
                var prefab = Resources.Load<GameObject>(SystemsResource);
                var systems = prefab != null ? Object.Instantiate(prefab) : new GameObject();
                systems.name = "[Systems]";
                if (prefab == null)
                {
                    systems.AddComponent<AudioManager>();
                    systems.AddComponent<SceneLoader>();
                    systems.AddComponent<Toast>();
                }
                if (!systems.TryGetComponent<AppLifecycle>(out _))
                {
                    systems.AddComponent<AppLifecycle>();
                }
                Object.DontDestroyOnLoad(systems);
            }
            AppSettings.Apply();
        }
    }

    public static class AppSettings
    {
        public static void Apply()
        {
            var settings = SaveSystem.Data.settings;
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.targetFrameRate = settings.batterySaver ? 30 : -1;
#else
            Application.targetFrameRate = settings.batterySaver ? 30 : 60;
#endif
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ApplySettings(settings);
            }
        }

        public static void Vibrate()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (SaveSystem.Data.settings.vibration)
            {
                Handheld.Vibrate();
            }
#endif
        }
    }
}
