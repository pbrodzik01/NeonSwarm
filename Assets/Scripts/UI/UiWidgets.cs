using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public static class UiWidgets
    {
        public static SwitchControl Switch(string name, Transform parent, string caption)
        {
            var theme = UiTheme.Current;
            var row = UiKit.Rect(name, parent);
            UiKit.Height(row, 120f);
            var label = UiKit.Label("Caption", row, caption, 52, theme.text, TextAnchor.MiddleLeft);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(0.6f, 1f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            var button = UiKit.Button("Switch", row, "WŁ.", theme.success, 46);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(0.62f, 0.1f);
            rect.anchorMax = new Vector2(1f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var control = row.gameObject.AddComponent<SwitchControl>();
            control.Setup(button, button.GetComponentInChildren<Text>());
            return control;
        }

        public static Text Caption(Transform parent, string text, int size = 48)
        {
            var label = UiKit.Label("Caption", parent, text, size, UiTheme.Current.textMuted, TextAnchor.MiddleLeft);
            UiKit.Height(label, size * 1.6f);
            return label;
        }
    }
}
