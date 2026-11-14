using System;
using System.Collections;
using UnityEngine;

namespace NeonSwarm
{
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private DrifterEnemy drifterPrefab;
        [SerializeField] private ChaserEnemy chaserPrefab;
        [SerializeField] private ShooterEnemy shooterPrefab;
        [SerializeField] private float telegraphSeconds = 0.6f;
        [SerializeField] private float minSpawnDistance = 3.8f;

        private LevelConfig config;
        private ArenaGenerator arena;
        private Transform player;
        private DifficultyDirector difficulty;
        private System.Random random;
        private Coroutine routine;
        private int pendingSpawns;

        public int CurrentWave { get; private set; }
        public int TotalWaves => config != null ? config.waves : 0;
        public int RemainingInWave { get; private set; }
        public int PendingSpawns => pendingSpawns;

        public event Action<int> WaveStarted;
        public event Action AllWavesCleared;

        public void Begin(LevelConfig levelConfig, ArenaGenerator arenaGenerator, Transform playerTransform, DifficultyDirector director, int seed)
        {
            config = levelConfig;
            arena = arenaGenerator;
            player = playerTransform;
            difficulty = director;
            random = new System.Random(seed);
            EnemyRegistry.Clear();
            PoolManager.Prewarm(drifterPrefab);
            PoolManager.Prewarm(chaserPrefab);
            PoolManager.Prewarm(shooterPrefab);
            routine = StartCoroutine(Run());
        }

        public void Stop()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        private IEnumerator Run()
        {
            for (var wave = 0; wave < config.waves; wave++)
            {
                CurrentWave = wave + 1;
                WaveStarted?.Invoke(CurrentWave);
                yield return new WaitForSeconds(1.4f);

                var size = WavePlanner.WaveSize(config.enemiesFirstWave, config.enemiesPerWave, wave, difficulty.Intensity);
                var plan = WavePlanner.PlanWave(config.enemies, wave, size, random);
                RemainingInWave = plan.Count;

                foreach (var kind in plan)
                {
                    while (EnemyRegistry.Count + pendingSpawns >= config.maxAlive)
                    {
                        yield return null;
                    }
                    StartCoroutine(SpawnWithTelegraph(kind));
                    yield return new WaitForSeconds(config.spawnInterval * difficulty.SpawnIntervalFactor);
                }

                while (EnemyRegistry.Count > 0 || pendingSpawns > 0)
                {
                    yield return null;
                }
                RemainingInWave = 0;

                if (wave < config.waves - 1)
                {
                    yield return new WaitForSeconds(config.wavePause);
                }
            }
            routine = null;
            AllWavesCleared?.Invoke();
        }

        private void OnEnable()
        {
            GameEvents.EnemyKilled += OnEnemyKilled;
        }

        private void OnDisable()
        {
            GameEvents.EnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemyKilled(EnemyKind kind, Vector3 position)
        {
            RemainingInWave = Mathf.Max(0, RemainingInWave - 1);
        }

        private IEnumerator SpawnWithTelegraph(EnemyKind kind)
        {
            pendingSpawns++;
            var prefab = PrefabFor(kind);
            var position = arena.RandomSpawnPoint(player.position, minSpawnDistance);
            Fx.Portal(position, prefab.Color);
            AudioManager.Play(SfxId.Spawn, 0.5f);
            yield return new WaitForSeconds(telegraphSeconds);
            var enemy = PoolManager.Spawn(prefab, position);
            var speed = config.speedMultiplier * difficulty.SpeedFactor;
            var fire = config.fireRateMultiplier * difficulty.FireRateFactor;
            enemy.Init(player, speed, fire, config.coinDropChance);
            pendingSpawns--;
        }

        private EnemyBase PrefabFor(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Chaser:
                    return chaserPrefab;
                case EnemyKind.Shooter:
                    return shooterPrefab;
                default:
                    return drifterPrefab;
            }
        }
    }
}
