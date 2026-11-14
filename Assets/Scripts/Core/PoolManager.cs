using System.Collections.Generic;
using UnityEngine;

namespace NeonSwarm
{
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager instance;

        private readonly Dictionary<Poolable, ObjectPool> pools = new Dictionary<Poolable, ObjectPool>();

        private static PoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObject("[Pools]").AddComponent<PoolManager>();
                }
                return instance;
            }
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            var poolable = prefab.GetComponent<Poolable>();
            if (poolable == null)
            {
                return Instantiate(prefab, position, rotation);
            }
            var item = Instance.GetPool(poolable).Get(position, rotation);
            return item.GetComponent<T>();
        }

        public static T Spawn<T>(T prefab, Vector3 position) where T : Component
        {
            return Spawn(prefab, position, Quaternion.identity);
        }

        public static void Prewarm(Component prefab)
        {
            if (prefab == null)
            {
                return;
            }
            var poolable = prefab.GetComponent<Poolable>();
            if (poolable != null)
            {
                Instance.GetPool(poolable).Prewarm(poolable.PrewarmCount);
            }
        }

        public static void Despawn(Component item)
        {
            var poolable = item.GetComponent<Poolable>();
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                Destroy(item.gameObject);
            }
        }

        public static string Describe()
        {
            if (instance == null)
            {
                return "pools: 0";
            }
            var all = 0;
            var free = 0;
            foreach (var pool in instance.pools.Values)
            {
                all += pool.CountAll;
                free += pool.CountFree;
            }
            return $"pools: {instance.pools.Count}, obj: {all}, free: {free}";
        }

        private ObjectPool GetPool(Poolable prefab)
        {
            if (!pools.TryGetValue(prefab, out var pool))
            {
                var root = new GameObject(prefab.name + " Pool").transform;
                root.SetParent(transform, false);
                pool = new ObjectPool(prefab, root);
                pools.Add(prefab, pool);
            }
            return pool;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
