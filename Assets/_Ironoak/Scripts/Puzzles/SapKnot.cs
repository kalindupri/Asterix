using System.Collections.Generic;
using Ironoak.Art;
using Ironoak.Controls;
using Ironoak.Core;
using Ironoak.Gameplay;
using Ironoak.Powers;
using Ironoak.Sound;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Ironoak.Puzzles
{
    /// <summary>
    /// A glowing knot in the ground. With Rootgrow, Brenna raises a solid root column from it
    /// (lifting the player, crates and enemies) or lowers it again. Sends a signal while fully
    /// raised, so knots can also act as dams, wedges and switches.
    ///
    /// Place the knot on the ground surface. The ground below must be at least
    /// <see cref="maxHeight"/> tiles deep (or end off-screen) so the retracted column stays hidden.
    /// </summary>
    public class SapKnot : SignalSender
    {
        static readonly List<SapKnot> All = new List<SapKnot>();

        [Range(1, 7)] public int maxHeight = 3;
        public float growSpeed = 9f;
        public bool startRaised;

        Rigidbody2D _column;
        SpriteRenderer _knotSr, _segmentSr, _capSr, _promptSr, _glowSr;
        Light2D _light;
        float _height, _target, _pulse;
        bool _highlighted, _settled = true;
        Sprite _promptKey, _promptPad;

        public bool Raised => _target > 0f && _height >= maxHeight - 0.01f;
        public float Height => _height;

        public static SapKnot FindNearest(Vector2 point, float range)
        {
            SapKnot best = null;
            float bestD = range;
            foreach (var k in All)
            {
                float d = Vector2.Distance(point, k.transform.position);
                if (d <= bestD) { bestD = d; best = k; }
            }
            return best;
        }

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        void Awake()
        {
            _knotSr = GetComponent<SpriteRenderer>();

            var col = new GameObject("RootColumn");
            col.transform.SetParent(transform, false);
            _column = col.AddComponent<Rigidbody2D>();
            _column.bodyType = RigidbodyType2D.Kinematic;
            _column.interpolation = RigidbodyInterpolation2D.Interpolate;
            var box = col.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, maxHeight);
            box.offset = new Vector2(0f, maxHeight * 0.5f);
            col.AddComponent<SolidGround>();

            _segmentSr = col.AddComponent<SpriteRenderer>();
            _segmentSr.sprite = SpriteLibrary.Get("root_segment");
            _segmentSr.drawMode = SpriteDrawMode.Tiled;
            _segmentSr.size = new Vector2(1f, maxHeight);
            _segmentSr.sortingOrder = -1; // ground tiles (order 0) hide the retracted part

            var cap = new GameObject("RootCap");
            cap.transform.SetParent(col.transform, false);
            cap.transform.localPosition = new Vector3(0f, maxHeight - 1f, 0f);
            _capSr = cap.AddComponent<SpriteRenderer>();
            _capSr.sprite = SpriteLibrary.Get("root_cap");
            _capSr.sortingOrder = -1;
            if (SpriteLibrary.IsPainted(_capSr.sprite))
            {
                // The painted crown is taller than a tile: seat its mossy top on the column's top.
                float h = _capSr.sprite.bounds.size.y;
                cap.transform.localPosition = new Vector3(0f, maxHeight - h * 0.82f, 0f);
                _segmentSr.size = new Vector2(1f, maxHeight - h * 0.5f);
            }

            // Painted knots get a soft amber halo and a 2D light while the Torc is awake.
            var glowSprite = SpriteLibrary.GetPaintedOrNull("Props", "sap_knot_glow");
            if (glowSprite != null && SpriteLibrary.IsPainted(_knotSr.sprite))
            {
                var glow = new GameObject("Glow");
                glow.transform.SetParent(transform, false);
                glow.transform.localPosition = new Vector3(0f, 0.38f, 0f);
                _glowSr = glow.AddComponent<SpriteRenderer>();
                _glowSr.sprite = glowSprite;
                _glowSr.sortingOrder = 1;
                _glowSr.enabled = false;
                if (SceneLighting.Active)
                {
                    _light = glow.AddComponent<Light2D>();
                    _light.lightType = Light2D.LightType.Point;
                    _light.color = new Color(1f, 0.72f, 0.32f);
                    _light.pointLightInnerRadius = 0.2f;
                    _light.pointLightOuterRadius = 2.6f;
                    _light.falloffIntensity = 0.6f;
                    _light.intensity = 0f;
                }
            }

            var prompt = new GameObject("Prompt");
            prompt.transform.SetParent(transform, false);
            _promptSr = prompt.AddComponent<SpriteRenderer>();
            _promptSr.sortingOrder = 45;
            _promptSr.enabled = false;
            _promptKey = PixelFont.Render("E", new Color32(255, 240, 200, 255), new Color32(255, 200, 90, 255), true, new Color32(29, 20, 36, 255));
            _promptPad = PixelFont.Render("Y", new Color32(255, 240, 200, 255), new Color32(255, 200, 90, 255), true, new Color32(29, 20, 36, 255));

            _target = _height = startRaised ? maxHeight : 0f;
            PlaceColumn(true);
        }

        void OnDestroy()
        {
            if (_promptKey != null) { Destroy(_promptKey.texture); Destroy(_promptKey); }
            if (_promptPad != null) { Destroy(_promptPad.texture); Destroy(_promptPad); }
        }

        public void Toggle()
        {
            _target = _target > 0f ? 0f : maxHeight;
            _settled = false;
            if (_target > 0f) Sfx.Play(SfxId.RootGrow);
            else
            {
                Sfx.Play(SfxId.RootRetract);
                Emit(false);
            }
            Fx.Dust(transform.position);
            CameraRig.Instance?.Shake(0.04f, 0.15f);
        }

        /// <summary>Instantly returns the knot to its starting state (used by chase/room resets).</summary>
        public void ResetKnot()
        {
            _target = _height = startRaised ? maxHeight : 0f;
            _settled = true;
            PlaceColumn(true);
            Emit(Raised);
        }

        public void SetHighlighted(bool on) => _highlighted = on;

        void FixedUpdate()
        {
            if (_settled) return;
            _height = Mathf.MoveTowards(_height, _target, growSpeed * Time.fixedDeltaTime);
            PlaceColumn(false);
            if (Mathf.Approximately(_height, _target))
            {
                _settled = true;
                if (_target > 0f)
                {
                    Emit(true);
                    Sfx.Play(SfxId.Thud, 0.5f);
                }
            }
        }

        void PlaceColumn(bool teleport)
        {
            Vector2 pos = (Vector2)transform.position + new Vector2(0f, _height - maxHeight);
            if (teleport || !Application.isPlaying)
            {
                _column.position = pos;
                _column.transform.position = pos;
            }
            else _column.MovePosition(pos);
        }

        void Update()
        {
            bool awake = PlayerPowers.Local != null && PlayerPowers.Local.Has(Power.Rootgrow);
            _pulse += Time.deltaTime * (_highlighted ? 6f : 2.5f);
            float glow = awake ? 0.75f + 0.25f * Mathf.Sin(_pulse) : 0.45f;
            if (_glowSr != null) // painted knot: already coloured, so only a gentle warm tint (no blow-out)
                _knotSr.color = awake ? new Color(1f, 0.9f + 0.06f * glow, 0.72f + 0.1f * glow) : new Color(0.62f, 0.62f, 0.6f);
            else
                _knotSr.color = awake ? new Color(glow + 0.25f, glow + 0.2f, glow * 0.6f) : new Color(glow, glow, glow);
            if (_glowSr != null)
            {
                _glowSr.enabled = awake;
                float a = (_highlighted ? 0.95f : 0.7f) * (0.75f + 0.25f * Mathf.Sin(_pulse));
                _glowSr.color = new Color(1f, 1f, 1f, a);
                float sc = _highlighted ? 1.15f : 1f;
                _glowSr.transform.localScale = new Vector3(sc, sc, 1f);
                if (_light != null) _light.intensity = awake ? (_highlighted ? 1.4f : 0.9f) * (0.8f + 0.2f * Mathf.Sin(_pulse)) : 0f;
            }

            _promptSr.enabled = _highlighted;
            if (_highlighted)
            {
                _promptSr.sprite = GameInput.UsingGamepad ? _promptPad : _promptKey;
                float y = _height + 1.4f + Mathf.Round(Mathf.Sin(_pulse) * 1.5f) / PixelArt.PPU;
                _promptSr.transform.localPosition = new Vector3(0f, y, 0f);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.6f, 0.9f, 0.3f, 0.6f);
            Vector3 p = transform.position;
            Gizmos.DrawWireCube(p + new Vector3(0f, maxHeight * 0.5f, 0f), new Vector3(0.9f, maxHeight, 0f));
        }
    }
}
