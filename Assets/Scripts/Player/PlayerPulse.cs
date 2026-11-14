using UnityEngine;

namespace NeonSwarm
{
    public class PlayerPulse : MonoBehaviour
    {
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        [SerializeField] private Animator animator;
        [SerializeField] private float baseCooldown = 8f;
        [SerializeField] private float radius = 2.8f;
        [SerializeField] private int damage = 2;
        [SerializeField] private float knockback = 3.5f;

        private readonly Collider2D[] hits = new Collider2D[48];
        private float cooldown;
        private float remaining;
        private PlayerHealth health;

        public bool Ready => remaining <= 0f && health.IsAlive;
        public float Progress => cooldown <= 0f ? 1f : 1f - remaining / cooldown;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            cooldown = baseCooldown;
        }

        public void Configure(float cooldownReduction)
        {
            cooldown = Mathf.Max(3f, baseCooldown - cooldownReduction);
            remaining = 0f;
        }

        public bool TryPulse()
        {
            if (!Ready || Time.timeScale <= 0f)
            {
                return false;
            }
            var origin = (Vector2)transform.position;
            var count = Physics2D.OverlapCircleNonAlloc(origin, radius, hits, Layers.Mask(Layers.Enemy, Layers.EnemyBullet));
            for (var i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.TryGetComponent<EnemyBase>(out var enemy))
                {
                    var push = ((Vector2)hit.transform.position - origin).normalized * knockback;
                    enemy.TakeDamage(damage, push);
                }
                else if (hit.TryGetComponent<Projectile>(out var projectile))
                {
                    projectile.Despawn();
                }
            }
            Fx.PulseRing(origin, radius, new Color(0.6f, 0.95f, 1f));
            AudioManager.Play(SfxId.Pulse);
            CameraRig.Shake(0.18f, 0.2f);
            animator.SetTrigger(AttackHash);
            remaining = cooldown;
            GameEvents.RaisePulseUsed();
            return true;
        }

        private void Update()
        {
            if (remaining > 0f)
            {
                remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            }
        }
    }
}
