using System;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private SwitchControl musicSwitch;
        [SerializeField] private SwitchControl sfxSwitch;
        [SerializeField] private SwitchControl vibrationSwitch;
        [SerializeField] private SwitchControl batterySwitch;
        [SerializeField] private SwitchControl fpsSwitch;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeValue;
        [SerializeField] private InputField nameInput;
        [SerializeField] private Button resetButton;
        [SerializeField] private Text resetLabel;
        [SerializeField] private Button closeButton;

        private bool resetArmed;
        private bool loading;

        public event Action Closed;

        public bool IsOpen => root != null && root.gameObject.activeSelf;

        private void Awake()
        {
            if (root == null)
            {
                BuildUI(transform, true);
            }
            musicSwitch.Changed += value => Change(s => s.music = value);
            sfxSwitch.Changed += value => Change(s => s.sfx = value);
            vibrationSwitch.Changed += value => Change(s => s.vibration = value);
            batterySwitch.Changed += value => Change(s => s.batterySaver = value);
            fpsSwitch.Changed += value => Change(s => s.showFps = value);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            nameInput.onEndEdit.AddListener(OnNameChanged);
            closeButton.onClick.AddListener(Close);
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetClicked);
            }
        }

        public void Show()
        {
            root.gameObject.SetActive(true);
            Load();
        }

        public void Close()
        {
            SaveSystem.Save();
            root.gameObject.SetActive(false);
            Closed?.Invoke();
        }

        private void Load()
        {
            loading = true;
            var settings = SaveSystem.Data.settings;
            musicSwitch.Value = settings.music;
            sfxSwitch.Value = settings.sfx;
            vibrationSwitch.Value = settings.vibration;
            batterySwitch.Value = settings.batterySaver;
            fpsSwitch.Value = settings.showFps;
            volumeSlider.value = settings.volume;
            volumeValue.text = Mathf.RoundToInt(settings.volume * 100f) + "%";
            nameInput.text = settings.playerName;
            resetArmed = false;
            if (resetLabel != null)
            {
                resetLabel.text = "RESETUJ POSTĘP";
            }
            loading = false;
        }

        private void Change(Action<SettingsData> apply)
        {
            if (loading)
            {
                return;
            }
            apply(SaveSystem.Data.settings);
            AppSettings.Apply();
        }

        private void OnVolumeChanged(float value)
        {
            volumeValue.text = Mathf.RoundToInt(value * 100f) + "%";
            Change(s => s.volume = value);
        }

        private void OnNameChanged(string value)
        {
            var trimmed = value.Trim();
            Change(s => s.playerName = string.IsNullOrEmpty(trimmed) ? "Pilot" : trimmed);
            nameInput.text = SaveSystem.Data.settings.playerName;
        }

        private void OnResetClicked()
        {
            if (!resetArmed)
            {
                resetArmed = true;
                resetLabel.text = "NA PEWNO? DOTKNIJ PONOWNIE";
                return;
            }
            SaveSystem.ResetProgress();
            GameEvents.RaiseNotification("Postęp został zresetowany");
            Load();
        }

        public void BuildUI(Transform parent, bool allowReset)
        {
            var theme = UiTheme.Current;
            root = UiKit.Modal("Settings", parent, "USTAWIENIA", out var body, out closeButton);
            var layout = UiKit.VStack(body, 14f);
            layout.childAlignment = TextAnchor.UpperCenter;

            musicSwitch = UiWidgets.Switch("Music", body, "Muzyka");
            sfxSwitch = UiWidgets.Switch("Sfx", body, "Efekty dźwiękowe");

            var volumeRow = UiKit.Rect("Volume", body);
            UiKit.Height(volumeRow, 150f);
            var volumeCaption = UiKit.Label("Caption", volumeRow, "Głośność", 52, theme.text, TextAnchor.UpperLeft);
            UiKit.Place(volumeCaption.rectTransform, new Vector2(0f, 1f), Vector2.zero, new Vector2(500f, 64f));
            volumeValue = UiKit.Label("Value", volumeRow, "80%", 48, theme.primary, TextAnchor.UpperRight, FontStyle.Bold);
            UiKit.Place(volumeValue.rectTransform, new Vector2(1f, 1f), Vector2.zero, new Vector2(200f, 64f));
            volumeSlider = UiKit.Slider("Slider", volumeRow);
            var sliderRect = (RectTransform)volumeSlider.transform;
            sliderRect.anchorMin = new Vector2(0f, 0f);
            sliderRect.anchorMax = new Vector2(1f, 0f);
            sliderRect.pivot = new Vector2(0.5f, 0f);
            sliderRect.sizeDelta = new Vector2(0f, 80f);
            sliderRect.anchoredPosition = Vector2.zero;

            vibrationSwitch = UiWidgets.Switch("Vibration", body, "Wibracje");
            batterySwitch = UiWidgets.Switch("Battery", body, "Oszczędzanie (30 FPS)");
            fpsSwitch = UiWidgets.Switch("Fps", body, "Licznik FPS");

            UiWidgets.Caption(body, "Nazwa gracza (ranking)", 40);
            nameInput = UiKit.Input("Name", body, "Pilot");
            UiKit.Height(nameInput, 110f);

            if (allowReset)
            {
                resetButton = UiKit.Button("Reset", body, "RESETUJ POSTĘP", theme.danger, 42);
                UiKit.Height(resetButton, 110f);
                resetLabel = resetButton.GetComponentInChildren<Text>();
            }

            root.gameObject.SetActive(false);
        }
    }
}
