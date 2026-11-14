using UnityEngine;

namespace NeonSwarm
{
    public enum ShopCategory
    {
        Skin,
        Weapon,
        Upgrade,
        PowerUp
    }

    public sealed class ShopItem
    {
        public string Id;
        public string Name;
        public string Description;
        public ShopCategory Category;
        public int[] Prices;
        public Color Color = Color.white;

        public int MaxLevel => Prices.Length;
        public bool IsConsumable => Category == ShopCategory.PowerUp;
    }

    public static class ShopCatalog
    {
        public const string DefaultSkinId = "skin_classic";
        public const string TwinCannonId = "weapon_twin";
        public const string ArmorId = "upg_armor";
        public const string FireRateId = "upg_firerate";
        public const string PulseId = "upg_pulse";
        public const string MagnetId = "upg_magnet";
        public const string StartShieldId = "pu_shield";
        public const int MaxStartShields = 5;

        public static readonly ShopItem[] Items =
        {
            Skin(DefaultSkinId, "Klasyczny", 0, new Color(0.15f, 0.91f, 1f)),
            Skin("skin_magma", "Magma", 150, new Color(1f, 0.55f, 0.15f)),
            Skin("skin_toxic", "Toksyczny", 250, new Color(0.35f, 1f, 0.45f)),
            Skin("skin_royal", "Fiolet", 400, new Color(0.75f, 0.45f, 1f)),
            new ShopItem
            {
                Id = TwinCannonId,
                Name = "Podwójne działo",
                Description = "Broń: dwa pociski w każdej salwie",
                Category = ShopCategory.Weapon,
                Prices = new[] { 500 },
                Color = new Color(1f, 0.82f, 0.25f)
            },
            Upgrade(ArmorId, "Pancerz", "+1 maks. HP za poziom", 200, 400),
            Upgrade(FireRateId, "Szybkostrzelność", "Krótsze przeładowanie (-12%)", 150, 300, 450),
            Upgrade(PulseId, "Generator pulsu", "Puls odnawia się 1,5 s szybciej", 200, 400),
            Upgrade(MagnetId, "Magnes", "Przyciąga monety z większej odległości", 250),
            new ShopItem
            {
                Id = StartShieldId,
                Name = "Tarcza startowa",
                Description = "Power-up: 5 s tarczy na starcie poziomu",
                Category = ShopCategory.PowerUp,
                Prices = new[] { 60 },
                Color = new Color(0.35f, 0.65f, 1f)
            }
        };

        public static ShopItem Find(string id)
        {
            foreach (var item in Items)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }

        private static ShopItem Skin(string id, string name, int price, Color color)
        {
            return new ShopItem
            {
                Id = id,
                Name = name,
                Description = "Skórka statku",
                Category = ShopCategory.Skin,
                Prices = new[] { price },
                Color = color
            };
        }

        private static ShopItem Upgrade(string id, string name, string description, params int[] prices)
        {
            return new ShopItem
            {
                Id = id,
                Name = name,
                Description = description,
                Category = ShopCategory.Upgrade,
                Prices = prices,
                Color = new Color(0.3f, 1f, 0.55f)
            };
        }
    }
}
