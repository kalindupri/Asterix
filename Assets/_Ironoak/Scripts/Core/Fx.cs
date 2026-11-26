using Ironoak.Art;
using UnityEngine;

namespace Ironoak.Core
{
    /// <summary>Lightweight pixel particle bursts (dust, debris, sparkles, splashes).</summary>
    public static class Fx
    {
        public static void Burst(Vector2 position, Color color, int count = 8, float speed = 3f, float life = 0.45f, float gravity = 6f)
        {
            var sprite = SpriteLibrary.Get("fx_particle");
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("fx");
                go.transform.position = position;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = color;
                sr.sortingOrder = 30;
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float s = speed * Random.Range(0.5f, 1f);
                var p = go.AddComponent<FxParticle>();
                p.velocity = new Vector2(Mathf.Cos(angle), Mathf.Abs(Mathf.Sin(angle)) * 0.8f + 0.2f) * s;
                p.life = life * Random.Range(0.7f, 1.2f);
                p.gravity = gravity;
            }
        }

        static Sprite[] _rocks;

        /// <summary>
        /// Tumbling painted rock chunks (Batch 2 debris_rock_1..3) for breaking walls.
        /// Returns false when that art isn't available (pixel mode), so callers can fall back to Burst.
        /// </summary>
        public static bool Chunks(Vector2 position, int count, Vector2 push, float spread = 0.4f)
        {
            if (IronoakConfig.PixelPerfect) return false;
            if (_rocks == null)
            {
                var list = new System.Collections.Generic.List<Sprite>();
                for (int i = 1; i <= 3; i++)
                {
                    var s = SpriteLibrary.GetPaintedOrNull("Props", "debris_rock_" + i);
                    if (s != null) list.Add(s);
                }
                _rocks = list.ToArray();
            }
            if (_rocks.Length == 0) return false;
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("chunk");
                go.transform.position = position + Random.insideUnitCircle * spread;
                go.transform.localScale = Vector3.one * Random.Range(0.45f, 0.85f);
                go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _rocks[Random.Range(0, _rocks.Length)];
                sr.sortingOrder = 30;
                var p = go.AddComponent<FxParticle>();
                p.velocity = push + new Vector2(Random.Range(-2.5f, 2.5f), Random.Range(2f, 6f));
                p.life = Random.Range(0.7f, 1.1f);
                p.gravity = 18f;
                p.spin = Random.Range(-540f, 540f);
                p.fadeLate = true;
            }
            return true;
        }

        public static void Dust(Vector2 position) => Burst(position, new Color(0.85f, 0.75f, 0.6f, 0.9f), 6, 2f, 0.35f, 2f);
    }

    public class FxParticle : MonoBehaviour
    {
        public Vector2 velocity;
        public float life = 0.4f;
        public float gravity = 6f;
        public float spin;
        [Tooltip("Stay opaque for most of the life, then fade (solid chunks).")]
        public bool fadeLate;

        SpriteRenderer _sr;
        float _age;

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        void Update()
        {
            float dt = Time.deltaTime;
            _age += dt;
            velocity.y -= gravity * dt;
            transform.position += (Vector3)(velocity * dt);
            if (spin != 0f) transform.Rotate(0f, 0f, spin * dt);
            if (_sr != null)
            {
                var c = _sr.color;
                float k = _age / life;
                c.a = Mathf.Clamp01(fadeLate ? (1f - k) * 4f : 1f - k);
                _sr.color = c;
            }
            if (_age >= life) Destroy(gameObject);
        }
    }
}
