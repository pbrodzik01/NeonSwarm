using UnityEngine;
using UnityEngine.UI;

namespace NeonSwarm
{
    public abstract class MenuPanel : MonoBehaviour
    {
        [SerializeField] protected RectTransform root;
        [SerializeField] protected RectTransform body;
        [SerializeField] protected Button closeButton;

        protected abstract string Title { get; }

        public bool IsOpen => root != null && root.gameObject.activeSelf;

        public static T Create<T>(Transform parent) where T : MenuPanel
        {
            var rect = UiKit.Rect(typeof(T).Name, parent);
            UiKit.Stretch(rect);
            rect.gameObject.SetActive(false);
            var panel = rect.gameObject.AddComponent<T>();
            panel.BuildUI();
            rect.gameObject.SetActive(true);
            return panel;
        }

        protected virtual void Awake()
        {
            if (root == null)
            {
                BuildUI();
            }
            closeButton.onClick.AddListener(Hide);
        }

        public void BuildUI()
        {
            root = UiKit.Modal("Modal", transform, Title, out body, out closeButton);
            BuildBody(body);
            root.gameObject.SetActive(false);
        }

        protected abstract void BuildBody(RectTransform container);

        public virtual void Show()
        {
            root.gameObject.SetActive(true);
            Refresh();
        }

        public virtual void Hide()
        {
            root.gameObject.SetActive(false);
        }

        protected virtual void Refresh()
        {
        }

        protected static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        protected static Image Row(Transform parent, float height)
        {
            var theme = UiTheme.Current;
            var row = UiKit.Panel("Row", parent, theme.panelLight, theme.rounded);
            UiKit.Height(row, height);
            row.raycastTarget = false;
            return row;
        }

        protected static Text Heading(Transform parent, string text)
        {
            var label = UiKit.Label("Heading", parent, text, 46, UiTheme.Current.accent, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Height(label, 80f);
            return label;
        }
    }
}
