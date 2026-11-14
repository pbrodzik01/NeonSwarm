namespace NeonSwarm
{
    public class MedkitPickup : Pickup
    {
        public override PickupKind Kind => PickupKind.Medkit;

        protected override void Apply(Player player)
        {
            if (player.Health.Heal(1))
            {
                Fx.Text(transform.position, "+1 HP", color);
            }
            else
            {
                ScoreManager.Add(ScoreRules.MedkitAtFullHp, transform.position);
            }
            AudioManager.Play(SfxId.Heal);
        }
    }
}
