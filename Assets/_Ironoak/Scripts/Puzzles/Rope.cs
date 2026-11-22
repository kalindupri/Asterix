using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>
    /// A taut rope between this transform and <see cref="toEnd"/> (local offset).
    /// Cut it with the sling to send a signal: drop a bridge, release a crate...
    /// </summary>
    public class Rope : SignalSender, IHittable
    {
        public Vector2 toEnd = new Vector2(0f, 3f);

        GameObject _visual;
        BoxCollider2D _hitbox;
        bool _cut;

        public bool IsCut => _cut;

        void Awake()
        {
            float length = toEnd.magnitude;
            float angle = Mathf.Atan2(toEnd.y, toEnd.x) * Mathf.Rad2Deg - 90f;

            _visual = new GameObject("RopeVisual");
            _visual.transform.SetParent(transform, false);
            _visual.transform.localPosition = toEnd * 0.5f;
            _visual.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            var sr = _visual.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLibrary.Get("rope");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(0.25f, length);
            sr.sortingOrder = 6;

            _hitbox = _visual.AddComponent<BoxCollider2D>();
            _hitbox.isTrigger = true;
            _hitbox.size = new Vector2(0.45f, Mathf.Max(0.2f, length - 0.2f));
        }

        public void OnSlingHit(Vector2 point, Vector2 direction)
        {
            if (_cut) return;
            _cut = true;
            _visual.SetActive(false);
            Sfx.Play(SfxId.Break, 0.6f);
            Fx.Burst(point, new Color(0.8f, 0.65f, 0.4f), 8, 2.5f, 0.5f);
            Emit(true);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.9f, 0.7f, 0.4f);
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)toEnd);
        }
    }
}
