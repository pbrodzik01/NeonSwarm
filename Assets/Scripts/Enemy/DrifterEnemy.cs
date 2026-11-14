using UnityEngine;

namespace NeonSwarm
{
    public class DrifterEnemy : EnemyBase
    {
        [SerializeField] private float waveAmplitude = 0.8f;
        [SerializeField] private float waveFrequency = 1.6f;
        [SerializeField] private float edgeMargin = 0.6f;
        [SerializeField] private float verticalDrift = 0.4f;

        private float horizontal = 1f;
        private float phase;

        protected override void OnSpawned()
        {
            horizontal = transform.position.x > 0f ? -1f : 1f;
            phase = Random.Range(0f, Mathf.PI * 2f);
        }

        protected override void Think()
        {
            var bounds = ArenaGenerator.Bounds;
            var x = transform.position.x;
            if (x > bounds.xMax - edgeMargin)
            {
                horizontal = -1f;
            }
            else if (x < bounds.xMin + edgeMargin)
            {
                horizontal = 1f;
            }
        }

        protected override void Tick()
        {
            var speed = moveSpeed * speedMultiplier;
            var vertical = Mathf.Sin(Time.time * waveFrequency + phase) * waveAmplitude;
            if (target != null)
            {
                vertical += Mathf.Clamp(target.position.y - transform.position.y, -1f, 1f) * verticalDrift;
            }
            desiredVelocity = new Vector2(horizontal * speed, vertical);
            visual.Rotate(0f, 0f, -horizontal * 120f * Time.deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer != Layers.Obstacle)
            {
                return;
            }
            var normal = collision.GetContact(0).normal;
            if (Mathf.Abs(normal.x) > 0.3f)
            {
                horizontal = Mathf.Sign(normal.x);
            }
            else
            {
                phase += Mathf.PI;
            }
        }
    }
}
