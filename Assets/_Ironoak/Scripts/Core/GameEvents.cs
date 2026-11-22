using System;

namespace Ironoak.Core
{
    /// <summary>Decoupled gameplay events raised by level objects and consumed by the game flow / UI.</summary>
    public static class GameEvents
    {
        public static event Action CoinCollected;
        public static event Action EnemyDefeated;
        public static event Action CheckpointReached;
        public static event Action GoalReached;

        public static void RaiseCoinCollected() => CoinCollected?.Invoke();
        public static void RaiseEnemyDefeated() => EnemyDefeated?.Invoke();
        public static void RaiseCheckpointReached() => CheckpointReached?.Invoke();
        public static void RaiseGoalReached() => GoalReached?.Invoke();
    }
}
