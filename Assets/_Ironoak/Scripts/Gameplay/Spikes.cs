using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>Sharpened wooden stakes. Damage on touch.</summary>
    public class Spikes : MonoBehaviour
    {
        public static Spikes Spawn(Transform parent, Vector2 position)
        {
            var go = new GameObject("Spikes");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLibrary.Get("spikes");
            sr.sortingOrder = 4;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.85f, 0.45f);
            col.offset = new Vector2(0f, 0.23f);
            return go.AddComponent<Spikes>();
        }

        void OnTriggerStay2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null) player.Health.Damage(transform.position);
        }
    }
}
