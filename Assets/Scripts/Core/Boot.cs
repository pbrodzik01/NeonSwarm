using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private float minimumSeconds = 1.2f;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text status;

        private void Awake()
        {
            if (progressFill == null)
            {
                BuildUI();
            }
        }

        public void BuildUI()
        {
            var theme = UiTheme.Current;
            UiKit.EnsureEventSystem();
            var canvas = UiKit.CreateCanvas("BootCanvas", 0, transform);
            var background = UiKit.Panel("Background", canvas.transform, theme.background);
            UiKit.Stretch(background.rectTransform);
            var logo = UiKit.Icon("Logo", canvas.transform, theme.ship, theme.primary, 260f);
            UiKit.Place(logo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(260f, 260f));
            var title = UiKit.Label("Title", canvas.transform, "NEON SWARM", 120, theme.primary, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1000f, 170f));
            var bar = UiKit.Panel("ProgressBar", canvas.transform, theme.panelLight, theme.rounded);
            UiKit.Place(bar.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(700f, 40f));
            progressFill = UiKit.Panel("Fill", bar.transform, theme.accent, theme.rounded);
            UiKit.Stretch(progressFill.rectTransform);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = 0f;
            status = UiKit.Label("Status", canvas.transform, "Wczytywanie...", 44, theme.textMuted);
            UiKit.Place(status.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(900f, 80f));
        }

        private IEnumerator Start()
        {
            var steps = new[] { "Wczytywanie zapisu...", "Konfiguracja dźwięku...", "Przygotowanie roju..." };
            for (var i = 0; i < steps.Length; i++)
            {
                status.text = steps[i];
                var target = (i + 1f) / steps.Length;
                var from = progressFill.fillAmount;
                var stepDuration = minimumSeconds / steps.Length;
                for (var t = 0f; t < stepDuration; t += Time.unscaledDeltaTime)
                {
                    progressFill.fillAmount = Mathf.Lerp(from, target, t / stepDuration);
                    yield return null;
                }
            }
            progressFill.fillAmount = 1f;
            SceneLoader.Load(SceneNames.MainMenu);
        }
    }
}
