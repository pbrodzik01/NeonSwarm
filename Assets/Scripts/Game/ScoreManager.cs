using UnityEngine;

namespace NeonSwarm
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private Color popupColor = new Color(1f, 0.85f, 0.3f);

        public static ScoreManager Instance { get; private set; }

        public int Score => GameSession.Score;
        public int LevelScore => GameSession.Score - GameSession.ScoreAtLevelStart;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void Add(int amount, Vector3? popupAt)
        {
            if (Instance != null)
            {
                Instance.AddScore(amount, popupAt);
            }
            else
            {
                GameSession.AddScore(amount);
            }
        }

        public void AddScore(int amount)
        {
            AddScore(amount, null);
        }

        public void AddScore(int amount, Vector3? popupAt)
        {
            GameSession.AddScore(amount);
            if (popupAt.HasValue)
            {
                Fx.Text(popupAt.Value + Vector3.up * 0.3f, "+" + amount, popupColor);
            }
        }
    }
}
