using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class Toast : MonoBehaviour
    {
        [SerializeField] private float visibleSeconds = 2.4f;

        private readonly Queue<string> queue = new Queue<string>();
        private CanvasGroup group;
        private Text label;
        private bool showing;

        private void Awake()
        {
            var theme = UiTheme.Current;
            var canvas = UiKit.CreateCanvas("Toasts", 900, transform);
            var safe = UiKit.SafeRoot(canvas.transform);
            var card = UiKit.Panel("Toast", safe, new Color(theme.panel.r, theme.panel.g, theme.panel.b, 0.95f), theme.rounded);
            UiKit.Place(card.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -430f), new Vector2(960f, 130f));
            card.raycastTarget = false;
            var outline = UiKit.Panel("Outline", card.transform, theme.gold, theme.roundedOutline);
            UiKit.Stretch(outline.rectTransform);
            outline.raycastTarget = false;
            label = UiKit.Label("Text", card.transform, string.Empty, 42, theme.gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(label.rectTransform, 24f, 8f, 24f, 8f);
            group = card.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            GameEvents.Notification += Enqueue;
        }

        private void OnDisable()
        {
            GameEvents.Notification -= Enqueue;
        }

        private void Enqueue(string message)
        {
            queue.Enqueue(message);
            if (!showing)
            {
                StartCoroutine(ShowQueue());
            }
        }

        private IEnumerator ShowQueue()
        {
            showing = true;
            while (queue.Count > 0)
            {
                label.text = queue.Dequeue();
                yield return FadeTo(1f);
                yield return new WaitForSecondsRealtime(visibleSeconds);
                yield return FadeTo(0f);
            }
            showing = false;
        }

        private IEnumerator FadeTo(float target)
        {
            var start = group.alpha;
            for (var t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(start, target, t / 0.2f);
                yield return null;
            }
            group.alpha = target;
        }
    }
}
