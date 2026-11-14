namespace NeonSwarm
{
    public class CoinPickup : Pickup
    {
        public override PickupKind Kind => PickupKind.Coin;

        protected override void Apply(Player player)
        {
            ScoreManager.Add(ScoreRules.Coin, transform.position);
            ProgressTracker.AddCoins(1);
            AudioManager.Play(SfxId.Coin, 0.8f);
        }
    }
}
