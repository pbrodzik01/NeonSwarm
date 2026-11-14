using System;
using UnityEngine;

namespace NeonSwarm
{
    [Serializable]
    public class EnemySpawnWeight
    {
        public EnemyKind kind;
        public int weight = 1;
        public int fromWave = 1;
    }

    [CreateAssetMenu(menuName = "Neon Swarm/Level Config", fileName = "LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        public int levelIndex;
        public string title = "Poziom 1";
        public string subtitle = "Rozgrzewka";
        public string difficultyLabel = "łatwa";
        public Color accent = new Color(0.15f, 0.91f, 1f);

        [Header("Waves")]
        public int waves = 3;
        public int enemiesFirstWave = 3;
        public int enemiesPerWave = 1;
        public float spawnInterval = 1.1f;
        public float wavePause = 2.5f;
        public int maxAlive = 8;

        [Header("Enemies")]
        public float speedMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public EnemySpawnWeight[] enemies = Array.Empty<EnemySpawnWeight>();

        [Header("Arena & pickups")]
        public int obstacleCount = 2;
        public float pickupInterval = 11f;
        [Range(0f, 1f)] public float coinDropChance = 0.6f;

        [Header("Scoring")]
        public float parTimeSeconds = 90f;

        public int TotalEnemiesAtBaseline
        {
            get
            {
                var total = 0;
                for (var wave = 0; wave < waves; wave++)
                {
                    total += enemiesFirstWave + enemiesPerWave * wave;
                }
                return total;
            }
        }
    }
}
