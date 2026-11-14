using UnityEngine;

namespace NeonSwarm
{
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private Vector2 visibleArea = new Vector2(9.2f, 15.2f);
        [SerializeField] private float topHudWorldMargin = 1.4f;

        private static CameraRig instance;

        private Camera cam;
        private Vector3 basePosition;
        private float shakeTime;
        private float shakeDuration;
        private float shakeAmplitude;
        private Vector2Int lastScreen;

        public static void Shake(float amplitude, float duration)
        {
            if (instance == null)
            {
                return;
            }
            if (amplitude >= instance.shakeAmplitude * (instance.shakeTime / Mathf.Max(0.01f, instance.shakeDuration)))
            {
                instance.shakeAmplitude = amplitude;
                instance.shakeDuration = duration;
                instance.shakeTime = duration;
            }
        }

        private void Awake()
        {
            instance = this;
            cam = GetComponent<Camera>();
            basePosition = transform.position;
            Fit();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void LateUpdate()
        {
            if (lastScreen.x != Screen.width || lastScreen.y != Screen.height)
            {
                Fit();
            }
            if (shakeTime > 0f)
            {
                shakeTime -= Time.unscaledDeltaTime;
                var strength = shakeAmplitude * Mathf.Clamp01(shakeTime / shakeDuration);
                transform.position = basePosition + (Vector3)(Random.insideUnitCircle * strength);
            }
            else
            {
                transform.position = basePosition;
            }
        }

        private void Fit()
        {
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            var aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 9f / 16f;
            var sizeForWidth = visibleArea.x * 0.5f / aspect;
            var sizeForHeight = (visibleArea.y + topHudWorldMargin) * 0.5f;
            cam.orthographicSize = Mathf.Max(sizeForWidth, sizeForHeight);
            basePosition = new Vector3(0f, topHudWorldMargin * 0.5f, basePosition.z);
            transform.position = basePosition;
        }
    }
}
