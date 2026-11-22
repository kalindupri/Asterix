using System.Collections.Generic;
using UnityEngine;

namespace Ironoak.Art
{
    /// <summary>Shared pixel-art constants and helpers.</summary>
    public static class PixelArt
    {
        /// <summary>Pixels per world unit. One tile = 16x16 px = 1 unit.</summary>
        public const int PPU = 16;

        public static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        public static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        public static readonly Vector2 BottomLeft = Vector2.zero;

        public static Color32 Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex.StartsWith("#") ? hex : "#" + hex, out var c);
            return c;
        }

        /// <summary>Builds a sprite from rows of characters (top row first) and a palette.</summary>
        public static Sprite FromRows(string[] rows, IReadOnlyDictionary<char, Color32> palette, Vector2 pivot, string name)
        {
            int h = rows.Length, w = 0;
            foreach (var r in rows) w = Mathf.Max(w, r.Length);
            var buf = new PixelBuffer(w, h);
            for (int y = 0; y < h; y++)
            {
                string row = rows[h - 1 - y];
                for (int x = 0; x < row.Length; x++)
                    if (palette.TryGetValue(row[x], out var c)) buf.Set(x, y, c);
            }
            return buf.ToSprite(pivot, name);
        }
    }

    /// <summary>A CPU-side RGBA canvas used to draw procedural sprites.</summary>
    public sealed class PixelBuffer
    {
        public readonly int Width, Height;
        readonly Color32[] _px;

        public PixelBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            _px = new Color32[width * height];
        }

        public void Set(int x, int y, Color32 c)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return;
            _px[y * Width + x] = c;
        }

        public void SetWrapX(int x, int y, Color32 c)
        {
            x %= Width;
            if (x < 0) x += Width;
            Set(x, y, c);
        }

        public Color32 Get(int x, int y)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return default;
            return _px[y * Width + x];
        }

        public Sprite ToSprite(Vector2 pivot, string name, TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = wrap,
                name = name
            };
            tex.SetPixels32(_px);
            tex.Apply(false, false);
            var sprite = Sprite.Create(tex, new Rect(0, 0, Width, Height), pivot, PixelArt.PPU, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }
    }
}
