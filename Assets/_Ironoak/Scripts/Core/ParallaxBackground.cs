using System.Collections.Generic;
using Ironoak.Art;
using UnityEngine;

namespace Ironoak.Core
{
    /// <summary>
    /// Endless multi-layer parallax driven by a <see cref="ParallaxSet"/>. Each seamless strip is
    /// laid out three times side by side and rearranged as the camera moves. Supports any number
    /// of layers, including foreground layers drawn in front of the player.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ParallaxBackground : MonoBehaviour
    {
        class Layer
        {
            public ParallaxLayerDef Def;
            public Transform[] Copies;
            public float Width, Height;
            public Vector2 PivotOffset; // from the sprite's bottom-left corner to its pivot, in world units
            public float SpriteW, SpriteH;
        }

        public static ParallaxBackground Instance { get; private set; }

        Camera _cam;
        Rect _bounds = new Rect(0f, 0f, 100f, 20f);
        string _setName;
        readonly List<Layer> _layers = new List<Layer>();
        readonly Dictionary<string, ParallaxSet> _cache = new Dictionary<string, ParallaxSet>();

        public static ParallaxBackground Create(Transform parent, Camera cam)
        {
            var go = new GameObject("Parallax");
            go.transform.SetParent(parent, false);
            var p = go.AddComponent<ParallaxBackground>();
            p._cam = cam;
            Instance = p;
            p.Use("Forest");
            return p;
        }

        /// <summary>The level area; the vertical parallax is measured from its bottom.</summary>
        public void SetBounds(Rect bounds) => _bounds = bounds;

        /// <summary>Switches to another parallax set (no-op if it's already shown).</summary>
        public void Use(string setName)
        {
            if (string.IsNullOrEmpty(setName)) setName = "Forest";
            if (setName == _setName && _layers.Count > 0) return;
            _setName = setName;
            if (!_cache.TryGetValue(setName, out var set) || set == null) _cache[setName] = set = ParallaxSet.Load(setName);
            Build(set);
        }

        void Build(ParallaxSet set)
        {
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            _layers.Clear();
            if (_cam != null) _cam.backgroundColor = set.clearColor;
            foreach (var def in set.layers) AddLayer(def);
        }

        static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (path.StartsWith("Painted/")) return Resources.Load<Sprite>("Art/" + path);
            return SpriteLibrary.Get(path);
        }

        void AddLayer(ParallaxLayerDef def)
        {
            var sprite = LoadSprite(def.sprite);
            if (sprite == null) return;
            float scale = def.scale <= 0f ? 1f : def.scale;
            var b = sprite.bounds;
            var layer = new Layer
            {
                Def = def,
                SpriteW = b.size.x,
                SpriteH = b.size.y,
                Width = b.size.x * scale,
                Height = b.size.y * scale,
                PivotOffset = new Vector2(-b.min.x, -b.min.y) * scale,
                Copies = new Transform[def.anchor == ParallaxAnchor.Cover ? 1 : 3]
            };
            for (int i = 0; i < layer.Copies.Length; i++)
            {
                var go = new GameObject($"{def.name}_{i}");
                go.transform.SetParent(transform, false);
                go.transform.localScale = new Vector3(scale, scale, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = def.sortingOrder;
                sr.color = def.tint;
                layer.Copies[i] = go.transform;
            }
            _layers.Add(layer);
        }

        static float Snap(float v) => IronoakConfig.PixelPerfect ? Mathf.Round(v * PixelArt.PPU) / PixelArt.PPU : v;

        void LateUpdate()
        {
            if (_cam == null) return;
            Vector3 cp = _cam.transform.position;
            float halfH = _cam.orthographicSize, halfW = halfH * _cam.aspect;
            float bottom = cp.y - halfH, top = cp.y + halfH;
            // How far the camera has climbed above its lowest possible position in this level.
            float rise = Mathf.Max(0f, cp.y - (_bounds.yMin + halfH));

            foreach (var layer in _layers)
            {
                var def = layer.Def;
                if (def.anchor == ParallaxAnchor.Cover)
                {
                    float cover = Mathf.Max(halfW * 2f / layer.SpriteW, halfH * 2f / layer.SpriteH) * 1.02f;
                    var t = layer.Copies[0];
                    t.localScale = new Vector3(cover, cover, 1f);
                    // Centre the sprite on the camera whatever its pivot.
                    var sb = t.GetComponent<SpriteRenderer>().sprite.bounds;
                    t.position = new Vector3(cp.x - sb.center.x * cover, cp.y - sb.center.y * cover, 0f);
                    continue;
                }

                // Layers whose top edge is cut off grow so they always span the view height.
                float scale = def.scale <= 0f ? 1f : def.scale;
                if (def.keepTopAboveView) scale = Mathf.Max(scale, (halfH * 2f + 0.3f) / layer.SpriteH);
                float width = layer.SpriteW * scale, height = layer.SpriteH * scale;
                Vector2 pivot = layer.PivotOffset / (def.scale <= 0f ? 1f : def.scale) * scale;

                float y0 = def.anchor == ParallaxAnchor.ViewTop
                    ? top + def.yOffset - height
                    : bottom + def.yOffset - rise * (1f - def.verticalFactor);
                if (def.keepTopAboveView) y0 = Mathf.Clamp(y0, top + 0.05f - height, bottom - 0.1f);

                float baseX = cp.x * def.factor;
                float k = Mathf.Floor((cp.x - halfW - baseX) / width);
                for (int i = 0; i < 3; i++)
                {
                    float x0 = baseX + (k + i) * width;
                    var t = layer.Copies[i];
                    t.localScale = new Vector3(scale, scale, 1f);
                    t.position = new Vector3(Snap(x0 + pivot.x), Snap(y0 + pivot.y), 0f);
                }
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
