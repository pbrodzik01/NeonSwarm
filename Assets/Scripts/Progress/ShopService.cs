namespace NeonSwarm
{
    public enum PurchaseResult
    {
        Purchased,
        Equipped,
        NotEnoughCoins,
        MaxLevel,
        UnknownItem
    }

    public struct PlayerLoadout
    {
        public int BonusHp;
        public float FireIntervalMultiplier;
        public float PulseCooldownReduction;
        public float MagnetRadius;
        public bool TwinCannon;
        public UnityEngine.Color SkinColor;
    }

    public static class ShopService
    {
        public static int GetLevel(SaveData save, ShopItem item)
        {
            switch (item.Category)
            {
                case ShopCategory.Skin:
                case ShopCategory.Weapon:
                    return save.ownedItems.Contains(item.Id) ? 1 : 0;
                case ShopCategory.PowerUp:
                    return save.startShields;
                default:
                    foreach (var entry in save.itemLevels)
                    {
                        if (entry.id == item.Id)
                        {
                            return entry.level;
                        }
                    }
                    return 0;
            }
        }

        public static int GetLevel(SaveData save, string id)
        {
            var item = ShopCatalog.Find(id);
            return item == null ? 0 : GetLevel(save, item);
        }

        public static bool IsMaxed(SaveData save, ShopItem item)
        {
            if (item.IsConsumable)
            {
                return save.startShields >= ShopCatalog.MaxStartShields;
            }
            return GetLevel(save, item) >= item.MaxLevel;
        }

        public static int NextPrice(SaveData save, ShopItem item)
        {
            if (item.IsConsumable)
            {
                return item.Prices[0];
            }
            var level = GetLevel(save, item);
            return level < item.Prices.Length ? item.Prices[level] : 0;
        }

        public static PurchaseResult TryBuy(SaveData save, string id)
        {
            var item = ShopCatalog.Find(id);
            if (item == null)
            {
                return PurchaseResult.UnknownItem;
            }
            if (item.Category == ShopCategory.Skin && save.ownedItems.Contains(id))
            {
                save.selectedSkin = id;
                return PurchaseResult.Equipped;
            }
            if (IsMaxed(save, item))
            {
                return PurchaseResult.MaxLevel;
            }
            var price = NextPrice(save, item);
            if (save.coins < price)
            {
                return PurchaseResult.NotEnoughCoins;
            }
            save.coins -= price;
            Apply(save, item);
            return PurchaseResult.Purchased;
        }

        public static PlayerLoadout BuildLoadout(SaveData save)
        {
            var skin = ShopCatalog.Find(save.selectedSkin) ?? ShopCatalog.Find(ShopCatalog.DefaultSkinId);
            return new PlayerLoadout
            {
                BonusHp = GetLevel(save, ShopCatalog.ArmorId),
                FireIntervalMultiplier = 1f - 0.12f * GetLevel(save, ShopCatalog.FireRateId),
                PulseCooldownReduction = 1.5f * GetLevel(save, ShopCatalog.PulseId),
                MagnetRadius = GetLevel(save, ShopCatalog.MagnetId) > 0 ? 3.5f : 1.3f,
                TwinCannon = save.ownedItems.Contains(ShopCatalog.TwinCannonId),
                SkinColor = skin.Color
            };
        }

        private static void Apply(SaveData save, ShopItem item)
        {
            switch (item.Category)
            {
                case ShopCategory.Skin:
                    save.ownedItems.Add(item.Id);
                    save.selectedSkin = item.Id;
                    break;
                case ShopCategory.Weapon:
                    save.ownedItems.Add(item.Id);
                    break;
                case ShopCategory.PowerUp:
                    save.startShields++;
                    break;
                default:
                    foreach (var entry in save.itemLevels)
                    {
                        if (entry.id == item.Id)
                        {
                            entry.level++;
                            return;
                        }
                    }
                    save.itemLevels.Add(new ItemLevel { id = item.Id, level = 1 });
                    break;
            }
        }
    }
}
