using UnityEngine;

namespace NeonSwarm
{
    public class ShooterEnemy : EnemyBase
    {
        [SerializeField] private Projectile bulletPrefab;
        [SerializeField] private float preferredDistance = 4.6f;
        [SerializeField] private float distanceTolerance = 1f;
        [SerializeField] private float fireInterval = 2.4f;
        [SerializeField] private float windUp = 0.4f;
        [SerializeField] private float bulletSpeedMultiplier = 1f;

        private float fireTimer;
        private float windUpTimer = -1f;
        private float strafe = 1f;
        private Vector2 aim;

        protected override void Awake()
        {
            base.Awake();
            PoolManager.Prewarm(bulletPrefab);
        }

        protected override void OnSpawned()
        {
            fireTimer = fireInterval * Random.Range(0.6f, 1f);
            windUpTimer = -1f;
            strafe = Random.value < 0.5f ? -1f : 1f;
        }

        protected override void Think()
        {
            if (!PlayerAlive)
            {
                desiredVelocity = Vector2.zero;
                return;
            }
            var direction = ToTarget(out var distance);
            aim = direction;
            var speed = moveSpeed * speedMultiplier;
            if (windUpTimer > 0f)
            {
                desiredVelocity = Vector2.zero;
            }
            else if (distance > preferredDistance + distanceTolerance || !HasLineOfSight())
            {
                desiredVelocity = AvoidObstacles(direction) * speed;
            }
            else if (distance < preferredDistance - distanceTolerance)
            {
                desiredVelocity = AvoidObstacles(-direction) * speed;
            }
            else
            {
                if (Random.value < 0.08f)
                {
                    strafe = -strafe;
                }
                desiredVelocity = new Vector2(-direction.y, direction.x) * (strafe * speed * 0.7f);
            }
        }

        protected override void Tick()
        {
            Face(aim, 360f);
            if (!PlayerAlive)
            {
                return;
            }
            if (windUpTimer > 0f)
            {
                windUpTimer -= Time.deltaTime;
                if (windUpTimer <= 0f)
                {
                    Shoot();
                }
                return;
            }
            fireTimer -= Time.deltaTime * fireRateMultiplier;
            if (fireTimer <= 0f)
            {
                fireTimer = fireInterval;
                windUpTimer = windUp;
                animator.SetTrigger(AttackHash);
            }
        }

        private bool HasLineOfSight()
        {
            return target != null && Physics2D.Linecast(transform.position, target.position, 1 << Layers.Obstacle).collider == null;
        }

        private void Shoot()
        {
            windUpTimer = -1f;
            var direction = ToTarget(out _);
            var origin = (Vector2)transform.position + direction * 0.5f;
            PoolManager.Spawn(bulletPrefab, origin).Launch(direction, bulletSpeedMultiplier * Mathf.Lerp(1f, speedMultiplier, 0.5f));
            AudioManager.Play(SfxId.EnemyShoot, 0.7f);
        }
    }
}
