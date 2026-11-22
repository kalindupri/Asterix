using System.Collections.Generic;
using Ironoak.Powers;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>
    /// One per level scene. Describes the level to the game flow: name, camera bounds,
    /// player spawn point and the powers Brenna starts with.
    /// </summary>
    public class LevelRoot : MonoBehaviour
    {
        public string displayName = "LEVEL";
        public Rect bounds = new Rect(0f, 0f, 100f, 20f);
        public Transform spawnPoint;
        public List<Power> startingPowers = new List<Power> { Power.Sling };

        public int TotalCoins => GetComponentsInChildren<Coin>(true).Length;

        public int TotalSecrets
        {
            get
            {
                int n = 0;
                foreach (var s in GetComponentsInChildren<SecretArea>(true)) if (!s.isTrial) n++;
                return n;
            }
        }

        public bool HasTrial
        {
            get
            {
                foreach (var s in GetComponentsInChildren<SecretArea>(true)) if (s.isTrial) return true;
                return false;
            }
        }

        public Vector2 Spawn => spawnPoint != null ? (Vector2)spawnPoint.position : new Vector2(2f, 4f);

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.4f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
