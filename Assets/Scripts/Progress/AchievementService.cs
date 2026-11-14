using System;
using System.Collections.Generic;

namespace NeonSwarm
{
    public sealed class AchievementDefinition
    {
        public string Id;
        public string Title;
        public string Description;
        public Func<SaveData, bool> IsMet;
    }

    public static class AchievementService
    {
        public static readonly AchievementDefinition[] All =
        {
            Define("a_first_blood", "Pierwsza krew", "Pokonaj pierwszego przeciwnika", s => s.stats.totalKills >= 1),
            Define("a_hunter", "Łowca roju", "Pokonaj 100 przeciwników", s => s.stats.totalKills >= 100),
            Define("a_treasurer", "Skarbnik", "Zbierz łącznie 300 monet", s => s.stats.totalCoins >= 300),
            Define("a_untouchable", "Nietykalny", "Ukończ poziom bez utraty HP", s => s.stats.flawlessLevels >= 1),
            Define("a_veteran", "Weteran", "Ukończ wszystkie 3 poziomy", s => s.stats.victories >= 1),
            Define("a_score5000", "Mistrz punktów", "Zdobądź 5000 pkt w jednej grze", s => s.bestScore >= 5000),
            Define("a_pulse", "Fala uderzeniowa", "Użyj pulsu 25 razy", s => s.stats.pulsesUsed >= 25)
        };

        public static bool IsUnlocked(SaveData save, string id) => save.achievements.Contains(id);

        public static List<AchievementDefinition> Evaluate(SaveData save)
        {
            var unlocked = new List<AchievementDefinition>();
            foreach (var achievement in All)
            {
                if (!save.achievements.Contains(achievement.Id) && achievement.IsMet(save))
                {
                    save.achievements.Add(achievement.Id);
                    unlocked.Add(achievement);
                }
            }
            return unlocked;
        }

        private static AchievementDefinition Define(string id, string title, string description, Func<SaveData, bool> isMet)
        {
            return new AchievementDefinition { Id = id, Title = title, Description = description, IsMet = isMet };
        }
    }

    public static class Leaderboard
    {
        public const int MaxEntries = 10;

        public static int Submit(SaveData save, string name, int score, int level, string date)
        {
            if (score <= 0)
            {
                return -1;
            }
            var entry = new LeaderboardEntry { name = name, score = score, level = level, date = date };
            var index = 0;
            while (index < save.leaderboard.Count && save.leaderboard[index].score >= score)
            {
                index++;
            }
            if (index >= MaxEntries)
            {
                return -1;
            }
            save.leaderboard.Insert(index, entry);
            if (save.leaderboard.Count > MaxEntries)
            {
                save.leaderboard.RemoveRange(MaxEntries, save.leaderboard.Count - MaxEntries);
            }
            return index;
        }
    }

    public static class ScoreRules
    {
        public const int Coin = 10;
        public const int LevelComplete = 500;
        public const int TimeBonusStep = 50;
        public const float TimeBonusStepSeconds = 10f;
        public const int FlawlessBonus = 250;
        public const int MedkitAtFullHp = 25;
        public const int PowerUp = 20;

        public static int ForEnemy(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Chaser:
                    return 150;
                case EnemyKind.Shooter:
                    return 200;
                default:
                    return 100;
            }
        }

        public static int TimeBonus(float parSeconds, float elapsedSeconds)
        {
            var saved = parSeconds - elapsedSeconds;
            if (saved <= 0f)
            {
                return 0;
            }
            return (int)(saved / TimeBonusStepSeconds + 1f) * TimeBonusStep;
        }
    }
}
