using UnityEngine;

namespace NeonSwarm
{
    [RequireComponent(typeof(PlayerController), typeof(PlayerHealth), typeof(PlayerShooter))]
    [RequireComponent(typeof(PlayerPulse))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;

        public static Player Instance { get; private set; }

        public PlayerController Controller { get; private set; }
        public PlayerHealth Health { get; private set; }
        public PlayerShooter Shooter { get; private set; }
        public PlayerPulse Pulse { get; private set; }
        public float MagnetRadius { get; private set; } = 1.3f;
        public Color SkinColor { get; private set; } = Color.cyan;

        private void Awake()
        {
            Instance = this;
            Controller = GetComponent<PlayerController>();
            Health = GetComponent<PlayerHealth>();
            Shooter = GetComponent<PlayerShooter>();
            Pulse = GetComponent<PlayerPulse>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ApplyLoadout(PlayerLoadout loadout)
        {
            SkinColor = loadout.SkinColor;
            if (body != null)
            {
                body.color = loadout.SkinColor;
            }
            MagnetRadius = loadout.MagnetRadius;
            Health.Configure(loadout.BonusHp);
            Shooter.Configure(loadout.FireIntervalMultiplier, loadout.TwinCannon, loadout.SkinColor);
            Pulse.Configure(loadout.PulseCooldownReduction);
        }

        public void SetControlsEnabled(bool enabled)
        {
            Controller.SetControlsEnabled(enabled);
            Shooter.SetFiring(enabled);
        }
    }
}
