using System.Collections.Generic;
using UnityEngine;

namespace NeonSwarm
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(int amount, Vector2 knockback);
    }

    public sealed class ObjectPool
    {
        private readonly Poolable prefab;
        private readonly Transform root;
        private readonly Stack<Poolable> free = new Stack<Poolable>();

        public int CountAll { get; private set; }
        public int CountFree => free.Count;

        public ObjectPool(Poolable prefab, Transform root)
        {
            this.prefab = prefab;
            this.root = root;
        }

        public void Prewarm(int count)
        {
            for (var i = CountAll; i < count; i++)
            {
                free.Push(Create());
            }
        }

        public Poolable Get(Vector3 position, Quaternion rotation)
        {
            var item = free.Count > 0 ? free.Pop() : Create();
            var t = item.transform;
            t.SetPositionAndRotation(position, rotation);
            item.IsInPool = false;
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(Poolable item)
        {
            if (item.IsInPool)
            {
                return;
            }
            item.IsInPool = true;
            item.gameObject.SetActive(false);
            free.Push(item);
        }

        private Poolable Create()
        {
            var item = Object.Instantiate(prefab, root);
            item.gameObject.SetActive(false);
            item.name = prefab.name;
            item.Pool = this;
            item.IsInPool = true;
            CountAll++;
            return item;
        }
    }
}
