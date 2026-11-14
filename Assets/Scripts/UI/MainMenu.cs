using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button rankingButton;
        [SerializeField] private Button missionsButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private LevelSelectPanel levelSelect;
        [SerializeField] private InstructionsPanel instructions;
        [SerializeField] private ShopPanel shop;
        [SerializeField] private LeaderboardPanel leaderboard;
        [SerializeField] private MissionsPanel missions;
        [SerializeField] private RectTransform[] decor;

        private MenuPanel[] panels;
        private Vector2[] decorVelocity;

        public LevelSelectPanel LevelSelect => levelSelect;

        private void Awake()
        {
            if (root == null)
            {
                BuildUI();
            }
            panels = new MenuPanel[] { levelSelect, instructions, shop, leaderboard, missions };
            playButton.onClick.AddListener(levelSelect.Show);
            instructionsButton.onClick.AddListener(instructions.Show);
            settingsButton.onClick.AddListener(() => SceneLoader.Load(SceneNames.Settings));
            shopButton.onClick.AddListener(shop.Show);
            rankingButton.onClick.AddListener(leaderboard.Show);
            missionsButton.onClick.AddListener(missions.Show);
            exitButton.onClick.AddListener(Quit);
#if UNITY_WEBGL && !UNITY_EDITOR
            exitButton.gameObject.SetActive(false);
#endif
            decorVelocity = new Vector2[decor.Length];
            for (var i = 0; i < decor.Length; i++)
            {
                decorVelocity[i] = Random.insideUnitCircle.normalized * Random.Range(30f, 70f);
            }
        }

        private void OnEnable()
        {
            GameEvents.CoinsChanged += OnCoinsChanged;
        }

        private void OnDisable()
        {
            GameEvents.CoinsChanged -= OnCoinsChanged;
        }

        private void Start()
        {
            AudioManager.PlayMusic(MusicId.Menu);
            Time.timeScale = 1f;
            var save = SaveSystem.Data;
            bestScoreText.text = $"REKORD: {save.bestScore:N0}";
            OnCoinsChanged(save.coins);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                foreach (var panel in panels)
                {
                    if (panel.IsOpen)
                    {
                        panel.Hide();
                        return;
                    }
                }
            }
            AnimateDecor();
        }

        private void AnimateDecor()
        {
            var bounds = root.rect;
            for (var i = 0; i < decor.Length; i++)
            {
                var item = decor[i];
                var position = item.anchoredPosition + decorVelocity[i] * Time.deltaTime;
                if (Mathf.Abs(position.x) > bounds.width * 0.5f)
                {
                    decorVelocity[i].x = -decorVelocity[i].x;
                }
                if (Mathf.Abs(position.y) > bounds.height * 0.5f)
                {
                    decorVelocity[i].y = -decorVelocity[i].y;
                }
                item.anchoredPosition = position;
                item.Rotate(0f, 0f, decorVelocity[i].x * 0.4f * Time.deltaTime);
            }
        }

        private void OnCoinsChanged(int coins)
        {
            coinsText.text = $"MONETY: {coins}";
        }

        private static void Quit()
        {
            SaveSystem.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void BuildUI(Sprite[] decorSprites = null)
        {
            var theme = UiTheme.Current;
            UiKit.EnsureEventSystem();
            var canvas = UiKit.CreateCanvas("Menu Canvas", 0, transform);
            var background = UiKit.Panel("Background", canvas.transform, theme.background);
            UiKit.Stretch(background.rectTransform);

            var decorRoot = UiKit.Rect("Decor", canvas.transform);
            UiKit.Stretch(decorRoot);
            var sprites = decorSprites != null && decorSprites.Length > 0 ? decorSprites : new[] { theme.ship };
            var colors = new[] { theme.accent, new Color(1f, 0.55f, 0.15f), theme.success, theme.primary };
            decor = new RectTransform[9];
            for (var i = 0; i < decor.Length; i++)
            {
                var color = colors[i % colors.Length];
                color.a = 0.22f;
                var image = UiKit.Icon("Decor" + i, decorRoot, sprites[i % sprites.Length], color, 120f + (i % 3) * 50f);
                image.rectTransform.anchoredPosition = new Vector2(Random.Range(-450f, 450f), Random.Range(-850f, 850f));
                decor[i] = image.rectTransform;
            }

            root = UiKit.SafeRoot(canvas.transform);

            var logo = UiKit.Icon("Logo", root, theme.ship, theme.primary, 200f);
            UiKit.Place(logo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(200f, 200f));
            var title = UiKit.Label("Title", root, "NEON SWARM", 130, theme.primary, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -320f), new Vector2(1060f, 170f));
            title.gameObject.AddComponent<Shadow>().effectColor = new Color(theme.accent.r, theme.accent.g, theme.accent.b, 0.8f);
            var subtitle = UiKit.Label("Subtitle", root, "Powstrzymaj neonowy rój", 48, theme.textMuted);
            UiKit.Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -480f), new Vector2(1000f, 70f));

            bestScoreText = UiKit.Label("Best", root, "REKORD: 0", 44, theme.text, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Place(bestScoreText.rectTransform, new Vector2(0.5f, 1f), new Vector2(-230f, -570f), new Vector2(460f, 70f));
            coinsText = UiKit.Label("Coins", root, "MONETY: 0", 44, theme.gold, TextAnchor.MiddleRight, FontStyle.Bold);
            UiKit.Place(coinsText.rectTransform, new Vector2(0.5f, 1f), new Vector2(230f, -570f), new Vector2(460f, 70f));

            var buttons = UiKit.Rect("Buttons", root);
            buttons.anchorMin = new Vector2(0.5f, 0f);
            buttons.anchorMax = new Vector2(0.5f, 0f);
            buttons.pivot = new Vector2(0.5f, 0f);
            buttons.sizeDelta = new Vector2(760f, 1140f);
            buttons.anchoredPosition = new Vector2(0f, 80f);
            UiKit.VStack(buttons, 26f);

            playButton = MenuButton(buttons, "GRAJ", theme.success, 170f, 72);
            instructionsButton = MenuButton(buttons, "INSTRUKCJA", theme.primary);
            settingsButton = MenuButton(buttons, "USTAWIENIA", theme.primary);
            shopButton = MenuButton(buttons, "SKLEP", theme.gold);
            rankingButton = MenuButton(buttons, "RANKING", theme.primary);
            missionsButton = MenuButton(buttons, "MISJE I OSIĄGNIĘCIA", theme.primary);
            exitButton = MenuButton(buttons, "WYJŚCIE", theme.danger);

            levelSelect = MenuPanel.Create<LevelSelectPanel>(root);
            instructions = MenuPanel.Create<InstructionsPanel>(root);
            shop = MenuPanel.Create<ShopPanel>(root);
            leaderboard = MenuPanel.Create<LeaderboardPanel>(root);
            missions = MenuPanel.Create<MissionsPanel>(root);
        }

        private static Button MenuButton(Transform parent, string text, Color color, float height = 128f, int fontSize = 50)
        {
            var button = UiKit.Button(text, parent, text, color, fontSize);
            UiKit.Height(button, height);
            return button;
        }
    }
}
