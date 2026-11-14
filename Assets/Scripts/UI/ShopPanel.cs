using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class ShopPanel : MenuPanel
    {
        [SerializeField] private Text coinsLabel;
        [SerializeField] private RectTransform listContent;

        protected override string Title => "SKLEP";

        protected override void BuildBody(RectTransform container)
        {
            var theme = UiTheme.Current;
            var wallet = UiKit.Panel("Wallet", container, theme.panelLight, theme.rounded);
            wallet.rectTransform.anchorMin = new Vector2(0f, 1f);
            wallet.rectTransform.anchorMax = new Vector2(1f, 1f);
            wallet.rectTransform.pivot = new Vector2(0.5f, 1f);
            wallet.rectTransform.sizeDelta = new Vector2(0f, 110f);
            var coin = UiKit.Icon("Coin", wallet.transform, theme.coin, Color.white, 70f);
            UiKit.Place(coin.rectTransform, new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(70f, 70f));
            coinsLabel = UiKit.Label("Coins", wallet.transform, "MONETY: 0", 52, theme.gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Stretch(coinsLabel.rectTransform, 120f, 0f, 20f, 0f);

            var scroll = UiKit.ScrollList("Items", container, out listContent);
            UiKit.Stretch(scroll, 0f, 130f, 0f, 0f);
        }

        protected override void Refresh()
        {
            var save = SaveSystem.Data;
            coinsLabel.text = $"MONETY: {save.coins}";
            ClearChildren(listContent);
            AddCategory("Skórki", ShopCategory.Skin);
            AddCategory("Broń", ShopCategory.Weapon);
            AddCategory("Ulepszenia", ShopCategory.Upgrade);
            AddCategory("Power-upy", ShopCategory.PowerUp);
        }

        private void AddCategory(string heading, ShopCategory category)
        {
            Heading(listContent, heading.ToUpperInvariant());
            foreach (var item in ShopCatalog.Items)
            {
                if (item.Category == category)
                {
                    AddItem(item);
                }
            }
        }

        private void AddItem(ShopItem item)
        {
            var theme = UiTheme.Current;
            var save = SaveSystem.Data;
            var row = Row(listContent, 190f);

            var swatch = UiKit.Icon("Swatch", row.transform, IconFor(item), item.Color, 110f);
            UiKit.Place(swatch.rectTransform, new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(110f, 110f));

            var name = UiKit.Label("Name", row.transform, item.Name, 42, theme.text, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Stretch(name.rectTransform, 150f, 22f, 290f, 110f);
            name.horizontalOverflow = HorizontalWrapMode.Overflow;
            var level = ShopService.GetLevel(save, item);
            var detail = item.Description;
            if (item.Category == ShopCategory.Upgrade)
            {
                detail += $"\nPoziom {level}/{item.MaxLevel}";
            }
            else if (item.IsConsumable)
            {
                detail += $"\nPosiadasz: {level}/{ShopCatalog.MaxStartShields}";
            }
            var description = UiKit.Label("Description", row.transform, detail, 32, theme.textMuted, TextAnchor.UpperLeft);
            UiKit.Stretch(description.rectTransform, 150f, 76f, 290f, 10f);

            var (label, color, interactable) = ButtonState(save, item);
            var button = UiKit.Button("Buy", row.transform, label, color, 38);
            UiKit.Place((RectTransform)button.transform, new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(255f, 120f));
            button.interactable = interactable;
            button.onClick.AddListener(() => Buy(item));
        }

        private static (string, Color, bool) ButtonState(SaveData save, ShopItem item)
        {
            var theme = UiTheme.Current;
            if (item.Category == ShopCategory.Skin && save.ownedItems.Contains(item.Id))
            {
                return save.selectedSkin == item.Id ? ("WYBRANO", theme.panel, false) : ("WYBIERZ", theme.primary, true);
            }
            if (ShopService.IsMaxed(save, item))
            {
                return (item.Category == ShopCategory.Weapon ? "POSIADANE" : "MAKS.", theme.panel, false);
            }
            var price = ShopService.NextPrice(save, item);
            return ($"{price} ●", save.coins >= price ? theme.gold : theme.panel, save.coins >= price);
        }

        private static Sprite IconFor(ShopItem item)
        {
            var theme = UiTheme.Current;
            switch (item.Category)
            {
                case ShopCategory.Skin:
                    return theme.ship;
                case ShopCategory.PowerUp:
                    return theme.shield;
                case ShopCategory.Weapon:
                    return theme.rapid;
                default:
                    return theme.star;
            }
        }

        private void Buy(ShopItem item)
        {
            var result = ShopService.TryBuy(SaveSystem.Data, item.Id);
            switch (result)
            {
                case PurchaseResult.Purchased:
                    AudioManager.Play(SfxId.Coin);
                    GameEvents.RaiseNotification($"Kupiono: {item.Name}");
                    break;
                case PurchaseResult.Equipped:
                    GameEvents.RaiseNotification($"Wybrano skórkę: {item.Name}");
                    break;
                case PurchaseResult.NotEnoughCoins:
                    GameEvents.RaiseNotification("Za mało monet");
                    break;
            }
            SaveSystem.Save();
            GameEvents.RaiseCoinsChanged(SaveSystem.Data.coins);
            Refresh();
        }
    }
}
