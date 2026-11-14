using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class LevelSelectPanel : MenuPanel
    {
        [SerializeField] private LevelConfig[] levels;
        [SerializeField] private Button[] levelButtons;
        [SerializeField] private Text[] levelTitles;
        [SerializeField] private Text[] levelDetails;
        [SerializeField] private Image[] lockIcons;

        protected override string Title => "WYBIERZ POZIOM";

        public void SetLevels(LevelConfig[] configs)
        {
            levels = configs;
        }

        protected override void Awake()
        {
            base.Awake();
            for (var i = 0; i < levelButtons.Length; i++)
            {
                var index = i;
                levelButtons[i].onClick.AddListener(() => StartLevel(index));
            }
        }

        protected override void BuildBody(RectTransform container)
        {
            var theme = UiTheme.Current;
            UiKit.VStack(container, 30f);
            var count = SceneNames.LevelCount;
            levelButtons = new Button[count];
            levelTitles = new Text[count];
            levelDetails = new Text[count];
            lockIcons = new Image[count];
            for (var i = 0; i < count; i++)
            {
                var button = UiKit.Button("Level" + (i + 1), container, string.Empty, theme.panelLight);
                levelButtons[i] = button;
                UiKit.Height(button, 300f);
                var label = button.GetComponentInChildren<Text>();
                label.gameObject.SetActive(false);
                levelTitles[i] = UiKit.Label("Title", button.transform, $"POZIOM {i + 1}", 64, theme.primary, TextAnchor.UpperLeft, FontStyle.Bold);
                UiKit.Stretch(levelTitles[i].rectTransform, 40f, 30f, 160f, 150f);
                levelDetails[i] = UiKit.Label("Details", button.transform, string.Empty, 38, theme.text, TextAnchor.UpperLeft);
                UiKit.Stretch(levelDetails[i].rectTransform, 40f, 120f, 160f, 20f);
                lockIcons[i] = UiKit.Icon("Lock", button.transform, theme.lockIcon, theme.textMuted, 110f);
                UiKit.Place(lockIcons[i].rectTransform, new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(110f, 110f));
            }
        }

        protected override void Refresh()
        {
            var theme = UiTheme.Current;
            var save = SaveSystem.Data;
            for (var i = 0; i < levelButtons.Length; i++)
            {
                var unlocked = i < save.unlockedLevels;
                var config = levels != null && i < levels.Length ? levels[i] : null;
                levelButtons[i].interactable = unlocked;
                lockIcons[i].gameObject.SetActive(!unlocked);
                var title = config != null ? config.title : $"Poziom {i + 1}";
                levelTitles[i].text = title.ToUpperInvariant();
                levelTitles[i].color = unlocked ? (config != null ? config.accent : theme.primary) : theme.textMuted;
                var best = i < save.levelBestScores.Count ? save.levelBestScores[i] : 0;
                levelDetails[i].text = config == null
                    ? string.Empty
                    : unlocked
                        ? $"{config.subtitle}\nTrudność: {config.difficultyLabel} · Fale: {config.waves} · Rekord: {best}"
                        : $"{config.subtitle}\nUkończ poziom {i}, aby odblokować";
            }
        }

        private void StartLevel(int index)
        {
            GameSession.StartRun(index);
            SceneLoader.Load(SceneNames.ForLevel(index));
        }
    }
}
