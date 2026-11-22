using System.Collections.Generic;
using UnityEngine;

namespace Ironoak.Art
{
    /// <summary>
    /// Built-in 5x7 bitmap font rendered straight to sprites, so all UI text is crisp
    /// pixel art at the game's native 480x270 resolution and needs no font assets.
    /// </summary>
    public static class PixelFont
    {
        public const int GlyphWidth = 5, GlyphHeight = 7, LineGap = 3;

        static readonly Dictionary<char, string> Source = new Dictionary<char, string>
        {
            { 'A', "01110|10001|10001|11111|10001|10001|10001" },
            { 'B', "11110|10001|10001|11110|10001|10001|11110" },
            { 'C', "01110|10001|10000|10000|10000|10001|01110" },
            { 'D', "11110|10001|10001|10001|10001|10001|11110" },
            { 'E', "11111|10000|10000|11110|10000|10000|11111" },
            { 'F', "11111|10000|10000|11110|10000|10000|10000" },
            { 'G', "01110|10001|10000|10111|10001|10001|01111" },
            { 'H', "10001|10001|10001|11111|10001|10001|10001" },
            { 'I', "01110|00100|00100|00100|00100|00100|01110" },
            { 'J', "00111|00010|00010|00010|00010|10010|01100" },
            { 'K', "10001|10010|10100|11000|10100|10010|10001" },
            { 'L', "10000|10000|10000|10000|10000|10000|11111" },
            { 'M', "10001|11011|10101|10101|10001|10001|10001" },
            { 'N', "10001|10001|11001|10101|10011|10001|10001" },
            { 'O', "01110|10001|10001|10001|10001|10001|01110" },
            { 'P', "11110|10001|10001|11110|10000|10000|10000" },
            { 'Q', "01110|10001|10001|10001|10101|10010|01101" },
            { 'R', "11110|10001|10001|11110|10100|10010|10001" },
            { 'S', "01111|10000|10000|01110|00001|00001|11110" },
            { 'T', "11111|00100|00100|00100|00100|00100|00100" },
            { 'U', "10001|10001|10001|10001|10001|10001|01110" },
            { 'V', "10001|10001|10001|10001|10001|01010|00100" },
            { 'W', "10001|10001|10001|10101|10101|10101|01010" },
            { 'X', "10001|10001|01010|00100|01010|10001|10001" },
            { 'Y', "10001|10001|01010|00100|00100|00100|00100" },
            { 'Z', "11111|00001|00010|00100|01000|10000|11111" },
            { '0', "01110|10001|10011|10101|11001|10001|01110" },
            { '1', "00100|01100|00100|00100|00100|00100|01110" },
            { '2', "01110|10001|00001|00010|00100|01000|11111" },
            { '3', "11111|00010|00100|00010|00001|10001|01110" },
            { '4', "00010|00110|01010|10010|11111|00010|00010" },
            { '5', "11111|10000|11110|00001|00001|10001|01110" },
            { '6', "00110|01000|10000|11110|10001|10001|01110" },
            { '7', "11111|00001|00010|00100|01000|01000|01000" },
            { '8', "01110|10001|10001|01110|10001|10001|01110" },
            { '9', "01110|10001|10001|01111|00001|00010|01100" },
            { ':', "00000|01100|01100|00000|01100|01100|00000" },
            { '!', "00100|00100|00100|00100|00100|00000|00100" },
            { '.', "00000|00000|00000|00000|00000|01100|01100" },
            { ',', "00000|00000|00000|00000|01100|00100|01000" },
            { '-', "00000|00000|00000|11111|00000|00000|00000" },
            { '+', "00000|00100|00100|11111|00100|00100|00000" },
            { '/', "00001|00010|00010|00100|01000|01000|10000" },
            { '?', "01110|10001|00001|00010|00100|00000|00100" },
            { '>', "01000|00100|00010|00001|00010|00100|01000" },
            { '<', "00010|00100|01000|10000|01000|00100|00010" },
            { '\'', "00100|00100|01000|00000|00000|00000|00000" },
            { '(', "00010|00100|01000|01000|01000|00100|00010" },
            { ')', "01000|00100|00010|00010|00010|00100|01000" },
            { '*', "00000|00100|10101|01110|10101|00100|00000" },
        };

        static readonly Dictionary<char, bool[]> Glyphs = new Dictionary<char, bool[]>();

        static bool[] Glyph(char c)
        {
            if (Glyphs.TryGetValue(c, out var g)) return g;
            g = new bool[GlyphWidth * GlyphHeight];
            if (Source.TryGetValue(c, out var src))
            {
                string bits = src.Replace("|", "");
                for (int i = 0; i < g.Length && i < bits.Length; i++) g[i] = bits[i] == '1';
            }
            Glyphs[c] = g;
            return g;
        }

        public static Vector2Int Measure(string text, bool outline)
        {
            var lines = (string.IsNullOrEmpty(text) ? " " : text).Split('\n');
            int maxLen = 1;
            foreach (var l in lines) maxLen = Mathf.Max(maxLen, l.Length);
            int pad = outline ? 1 : 0;
            int w = maxLen * (GlyphWidth + 1) - 1 + pad * 2;
            int h = lines.Length * (GlyphHeight + LineGap) - LineGap + pad * 2;
            return new Vector2Int(w, h);
        }

        /// <summary>Renders text (multi-line, centred per line) with a vertical colour gradient and optional outline.</summary>
        public static Sprite Render(string text, Color32 top, Color32 bottom, bool outline, Color32 outlineColor)
        {
            text = string.IsNullOrEmpty(text) ? " " : text.ToUpperInvariant();
            var lines = text.Split('\n');
            var size = Measure(text, outline);
            int w = size.x, h = size.y, pad = outline ? 1 : 0;

            // 0 = empty, otherwise (glyph row + 1) so we can apply a per-glyph gradient
            var mask = new sbyte[w, h];
            for (int li = 0; li < lines.Length; li++)
            {
                string line = lines[li];
                int lineW = Mathf.Max(0, line.Length * (GlyphWidth + 1) - 1);
                int ox = pad + (w - pad * 2 - lineW) / 2;
                int topY = h - pad - li * (GlyphHeight + LineGap);
                for (int ci = 0; ci < line.Length; ci++)
                {
                    var g = Glyph(line[ci]);
                    for (int gy = 0; gy < GlyphHeight; gy++)
                        for (int gx = 0; gx < GlyphWidth; gx++)
                            if (g[gy * GlyphWidth + gx])
                                mask[ox + ci * (GlyphWidth + 1) + gx, topY - 1 - gy] = (sbyte)(gy + 1);
                }
            }

            var buf = new PixelBuffer(w, h);
            if (outline)
            {
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        if (mask[x, y] != 0) continue;
                        bool near = false;
                        for (int dy = -1; dy <= 1 && !near; dy++)
                            for (int dx = -1; dx <= 1 && !near; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                near = nx >= 0 && ny >= 0 && nx < w && ny < h && mask[nx, ny] != 0;
                            }
                        if (near) buf.Set(x, y, outlineColor);
                    }
            }

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int m = mask[x, y];
                    if (m == 0) continue;
                    float t = (m - 1) / (float)(GlyphHeight - 1);
                    buf.Set(x, y, Color32.Lerp(top, bottom, t));
                }

            return buf.ToSprite(PixelArt.Center, "Text");
        }
    }
}
