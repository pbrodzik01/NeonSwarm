using UnityEngine;

namespace NeonSwarm
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 13f;
        [SerializeField] private float lifetime = 1.6f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float knockback = 1.5f;
        [SerializeField] private Color impactColor = Color.cyan;

        private Rigidbody2D body;
        private Poolable poolable;
        private float age;
        private bool spent;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            poolable = GetComponent<Poolable>();
        }

        public void Launch(Vector2 direction, float speedMultiplier = 1f)
        {
            var dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            transform.up = dir;
            body.velocity = dir * (speed * speedMultiplier);
            age = 0f;
            spent = false;
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (spent)
            {
                return;
            }
            if (other.gameObject.layer == Layers.Obstacle)
            {
                Fx.Hit(transform.position, impactColor);
                Despawn();
                return;
            }
            if (other.TryGetComponent<IDamageable>(out var target) && target.IsAlive)
            {
                target.TakeDamage(damage, body.velocity.normalized * knockback);
                Fx.Hit(transform.position, impactColor);
                Despawn();
            }
        }

        public void Despawn()
        {
            if (spent)
            {
                return;
            }
            spent = true;
            body.velocity = Vector2.zero;
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
}
