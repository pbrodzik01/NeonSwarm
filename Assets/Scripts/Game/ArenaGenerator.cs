using System.Collections.Generic;
using UnityEngine;

namespace NeonSwarm
{
    public class ArenaGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(8.4f, 13.6f);
        [SerializeField] private Transform obstaclePrefab;
        [SerializeField] private Transform obstacleRoot;
        [SerializeField] private float playerSafeRadius = 2.4f;
        [SerializeField] private float obstacleSpacing = 2.3f;

        private readonly List<Vector2> obstacles = new List<Vector2>();

        public static Rect Bounds { get; private set; } = new Rect(-4.2f, -6.8f, 8.4f, 13.6f);

        public Vector2 Size => size;

        private void Awake()
        {
            Bounds = new Rect(-size.x * 0.5f, -size.y * 0.5f, size.x, size.y);
        }

        public void Generate(int count, int seed)
        {
            obstacles.Clear();
            if (obstaclePrefab == null || count <= 0)
            {
                return;
            }
            var random = new System.Random(seed);
            var inner = Shrink(Bounds, 1.3f);
            var attempts = 0;
            while (obstacles.Count < count && attempts++ < count * 40)
            {
                var point = RandomPoint(random, inner);
                if (point.magnitude < playerSafeRadius || !FarFromObstacles(point, obstacleSpacing))
                {
                    continue;
                }
                obstacles.Add(point);
                var obstacle = Instantiate(obstaclePrefab, point, Quaternion.identity, obstacleRoot != null ? obstacleRoot : transform);
                var scale = 0.85f + (float)random.NextDouble() * 0.45f;
                obstacle.localScale = new Vector3(scale, scale, 1f);
            }
        }

        public Vector2 RandomSpawnPoint(Vector2 avoid, float minDistance)
        {
            var edge = Shrink(Bounds, 0.7f);
            var best = Vector2.zero;
            var bestDistance = -1f;
            for (var i = 0; i < 16; i++)
            {
                var point = RandomEdgePoint(edge);
                if (!FarFromObstacles(point, 1.2f))
                {
                    continue;
                }
                var distance = Vector2.Distance(point, avoid);
                if (distance >= minDistance)
                {
                    return point;
                }
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = point;
                }
            }
            return bestDistance >= 0f ? best : new Vector2(0f, edge.yMax);
        }

        public Vector2 RandomFreePoint(float margin)
        {
            var inner = Shrink(Bounds, margin);
            for (var i = 0; i < 20; i++)
            {
                var point = new Vector2(Random.Range(inner.xMin, inner.xMax), Random.Range(inner.yMin, inner.yMax));
                if (FarFromObstacles(point, 1.1f))
                {
                    return point;
                }
            }
            return Vector2.zero;
        }

        public static Vector2 ClampInside(Vector2 point, float margin)
        {
            var inner = Shrink(Bounds, margin);
            return new Vector2(Mathf.Clamp(point.x, inner.xMin, inner.xMax), Mathf.Clamp(point.y, inner.yMin, inner.yMax));
        }

        private bool FarFromObstacles(Vector2 point, float distance)
        {
            foreach (var obstacle in obstacles)
            {
                if ((obstacle - point).sqrMagnitude < distance * distance)
                {
                    return false;
                }
            }
            return true;
        }

        private static Vector2 RandomPoint(System.Random random, Rect area)
        {
            return new Vector2(
                area.xMin + (float)random.NextDouble() * area.width,
                area.yMin + (float)random.NextDouble() * area.height);
        }

        private static Vector2 RandomEdgePoint(Rect area)
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    return new Vector2(Random.Range(area.xMin, area.xMax), area.yMax);
                case 1:
                    return new Vector2(Random.Range(area.xMin, area.xMax), area.yMin);
                case 2:
                    return new Vector2(area.xMin, Random.Range(area.yMin, area.yMax));
                default:
                    return new Vector2(area.xMax, Random.Range(area.yMin, area.yMax));
            }
        }

        private static Rect Shrink(Rect rect, float margin)
        {
            return new Rect(rect.xMin + margin, rect.yMin + margin, rect.width - margin * 2f, rect.height - margin * 2f);
        }
    }
}
