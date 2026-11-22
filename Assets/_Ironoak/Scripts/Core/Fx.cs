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

        public static void Dust(Vector2 position) => Burst(position, new Color(0.85f, 0.75f, 0.6f, 0.9f), 6, 2f, 0.35f, 2f);
    }

    public class FxParticle : MonoBehaviour
    {
        public Vector2 velocity;
        public float life = 0.4f;
        public float gravity = 6f;

        SpriteRenderer _sr;
        float _age;

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        void Update()
        {
            float dt = Time.deltaTime;
            _age += dt;
            velocity.y -= gravity * dt;
            transform.position += (Vector3)(velocity * dt);
            if (_sr != null)
            {
                var c = _sr.color;
                c.a = Mathf.Clamp01(1f - _age / life);
                _sr.color = c;
            }
            if (_age >= life) Destroy(gameObject);
        }
    }
}
