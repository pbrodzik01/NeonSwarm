using System.Collections.Generic;
using UnityEngine;

namespace NeonSwarm
{
    public static class EnemyRegistry
    {
        private static readonly List<EnemyBase> Active = new List<EnemyBase>(64);

        public static int Count => Active.Count;

        public static void Register(EnemyBase enemy)
        {
            if (!Active.Contains(enemy))
            {
                Active.Add(enemy);
            }
        }

        public static void Unregister(EnemyBase enemy)
        {
            Active.Remove(enemy);
        }

        public static void Clear()
        {
            Active.Clear();
        }

        public static EnemyBase FindNearest(Vector2 position, float maxDistance)
        {
            EnemyBase nearest = null;
            var best = maxDistance * maxDistance;
            for (var i = 0; i < Active.Count; i++)
            {
                var enemy = Active[i];
                var distance = ((Vector2)enemy.transform.position - position).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        public static EnemyBase FindNearestVisible(Vector2 position, float maxDistance, int obstacleMask)
        {
            EnemyBase nearest = null;
            var best = maxDistance * maxDistance;
            for (var i = 0; i < Active.Count; i++)
            {
                var enemy = Active[i];
                var enemyPosition = (Vector2)enemy.transform.position;
                var distance = (enemyPosition - position).sqrMagnitude;
                if (distance < best && Physics2D.Linecast(position, enemyPosition, obstacleMask).collider == null)
                {
                    best = distance;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        public static void ForEach(System.Action<EnemyBase> action)
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                action(Active[i]);
            }
        }
    }
}
