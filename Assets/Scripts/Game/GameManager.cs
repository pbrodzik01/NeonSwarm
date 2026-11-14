using System;
using System.Collections;
using UnityEngine;

namespace NeonSwarm
{
    public enum GameState
    {
        Intro,
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }

    public struct LevelSummary
    {
        public int LevelIndex;
        public int LevelScore;
        public int CompletionBonus;
        public int TimeBonus;
        public int FlawlessBonus;
        public float Seconds;
        public int TotalScore;
        public bool HasNextLevel;
    }

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelConfig config;
        [SerializeField] private Player player;
        [SerializeField] private ArenaGenerator arena;
        [SerializeField] private WaveSpawner spawner;
        [SerializeField] private PickupSpawner pickups;
        [SerializeField] private DifficultyDirector difficulty;
        [SerializeField] private Hud hud;
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private LevelCompletePanel levelCompletePanel;

        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Intro;
        public LevelConfig Config => config;
        public float Elapsed { get; private set; }
        public WaveSpawner Spawner => spawner;
        public DifficultyDirector Difficulty => difficulty;

        public event Action<GameState> StateChanged;

        private void Awake()
        {
            Instance = this;
            GameSession.BeginLevel(config.levelIndex);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void OnEnable()
        {
            GameEvents.PlayerDied += OnPlayerDied;
            spawner.AllWavesCleared += OnAllWavesCleared;
        }

        private void OnDisable()
        {
            GameEvents.PlayerDied -= OnPlayerDied;
            spawner.AllWavesCleared -= OnAllWavesCleared;
        }

        private void OnDestroy()
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private IEnumerator Start()
        {
            AudioManager.PlayMusic(MusicId.Gameplay);
            Fx.Prewarm();
            var save = SaveSystem.Data;
            player.ApplyLoadout(ShopService.BuildLoadout(save));
            player.SetControlsEnabled(false);
            hud.Bind(this, player);

            var seed = Environment.TickCount ^ (config.levelIndex * 7919);
            arena.Generate(config.obstacleCount, seed);

            SetState(GameState.Intro);
            hud.ShowBanner(config.title.ToUpperInvariant(), config.subtitle, 1.6f);
            yield return new WaitForSeconds(1.2f);
            for (var count = 3; count > 0; count--)
            {
                hud.ShowCountdown(count.ToString());
                AudioManager.Play(SfxId.Countdown);
                yield return new WaitForSeconds(0.6f);
            }
            if (State != GameState.Intro)
            {
                yield break;
            }
            hud.ShowCountdown("START!");
            AudioManager.Play(SfxId.Go);

            if (save.startShields > 0)
            {
                save.startShields--;
                player.Health.ActivateShield(5f);
                GameEvents.RaiseNotification("Tarcza startowa aktywna (5 s)");
            }

            player.SetControlsEnabled(true);
            difficulty.Begin(player.Health);
            spawner.Begin(config, arena, player.transform, difficulty, seed);
            pickups.Begin(arena, config.pickupInterval);
            SetState(GameState.Playing);
        }

        private void Update()
        {
            if (State == GameState.Playing)
            {
                Elapsed += Time.deltaTime;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (State == GameState.Playing)
                {
                    Pause();
                }
                else if (State == GameState.Paused)
                {
                    pauseMenu.Back();
                }
            }
        }

        public void Pause()
        {
            if (State != GameState.Playing)
            {
                return;
            }
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            AudioManager.SetGameplayPaused(true);
            pauseMenu.Show();
        }

        public void Resume()
        {
            if (State != GameState.Paused)
            {
                return;
            }
            pauseMenu.Hide();
            Time.timeScale = 1f;
            AudioManager.SetGameplayPaused(false);
            SetState(GameState.Playing);
        }

        public void RestartLevel()
        {
            GameSession.RestartLevel();
            SceneLoader.Reload();
        }

        public void QuitToMenu()
        {
            SaveSystem.Save();
            SceneLoader.Load(SceneNames.MainMenu);
        }

        public void GoToNextLevel()
        {
            SceneLoader.Load(SceneNames.ForLevel(config.levelIndex + 1));
        }

        public void FinishRunToMenu()
        {
            ProgressTracker.FinishRun(RunOutcome.None);
            SceneLoader.Load(SceneNames.MainMenu);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                Pause();
                SaveSystem.Save();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && !Application.isEditor)
            {
                Pause();
            }
        }

        private void OnPlayerDied()
        {
            if (State != GameState.Playing && State != GameState.Intro)
            {
                return;
            }
            SetState(GameState.GameOver);
            spawner.Stop();
            pickups.Stop();
            difficulty.Stop();
            StartCoroutine(GameOverRoutine());
        }

        private IEnumerator GameOverRoutine()
        {
            hud.ShowBanner("ZESTRZELONY!", "Koniec gry", 1.5f);
            yield return new WaitForSecondsRealtime(1.6f);
            ProgressTracker.RecordLevelScore(config.levelIndex, GameSession.Score - GameSession.ScoreAtLevelStart);
            ProgressTracker.FinishRun(RunOutcome.Defeat);
            AudioManager.Play(SfxId.Lose);
            SceneLoader.Load(SceneNames.GameOver);
        }

        private void OnAllWavesCleared()
        {
            if (State != GameState.Playing)
            {
                return;
            }
            SetState(GameState.LevelComplete);
            pickups.Stop();
            difficulty.Stop();
            player.SetControlsEnabled(false);
            player.Health.MakeInvulnerable(999f);
            StartCoroutine(LevelCompleteRoutine());
        }

        private IEnumerator LevelCompleteRoutine()
        {
            var flawless = !player.Health.TookDamage;
            var summary = new LevelSummary
            {
                LevelIndex = config.levelIndex,
                LevelScore = GameSession.Score - GameSession.ScoreAtLevelStart,
                CompletionBonus = ScoreRules.LevelComplete,
                TimeBonus = ScoreRules.TimeBonus(config.parTimeSeconds, Elapsed),
                FlawlessBonus = flawless ? ScoreRules.FlawlessBonus : 0,
                Seconds = Elapsed,
                HasNextLevel = !GameSession.IsLastLevel(config.levelIndex)
            };
            GameSession.AddScore(summary.CompletionBonus + summary.TimeBonus + summary.FlawlessBonus);
            summary.TotalScore = GameSession.Score;
            GameSession.MarkLevelCleared();
            ProgressTracker.RecordLevelScore(config.levelIndex, GameSession.Score - GameSession.ScoreAtLevelStart);
            GameEvents.RaiseLevelCompleted(config.levelIndex, flawless);
            AudioManager.Play(SfxId.Win);
            hud.ShowBanner("POZIOM UKOŃCZONY!", flawless ? "Bez obrażeń!" : string.Empty, 1.6f);
            yield return new WaitForSecondsRealtime(1.8f);

            if (summary.HasNextLevel)
            {
                levelCompletePanel.Show(summary, this);
            }
            else
            {
                ProgressTracker.FinishRun(RunOutcome.Victory);
                SceneLoader.Load(SceneNames.GameOver);
            }
        }

        private void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
