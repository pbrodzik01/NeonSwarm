using UnityEngine;

namespace NeonSwarm
{
    [DisallowMultipleComponent]
    public class Poolable : MonoBehaviour
    {
        [SerializeField] private int prewarmCount = 8;

        internal ObjectPool Pool;
        internal bool IsInPool;

        public int PrewarmCount => prewarmCount;

        public void Release()
        {
            if (Pool != null)
            {
                Pool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
