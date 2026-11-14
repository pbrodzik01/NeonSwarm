using UnityEngine;

namespace NeonSwarm
{
    public class PlayerShooter : MonoBehaviour
    {
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        [SerializeField] private Projectile bulletPrefab;
        [SerializeField] private Animator animator;
        [SerializeField] private float fireInterval = 0.3f;
        [SerializeField] private float range = 9f;
        [SerializeField] private float muzzleDistance = 0.55f;
        [SerializeField] private float twinSpacing = 0.2f;

        private PlayerController controller;
        private float cooldown;
        private float intervalMultiplier = 1f;
        private float rapidUntil;
        private bool twin;
        private bool firing = true;

        public bool RapidActive => Time.time < rapidUntil;
        public float RapidRemaining => Mathf.Max(0f, rapidUntil - Time.time);
        public float Range => range;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
        }

        private void Start()
        {
            PoolManager.Prewarm(bulletPrefab);
        }

        public void Configure(float multiplier, bool twinCannon, Color tint)
        {
            intervalMultiplier = Mathf.Clamp(multiplier, 0.4f, 1f);
            twin = twinCannon;
        }

        public void SetFiring(bool enabled)
        {
            firing = enabled;
        }

        public void ActivateRapidFire(float seconds)
        {
            rapidUntil = Mathf.Max(rapidUntil, Time.time + seconds);
        }

        private void Update()
        {
            if (!firing)
            {
                return;
            }
            cooldown -= Time.deltaTime;
            if (cooldown > 0f)
            {
                return;
            }
            var target = EnemyRegistry.FindNearestVisible(transform.position, range, 1 << Layers.Obstacle);
            if (target == null)
            {
                return;
            }
            var direction = ((Vector2)(target.transform.position - transform.position)).normalized;
            controller.SetAim(direction);
            Fire(direction);
            cooldown = fireInterval * intervalMultiplier * (RapidActive ? 0.5f : 1f);
        }

        private void Fire(Vector2 direction)
        {
            var origin = (Vector2)transform.position + direction * muzzleDistance;
            if (twin)
            {
                var side = new Vector2(-direction.y, direction.x) * twinSpacing;
                Spawn(origin + side, direction);
                Spawn(origin - side, direction);
            }
            else
            {
                Spawn(origin, direction);
            }
            animator.SetTrigger(AttackHash);
            AudioManager.Play(SfxId.Shoot, 0.55f);
        }

        private void Spawn(Vector2 position, Vector2 direction)
        {
            PoolManager.Spawn(bulletPrefab, position).Launch(direction);
        }
    }
}
