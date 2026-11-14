using System;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class SwitchControl : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text label;
        [SerializeField] private bool value = true;

        public event Action<bool> Changed;

        public bool Value
        {
            get => value;
            set
            {
                this.value = value;
                Refresh();
            }
        }

        public void Setup(Button targetButton, Text targetLabel)
        {
            button = targetButton;
            label = targetLabel;
            if (Application.isPlaying)
            {
                button.onClick.AddListener(Toggle);
            }
            Refresh();
        }

        private void Awake()
        {
            if (button == null)
            {
                return;
            }
            button.onClick.AddListener(Toggle);
            Refresh();
        }

        private void Toggle()
        {
            Value = !value;
            Changed?.Invoke(value);
        }

        private void Refresh()
        {
            if (button == null)
            {
                return;
            }
            var theme = UiTheme.Current;
            label.text = value ? "WŁ." : "WYŁ.";
            button.image.color = value ? theme.success : theme.panelLight;
            label.color = value ? theme.dark : theme.textMuted;
        }
    }
}
