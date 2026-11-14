using UnityEngine;

namespace NeonSwarm.EditorTools
{
    internal static class LevelFactory
    {
        public static LevelConfig[] Create()
        {
            var level1 = EffectFactory.LoadOrCreate<LevelConfig>("Assets/Data/Levels/Level1.asset");
            Fill(level1, 0, "Poziom 1", "Pierwszy kontakt", "łatwa", new Color(0.15f, 0.91f, 1f),
                waves: 3, first: 3, perWave: 1, interval: 1.2f, maxAlive: 6, speed: 1f, fire: 1f,
                obstacles: 2, pickupInterval: 10f, coinChance: 0.65f, par: 75f,
                Weight(EnemyKind.Drifter, 3, 1),
                Weight(EnemyKind.Chaser, 1, 2));

            var level2 = EffectFactory.LoadOrCreate<LevelConfig>("Assets/Data/Levels/Level2.asset");
            Fill(level2, 1, "Poziom 2", "Gniazdo łowców", "średnia", new Color(1f, 0.24f, 0.8f),
                waves: 3, first: 6, perWave: 1, interval: 1f, maxAlive: 8, speed: 1.3f, fire: 1f,
                obstacles: 3, pickupInterval: 11f, coinChance: 0.55f, par: 110f,
                Weight(EnemyKind.Drifter, 2, 1),
                Weight(EnemyKind.Chaser, 3, 1),
                Weight(EnemyKind.Shooter, 1, 2));

            var level3 = EffectFactory.LoadOrCreate<LevelConfig>("Assets/Data/Levels/Level3.asset");
            Fill(level3, 2, "Poziom 3", "Serce roju", "trudna", new Color(1f, 0.55f, 0.15f),
                waves: 3, first: 10, perWave: 2, interval: 0.85f, maxAlive: 11, speed: 1.6f, fire: 1.3f,
                obstacles: 4, pickupInterval: 12f, coinChance: 0.5f, par: 150f,
                Weight(EnemyKind.Drifter, 1, 1),
                Weight(EnemyKind.Chaser, 3, 1),
                Weight(EnemyKind.Shooter, 2, 1));

            return new[] { level1, level2, level3 };
        }

        private static EnemySpawnWeight Weight(EnemyKind kind, int weight, int fromWave)
        {
            return new EnemySpawnWeight { kind = kind, weight = weight, fromWave = fromWave };
        }

        private static void Fill(LevelConfig config, int index, string title, string subtitle, string difficulty, Color accent,
            int waves, int first, int perWave, float interval, int maxAlive, float speed, float fire,
            int obstacles, float pickupInterval, float coinChance, float par, params EnemySpawnWeight[] enemies)
        {
            config.levelIndex = index;
            config.title = title;
            config.subtitle = subtitle;
            config.difficultyLabel = difficulty;
            config.accent = accent;
            config.waves = waves;
            config.enemiesFirstWave = first;
            config.enemiesPerWave = perWave;
            config.spawnInterval = interval;
            config.wavePause = 2.5f;
            config.maxAlive = maxAlive;
            config.speedMultiplier = speed;
            config.fireRateMultiplier = fire;
            config.obstacleCount = obstacles;
            config.pickupInterval = pickupInterval;
            config.coinDropChance = coinChance;
            config.parTimeSeconds = par;
            config.enemies = enemies;
            UnityEditor.EditorUtility.SetDirty(config);
        }
    }
}
