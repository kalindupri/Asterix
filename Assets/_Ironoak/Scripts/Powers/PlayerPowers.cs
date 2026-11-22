using System;
using UnityEngine;

namespace Ironoak.Powers
{
    public enum Power { Sling = 0, Rootgrow = 1, Branchgrow = 2, Vinefall = 3, Lodestone = 4, ForgeHeat = 5 }

    /// <summary>Which of Brenna's abilities are unlocked.</summary>
    public class PlayerPowers : MonoBehaviour
    {
        public static PlayerPowers Local { get; private set; }

        public event Action<Power> Unlocked;

        int _mask;

        void Awake() => Local = this;

        void OnDestroy()
        {
            if (Local == this) Local = null;
        }

        public bool Has(Power p) => (_mask & (1 << (int)p)) != 0;

        public void Unlock(Power p, bool silent = false)
        {
            if (Has(p)) return;
            _mask |= 1 << (int)p;
            if (!silent) Unlocked?.Invoke(p);
        }

        public static string DisplayName(Power p)
        {
            switch (p)
            {
                case Power.Sling: return "SLING";
                case Power.Rootgrow: return "ROOTGROW";
                case Power.Branchgrow: return "BRANCHGROW";
                case Power.Vinefall: return "VINEFALL";
                case Power.Lodestone: return "LODESTONE";
                default: return "FORGE-HEAT";
            }
        }
    }
}
