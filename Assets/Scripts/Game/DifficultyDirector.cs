using System.Collections.Generic;
using UnityEngine;

namespace NeonSwarm
{
    public class DifficultyDirector : MonoBehaviour
    {
        [SerializeField] private float evaluateEvery = 4f;
        [SerializeField] private float killWindowSeconds = 30f;

        private readonly DifficultyModel model = new DifficultyModel();
        private readonly Queue<float> killTimes = new Queue<float>();
        private PlayerHealth health;
        private float timer;
        private bool running;

        public float Intensity => model.Intensity;
        public float SpeedFactor => model.SpeedFactor;
        public float SpawnIntervalFactor => model.SpawnIntervalFactor;
        public float FireRateFactor => model.FireRateFactor;

        private void OnEnable()
        {
            GameEvents.EnemyKilled += OnEnemyKilled;
        }

        private void OnDisable()
        {
            GameEvents.EnemyKilled -= OnEnemyKilled;
        }

        public void Begin(PlayerHealth playerHealth)
        {
            health = playerHealth;
            model.Reset();
            killTimes.Clear();
            timer = evaluateEvery;
            running = true;
        }

        public void Stop()
        {
            running = false;
        }

        private void OnEnemyKilled(EnemyKind kind, Vector3 position)
        {
            killTimes.Enqueue(Time.time);
        }

        private void Update()
        {
            if (!running || health == null)
            {
                return;
            }
            timer -= Time.deltaTime;
            if (timer > 0f)
            {
                return;
            }
            timer = evaluateEvery;
            while (killTimes.Count > 0 && Time.time - killTimes.Peek() > killWindowSeconds)
            {
                killTimes.Dequeue();
            }
            var killsPerMinute = killTimes.Count * (60f / killWindowSeconds);
            var healthRatio = health.Max > 0 ? (float)health.Current / health.Max : 1f;
            model.Evaluate(healthRatio, killsPerMinute, Time.time - health.LastHitTime);
        }
    }
}
