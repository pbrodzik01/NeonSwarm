using UnityEngine;

namespace NeonSwarm
{
    public class PickupSpawner : MonoBehaviour
    {
        [SerializeField] private CoinPickup coinPrefab;
        [SerializeField] private MedkitPickup medkitPrefab;
        [SerializeField] private ShieldPickup shieldPrefab;
        [SerializeField] private RapidFirePickup rapidPrefab;

        private ArenaGenerator arena;
        private float interval = 11f;
        private float timer;
        private bool running;

        public static PickupSpawner Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Begin(ArenaGenerator arenaGenerator, float spawnInterval)
        {
            arena = arenaGenerator;
            interval = spawnInterval;
            timer = interval * 0.6f;
            running = true;
            PoolManager.Prewarm(coinPrefab);
            PoolManager.Prewarm(medkitPrefab);
            PoolManager.Prewarm(shieldPrefab);
            PoolManager.Prewarm(rapidPrefab);
        }

        public void Stop()
        {
            running = false;
        }

        public void DropCoins(Vector2 position, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var offset = count > 1 ? Random.insideUnitCircle * 0.4f : Vector2.zero;
                PoolManager.Spawn(coinPrefab, ArenaGenerator.ClampInside(position + offset, 0.4f));
            }
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }
            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }
            timer = interval * Random.Range(0.8f, 1.2f);
            SpawnRandom();
        }

        private void SpawnRandom()
        {
            var point = arena.RandomFreePoint(1.2f);
            var player = Player.Instance;
            var lowHealth = player != null && player.Health.Current <= 1;
            var roll = Random.value;
            if (lowHealth && roll < 0.5f)
            {
                PoolManager.Spawn(medkitPrefab, point);
            }
            else if (roll < 0.3f)
            {
                PoolManager.Spawn(medkitPrefab, point);
            }
            else if (roll < 0.55f)
            {
                PoolManager.Spawn(shieldPrefab, point);
            }
            else if (roll < 0.8f)
            {
                PoolManager.Spawn(rapidPrefab, point);
            }
            else
            {
                DropCoins(point, 4);
            }
        }
    }
}
