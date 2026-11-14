using System.Collections;
using UnityEngine;

namespace NeonSwarm
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        protected static readonly int SpeedHash = Animator.StringToHash("Speed");
        protected static readonly int AttackHash = Animator.StringToHash("Attack");
        protected static readonly int HurtHash = Animator.StringToHash("Hurt");
        protected static readonly int DeadHash = Animator.StringToHash("Dead");

        [SerializeField] protected EnemyKind kind;
        [SerializeField] protected int maxHp = 1;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float acceleration = 12f;
        [SerializeField] protected int contactDamage = 1;
        [SerializeField] protected Color color = Color.magenta;
        [SerializeField] protected Animator animator;
        [SerializeField] protected Transform visual;
        [SerializeField] protected SpriteRenderer flash;
        [SerializeField] private float thinkInterval = 0.2f;

        protected Rigidbody2D body;
        protected Transform target;
        protected float speedMultiplier = 1f;
        protected float fireRateMultiplier = 1f;
        protected Vector2 desiredVelocity;

        private Collider2D hitbox;
        private Poolable poolable;
        private int hp;
        private bool alive;
        private float thinkTimer;
        private float coinDropChance;

        public EnemyKind Kind => kind;
        public bool IsAlive => alive;
        public Color Color => color;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<Collider2D>();
            poolable = GetComponent<Poolable>();
        }

        protected virtual void OnEnable()
        {
            hp = maxHp;
            alive = true;
            hitbox.enabled = true;
            body.velocity = Vector2.zero;
            desiredVelocity = Vector2.zero;
            thinkTimer = Random.Range(0f, thinkInterval);
            ResetVisualState();
            EnemyRegistry.Register(this);
        }

        private void ResetVisualState()
        {
            visual.localScale = Vector3.one;
            visual.localRotation = Quaternion.identity;
            if (flash != null)
            {
                var c = flash.color;
                c.a = 0f;
                flash.color = c;
            }
            animator.SetBool(DeadHash, false);
            animator.Rebind();
            animator.Update(0f);
        }

        protected virtual void OnDisable()
        {
            EnemyRegistry.Unregister(this);
        }

        public virtual void Init(Transform player, float speedFactor, float fireFactor, float dropChance)
        {
            target = player;
            speedMultiplier = speedFactor;
            fireRateMultiplier = fireFactor;
            coinDropChance = dropChance;
            OnSpawned();
        }

        protected virtual void OnSpawned()
        {
        }

        protected abstract void Think();

        protected virtual void Tick()
        {
        }

        private void Update()
        {
            if (!alive)
            {
                return;
            }
            thinkTimer -= Time.deltaTime;
            if (thinkTimer <= 0f)
            {
                thinkTimer = thinkInterval;
                KeepInsideArena();
                if (visual.localScale.x < 0.3f)
                {
                    ResetVisualState();
                }
                Think();
            }
            Tick();
            animator.SetFloat(SpeedHash, body.velocity.magnitude);
        }

        private void FixedUpdate()
        {
            if (!alive)
            {
                return;
            }
            body.velocity = Vector2.MoveTowards(body.velocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
        }

        public void TakeDamage(int amount, Vector2 knockback)
        {
            if (!alive)
            {
                return;
            }
            hp -= amount;
            body.AddForce(knockback, ForceMode2D.Impulse);
            if (hp <= 0)
            {
                Die();
                return;
            }
            animator.SetTrigger(HurtHash);
            AudioManager.Play(SfxId.Hit, 0.7f);
        }

        protected Vector2 ToTarget(out float distance)
        {
            if (target == null)
            {
                distance = float.MaxValue;
                return Vector2.zero;
            }
            var delta = (Vector2)(target.position - transform.position);
            distance = delta.magnitude;
            return distance > 0.0001f ? delta / distance : Vector2.zero;
        }

        protected void Face(Vector2 direction, float degreesPerSecond = 540f)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            visual.rotation = Quaternion.RotateTowards(visual.rotation, Quaternion.Euler(0f, 0f, angle), degreesPerSecond * Time.deltaTime);
        }

        protected Vector2 AvoidObstacles(Vector2 direction, float lookAhead = 1.6f)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return direction;
            }
            var radius = hitbox is CircleCollider2D circle ? circle.radius * 0.9f : 0.3f;
            var hit = Physics2D.CircleCast(body.position, radius, direction, lookAhead, ObstacleMask);
            if (hit.collider == null)
            {
                return direction;
            }
            var tangent = new Vector2(-hit.normal.y, hit.normal.x);
            if (Vector2.Dot(tangent, direction) < 0f)
            {
                tangent = -tangent;
            }
            return (tangent + hit.normal * 0.35f + direction * 0.25f).normalized;
        }

        private static readonly int ObstacleMask = 1 << Layers.Obstacle;

        private void KeepInsideArena()
        {
            var position = body.position;
            if (ArenaGenerator.Bounds.Contains(position))
            {
                return;
            }
            body.position = ArenaGenerator.ClampInside(position, 0.5f);
            body.velocity = Vector2.zero;
        }

        protected bool PlayerAlive => target != null && Player.Instance != null && Player.Instance.Health.IsAlive;

        private void Die()
        {
            alive = false;
            hitbox.enabled = false;
            desiredVelocity = Vector2.zero;
            body.velocity = Vector2.zero;
            EnemyRegistry.Unregister(this);
            animator.SetBool(DeadHash, true);

            var position = transform.position;
            var points = ScoreRules.ForEnemy(kind);
            Fx.Explosion(position, color);
            AudioManager.Play(SfxId.Explosion, 0.8f);
            CameraRig.Shake(0.08f, 0.12f);
            ScoreManager.Add(points, position);
            if (Random.value < coinDropChance && PickupSpawner.Instance != null)
            {
                PickupSpawner.Instance.DropCoins(position, kind == EnemyKind.Shooter ? 2 : 1);
            }
            GameEvents.RaiseEnemyKilled(kind, position);
            StartCoroutine(ReleaseAfterDeath());
        }

        private IEnumerator ReleaseAfterDeath()
        {
            yield return new WaitForSeconds(0.4f);
            ResetVisualState();
            if (poolable != null)
            {
                poolable.Release();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!alive || contactDamage <= 0 || !collision.collider.CompareTag(Tags.Player))
            {
                return;
            }
            if (collision.collider.TryGetComponent<PlayerHealth>(out var health))
            {
                var push = ((Vector2)(collision.transform.position - transform.position)).normalized * 4f;
                health.TakeDamage(contactDamage, push);
            }
        }
    }
}
