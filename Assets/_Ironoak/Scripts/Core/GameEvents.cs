using System;
using Ironoak.Gameplay;
using Ironoak.Powers;

namespace Ironoak.Core
{
    /// <summary>Decoupled gameplay events raised by level objects and consumed by the game flow / UI.</summary>
    public static class GameEvents
    {
        public static event Action CoinCollected;
        public static event Action EnemyDefeated;
        public static event Action CheckpointReached;
        public static event Action GoalReached;
        public static event Action<SecretArea> SecretFound;
        public static event Action<Collectible> CollectibleFound;
        public static event Action<Power> PowerUnlocked;
        public static event Action<string> Message;

        public static void RaiseCoinCollected() => CoinCollected?.Invoke();
        public static void RaiseEnemyDefeated() => EnemyDefeated?.Invoke();
        public static void RaiseCheckpointReached() => CheckpointReached?.Invoke();
        public static void RaiseGoalReached() => GoalReached?.Invoke();
        public static void RaiseSecretFound(SecretArea s) => SecretFound?.Invoke(s);
        public static void RaiseCollectibleFound(Collectible c) => CollectibleFound?.Invoke(c);
        public static void RaisePowerUnlocked(Power p) => PowerUnlocked?.Invoke(p);
        public static void RaiseMessage(string text) => Message?.Invoke(text);
    }
}
