using UnityEngine;

namespace NeonSwarm
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private Rect applied;
        private Vector2Int screen;

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (applied != Screen.safeArea || screen.x != Screen.width || screen.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            var safe = Screen.safeArea;
            applied = safe;
            screen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
