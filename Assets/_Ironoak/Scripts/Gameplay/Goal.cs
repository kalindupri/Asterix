using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>Village banner at the end of the level.</summary>
    public class Goal : MonoBehaviour
    {
        bool _reached;

        public static Goal Spawn(Transform parent, Vector2 position)
        {
            var go = new GameObject("Goal");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLibrary.Get("goal_banner");
            sr.sortingOrder = 3;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 3f);
            col.offset = new Vector2(0f, 1.5f);
            return go.AddComponent<Goal>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_reached) return;
            var player = other.GetComponent<PlayerController>();
            if (player == null || player.Health.Dead) return;
            _reached = true;
            GameEvents.RaiseGoalReached();
        }
    }
}
