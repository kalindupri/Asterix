using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Puzzles;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Powers
{
    /// <summary>A thrown river stone. Triggers <see cref="IHittable"/> things, breaks on solid ground.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SlingStone : MonoBehaviour
    {
        const float Lifetime = 3f;

        Rigidbody2D _rb;
        float _age;
        bool _spent;

        public static SlingStone Spawn(Vector2 position, Vector2 velocity, float gravityScale, Collider2D ignore)
        {
            var go = new GameObject("SlingStone");
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLibrary.Get("fx_stone");
            sr.sortingOrder = 12;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = gravityScale;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.12f;
            if (ignore != null) Physics2D.IgnoreCollision(col, ignore, true);
            rb.linearVelocity = velocity;
            return go.AddComponent<SlingStone>();
        }

        void Awake() => _rb = GetComponent<Rigidbody2D>();

        void Update()
        {
            _age += Time.deltaTime;
            if (_age > Lifetime) Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D c) => Hit(c.collider, c.GetContact(0).point, true);

        void OnTriggerEnter2D(Collider2D other) => Hit(other, transform.position, false);

        void Hit(Collider2D col, Vector2 point, bool solid)
        {
            if (_spent) return;
            var hittable = col.GetComponentInParent<IHittable>();
            if (hittable == null && !solid) return; // pass through coins, zones, water...
            _spent = true;
            Vector2 dir = _rb.linearVelocity.sqrMagnitude > 0.01f ? _rb.linearVelocity.normalized : Vector2.right;
            if (hittable != null) hittable.OnSlingHit(point, dir);
            else Sfx.Play(SfxId.Hit, 0.6f);
            Fx.Burst(point, new Color(0.75f, 0.72f, 0.66f), 5, 2f, 0.3f);
            Destroy(gameObject);
        }
    }
}
