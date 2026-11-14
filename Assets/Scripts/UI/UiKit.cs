using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonSwarm
{
    public static class UiKit
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        private static UiTheme Theme => UiTheme.Current;

        public static Canvas CreateCanvas(string name, int sortingOrder, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static RectTransform SafeRoot(Transform canvas)
        {
            var rect = Rect("SafeArea", canvas);
            Stretch(rect);
            rect.gameObject.AddComponent<SafeArea>();
            return rect;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static void Stretch(RectTransform rect, float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static Image Panel(string name, Transform parent, Color color, Sprite sprite = null)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            if (sprite != null && sprite.border != Vector4.zero)
            {
                image.type = Image.Type.Sliced;
            }
            return image;
        }

        public static Image Icon(string name, Transform parent, Sprite sprite, Color color, float size)
        {
            var image = Panel(name, parent, color, sprite);
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.rectTransform.sizeDelta = new Vector2(size, size);
            return image;
        }

        public static Text Label(string name, Transform parent, string text, int size, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var rect = Rect(name, parent);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = Theme.font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = anchor;
            label.fontStyle = style;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = true;
            return label;
        }

        public static Button Button(string name, Transform parent, string text, Color color, int fontSize = 52)
        {
            var image = Panel(name, parent, color, Theme.rounded);
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
            colors.fadeDuration = 0.06f;
            button.colors = colors;
            image.gameObject.AddComponent<ButtonSfx>();
            var label = Label("Label", image.transform, text, fontSize, Theme.dark, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(label.rectTransform, 16f, 8f, 16f, 8f);
            return button;
        }

        public static Button IconButton(string name, Transform parent, Sprite icon, Color background, float size)
        {
            var image = Panel(name, parent, background, Theme.circle);
            image.type = Image.Type.Simple;
            image.rectTransform.sizeDelta = new Vector2(size, size);
            var button = image.gameObject.AddComponent<Button>();
            image.gameObject.AddComponent<ButtonSfx>();
            var glyph = Icon("Icon", image.transform, icon, Theme.dark, size * 0.55f);
            glyph.rectTransform.anchoredPosition = Vector2.zero;
            return button;
        }

        public static Slider Slider(string name, Transform parent)
        {
            var root = Rect(name, parent);
            var slider = root.gameObject.AddComponent<Slider>();
            var background = Panel("Background", root, Theme.panelLight, Theme.rounded);
            Stretch(background.rectTransform, 0f, 22f, 0f, 22f);
            var fillArea = Rect("Fill Area", root);
            Stretch(fillArea, 10f, 22f, 10f, 22f);
            var fill = Panel("Fill", fillArea, Theme.primary, Theme.rounded);
            Stretch(fill.rectTransform);
            var handleArea = Rect("Handle Area", root);
            Stretch(handleArea, 30f, 0f, 30f, 0f);
            var handle = Panel("Handle", handleArea, Color.white, Theme.circle);
            handle.type = Image.Type.Simple;
            handle.rectTransform.sizeDelta = new Vector2(70f, 0f);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        public static InputField Input(string name, Transform parent, string placeholder)
        {
            var background = Panel(name, parent, Theme.panelLight, Theme.rounded);
            var field = background.gameObject.AddComponent<InputField>();
            var text = Label("Text", background.transform, string.Empty, 46, Theme.text, TextAnchor.MiddleLeft);
            Stretch(text.rectTransform, 30f, 6f, 30f, 6f);
            text.supportRichText = false;
            var hint = Label("Placeholder", background.transform, placeholder, 46, Theme.textMuted, TextAnchor.MiddleLeft, FontStyle.Italic);
            Stretch(hint.rectTransform, 30f, 6f, 30f, 6f);
            field.textComponent = text;
            field.placeholder = hint;
            field.characterLimit = 14;
            return field;
        }

        public static RectTransform ScrollList(string name, Transform parent, out RectTransform content)
        {
            var root = Rect(name, parent);
            var scroll = root.gameObject.AddComponent<ScrollRect>();
            var viewport = Panel("Viewport", root, new Color(1f, 1f, 1f, 0.01f));
            Stretch(viewport.rectTransform);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = Rect("Content", viewport.transform);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 18f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport.rectTransform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            return root;
        }

        public static VerticalLayoutGroup VStack(RectTransform rect, float spacing, int padding = 0)
        {
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            return layout;
        }

        public static LayoutElement Height(Component component, float height)
        {
            if (!component.TryGetComponent<LayoutElement>(out var element))
            {
                element = component.gameObject.AddComponent<LayoutElement>();
            }
            element.minHeight = height;
            element.preferredHeight = height;
            return element;
        }

        public static RectTransform Modal(string name, Transform parent, string title, out RectTransform body, out Button close)
        {
            var dim = Panel(name, parent, new Color(0f, 0f, 0f, 0.72f));
            Stretch(dim.rectTransform);
            var card = Panel("Card", dim.transform, Theme.panel, Theme.rounded);
            Stretch(card.rectTransform, 50f, 140f, 50f, 140f);
            var outline = Panel("Outline", card.transform, Theme.primary, Theme.roundedOutline);
            Stretch(outline.rectTransform);
            outline.raycastTarget = false;
            var heading = Label("Title", card.transform, title, 76, Theme.primary, TextAnchor.MiddleCenter, FontStyle.Bold);
            Place(heading.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(900f, 110f));
            body = Rect("Body", card.transform);
            Stretch(body, 40f, 170f, 40f, 190f);
            close = Button("Close", card.transform, "POWRÓT", Theme.primary);
            Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(560f, 120f));
            return dim.rectTransform;
        }
    }
}
