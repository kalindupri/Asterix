using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>Weak, cracked stones. Break them with the sling to reveal hidden paths.</summary>
    public class CrackedWall : MonoBehaviour, IHittable
    {
        [Min(1)] public int hitsToBreak = 1;
        [Range(1, 6)] public int height = 2;

        int _hits;
        bool _broken;

        public bool Broken => _broken;

        void Awake() => Configure();
        void OnValidate() => Configure();

        /// <summary>Sizes collider and sprite from <see cref="height"/>.</summary>
        public void Configure()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = new Vector2(1f, height);
                box.offset = new Vector2(0f, height * 0.5f);
            }
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(1f, height);
            }
        }

        public void OnSlingHit(Vector2 point, Vector2 direction)
        {
            if (_broken) return;
            _hits++;
            CameraRig.Instance?.Shake(0.04f, 0.1f);
            if (_hits < hitsToBreak)
            {
                Sfx.Play(SfxId.Hit);
                return;
            }
            _broken = true;
            Sfx.Play(SfxId.Break);
            CameraRig.Instance?.Shake(0.1f, 0.25f);
            var b = GetComponent<Collider2D>().bounds;
            Vector2 push = new Vector2(Mathf.Sign(direction.x == 0f ? 1f : direction.x) * 2.5f, 0f);
            for (float y = b.min.y + 0.5f; y < b.max.y; y += 1f)
            {
                var p = new Vector2(b.center.x, y);
                if (Fx.Chunks(p, 4, push)) Fx.Burst(p, new Color(0.62f, 0.56f, 0.5f, 0.8f), 6, 2.5f, 0.6f, 4f); // dust
                else Fx.Burst(p, new Color(0.55f, 0.52f, 0.5f), 10, 4f, 0.7f, 9f);
            }
            gameObject.SetActive(false);
        }
    }
}
