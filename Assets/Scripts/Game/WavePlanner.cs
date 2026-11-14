using System.Collections.Generic;
using UnityEngine;

namespace NeonSwarm
{
    public static class WavePlanner
    {
        public static int WaveSize(int firstWave, int perWave, int waveIndex, float intensity)
        {
            var baseline = firstWave + perWave * waveIndex;
            var scaled = baseline * Mathf.Lerp(1f, intensity, 0.6f);
            return Mathf.Max(1, Mathf.RoundToInt(scaled));
        }

        public static List<EnemyKind> PlanWave(IList<EnemySpawnWeight> weights, int waveIndex, int count, System.Random random)
        {
            var available = new List<EnemySpawnWeight>();
            var totalWeight = 0;
            foreach (var entry in weights)
            {
                if (entry.weight > 0 && entry.fromWave <= waveIndex + 1)
                {
                    available.Add(entry);
                    totalWeight += entry.weight;
                }
            }

            var plan = new List<EnemyKind>(count);
            if (available.Count == 0)
            {
                for (var i = 0; i < count; i++)
                {
                    plan.Add(EnemyKind.Drifter);
                }
                return plan;
            }

            foreach (var entry in available)
            {
                if (entry.fromWave == waveIndex + 1 && waveIndex > 0 && plan.Count < count)
                {
                    plan.Add(entry.kind);
                }
            }

            while (plan.Count < count)
            {
                var roll = random.Next(totalWeight);
                foreach (var entry in available)
                {
                    roll -= entry.weight;
                    if (roll < 0)
                    {
                        plan.Add(entry.kind);
                        break;
                    }
                }
            }

            for (var i = plan.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (plan[i], plan[j]) = (plan[j], plan[i]);
            }
            return plan;
        }
    }

    public sealed class DifficultyModel
    {
        public const float Min = 0.8f;
        public const float Max = 1.4f;
        public const float Step = 0.1f;

        public float Intensity { get; private set; } = 1f;

        public float SpeedFactor => 1f + (Intensity - 1f) * 0.5f;
        public float SpawnIntervalFactor => 1f / Intensity;
        public float FireRateFactor => 1f + (Intensity - 1f) * 0.7f;

        public void Reset(float value = 1f)
        {
            Intensity = Mathf.Clamp(value, Min, Max);
        }

        public float Evaluate(float healthRatio, float killsPerMinute, float secondsSinceHit)
        {
            var target = 1f;
            if (healthRatio >= 0.99f && secondsSinceHit > 15f)
            {
                target += 0.2f;
            }
            if (killsPerMinute > 24f)
            {
                target += 0.2f;
            }
            if (healthRatio <= 0.34f)
            {
                target -= 0.25f;
            }
            if (secondsSinceHit < 4f)
            {
                target -= 0.1f;
            }
            Intensity = Mathf.Clamp(Mathf.MoveTowards(Intensity, target, Step), Min, Max);
            return Intensity;
        }
    }
}
