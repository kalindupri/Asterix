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

        SpriteRenderer _sr;
        BoxCollider2D _box;
        float _level = 1f;
        bool _draining;

        public bool Drained => _level <= 0f;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _box = GetComponent<BoxCollider2D>();
            Apply();
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
