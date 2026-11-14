using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class FpsCounter : MonoBehaviour
    {
        [SerializeField] private float refreshSeconds = 0.5f;

        private Text label;
        private GameObject canvasObject;
        private int frames;
        private float elapsed;

        private void Awake()
        {
            var canvas = UiKit.CreateCanvas("FPS", 950, transform);
            canvasObject = canvas.gameObject;
            var safe = UiKit.SafeRoot(canvas.transform);
            label = UiKit.Label("Fps", safe, "FPS", 30, UiTheme.Current.success, TextAnchor.LowerLeft, FontStyle.Bold);
            UiKit.Place(label.rectTransform, new Vector2(0f, 0f), new Vector2(16f, 8f), new Vector2(900f, 44f));
            label.gameObject.AddComponent<Outline>();
        }

        private void Update()
        {
            var visible = SaveSystem.Data.settings.showFps;
            if (canvasObject.activeSelf != visible)
            {
                canvasObject.SetActive(visible);
            }
            if (!visible)
            {
                return;
            }
            frames++;
            elapsed += Time.unscaledDeltaTime;
            if (elapsed < refreshSeconds)
            {
                return;
            }
            var fps = frames / elapsed;
            label.text = $"FPS {fps:0} · {1000f / fps:0.0} ms · {PoolManager.Describe()}";
            frames = 0;
            elapsed = 0f;
        }
    }
}
