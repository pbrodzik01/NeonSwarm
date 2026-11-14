using System;
using UnityEngine;

namespace NeonSwarm
{
    public enum PickupKind
    {
        Coin,
        Medkit,
        Shield,
        RapidFire
    }

    public enum EnemyKind
    {
        Drifter,
        Chaser,
        Shooter
    }

    public static class GameEvents
    {
        public static event Action<EnemyKind, Vector3> EnemyKilled;
        public static event Action<PickupKind> PickupCollected;
        public static event Action<int, int> PlayerHealthChanged;
        public static event Action PlayerDamaged;
        public static event Action PlayerDied;
        public static event Action PulseUsed;
        public static event Action<int, bool> LevelCompleted;
        public static event Action<int> ScoreChanged;
        public static event Action<int> CoinsChanged;
        public static event Action<string> Notification;

        public static void RaiseEnemyKilled(EnemyKind kind, Vector3 position) => EnemyKilled?.Invoke(kind, position);
        public static void RaisePickupCollected(PickupKind kind) => PickupCollected?.Invoke(kind);
        public static void RaisePlayerHealthChanged(int current, int max) => PlayerHealthChanged?.Invoke(current, max);
        public static void RaisePlayerDamaged() => PlayerDamaged?.Invoke();
        public static void RaisePlayerDied() => PlayerDied?.Invoke();
        public static void RaisePulseUsed() => PulseUsed?.Invoke();
        public static void RaiseLevelCompleted(int levelIndex, bool flawless) => LevelCompleted?.Invoke(levelIndex, flawless);
        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
        public static void RaiseCoinsChanged(int coins) => CoinsChanged?.Invoke(coins);
        public static void RaiseNotification(string message) => Notification?.Invoke(message);
    }
}
