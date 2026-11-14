using UnityEngine;

namespace NeonSwarm
{
    [RequireComponent(typeof(TextMesh))]
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private float riseSpeed = 1.4f;

        private TextMesh textMesh;
        private Poolable poolable;
        private float age;
        private Color color;

        private void Awake()
        {
            textMesh = GetComponent<TextMesh>();
            poolable = GetComponent<Poolable>();
            GetComponent<MeshRenderer>().sortingOrder = SortingOrder.FloatingText;
        }

        public void Show(string text, Color tint)
        {
            textMesh.text = text;
            color = tint;
            textMesh.color = tint;
            age = 0f;
        }

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += Vector3.up * (riseSpeed * Time.deltaTime);
            var faded = color;
            faded.a = 1f - Mathf.Clamp01(age / duration);
            textMesh.color = faded;
            if (age >= duration)
            {
                poolable.Release();
            }
        }
    }
}
