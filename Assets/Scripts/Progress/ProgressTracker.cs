using System;
using UnityEngine;

namespace NeonSwarm
{
    public static class ProgressTracker
    {
        private static bool initialized;

        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }
            initialized = true;
            GameEvents.EnemyKilled += OnEnemyKilled;
            GameEvents.PickupCollected += OnPickupCollected;
            GameEvents.PulseUsed += OnPulseUsed;
            GameEvents.LevelCompleted += OnLevelCompleted;
        }

        public static void AddCoins(int amount)
        {
            var save = SaveSystem.Data;
            save.coins += amount;
            save.stats.totalCoins += amount;
            GameSession.RegisterCoins(amount);
            GameEvents.RaiseCoinsChanged(save.coins);
            Report(MissionStat.Coins, amount);
        }

        public static int FinishRun(RunOutcome outcome)
        {
            var save = SaveSystem.Data;
            save.stats.gamesPlayed++;
            if (outcome == RunOutcome.Victory)
            {
                save.stats.victories++;
            }
            var score = GameSession.Score;
            var newRecord = score > GameSession.BestScoreBeforeRun;
            if (score > save.bestScore)
            {
                save.bestScore = score;
            }
            var rank = Leaderboard.Submit(save, save.settings.playerName, score, GameSession.LevelIndex + 1, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            GameSession.EndRun(outcome, newRecord);
            CheckAchievements();
            SaveSystem.Save();
            return rank;
        }

        public static void RecordLevelScore(int levelIndex, int levelScore)
        {
            var save = SaveSystem.Data;
            if (levelIndex >= 0 && levelIndex < save.levelBestScores.Count && levelScore > save.levelBestScores[levelIndex])
            {
                save.levelBestScores[levelIndex] = levelScore;
            }
            if (GameSession.Score > save.bestScore)
            {
                save.bestScore = GameSession.Score;
            }
        }

        private static void OnEnemyKilled(EnemyKind kind, Vector3 position)
        {
            SaveSystem.Data.stats.totalKills++;
            GameSession.RegisterKill();
            Report(MissionStat.Kills, 1);
            CheckAchievements();
        }

        private static void OnPickupCollected(PickupKind kind)
        {
            SaveSystem.Data.stats.pickupsCollected++;
            if (kind != PickupKind.Coin)
            {
                Report(MissionStat.Pickups, 1);
            }
        }

        private static void OnPulseUsed()
        {
            SaveSystem.Data.stats.pulsesUsed++;
            Report(MissionStat.Pulses, 1);
            CheckAchievements();
        }

        private static void OnLevelCompleted(int levelIndex, bool flawless)
        {
            var save = SaveSystem.Data;
            save.stats.levelsCompleted++;
            Report(MissionStat.LevelsCompleted, 1);
            if (flawless)
            {
                save.stats.flawlessLevels++;
                Report(MissionStat.FlawlessLevels, 1);
            }
            save.unlockedLevels = Mathf.Clamp(Mathf.Max(save.unlockedLevels, levelIndex + 2), 1, SceneNames.LevelCount);
            CheckAchievements();
            SaveSystem.Save();
        }

        private static void Report(MissionStat stat, int amount)
        {
            var save = SaveSystem.Data;
            var completed = MissionService.Report(save, stat, amount);
            foreach (var mission in completed)
            {
                GameEvents.RaiseNotification($"Misja ukończona: {mission.Title}  +{mission.Reward} monet");
                GameEvents.RaiseCoinsChanged(save.coins);
            }
            if (completed.Count > 0)
            {
                SaveSystem.Save();
            }
        }

        private static void CheckAchievements()
        {
            var unlocked = AchievementService.Evaluate(SaveSystem.Data);
            foreach (var achievement in unlocked)
            {
                GameEvents.RaiseNotification($"Osiągnięcie: {achievement.Title}");
                AudioManager.Play(SfxId.PowerUp);
            }
            if (unlocked.Count > 0)
            {
                SaveSystem.Save();
            }
        }
    }
}
