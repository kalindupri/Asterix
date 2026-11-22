using System;
using UnityEngine;

namespace Ironoak.Art
{
    /// <summary>
    /// Anti-aliased "painted" drawing for placeholder art: shapes are signed-distance functions in
    /// pixel space, filled with soft top-left lighting and a dark outline. Output is bilinear-filtered.
    /// </summary>
    public sealed class PaintCanvas
    {
        public readonly int Width, Height;
        readonly Color[] _px;

        public static readonly Color Ink = new Color(0.17f, 0.10f, 0.13f, 1f);

        public PaintCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _px = new Color[width * height];
        }

        // ---------- signed distance functions (pixels; negative = inside) ----------

        public static Func<float, float, float> Circle(float cx, float cy, float r) =>
            (x, y) => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r;

        public static Func<float, float, float> Ellipse(float cx, float cy, float rx, float ry) =>
            (x, y) =>
            {
                float nx = (x - cx) / rx, ny = (y - cy) / ry;
                return (Mathf.Sqrt(nx * nx + ny * ny) - 1f) * Mathf.Min(rx, ry);
            };

        public static Func<float, float, float> Capsule(float ax, float ay, float bx, float by, float r) =>
            Capsule(ax, ay, bx, by, r, r);

        /// <summary>Capsule whose radius tapers from ra (at a) to rb (at b).</summary>
        public static Func<float, float, float> Capsule(float ax, float ay, float bx, float by, float ra, float rb) =>
            (x, y) =>
            {
                float px = x - ax, py = y - ay, dx = bx - ax, dy = by - ay;
                float h = Mathf.Clamp01((px * dx + py * dy) / (dx * dx + dy * dy));
                float qx = px - dx * h, qy = py - dy * h;
                return Mathf.Sqrt(qx * qx + qy * qy) - Mathf.Lerp(ra, rb, h);
            };

        public static Func<float, float, float> RoundBox(float cx, float cy, float hw, float hh, float r) =>
            (x, y) =>
            {
                float qx = Mathf.Abs(x - cx) - hw + r, qy = Mathf.Abs(y - cy) - hh + r;
                float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
                return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
            };

        public static Func<float, float, float> Union(Func<float, float, float> a, Func<float, float, float> b) =>
            (x, y) => Mathf.Min(a(x, y), b(x, y));

        // ---------- painting ----------

        /// <summary>Paints a shape with a light-from-top-left gradient and an outline.</summary>
        public void Shape(Func<float, float, float> sdf, Color fill, float outline = 2f, float shade = 0.22f,
                          Func<float, float, Color> pattern = null, Color? outlineColor = null)
        {
            Color ink = outlineColor ?? Ink;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float d = sdf(px, py);
                    if (d > 1f) continue;
                    float coverage = Mathf.Clamp01(0.5f - d);

                    Color baseCol = pattern != null ? pattern(px, py) : fill;
                    // Distance toward the light (up-left) tells us if this pixel faces it.
                    float toward = d - sdf(px - 1.5f, py + 1.5f);
                    float light = Mathf.Clamp(toward / 2.1f, -1f, 1f);
                    Color c = baseCol * (1f + shade * light);
                    c.a = baseCol.a;

                    if (outline > 0f)
                    {
                        float inkMix = Mathf.Clamp01(d + outline + 0.5f);
                        c = Color.Lerp(c, ink, inkMix);
                    }
                    Blend(x, y, c, coverage * c.a);
                }
        }

        /// <summary>Unlit, unoutlined soft detail (blush, highlights, shadows).</summary>
        public void Soft(Func<float, float, float> sdf, Color color, float softness = 2f)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    float d = sdf(x + 0.5f, y + 0.5f);
                    if (d > softness) continue;
                    float a = Mathf.Clamp01(0.5f - d / softness) * color.a;
                    Blend(x, y, color, a);
                }
        }

        void Blend(int x, int y, Color c, float a)
        {
            if (a <= 0f) return;
            int i = y * Width + x;
            Color dst = _px[i];
            float outA = a + dst.a * (1f - a);
            if (outA <= 0f) return;
            Color o = (c * a + dst * dst.a * (1f - a)) / outA;
            o.a = outA;
            _px[i] = o;
        }

        public Sprite ToSprite(Vector2 pivot, float pixelsPerUnit, string name)
        {
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = name
            };
            tex.SetPixels(_px);
            tex.Apply(false, false);
            var sprite = Sprite.Create(tex, new Rect(0, 0, Width, Height), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }
    }
}
