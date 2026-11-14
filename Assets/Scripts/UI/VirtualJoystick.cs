using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        [SerializeField] private RectTransform baseRect;
        [SerializeField] private RectTransform knob;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float radius = 150f;
        [SerializeField] private float deadZone = 0.12f;
        [SerializeField] private float idleAlpha = 0.4f;

        private RectTransform area;
        private Vector2 restPosition;
        private int pointerId = NoPointer;

        public Vector2 Direction { get; private set; }
        public bool IsHeld => pointerId != NoPointer;

        public static VirtualJoystick Create(Transform parent)
        {
            var theme = UiTheme.Current;
            var zone = UiKit.Panel("JoystickZone", parent, new Color(1f, 1f, 1f, 0.001f));
            var zoneRect = zone.rectTransform;
            zoneRect.anchorMin = new Vector2(0f, 0f);
            zoneRect.anchorMax = new Vector2(0.62f, 0.42f);
            zoneRect.offsetMin = Vector2.zero;
            zoneRect.offsetMax = Vector2.zero;
            zoneRect.pivot = new Vector2(0f, 0f);

            var stickBase = UiKit.Icon("Base", zone.transform, theme.joystickBase, new Color(1f, 1f, 1f, 0.9f), 340f);
            var baseRect = stickBase.rectTransform;
            baseRect.anchorMin = Vector2.zero;
            baseRect.anchorMax = Vector2.zero;
            baseRect.pivot = new Vector2(0.5f, 0.5f);
            baseRect.anchoredPosition = new Vector2(250f, 260f);

            var knob = UiKit.Icon("Knob", stickBase.transform, theme.joystickKnob, theme.primary, 170f);
            knob.rectTransform.anchoredPosition = Vector2.zero;

            var joystick = zone.gameObject.AddComponent<VirtualJoystick>();
            joystick.baseRect = baseRect;
            joystick.knob = knob.rectTransform;
            joystick.group = stickBase.gameObject.AddComponent<CanvasGroup>();
            joystick.group.blocksRaycasts = false;
            if (Application.isPlaying)
            {
                joystick.Initialize();
            }
            return joystick;
        }

        private void Awake()
        {
            if (baseRect != null)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            area = (RectTransform)transform;
            restPosition = baseRect.anchoredPosition;
            group.alpha = idleAlpha;
        }

        private void OnDisable()
        {
            Release();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerId != NoPointer)
            {
                return;
            }
            pointerId = eventData.pointerId;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position, eventData.pressEventCamera, out var local))
            {
                var bottomLeft = local + area.rect.size * area.pivot;
                baseRect.anchoredPosition = ClampInsideArea(bottomLeft);
            }
            group.alpha = 1f;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerId)
            {
                return;
            }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, eventData.position, eventData.pressEventCamera, out var local))
            {
                return;
            }
            var offset = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = offset;
            var value = offset / radius;
            Direction = value.magnitude < deadZone ? Vector2.zero : value;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId)
            {
                Release();
            }
        }

        private void Release()
        {
            pointerId = NoPointer;
            Direction = Vector2.zero;
            if (knob != null)
            {
                knob.anchoredPosition = Vector2.zero;
                baseRect.anchoredPosition = restPosition;
                group.alpha = idleAlpha;
            }
        }

        private Vector2 ClampInsideArea(Vector2 point)
        {
            var margin = baseRect.sizeDelta * 0.5f;
            var size = area.rect.size;
            return new Vector2(
                Mathf.Clamp(point.x, margin.x, Mathf.Max(margin.x, size.x - margin.x)),
                Mathf.Clamp(point.y, margin.y, Mathf.Max(margin.y, size.y - margin.y)));
        }
    }
}
