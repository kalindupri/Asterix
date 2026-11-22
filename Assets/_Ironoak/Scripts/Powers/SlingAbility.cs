using Ironoak.Art;
using Ironoak.Controls;
using Ironoak.Core;
using Ironoak.Gameplay;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Powers
{
    /// <summary>
    /// Brenna's sling. Tap Fire for a quick throw forward; hold Fire to plant your feet and
    /// aim (mouse, right stick, or Up/Down), with a dotted arc preview. Release to throw.
    /// </summary>
    public class SlingAbility : MonoBehaviour
    {
        public float stoneSpeed = 16f;
        public float stoneGravity = 1.3f;
        public float cooldown = 0.25f;
        public float tapThreshold = 0.15f;
        public float defaultAngle = 20f;

        const int PreviewDots = 16;

        PlayerController _pc;
        PlayerPowers _powers;
        bool _aiming;
        float _aimStart, _cooldown, _keyAngle;
        Vector2 _dir = Vector2.right;
        SpriteRenderer[] _dots;

        public bool Aiming => _aiming;
        public Vector2 AimDirection => _dir;
        public event System.Action Thrown;

        void Awake()
        {
            _pc = GetComponent<PlayerController>();
            _powers = GetComponent<PlayerPowers>();
            _dots = new SpriteRenderer[PreviewDots];
            var dotSprite = SpriteLibrary.Get("fx_dot");
            for (int i = 0; i < PreviewDots; i++)
            {
                var go = new GameObject("AimDot");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = dotSprite;
                sr.sortingOrder = 40;
                sr.enabled = false;
                _dots[i] = sr;
            }
        }

        Vector2 Hand => (Vector2)transform.position + new Vector2(0.3f * _pc.Facing, 0.95f);
        Vector2 Gravity => Physics2D.gravity * stoneGravity;

        void Update()
        {
            _cooldown -= Time.deltaTime;
            if (!_pc.InputEnabled || _powers == null || !_powers.Has(Power.Sling) || Time.timeScale == 0f)
            {
                if (_aiming && Time.timeScale != 0f) Cancel();
                return;
            }

            if (!_aiming && GameInput.FirePressed && _cooldown <= 0f)
            {
                _aiming = true;
                _aimStart = Time.time;
                _keyAngle = defaultAngle;
            }

            if (_aiming)
            {
                _dir = ComputeAim();
                if (Mathf.Abs(_dir.x) > 0.05f) _pc.FaceTowards(_dir.x);
                bool held = Time.time - _aimStart >= tapThreshold;
                _pc.AimLock = held;
                ShowPreview(held);
                if (!GameInput.FireHeld) Throw();
            }
        }

        Vector2 ComputeAim()
        {
            if (GameInput.AimStick.sqrMagnitude > 0.25f) return GameInput.AimStick.normalized;
            if (GameInput.MouseActive && !GameInput.UsingGamepad && PixelDisplay.Instance != null)
            {
                Vector2 d = PixelDisplay.Instance.ScreenToWorld(GameInput.MouseScreen) - Hand;
                if (d.sqrMagnitude > 0.04f) return d.normalized;
            }
            _keyAngle = Mathf.Clamp(_keyAngle + GameInput.Vertical * 110f * Time.deltaTime, -60f, 88f);
            float a = _keyAngle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a) * _pc.Facing, Mathf.Sin(a));
        }

        void ShowPreview(bool visible)
        {
            Vector2 p = Hand, v = _dir * stoneSpeed, g = Gravity;
            const float step = 0.045f;
            bool blocked = false;
            for (int i = 0; i < _dots.Length; i++)
            {
                Vector2 next = p;
                for (int s = 0; s < 2; s++)
                {
                    Vector2 prev = next;
                    v += g * step;
                    next += v * step;
                    if (!blocked && Physics2D.Linecast(prev, next).collider is Collider2D c && !c.isTrigger && c.attachedRigidbody != _pc.Body)
                        blocked = true;
                }
                p = next;
                var dot = _dots[i];
                dot.enabled = visible && !blocked;
                dot.transform.position = p;
                dot.color = new Color(1f, 0.95f, 0.75f, 1f - i / (float)_dots.Length * 0.8f);
            }
        }

        void Throw()
        {
            SlingStone.Spawn(Hand, _dir * stoneSpeed, stoneGravity, _pc.Collider);
            Sfx.Play(SfxId.Sling);
            _cooldown = cooldown;
            Thrown?.Invoke();
            Cancel();
        }

        void Cancel()
        {
            _aiming = false;
            _pc.AimLock = false;
            foreach (var d in _dots) d.enabled = false;
        }
    }
}
