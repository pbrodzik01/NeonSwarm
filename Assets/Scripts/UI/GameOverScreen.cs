using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Text title;
        [SerializeField] private Text subtitle;
        [SerializeField] private Text score;
        [SerializeField] private Text best;
        [SerializeField] private GameObject recordBadge;
        [SerializeField] private Text statsLabels;
        [SerializeField] private Text statsValues;
        [SerializeField] private Button retryButton;
        [SerializeField] private Text retryLabel;
        [SerializeField] private Button menuButton;

        private bool victory;

        private void Awake()
        {
            if (root == null)
            {
                BuildUI();
            }
            retryButton.onClick.AddListener(Retry);
            menuButton.onClick.AddListener(() => SceneLoader.Load(SceneNames.MainMenu));
        }

        private void Start()
        {
            var theme = UiTheme.Current;
            var save = SaveSystem.Data;
            victory = GameSession.Outcome == RunOutcome.Victory;
            AudioManager.PlayMusic(MusicId.Menu);
            AudioManager.Play(victory ? SfxId.Win : SfxId.Lose);

            title.text = victory ? "ZWYCIĘSTWO!" : "KONIEC GRY";
            title.color = victory ? theme.success : theme.danger;
            subtitle.text = victory ? "Rój został rozbity. Wszystkie poziomy ukończone!" : $"Twój statek został zniszczony na poziomie {GameSession.LevelIndex + 1}.";
            score.text = GameSession.Score.ToString("N0");
            best.text = $"NAJLEPSZY WYNIK: {save.bestScore:N0}";
            recordBadge.SetActive(GameSession.NewRecord);
            var rank = save.leaderboard.FindIndex(e => e.score == GameSession.Score && e.name == save.settings.playerName);
            statsLabels.text = "Osiągnięty poziom\nPokonani wrogowie\nZebrane monety\nMiejsce w rankingu";
            statsValues.text = $"{GameSession.LevelIndex + 1}/{SceneNames.LevelCount}\n{GameSession.RunKills}\n{GameSession.RunCoins}\n{(rank >= 0 ? "#" + (rank + 1) : "-")}";
            retryLabel.text = victory ? "ZAGRAJ OD NOWA" : "SPRÓBUJ PONOWNIE";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneLoader.Load(SceneNames.MainMenu);
            }
        }

        private void Retry()
        {
            if (victory)
            {
                GameSession.StartRun(0);
                SceneLoader.Load(SceneNames.ForLevel(0));
                return;
            }
            GameSession.RestartLevel();
            SceneLoader.Load(SceneNames.ForLevel(GameSession.LevelIndex));
        }

        public void BuildUI()
        {
            var theme = UiTheme.Current;
            UiKit.EnsureEventSystem();
            var canvas = UiKit.CreateCanvas("GameOver Canvas", 0, transform);
            var background = UiKit.Panel("Background", canvas.transform, theme.background);
            UiKit.Stretch(background.rectTransform);
            root = UiKit.SafeRoot(canvas.transform);

            title = UiKit.Label("Title", root, "KONIEC GRY", 120, theme.danger, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1060f, 170f));
            subtitle = UiKit.Label("Subtitle", root, string.Empty, 42, theme.textMuted);
            UiKit.Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -320f), new Vector2(980f, 120f));

            var scoreCaption = UiKit.Label("ScoreCaption", root, "WYNIK", 46, theme.textMuted);
            UiKit.Place(scoreCaption.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -470f), new Vector2(600f, 70f));
            score = UiKit.Label("Score", root, "0", 150, theme.gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(score.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -540f), new Vector2(1000f, 200f));
            best = UiKit.Label("Best", root, string.Empty, 44, theme.text);
            UiKit.Place(best.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -750f), new Vector2(900f, 70f));

            var badge = UiKit.Panel("NewRecord", root, theme.accent, theme.rounded);
            UiKit.Place(badge.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -840f), new Vector2(520f, 90f));
            var badgeText = UiKit.Label("Text", badge.transform, "NOWY REKORD!", 48, theme.text, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(badgeText.rectTransform);
            recordBadge = badge.gameObject;

            var stats = UiKit.Panel("Stats", root, theme.panel, theme.rounded);
            UiKit.Place(stats.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -970f), new Vector2(900f, 360f));
            statsLabels = UiKit.Label("Labels", stats.transform, string.Empty, 42, theme.textMuted, TextAnchor.MiddleLeft);
            UiKit.Stretch(statsLabels.rectTransform, 50f, 20f, 50f, 20f);
            statsLabels.lineSpacing = 1.3f;
            statsValues = UiKit.Label("Values", stats.transform, string.Empty, 42, theme.text, TextAnchor.MiddleRight, FontStyle.Bold);
            UiKit.Stretch(statsValues.rectTransform, 50f, 20f, 50f, 20f);
            statsValues.lineSpacing = 1.3f;

            retryButton = UiKit.Button("Retry", root, "SPRÓBUJ PONOWNIE", theme.success, 54);
            UiKit.Place((RectTransform)retryButton.transform, new Vector2(0.5f, 0f), new Vector2(0f, 290f), new Vector2(780f, 160f));
            retryLabel = retryButton.GetComponentInChildren<Text>();
            menuButton = UiKit.Button("Menu", root, "MENU GŁÓWNE", theme.primary, 54);
            UiKit.Place((RectTransform)menuButton.transform, new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(780f, 160f));
        }
    }
}
