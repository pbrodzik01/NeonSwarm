using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.25f;

        private CanvasGroup fader;
        private bool loading;

        public static SceneLoader Instance { get; private set; }

        public static void Load(string sceneName)
        {
            if (Instance == null)
            {
                Time.timeScale = 1f;
                AudioManager.SetGameplayPaused(false);
                SceneManager.LoadScene(sceneName);
                return;
            }
            Time.timeScale = 0f;
            AudioManager.SetGameplayPaused(true);
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.LoadRoutine(sceneName));
        }

        public static void Reload()
        {
            Load(SceneManager.GetActiveScene().name);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            BuildFader();
        }

        private void BuildFader()
        {
            var canvas = UiKit.CreateCanvas("Fader", 1000, transform);
            var image = UiKit.Panel("Black", canvas.transform, UiTheme.Current.dark);
            UiKit.Stretch(image.rectTransform);
            fader = canvas.gameObject.AddComponent<CanvasGroup>();
            fader.alpha = 0f;
            fader.blocksRaycasts = false;
            fader.interactable = false;
        }

        public static bool IsLoading => Instance != null && Instance.loading;

        private IEnumerator LoadRoutine(string sceneName)
        {
            loading = true;
            fader.blocksRaycasts = true;
            yield return Fade(fader.alpha, 1f);
            var operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
            {
                yield return null;
            }
            Time.timeScale = 1f;
            AudioManager.SetGameplayPaused(false);
            yield return Fade(1f, 0f);
            fader.blocksRaycasts = false;
            loading = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (var t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                fader.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            fader.alpha = to;
        }
    }
}
