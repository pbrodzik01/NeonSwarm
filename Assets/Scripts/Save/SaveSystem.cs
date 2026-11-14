using System;
using System.IO;
using UnityEngine;

namespace NeonSwarm
{
    public static class SaveSystem
    {
        public const int CurrentVersion = 1;
        private const string FileName = "save.json";
        private const string WebPrefsKey = "neonswarm_save";

        private static SaveData data;
        private static string overridePath;

        public static event Action<SaveData> Saved;

        public static SaveData Data
        {
            get
            {
                if (data == null)
                {
                    Load();
                }
                return data;
            }
        }

        public static string FilePath => overridePath ?? Path.Combine(Application.persistentDataPath, FileName);

        public static void UsePath(string path)
        {
            overridePath = path;
            data = null;
        }

        public static SaveData Load()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            data = Parse(PlayerPrefs.GetString(WebPrefsKey, string.Empty), WebPrefsKey) ?? new SaveData();
#else
            data = ReadFrom(FilePath) ?? ReadFrom(FilePath + ".bak") ?? new SaveData();
#endif
            Normalize(data);
            return data;
        }

        public static void Save()
        {
            if (data == null)
            {
                return;
            }
            try
            {
                var json = JsonUtility.ToJson(data, true);
#if UNITY_WEBGL && !UNITY_EDITOR
                PlayerPrefs.SetString(WebPrefsKey, json);
                PlayerPrefs.Save();
#else
                var path = FilePath;
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var temp = path + ".tmp";
                File.WriteAllText(temp, json);
                if (File.Exists(path))
                {
                    File.Copy(path, path + ".bak", true);
                    File.Delete(path);
                }
                File.Move(temp, path);
#endif
                Saved?.Invoke(data);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveSystem] Save failed: {exception.Message}");
            }
        }

        public static void ResetProgress()
        {
            var keptSettings = Data.settings;
            data = new SaveData { settings = keptSettings };
            Normalize(data);
            Save();
        }

        private static SaveData ReadFrom(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }
                return Parse(File.ReadAllText(path), path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveSystem] Could not read {path}: {exception.Message}");
                return null;
            }
        }

        private static SaveData Parse(string json, string source)
        {
            try
            {
                return string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[SaveSystem] Could not parse {source}: {exception.Message}");
                return null;
            }
        }

        private static void Normalize(SaveData save)
        {
            save.version = CurrentVersion;
            save.settings ??= new SettingsData();
            save.stats ??= new StatsData();
            save.leaderboard ??= new System.Collections.Generic.List<LeaderboardEntry>();
            save.achievements ??= new System.Collections.Generic.List<string>();
            save.missions ??= new System.Collections.Generic.List<MissionProgress>();
            save.ownedItems ??= new System.Collections.Generic.List<string>();
            save.itemLevels ??= new System.Collections.Generic.List<ItemLevel>();
            save.levelBestScores ??= new System.Collections.Generic.List<int>();
            while (save.levelBestScores.Count < SceneNames.LevelCount)
            {
                save.levelBestScores.Add(0);
            }
            save.unlockedLevels = Mathf.Clamp(save.unlockedLevels, 1, SceneNames.LevelCount);
            save.settings.volume = Mathf.Clamp01(save.settings.volume);
            if (string.IsNullOrWhiteSpace(save.settings.playerName))
            {
                save.settings.playerName = "Pilot";
            }
            if (!save.ownedItems.Contains(ShopCatalog.DefaultSkinId))
            {
                save.ownedItems.Add(ShopCatalog.DefaultSkinId);
            }
            if (string.IsNullOrEmpty(save.selectedSkin) || !save.ownedItems.Contains(save.selectedSkin))
            {
                save.selectedSkin = ShopCatalog.DefaultSkinId;
            }
            MissionService.EnsureActiveMissions(save);
        }
    }
}
