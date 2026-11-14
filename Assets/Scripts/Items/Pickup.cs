using UnityEngine;

namespace NeonSwarm
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class Pickup : MonoBehaviour
    {
        [SerializeField] protected Color color = Color.white;
        [SerializeField] private Transform visual;
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private bool magnetic;
        [SerializeField] private float magnetSpeed = 9f;

        private float age;
        private float bobPhase;
        private Poolable poolable;
        private bool collected;

        public abstract PickupKind Kind { get; }

        protected abstract void Apply(Player player);

        private void Awake()
        {
            poolable = GetComponent<Poolable>();
        }

        private void OnEnable()
        {
            age = 0f;
            collected = false;
            bobPhase = Random.Range(0f, Mathf.PI * 2f);
            if (visual != null)
            {
                visual.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (visual != null)
            {
                visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 4f + bobPhase) * 0.08f, 0f);
                var blink = lifetime - age < 2.5f && Mathf.Repeat(age * 6f, 1f) < 0.35f;
                visual.gameObject.SetActive(!blink);
            }
            if (magnetic)
            {
                PullTowardsPlayer();
            }
            if (age >= lifetime)
            {
                Release();
            }
        }

        private void PullTowardsPlayer()
        {
            var player = Player.Instance;
            if (player == null || !player.Health.IsAlive)
            {
                return;
            }
            var delta = player.transform.position - transform.position;
            var radius = player.MagnetRadius;
            if (delta.sqrMagnitude < radius * radius)
            {
                transform.position += delta.normalized * (magnetSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || !other.CompareTag(Tags.Player) || !other.TryGetComponent<Player>(out var player) || !player.Health.IsAlive)
            {
                return;
            }
            collected = true;
            Apply(player);
            Fx.Sparkle(transform.position, color);
            GameEvents.RaisePickupCollected(Kind);
            Release();
        }

        private void Release()
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
}
