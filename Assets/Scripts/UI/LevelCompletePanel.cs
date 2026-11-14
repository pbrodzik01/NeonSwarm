using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class LevelCompletePanel : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Text title;
        [SerializeField] private Text breakdown;
        [SerializeField] private Text values;
        [SerializeField] private Text total;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button menuButton;

        private GameManager game;

        private void Awake()
        {
            if (root == null)
            {
                BuildUI();
            }
            nextButton.onClick.AddListener(() => game.GoToNextLevel());
            menuButton.onClick.AddListener(() => game.FinishRunToMenu());
            root.gameObject.SetActive(false);
        }

        public void Show(LevelSummary summary, GameManager manager)
        {
            game = manager;
            title.text = $"POZIOM {summary.LevelIndex + 1} UKOŃCZONY";
            breakdown.text =
                "Punkty z poziomu\n" +
                "Ukończenie poziomu\n" +
                $"Bonus czasowy ({summary.Seconds:0} s)\n" +
                "Bez obrażeń\n" +
                "Monety w portfelu";
            values.text =
                $"{summary.LevelScore}\n" +
                $"+{summary.CompletionBonus}\n" +
                $"+{summary.TimeBonus}\n" +
                $"+{summary.FlawlessBonus}\n" +
                $"{SaveSystem.Data.coins}";
            total.text = $"WYNIK: {summary.TotalScore:N0}";
            root.gameObject.SetActive(true);
        }

        public void BuildUI()
        {
            var theme = UiTheme.Current;
            var canvas = UiKit.CreateCanvas("LevelComplete Canvas", 40, transform);
            var safe = UiKit.SafeRoot(canvas.transform);
            root = UiKit.Modal("LevelComplete", safe, "POZIOM UKOŃCZONY", out var body, out menuButton);
            title = root.Find("Card/Title").GetComponent<Text>();
            title.color = theme.success;
            title.fontSize = 64;
            menuButton.GetComponentInChildren<Text>().text = "MENU";
            menuButton.image.color = theme.panelLight;
            menuButton.GetComponentInChildren<Text>().color = theme.text;

            breakdown = UiKit.Label("Breakdown", body, string.Empty, 44, theme.text, TextAnchor.UpperLeft);
            UiKit.Stretch(breakdown.rectTransform, 20f, 30f, 20f, 520f);
            breakdown.lineSpacing = 1.4f;
            values = UiKit.Label("Values", body, string.Empty, 44, theme.gold, TextAnchor.UpperRight, FontStyle.Bold);
            UiKit.Stretch(values.rectTransform, 20f, 30f, 20f, 520f);
            values.lineSpacing = 1.4f;
            total = UiKit.Label("Total", body, string.Empty, 72, theme.gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(total.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 330f), new Vector2(900f, 120f));
            nextButton = UiKit.Button("Next", body, "NASTĘPNY POZIOM", theme.success);
            UiKit.Place((RectTransform)nextButton.transform, new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(760f, 150f));
            root.gameObject.SetActive(false);
        }
    }
}
