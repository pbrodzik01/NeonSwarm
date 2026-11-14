using UnityEngine;

namespace NeonSwarm
{
    public enum RunOutcome
    {
        None,
        Defeat,
        Victory
    }

    public static class GameSession
    {
        public static bool HasActiveRun { get; private set; }
        public static int LevelIndex { get; private set; }
        public static int Score { get; private set; }
        public static int ScoreAtLevelStart { get; private set; }
        public static int RunKills { get; private set; }
        public static int RunCoins { get; private set; }
        public static int LevelsCleared { get; private set; }
        public static RunOutcome Outcome { get; private set; }
        public static bool NewRecord { get; private set; }
        public static int BestScoreBeforeRun { get; private set; }

        public static void StartRun(int levelIndex)
        {
            HasActiveRun = true;
            BestScoreBeforeRun = SaveSystem.Data.bestScore;
            LevelIndex = Mathf.Clamp(levelIndex, 0, SceneNames.LevelCount - 1);
            Score = 0;
            ScoreAtLevelStart = 0;
            RunKills = 0;
            RunCoins = 0;
            LevelsCleared = 0;
            Outcome = RunOutcome.None;
            NewRecord = false;
        }

        public static void BeginLevel(int levelIndex)
        {
            if (!HasActiveRun)
            {
                StartRun(levelIndex);
            }
            LevelIndex = Mathf.Clamp(levelIndex, 0, SceneNames.LevelCount - 1);
            ScoreAtLevelStart = Score;
            Outcome = RunOutcome.None;
        }

        public static void RestartLevel()
        {
            HasActiveRun = true;
            Score = ScoreAtLevelStart;
            Outcome = RunOutcome.None;
            NewRecord = false;
        }

        public static int AddScore(int amount)
        {
            Score = Mathf.Max(0, Score + amount);
            GameEvents.RaiseScoreChanged(Score);
            return Score;
        }

        public static void RegisterKill() => RunKills++;

        public static void RegisterCoins(int amount) => RunCoins += amount;

        public static void MarkLevelCleared() => LevelsCleared++;

        public static void EndRun(RunOutcome outcome, bool newRecord)
        {
            Outcome = outcome;
            NewRecord = newRecord;
            HasActiveRun = false;
        }

        public static bool IsLastLevel(int levelIndex) => levelIndex >= SceneNames.LevelCount - 1;
    }
}
