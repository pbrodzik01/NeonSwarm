using UnityEngine;

namespace NeonSwarm
{
    public class PooledEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private SpriteRenderer ring;
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private float ringStartScale = 0.2f;
        [SerializeField] private float ringEndScale = 2f;
        [SerializeField] private bool ringSpins;

        private float age;
        private float scale = 1f;
        private Color color = Color.white;
        private Poolable poolable;

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        public void Play(Color tint, float sizeMultiplier)
        {
            color = tint;
            scale = sizeMultiplier;
            age = 0f;
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(tint, Color.Lerp(tint, Color.white, 0.6f));
                particles.transform.localScale = Vector3.one * Mathf.Max(0.3f, sizeMultiplier);
                particles.Clear(true);
                particles.Play(true);
            }
            UpdateRing();
        }

        private void Update()
        {
            age += Time.deltaTime;
            UpdateRing();
            if (age >= duration)
            {
                if (poolable != null)
                {
                    poolable.Release();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        private void UpdateRing()
        {
            if (ring == null)
            {
                return;
            }
            var t = Mathf.Clamp01(age / duration);
            var eased = 1f - (1f - t) * (1f - t);
            var size = Mathf.Lerp(ringStartScale, ringEndScale, eased) * scale;
            ring.transform.localScale = new Vector3(size, size, 1f);
            if (ringSpins)
            {
                ring.transform.localRotation = Quaternion.Euler(0f, 0f, age * 360f);
            }
            var faded = color;
            faded.a = 1f - t;
            ring.color = faded;
        }
    }
}
