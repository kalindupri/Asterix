using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Gameplay;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>
    /// A plank bridge hinged at this transform (place it on the ground edge). Stands upright
    /// until powered (usually by cutting its <see cref="Rope"/>), then falls flat across the gap.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Drawbridge : SignalReceiver
    {
        [Range(2, 10)] public int length = 6;
        [Tooltip("Bridge spans to the left of the hinge.")]
        public bool spanLeft = true;
        public float fallTime = 0.7f;

        const float Thickness = 0.4f;

        Rigidbody2D _rb;
        float _t = -1f;
        bool _down;

        float RaisedAngle => 90f;
        float LoweredAngle => spanLeft ? 180f : 0f;
        public bool IsDown => _down;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            float offsetY = spanLeft ? Thickness * 0.5f : -Thickness * 0.5f;

            var plank = new GameObject("Plank");
            plank.transform.SetParent(transform, false);
            plank.transform.localPosition = new Vector3(length * 0.5f, offsetY, 0f);
            var sr = plank.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLibrary.Get("bridge_plank");
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(length, Thickness);
            sr.sortingOrder = 2;

            // Rounded edges (same outer size) so Brenna rolls onto the flush plank instead of snagging its corner.
            const float radius = 0.08f;
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(length - radius * 2f, Thickness - radius * 2f);
            box.edgeRadius = radius;
            box.offset = new Vector2(length * 0.5f, offsetY);
            gameObject.AddComponent<SolidGround>();

            _rb.rotation = RaisedAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, RaisedAngle);
        }

        protected override void OnPoweredChanged(bool powered)
        {
            if (powered && _t < 0f) _t = 0f;
        }

        void FixedUpdate()
        {
            if (_t < 0f || _down) return;
            _t += Time.fixedDeltaTime / fallTime;
            float eased = Mathf.Clamp01(_t * _t); // accelerates like a falling plank
            _rb.MoveRotation(Mathf.Lerp(RaisedAngle, LoweredAngle, eased));
            if (_t >= 1f)
            {
                _down = true;
                Sfx.Play(SfxId.Thud);
                CameraRig.Instance?.Shake(0.08f, 0.25f);
                Vector2 far = (Vector2)transform.position + new Vector2(spanLeft ? -length : length, 0f);
                Fx.Dust(far);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.8f, 0.6f, 0.3f);
            Vector3 p = transform.position;
            Gizmos.DrawLine(p, p + new Vector3(spanLeft ? -length : length, 0f, 0f));
        }
    }
}
