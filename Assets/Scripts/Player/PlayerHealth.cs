using UnityEngine;

namespace NeonSwarm
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        [SerializeField] private int baseMaxHp = 3;
        [SerializeField] private float hitInvulnerability = 1.2f;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer shieldBubble;

        private int current;
        private int max;
        private float invulnerableUntil;
        private float shieldUntil;
        private bool dead;
        private Rigidbody2D rigidBody;
        private Collider2D hitbox;

        public int Current => current;
        public int Max => max;
        public bool IsAlive => !dead;
        public bool TookDamage { get; private set; }
        public float LastHitTime { get; private set; } = -999f;
        public bool IsShielded => Time.time < shieldUntil;
        public float ShieldRemaining => Mathf.Max(0f, shieldUntil - Time.time);

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<Collider2D>();
            max = baseMaxHp;
            current = max;
        }

        public void Configure(int bonusHp)
        {
            max = baseMaxHp + Mathf.Max(0, bonusHp);
            current = max;
            dead = false;
            TookDamage = false;
            GameEvents.RaisePlayerHealthChanged(current, max);
        }

        public void TakeDamage(int amount, Vector2 knockback)
        {
            if (dead || amount <= 0)
            {
                return;
            }
            if (IsShielded)
            {
                Fx.Hit(transform.position, new Color(0.4f, 0.7f, 1f));
                return;
            }
            if (Time.time < invulnerableUntil)
            {
                return;
            }

            current = Mathf.Max(0, current - amount);
            TookDamage = true;
            LastHitTime = Time.time;
            invulnerableUntil = Time.time + hitInvulnerability;
            rigidBody.AddForce(knockback, ForceMode2D.Impulse);
            animator.SetTrigger(HurtHash);
            AudioManager.Play(SfxId.PlayerHurt);
            CameraRig.Shake(0.22f, 0.25f);
            AppSettings.Vibrate();
            GameEvents.RaisePlayerDamaged();
            GameEvents.RaisePlayerHealthChanged(current, max);

            if (current == 0)
            {
                Die();
            }
        }

        public bool Heal(int amount)
        {
            if (dead || current >= max)
            {
                return false;
            }
            current = Mathf.Min(max, current + amount);
            GameEvents.RaisePlayerHealthChanged(current, max);
            return true;
        }

        public void ActivateShield(float seconds)
        {
            shieldUntil = Mathf.Max(shieldUntil, Time.time + seconds);
        }

        public void MakeInvulnerable(float seconds)
        {
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
        }

        private void Update()
        {
            if (shieldBubble != null)
            {
                var shielded = IsShielded && !dead;
                shieldBubble.enabled = shielded;
                if (shielded)
                {
                    var blink = ShieldRemaining < 1.2f && Mathf.Repeat(Time.time * 8f, 1f) < 0.5f;
                    var c = shieldBubble.color;
                    c.a = blink ? 0.25f : 0.55f + Mathf.Sin(Time.time * 6f) * 0.15f;
                    shieldBubble.color = c;
                }
            }
            if (body != null)
            {
                var flicker = !dead && Time.time < invulnerableUntil && Mathf.Repeat(Time.time * 12f, 1f) < 0.5f;
                body.enabled = !flicker;
            }
        }

        private void Die()
        {
            dead = true;
            if (body != null)
            {
                body.enabled = true;
            }
            hitbox.enabled = false;
            rigidBody.velocity = Vector2.zero;
            animator.SetBool(DeadHash, true);
            Fx.Explosion(transform.position, body != null ? body.color : Color.cyan, 1.6f);
            AudioManager.Play(SfxId.Explosion);
            CameraRig.Shake(0.45f, 0.5f);
            GetComponent<Player>().SetControlsEnabled(false);
            GameEvents.RaisePlayerDied();
        }
    }
}
