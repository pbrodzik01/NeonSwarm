using UnityEngine;

namespace NeonSwarm
{
    public class RapidFirePickup : Pickup
    {
        [SerializeField] private float duration = 6f;

        public override PickupKind Kind => PickupKind.RapidFire;

        protected override void Apply(Player player)
        {
            player.Shooter.ActivateRapidFire(duration);
            ScoreManager.Add(ScoreRules.PowerUp, null);
            Fx.Text(transform.position, "TURBO!", color);
            AudioManager.Play(SfxId.PowerUp);
        }
    }
}
