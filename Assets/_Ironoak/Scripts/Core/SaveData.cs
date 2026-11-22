using UnityEngine;

namespace Ironoak.Core
{
    /// <summary>Persistent best results (PlayerPrefs for the POC; swap for a file/cloud save later).</summary>
    public static class SaveData
    {
        const string KeyCoins = "ironoak.best.coins";
        const string KeyTime = "ironoak.best.time";

        public static int BestCoins => PlayerPrefs.GetInt(KeyCoins, 0);
        public static float BestTime => PlayerPrefs.GetFloat(KeyTime, 0f);
        public static bool HasRecord => BestTime > 0f;

        /// <summary>Stores the result if it beats the record. Returns true for a new best.</summary>
        public static bool Submit(int coins, float time)
        {
            bool better = !HasRecord || coins > BestCoins || (coins == BestCoins && time < BestTime);
            if (!better) return false;
            PlayerPrefs.SetInt(KeyCoins, coins);
            PlayerPrefs.SetFloat(KeyTime, time);
            PlayerPrefs.Save();
            return true;
        }

        public static string FormatTime(float seconds)
        {
            int s = Mathf.FloorToInt(seconds);
            return $"{s / 60}:{s % 60:00}";
        }
    }
}
