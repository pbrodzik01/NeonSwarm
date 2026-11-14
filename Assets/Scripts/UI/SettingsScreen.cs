using UnityEngine;

namespace NeonSwarm
{
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private SettingsPanel panel;

        private void Awake()
        {
            if (panel == null)
            {
                BuildUI();
            }
            panel.Closed += () => SceneLoader.Load(SceneNames.MainMenu);
        }

        private void Start()
        {
            AudioManager.PlayMusic(MusicId.Menu);
            panel.Show();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && panel.IsOpen)
            {
                panel.Close();
            }
        }

        public void BuildUI()
        {
            var theme = UiTheme.Current;
            UiKit.EnsureEventSystem();
            var canvas = UiKit.CreateCanvas("Settings Canvas", 0, transform);
            var background = UiKit.Panel("Background", canvas.transform, theme.background);
            UiKit.Stretch(background.rectTransform);
            var safe = UiKit.SafeRoot(canvas.transform);
            var holder = UiKit.Rect("SettingsPanel", safe);
            UiKit.Stretch(holder);
            holder.gameObject.SetActive(false);
            panel = holder.gameObject.AddComponent<SettingsPanel>();
            panel.BuildUI(holder, true);
            holder.gameObject.SetActive(true);
        }
    }
}
