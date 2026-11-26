using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>
    /// A wooden portcullis. Slides up into the wall above while powered.
    /// Put solid tiles above it so the player can't jump over, and so the open gate is hidden.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Gate : SignalReceiver
    {
        [Range(1, 8)] public int height = 4;
        public float speed = 6f;

        Rigidbody2D _rb;
        Vector2 _closed;
        float _open, _targetOpen;

        public bool IsOpen => _open >= height - 0.05f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _closed = transform.position;
            Configure();
            BuildPaintedLook();
        }

        /// <summary>
        /// Painted gates: the repeating bars end in a separate spiked bottom row, so the tiled
        /// root sprite is swapped for two children (they slide with the gate).
        /// </summary>
        void BuildPaintedLook()
        {
            var sr = GetComponent<SpriteRenderer>();
            var bottom = SpriteLibrary.GetPaintedOrNull("Props", "gate_portcullis_bottom");
            if (sr == null || bottom == null || !SpriteLibrary.IsPainted(sr.sprite)) return;
            float bottomH = bottom.bounds.size.y, overlap = 0.06f;

            var bars = new GameObject("Bars").AddComponent<SpriteRenderer>();
            bars.transform.SetParent(transform, false);
            bars.transform.localPosition = new Vector3(0f, bottomH - overlap, 0f);
            bars.sprite = sr.sprite;
            bars.drawMode = SpriteDrawMode.Tiled;
            bars.size = new Vector2(1f, Mathf.Max(0.1f, height - bottomH + overlap));
            bars.sortingOrder = sr.sortingOrder;

            var foot = new GameObject("Bottom").AddComponent<SpriteRenderer>();
            foot.transform.SetParent(transform, false);
            foot.sprite = bottom;
            foot.sortingOrder = sr.sortingOrder;
            sr.enabled = false;
        }

        /// <summary>Sizes collider and sprite from <see cref="height"/>.</summary>
        public void Configure()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = new Vector2(0.9f, height);
                box.offset = new Vector2(0f, height * 0.5f);
            }
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(1f, height);
            }
        }

        void OnValidate() => Configure();

        protected override void OnPoweredChanged(bool powered)
        {
            _targetOpen = powered ? height : 0f;
            Sfx.Play(SfxId.Gate);
            CameraRig.Instance?.Shake(0.03f, 0.3f);
        }

        void FixedUpdate()
        {
            if (Mathf.Approximately(_open, _targetOpen)) return;
            _open = Mathf.MoveTowards(_open, _targetOpen, speed * Time.fixedDeltaTime);
            _rb.MovePosition(_closed + Vector2.up * _open);
        }
    }
}
