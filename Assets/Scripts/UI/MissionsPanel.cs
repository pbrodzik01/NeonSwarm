using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class MissionsPanel : MenuPanel
    {
        [SerializeField] private RectTransform listContent;

        protected override string Title => "MISJE";

        protected override void BuildBody(RectTransform container)
        {
            var scroll = UiKit.ScrollList("Entries", container, out listContent);
            UiKit.Stretch(scroll);
        }

        protected override void Refresh()
        {
            var theme = UiTheme.Current;
            var save = SaveSystem.Data;
            ClearChildren(listContent);

            Heading(listContent, "AKTYWNE MISJE");
            foreach (var progress in save.missions)
            {
                var mission = MissionCatalog.Find(progress.id);
                if (mission == null)
                {
                    continue;
                }
                var row = Row(listContent, 190f);
                var title = UiKit.Label("Title", row.transform, $"MISJA: {mission.Title}", 40, theme.text, TextAnchor.UpperLeft, FontStyle.Bold);
                UiKit.Stretch(title.rectTransform, 30f, 20f, 30f, 100f);
                var bar = UiKit.Panel("Bar", row.transform, theme.panel, theme.rounded);
                bar.rectTransform.anchorMin = new Vector2(0f, 0f);
                bar.rectTransform.anchorMax = new Vector2(1f, 0f);
                bar.rectTransform.pivot = new Vector2(0.5f, 0f);
                bar.rectTransform.offsetMin = new Vector2(30f, 28f);
                bar.rectTransform.offsetMax = new Vector2(-330f, 70f);
                var fill = UiKit.Panel("Fill", bar.transform, theme.success, theme.rounded);
                UiKit.Stretch(fill.rectTransform);
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillAmount = Mathf.Clamp01((float)progress.progress / mission.Target);
                var count = UiKit.Label("Progress", row.transform, $"{progress.progress} / {mission.Target}", 40, theme.success, TextAnchor.LowerRight, FontStyle.Bold);
                UiKit.Stretch(count.rectTransform, 0f, 0f, 160f, 20f);
                var reward = UiKit.Label("Reward", row.transform, $"+{mission.Reward} ●", 38, theme.gold, TextAnchor.LowerRight, FontStyle.Bold);
                UiKit.Stretch(reward.rectTransform, 0f, 0f, 24f, 20f);
            }

            Heading(listContent, $"OSIĄGNIĘCIA ({save.achievements.Count}/{AchievementService.All.Length})");
            foreach (var achievement in AchievementService.All)
            {
                var unlocked = AchievementService.IsUnlocked(save, achievement.Id);
                var row = Row(listContent, 150f);
                var icon = UiKit.Icon("Icon", row.transform, unlocked ? theme.trophy : theme.lockIcon, unlocked ? theme.gold : theme.textMuted, 90f);
                UiKit.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(90f, 90f));
                var title = UiKit.Label("Title", row.transform, achievement.Title, 42, unlocked ? theme.gold : theme.text, TextAnchor.UpperLeft, FontStyle.Bold);
                UiKit.Stretch(title.rectTransform, 140f, 18f, 20f, 70f);
                var description = UiKit.Label("Description", row.transform, achievement.Description, 32, theme.textMuted, TextAnchor.LowerLeft);
                UiKit.Stretch(description.rectTransform, 140f, 70f, 20f, 18f);
            }
        }
    }
}
