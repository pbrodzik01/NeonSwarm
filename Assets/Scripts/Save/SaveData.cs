using System;
using System.Collections.Generic;

namespace NeonSwarm
{
    [Serializable]
    public class SaveData
    {
        public int version = SaveSystem.CurrentVersion;
        public int bestScore;
        public int unlockedLevels = 1;
        public int coins;
        public SettingsData settings = new SettingsData();
        public StatsData stats = new StatsData();
        public List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
        public List<string> achievements = new List<string>();
        public List<MissionProgress> missions = new List<MissionProgress>();
        public int nextMissionIndex;
        public List<string> ownedItems = new List<string>();
        public List<ItemLevel> itemLevels = new List<ItemLevel>();
        public string selectedSkin = ShopCatalog.DefaultSkinId;
        public int startShields;
        public List<int> levelBestScores = new List<int>();
    }

    [Serializable]
    public class SettingsData
    {
        public bool music = true;
        public bool sfx = true;
        public float volume = 0.8f;
        public bool vibration = true;
        public bool batterySaver;
        public bool showFps;
        public string playerName = "Pilot";
    }

    [Serializable]
    public class StatsData
    {
        public int totalKills;
        public int totalCoins;
        public int levelsCompleted;
        public int flawlessLevels;
        public int pulsesUsed;
        public int gamesPlayed;
        public int victories;
        public int pickupsCollected;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string name;
        public int score;
        public int level;
        public string date;
    }

    [Serializable]
    public class MissionProgress
    {
        public string id;
        public int progress;
    }

    [Serializable]
    public class ItemLevel
    {
        public string id;
        public int level;
    }
}
