using UnityEngine;

namespace NeonSwarm
{
    public class ShieldPickup : Pickup
    {
        [SerializeField] private float duration = 5f;

        public override PickupKind Kind => PickupKind.Shield;

        protected override void Apply(Player player)
        {
            player.Health.ActivateShield(duration);
            ScoreManager.Add(ScoreRules.PowerUp, null);
            Fx.Text(transform.position, "TARCZA!", color);
            AudioManager.Play(SfxId.PowerUp);
        }
    }
}
