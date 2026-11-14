using UnityEngine;

namespace NeonSwarm
{
    public class ChaserEnemy : EnemyBase
    {
        [SerializeField] private float dashRange = 2.8f;
        [SerializeField] private float dashSpeedMultiplier = 2.6f;
        [SerializeField] private float dashDuration = 0.35f;
        [SerializeField] private float dashCooldown = 2.2f;

        private float dashTimer;
        private float dashCooldownTimer;
        private Vector2 heading;

        protected override void OnSpawned()
        {
            dashTimer = 0f;
            dashCooldownTimer = dashCooldown * 0.5f;
        }

        protected override void Think()
        {
            if (!PlayerAlive)
            {
                heading = Vector2.zero;
                return;
            }
            var direction = ToTarget(out var distance);
            if (dashTimer <= 0f)
            {
                heading = AvoidObstacles(direction);
            }
            if (distance < dashRange && dashCooldownTimer <= 0f)
            {
                heading = direction;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown / fireRateMultiplier;
                animator.SetTrigger(AttackHash);
            }
        }

        protected override void Tick()
        {
            dashTimer -= Time.deltaTime;
            dashCooldownTimer -= Time.deltaTime;
            var speed = moveSpeed * speedMultiplier * (dashTimer > 0f ? dashSpeedMultiplier : 1f);
            desiredVelocity = heading * speed;
            Face(heading, dashTimer > 0f ? 1080f : 420f);
        }
    }
}
