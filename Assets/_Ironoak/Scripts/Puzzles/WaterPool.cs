using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Gameplay;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>
    /// Deep water (Brenna can't swim): falling in returns her to safe ground without damage.
    /// When powered (e.g. a root dams the stream feeding it) the pool drains away.
    /// Place the transform at the bottom-centre of the pool.
    /// </summary>
    public class WaterPool : SignalReceiver
    {
        public float width = 4f;
        public float depth = 2f;
        public bool hazard = true;
        public float drainTime = 2.2f;
        [Tooltip("Optional: faded in as the water drains (e.g. silt and reeds piled against a dam).")]
        public SpriteRenderer revealOnDrain;

        SpriteRenderer _sr, _surface;
        BoxCollider2D _box;
        float _level = 1f;
        bool _draining;

        public bool Drained => _level <= 0f;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _box = GetComponent<BoxCollider2D>();
            var foam = SpriteLibrary.GetPaintedOrNull("Props", "water_surface");
            if (foam != null && _sr != null && SpriteLibrary.IsPainted(_sr.sprite))
            {
                _sr.color = Color.white; // the painted water already carries its transparency
                _surface = new GameObject("Surface").AddComponent<SpriteRenderer>();
                _surface.transform.SetParent(transform, false);
                _surface.sprite = foam;
                _surface.drawMode = SpriteDrawMode.Tiled;
                _surface.sortingOrder = _sr.sortingOrder + 1;
            }
            if (revealOnDrain != null) SetAlpha(revealOnDrain, 0f);
            Apply();
        }

        static void SetAlpha(SpriteRenderer r, float a)
        {
            var c = r.color;
            c.a = a;
            r.color = c;
        }

        protected override void OnPoweredChanged(bool powered)
        {
            if (!powered || _draining) return;
            _draining = true;
            Sfx.Play(SfxId.Splash);
        }

        void Update()
        {
            if (!_draining || _level <= 0f) return;
            _level = Mathf.Max(0f, _level - Time.deltaTime / drainTime);
            Apply();
            if (_level <= 0f) gameObject.SetActive(false);
        }

        void Apply()
        {
            float d = depth * _level;
            if (_sr != null)
            {
                _sr.drawMode = SpriteDrawMode.Tiled;
                _sr.size = new Vector2(width, Mathf.Max(0.01f, d));
            }
            if (_surface != null)
            {
                // Foam crest sits a hair above the water line; shallow water gets a thinner strip.
                float h = _surface.sprite.bounds.size.y;
                float k = Mathf.Clamp01((d + 0.1f) / h);
                _surface.transform.localScale = new Vector3(1f, k, 1f);
                _surface.size = new Vector2(width, h);
                _surface.transform.localPosition = new Vector3(0f, d + 0.06f - h * k, 0f);
                _surface.enabled = d > 0.02f;
            }
            if (revealOnDrain != null) SetAlpha(revealOnDrain, 1f - _level);
            if (_box != null)
            {
                _box.size = new Vector2(width, Mathf.Max(0.01f, d - 0.25f));
                _box.offset = new Vector2(0f, _box.size.y * 0.5f);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!hazard || _level <= 0.1f) return;
            var rb = other.attachedRigidbody;
            if (rb == null) return;
            var player = rb.GetComponent<PlayerController>();
            if (player != null)
            {
                Sfx.Play(SfxId.Splash);
                Fx.Burst(other.transform.position, new Color(0.6f, 0.8f, 1f), 10, 4f, 0.5f);
                player.Health.ReturnToSafeGround();
                return;
            }
            var crate = rb.GetComponent<Crate>();
            if (crate != null) crate.Respawn();
            var soldier = rb.GetComponent<RomanSoldier>();
            if (soldier != null) Destroy(soldier.gameObject);
        }
    }
}
