using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class Hud : MonoBehaviour
    {
        private const int MaxHearts = 6;

        [SerializeField] private RectTransform root;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text waveText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text difficultyText;
        [SerializeField] private Text enemiesText;
        [SerializeField] private Image[] hearts;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button pulseButton;
        [SerializeField] private Image pulseFill;
        [SerializeField] private GameObject shieldIndicator;
        [SerializeField] private Text shieldText;
        [SerializeField] private GameObject rapidIndicator;
        [SerializeField] private Text rapidText;
        [SerializeField] private CanvasGroup banner;
        [SerializeField] private Text bannerTitle;
        [SerializeField] private Text bannerSubtitle;
        [SerializeField] private Text countdownText;
        [SerializeField] private VirtualJoystick joystick;

        private GameManager game;
        private Player player;
        private int shownSeconds = -1;
        private Coroutine bannerRoutine;
        private Coroutine countdownRoutine;

        public VirtualJoystick Joystick => joystick;

        private void Awake()
        {
            if (root == null)
            {
                BuildUI();
            }
            banner.alpha = 0f;
            countdownText.text = string.Empty;
        }

        public void Bind(GameManager manager, Player target)
        {
            game = manager;
            player = target;
            player.Controller.SetJoystick(joystick);
            pauseButton.onClick.AddListener(game.Pause);
            pulseButton.onClick.AddListener(() => player.Pulse.TryPulse());
            levelText.text = $"POZIOM {game.Config.levelIndex + 1}/{SceneNames.LevelCount}";
            OnScoreChanged(GameSession.Score);
            OnCoinsChanged(SaveSystem.Data.coins);
            OnHealthChanged(player.Health.Current, player.Health.Max);
            game.Spawner.WaveStarted += OnWaveStarted;
            waveText.text = $"FALA 0/{game.Config.waves}";
        }

        private void OnEnable()
        {
            GameEvents.ScoreChanged += OnScoreChanged;
            GameEvents.CoinsChanged += OnCoinsChanged;
            GameEvents.PlayerHealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            GameEvents.ScoreChanged -= OnScoreChanged;
            GameEvents.CoinsChanged -= OnCoinsChanged;
            GameEvents.PlayerHealthChanged -= OnHealthChanged;
            if (game != null)
            {
                game.Spawner.WaveStarted -= OnWaveStarted;
            }
        }

        private void Update()
        {
            if (game == null)
            {
                return;
            }
            var seconds = Mathf.FloorToInt(game.Elapsed);
            if (seconds != shownSeconds)
            {
                shownSeconds = seconds;
                timeText.text = $"{seconds / 60:00}:{seconds % 60:00}";
                difficultyText.text = $"TEMPO x{game.Difficulty.Intensity:0.00}";
            }
            enemiesText.text = game.State == GameState.Playing ? $"WROGOWIE: {game.Spawner.RemainingInWave}" : string.Empty;

            var pulse = player.Pulse;
            pulseFill.fillAmount = 1f - pulse.Progress;
            pulseButton.interactable = pulse.Ready && game.State == GameState.Playing;

            var shield = player.Health.ShieldRemaining;
            shieldIndicator.SetActive(shield > 0f);
            if (shield > 0f)
            {
                shieldText.text = $"{shield:0.0}s";
            }
            var rapid = player.Shooter.RapidRemaining;
            rapidIndicator.SetActive(rapid > 0f);
            if (rapid > 0f)
            {
                rapidText.text = $"{rapid:0.0}s";
            }
        }

        public void ShowBanner(string title, string subtitle, float seconds)
        {
            if (bannerRoutine != null)
            {
                StopCoroutine(bannerRoutine);
            }
            bannerRoutine = StartCoroutine(BannerRoutine(title, subtitle, seconds));
        }

        public void ShowCountdown(string text)
        {
            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
            }
            countdownRoutine = StartCoroutine(CountdownRoutine(text));
        }

        private void OnWaveStarted(int wave)
        {
            waveText.text = $"FALA {wave}/{game.Config.waves}";
            ShowBanner($"FALA {wave}", wave == game.Config.waves ? "Ostatnia fala!" : string.Empty, 1.2f);
        }

        private void OnScoreChanged(int score)
        {
            scoreText.text = score.ToString("N0");
        }

        private void OnCoinsChanged(int coins)
        {
            coinsText.text = coins.ToString();
        }

        private void OnHealthChanged(int current, int max)
        {
            var theme = UiTheme.Current;
            for (var i = 0; i < hearts.Length; i++)
            {
                hearts[i].gameObject.SetActive(i < max);
                hearts[i].sprite = i < current ? theme.heart : theme.heartEmpty;
                hearts[i].color = i < current ? theme.danger : new Color(1f, 1f, 1f, 0.35f);
            }
        }

        private IEnumerator BannerRoutine(string title, string subtitle, float seconds)
        {
            bannerTitle.text = title;
            bannerSubtitle.text = subtitle;
            for (var t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                banner.alpha = t / 0.2f;
                yield return null;
            }
            banner.alpha = 1f;
            yield return new WaitForSecondsRealtime(seconds);
            for (var t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                banner.alpha = 1f - t / 0.3f;
                yield return null;
            }
            banner.alpha = 0f;
        }

        private IEnumerator CountdownRoutine(string text)
        {
            countdownText.text = text;
            var rect = countdownText.rectTransform;
            for (var t = 0f; t < 0.5f; t += Time.unscaledDeltaTime)
            {
                var k = t / 0.5f;
                rect.localScale = Vector3.one * Mathf.Lerp(1.6f, 1f, k);
                var c = countdownText.color;
                c.a = 1f - k * 0.6f;
                countdownText.color = c;
                yield return null;
            }
            countdownText.text = string.Empty;
        }

        public void BuildUI()
        {
            var theme = UiTheme.Current;
            UiKit.EnsureEventSystem();
            var canvas = UiKit.CreateCanvas("HUD Canvas", 10, transform);
            root = UiKit.SafeRoot(canvas.transform);

            var top = UiKit.Panel("TopBar", root, new Color(theme.panel.r, theme.panel.g, theme.panel.b, 0.82f), theme.rounded);
            var topRect = top.rectTransform;
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = new Vector2(1f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(-30f, 250f);
            topRect.anchoredPosition = new Vector2(0f, -15f);
            top.raycastTarget = false;

            var scoreCaption = UiKit.Label("ScoreCaption", top.transform, "WYNIK", 34, theme.textMuted, TextAnchor.UpperLeft);
            UiKit.Place(scoreCaption.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -20f), new Vector2(360f, 50f));
            scoreText = UiKit.Label("Score", top.transform, "0", 64, theme.text, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Place(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(36f, -58f), new Vector2(380f, 80f));

            levelText = UiKit.Label("Level", top.transform, "POZIOM 1/3", 40, theme.primary, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Place(levelText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(380f, 56f));
            waveText = UiKit.Label("Wave", top.transform, "FALA 0/3", 36, theme.text, TextAnchor.UpperCenter);
            UiKit.Place(waveText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(380f, 50f));

            pauseButton = UiKit.IconButton("Pause", top.transform, theme.pause, theme.primary, 130f);
            UiKit.Place((RectTransform)pauseButton.transform, new Vector2(1f, 1f), new Vector2(-24f, -20f), new Vector2(130f, 130f));

            var heartsRow = UiKit.Rect("Hearts", top.transform);
            UiKit.Place(heartsRow, new Vector2(0f, 0f), new Vector2(30f, 22f), new Vector2(420f, 70f));
            var heartsLayout = heartsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            heartsLayout.spacing = 8f;
            heartsLayout.childControlWidth = false;
            heartsLayout.childControlHeight = false;
            heartsLayout.childAlignment = TextAnchor.MiddleLeft;
            hearts = new Image[MaxHearts];
            for (var i = 0; i < MaxHearts; i++)
            {
                hearts[i] = UiKit.Icon("Heart" + i, heartsRow, theme.heart, theme.danger, 62f);
            }

            timeText = UiKit.Label("Time", top.transform, "00:00", 44, theme.text, TextAnchor.LowerCenter, FontStyle.Bold);
            UiKit.Place(timeText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(240f, 60f));

            var coinIcon = UiKit.Icon("CoinIcon", top.transform, theme.coin, Color.white, 54f);
            UiKit.Place(coinIcon.rectTransform, new Vector2(1f, 0f), new Vector2(-200f, 28f), new Vector2(54f, 54f));
            coinsText = UiKit.Label("Coins", top.transform, "0", 44, theme.gold, TextAnchor.LowerLeft, FontStyle.Bold);
            UiKit.Place(coinsText.rectTransform, new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(170f, 60f));

            difficultyText = UiKit.Label("Difficulty", root, "TEMPO x1.00", 30, theme.textMuted, TextAnchor.UpperLeft);
            UiKit.Place(difficultyText.rectTransform, new Vector2(0f, 1f), new Vector2(40f, -275f), new Vector2(400f, 44f));
            enemiesText = UiKit.Label("Enemies", root, string.Empty, 30, theme.textMuted, TextAnchor.UpperRight);
            UiKit.Place(enemiesText.rectTransform, new Vector2(1f, 1f), new Vector2(-40f, -275f), new Vector2(400f, 44f));

            shieldIndicator = BuildIndicator("Shield", theme.shield, new Color(0.4f, 0.7f, 1f), new Vector2(40f, -330f), out shieldText);
            rapidIndicator = BuildIndicator("Rapid", theme.rapid, theme.gold, new Vector2(260f, -330f), out rapidText);

            var bannerRect = UiKit.Rect("Banner", root);
            UiKit.Place(bannerRect, new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(1000f, 260f));
            banner = bannerRect.gameObject.AddComponent<CanvasGroup>();
            banner.blocksRaycasts = false;
            bannerTitle = UiKit.Label("Title", bannerRect, string.Empty, 96, theme.primary, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(bannerTitle.rectTransform, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(1000f, 140f));
            bannerTitle.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
            bannerSubtitle = UiKit.Label("Subtitle", bannerRect, string.Empty, 50, theme.text, TextAnchor.MiddleCenter);
            UiKit.Place(bannerSubtitle.rectTransform, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(1000f, 100f));

            countdownText = UiKit.Label("Countdown", root, string.Empty, 200, theme.gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(countdownText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(900f, 300f));
            countdownText.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

            joystick = VirtualJoystick.Create(root);

            pulseButton = UiKit.IconButton("Pulse", root, theme.pulse, theme.accent, 240f);
            UiKit.Place((RectTransform)pulseButton.transform, new Vector2(1f, 0f), new Vector2(-70f, 120f), new Vector2(240f, 240f));
            pulseFill = UiKit.Panel("Cooldown", pulseButton.transform, new Color(0f, 0f, 0f, 0.6f), theme.circle);
            UiKit.Stretch(pulseFill.rectTransform);
            pulseFill.type = Image.Type.Filled;
            pulseFill.fillMethod = Image.FillMethod.Radial360;
            pulseFill.fillOrigin = (int)Image.Origin360.Top;
            pulseFill.fillClockwise = false;
            pulseFill.raycastTarget = false;
            var pulseLabel = UiKit.Label("Label", pulseButton.transform, "PULS", 38, theme.text, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(pulseLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, -56f), new Vector2(240f, 50f));
        }

        private GameObject BuildIndicator(string name, Sprite icon, Color color, Vector2 position, out Text label)
        {
            var theme = UiTheme.Current;
            var chip = UiKit.Panel(name + "Indicator", root, new Color(theme.panel.r, theme.panel.g, theme.panel.b, 0.85f), theme.rounded);
            UiKit.Place(chip.rectTransform, new Vector2(0f, 1f), position, new Vector2(200f, 80f));
            chip.raycastTarget = false;
            var image = UiKit.Icon("Icon", chip.transform, icon, color, 56f);
            UiKit.Place(image.rectTransform, new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(56f, 56f));
            label = UiKit.Label("Time", chip.transform, "5.0s", 38, color, TextAnchor.MiddleRight, FontStyle.Bold);
            UiKit.Place(label.rectTransform, new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(120f, 60f));
            chip.gameObject.SetActive(false);
            return chip.gameObject;
        }
    }
}
