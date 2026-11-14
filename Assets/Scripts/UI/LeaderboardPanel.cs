using UnityEngine;

namespace NeonSwarm
{
    public class LeaderboardPanel : MenuPanel
    {
        [SerializeField] private RectTransform listContent;

        protected override string Title => "RANKING";

        protected override void BuildBody(RectTransform container)
        {
            var scroll = UiKit.ScrollList("Entries", container, out listContent);
            UiKit.Stretch(scroll);
        }

        protected override void Refresh()
        {
            var theme = UiTheme.Current;
            ClearChildren(listContent);
            var entries = SaveSystem.Data.leaderboard;
            if (entries.Count == 0)
            {
                var empty = UiKit.Label("Empty", listContent, "Brak wyników.\nZagraj, aby trafić do rankingu!", 46, theme.textMuted);
                UiKit.Height(empty, 300f);
                return;
            }
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var row = Row(listContent, 140f);
                var medal = i == 0 ? theme.gold : i == 1 ? new Color(0.8f, 0.85f, 0.95f) : i == 2 ? new Color(1f, 0.6f, 0.35f) : theme.textMuted;
                var rank = UiKit.Label("Rank", row.transform, $"#{i + 1}", 52, medal, TextAnchor.MiddleLeft, FontStyle.Bold);
                UiKit.Stretch(rank.rectTransform, 30f, 0f, 0f, 0f);
                var name = UiKit.Label("Name", row.transform, entry.name, 44, theme.text, TextAnchor.UpperLeft, FontStyle.Bold);
                UiKit.Stretch(name.rectTransform, 150f, 18f, 330f, 60f);
                var meta = UiKit.Label("Meta", row.transform, $"Poziom {entry.level} · {entry.date}", 28, theme.textMuted, TextAnchor.LowerLeft);
                UiKit.Stretch(meta.rectTransform, 150f, 70f, 250f, 16f);
                meta.horizontalOverflow = HorizontalWrapMode.Overflow;
                var score = UiKit.Label("Score", row.transform, entry.score.ToString("N0"), 52, theme.primary, TextAnchor.MiddleRight, FontStyle.Bold);
                UiKit.Stretch(score.rectTransform, 0f, 0f, 30f, 0f);
            }
        }
    }
}
