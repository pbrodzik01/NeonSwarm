using System.Collections.Generic;

namespace NeonSwarm
{
    public enum MissionStat
    {
        Kills,
        Coins,
        FlawlessLevels,
        Pulses,
        LevelsCompleted,
        Pickups
    }

    public sealed class MissionDefinition
    {
        public string Id;
        public string Title;
        public MissionStat Stat;
        public int Target;
        public int Reward;

        public MissionDefinition(string id, string title, MissionStat stat, int target, int reward)
        {
            Id = id;
            Title = title;
            Stat = stat;
            Target = target;
            Reward = reward;
        }
    }

    public static class MissionCatalog
    {
        public static readonly MissionDefinition[] All =
        {
            new MissionDefinition("m_kill20", "Pokonaj 20 przeciwników", MissionStat.Kills, 20, 50),
            new MissionDefinition("m_coins30", "Zbierz 30 monet", MissionStat.Coins, 30, 40),
            new MissionDefinition("m_pulse10", "Użyj pulsu 10 razy", MissionStat.Pulses, 10, 40),
            new MissionDefinition("m_flawless", "Ukończ poziom bez obrażeń", MissionStat.FlawlessLevels, 1, 100),
            new MissionDefinition("m_levels3", "Ukończ 3 poziomy", MissionStat.LevelsCompleted, 3, 80),
            new MissionDefinition("m_pickups15", "Zbierz 15 bonusów", MissionStat.Pickups, 15, 50),
            new MissionDefinition("m_kill75", "Pokonaj 75 przeciwników", MissionStat.Kills, 75, 120),
            new MissionDefinition("m_coins120", "Zbierz 120 monet", MissionStat.Coins, 120, 100)
        };

        public static MissionDefinition Find(string id)
        {
            foreach (var mission in All)
            {
                if (mission.Id == id)
                {
                    return mission;
                }
            }
            return null;
        }
    }

    public static class MissionService
    {
        public const int ActiveCount = 3;

        public static void EnsureActiveMissions(SaveData save)
        {
            save.missions.RemoveAll(m => MissionCatalog.Find(m.id) == null);
            var guard = 0;
            while (save.missions.Count < ActiveCount && guard++ < MissionCatalog.All.Length * 2)
            {
                var candidate = MissionCatalog.All[save.nextMissionIndex % MissionCatalog.All.Length];
                save.nextMissionIndex = (save.nextMissionIndex + 1) % MissionCatalog.All.Length;
                if (!IsActive(save, candidate.Id))
                {
                    save.missions.Add(new MissionProgress { id = candidate.Id, progress = 0 });
                }
            }
        }

        public static List<MissionDefinition> Report(SaveData save, MissionStat stat, int amount)
        {
            var completed = new List<MissionDefinition>();
            if (amount <= 0)
            {
                return completed;
            }
            for (var i = save.missions.Count - 1; i >= 0; i--)
            {
                var progress = save.missions[i];
                var definition = MissionCatalog.Find(progress.id);
                if (definition == null || definition.Stat != stat)
                {
                    continue;
                }
                progress.progress += amount;
                if (progress.progress >= definition.Target)
                {
                    save.coins += definition.Reward;
                    save.missions.RemoveAt(i);
                    completed.Add(definition);
                }
            }
            if (completed.Count > 0)
            {
                EnsureActiveMissions(save);
            }
            return completed;
        }

        private static bool IsActive(SaveData save, string id)
        {
            foreach (var mission in save.missions)
            {
                if (mission.id == id)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
