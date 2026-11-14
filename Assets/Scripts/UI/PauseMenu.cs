using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private SettingsPanel settings;
        [SerializeField] private GameManager game;

        private void Awake()
        {
            if (root == null)
            {
                BuildUI();
            }
            resumeButton.onClick.AddListener(() => game.Resume());
            optionsButton.onClick.AddListener(OpenOptions);
            restartButton.onClick.AddListener(() => game.RestartLevel());
            menuButton.onClick.AddListener(() => game.QuitToMenu());
            settings.Closed += () => root.gameObject.SetActive(true);
            root.gameObject.SetActive(false);
        }

        public void SetGame(GameManager manager)
        {
            game = manager;
        }

        public void Show()
        {
            root.gameObject.SetActive(true);
        }

        public void Hide()
        {
            root.gameObject.SetActive(false);
            if (settings.IsOpen)
            {
                settings.Close();
            }
        }

        public void Back()
        {
            if (settings.IsOpen)
            {
                settings.Close();
            }
            else
            {
                game.Resume();
            }
        }

        private void OpenOptions()
        {
            root.gameObject.SetActive(false);
            settings.Show();
        }

        public void BuildUI()
        {
            var theme = UiTheme.Current;
            var canvas = UiKit.CreateCanvas("Pause Canvas", 50, transform);
            var safe = UiKit.SafeRoot(canvas.transform);

            var dim = UiKit.Panel("Pause", safe, new Color(0f, 0f, 0f, 0.75f));
            UiKit.Stretch(dim.rectTransform);
            root = dim.rectTransform;
            var card = UiKit.Panel("Card", dim.transform, theme.panel, theme.rounded);
            UiKit.Place(card.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 1060f));
            var outline = UiKit.Panel("Outline", card.transform, theme.primary, theme.roundedOutline);
            UiKit.Stretch(outline.rectTransform);
            outline.raycastTarget = false;

            var title = UiKit.Label("Title", card.transform, "PAUZA", 110, theme.primary, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(700f, 160f));

            var buttons = UiKit.Rect("Buttons", card.transform);
            UiKit.Stretch(buttons, 90f, 260f, 90f, 70f);
            UiKit.VStack(buttons, 34f);
            resumeButton = UiKit.Button("Resume", buttons, "WZNÓW", theme.success);
            optionsButton = UiKit.Button("Options", buttons, "OPCJE", theme.primary);
            restartButton = UiKit.Button("Restart", buttons, "RESTART POZIOMU", theme.gold);
            menuButton = UiKit.Button("Menu", buttons, "MENU", theme.danger);
            foreach (var button in new[] { resumeButton, optionsButton, restartButton, menuButton })
            {
                UiKit.Height(button, 150f);
            }

            var options = UiKit.Rect("Options", safe);
            UiKit.Stretch(options);
            options.gameObject.SetActive(false);
            settings = options.gameObject.AddComponent<SettingsPanel>();
            settings.BuildUI(options, false);
            options.gameObject.SetActive(true);
            dim.gameObject.SetActive(false);
        }
    }
}
