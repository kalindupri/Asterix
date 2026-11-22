using System.Collections.Generic;
using UnityEngine;

namespace Ironoak.Art
{
    /// <summary>
    /// Placeholder pixel art generated in code. Every sprite here is meant to be
    /// replaced by final art in Resources/Art (see SpriteLibrary / docs/ART_SPEC.md).
    /// All characters are original Ironoak designs.
    /// </summary>
    internal static class ProceduralArt
    {
        static Color32 C(string h) => PixelArt.Hex(h);

        static readonly Dictionary<char, Color32> Pal = new Dictionary<char, Color32>
        {
            { 'K', C("2b1d2e") }, // outline
            { 'H', C("c8452c") }, // hair
            { 'h', C("8a2a1f") }, // hair shadow / braid
            { 'S', C("f2c09a") }, // skin
            { 's', C("d3916b") }, // skin shadow
            { 'G', C("3f8f4a") }, // tunic
            { 'g', C("2a5e34") }, // tunic shadow
            { 'B', C("6b4226") }, // leather
            { 'Y', C("e8b83a") }, // gold
            { 'P', C("7a5a3a") }, // trousers
            { 'W', C("ffffff") }, // eye white
            { 'E', C("1a1020") }, // pupil
            { 'R', C("b0302a") }, // roman red
            { 'r', C("7a1f1c") }, // roman red shadow
            { 'M', C("aeb4bf") }, // metal
            { 'm', C("6d7380") }, // metal shadow
            { 'C', C("d8453a") }, // crest
        };

        // ---------- Brenna (player), 16x16, facing right ----------
        static readonly string[] PlayerBody =
        {
            ".....KKKKK......",
            "....KHHHHHK.....",
            "...KHHHHHHHK....",
            "...KHHHSSSSK....",
            "..KhHHSSSWEK....",
            "..KhHHSSSSSK....",
            "..KhHKsSSSsK....",
            "..Kh.KKSSSK.....",
            "..KhKGGGGGGK....",
            "...KGGGGGGGGK...",
            "..KSGGGGGGGSK...",
            "..KSBBYBBBBSK...",
            "...KgGGGGGgK....",
        };
        static readonly string[] PlayerLegsIdle =
        {
            "...KPPK.KPPK....",
            "...KPPK.KPPK....",
            "...KBBBKKBBBK...",
        };
        static readonly string[] PlayerLegsRun1 =
        {
            "..KPPK..KPPK....",
            ".KPPK....KPPK...",
            ".KBBK.....KBBBK.",
        };
        static readonly string[] PlayerLegsRun2 =
        {
            "...KPPPPK.......",
            "...KPPKBBK......",
            "...KBBBK........",
        };
        static readonly string[] PlayerLegsJump =
        {
            "...KPPK.KPPK....",
            "...KBBBKKPPK....",
            ".........KBBBK..",
        };

        // ---------- Roman legionary (enemy), 16x16, facing right ----------
        static readonly string[] RomanBody =
        {
            "......CCCC......",
            ".....KCCCCK.....",
            "....KMMMMMMK....",
            "...KMMMMMMMMK...",
            "...KmMSSSSMmK...",
            "...KmSSSWESmK...",
            "...KmSSSSSSmK...",
            "....KKsSSsKK....",
            "...KMMMMMMMMK...",
            "..KSMmMmMmMMSK..",
            "..KSRRRRRRRRSK..",
            "...KBBBYBBBBK...",
            "...KRrRrRrRrK...",
        };
        static readonly string[] RomanLegs1 =
        {
            "....KSK..KSK....",
            "....KSK..KSK....",
            "...KBBK..KBBK...",
        };
        static readonly string[] RomanLegs2 =
        {
            "....KSK.KSK.....",
            "...KSK...KSK....",
            "..KBBK....KBBK..",
        };

        static readonly string[] Heart =
        {
            "..KK.KK..",
            ".KRRKRRK.",
            "KRWRRRRRK",
            "KRRRRRRRK",
            ".KRRRRRK.",
            "..KRRRK..",
            "...KRK...",
            "....K....",
        };

        static string[] Join(string[] a, string[] b)
        {
            var r = new string[a.Length + b.Length];
            a.CopyTo(r, 0);
            b.CopyTo(r, a.Length);
            return r;
        }

        public static Sprite Create(string name)
        {
            switch (name)
            {
                case "player_idle": return PixelArt.FromRows(Join(PlayerBody, PlayerLegsIdle), Pal, PixelArt.BottomCenter, name);
                case "player_run1": return PixelArt.FromRows(Join(PlayerBody, PlayerLegsRun1), Pal, PixelArt.BottomCenter, name);
                case "player_run2": return PixelArt.FromRows(Join(PlayerBody, PlayerLegsRun2), Pal, PixelArt.BottomCenter, name);
                case "player_jump": return PixelArt.FromRows(Join(PlayerBody, PlayerLegsJump), Pal, PixelArt.BottomCenter, name);

                case "roman_walk1": return PixelArt.FromRows(Join(RomanBody, RomanLegs1), Pal, PixelArt.BottomCenter, name);
                case "roman_walk2": return PixelArt.FromRows(Join(RomanBody, RomanLegs2), Pal, PixelArt.BottomCenter, name);
                case "roman_squash":
                {
                    var full = Join(RomanBody, RomanLegs1);
                    var rows = new string[8];
                    for (int i = 0; i < 8; i++) rows[i] = full[i * 2 + 1];
                    return PixelArt.FromRows(rows, Pal, PixelArt.BottomCenter, name);
                }

                case "tile_grass": return Dirt(true, name);
                case "tile_dirt": return Dirt(false, name);
                case "tile_stone": return Stone(name);
                case "tile_plank": return Plank(name);

                case "coin_0": return Coin(0, name);
                case "coin_1": return Coin(1, name);
                case "coin_2": return Coin(2, name);
                case "coin_3": return Coin(3, name);

                case "spikes": return Spikes(name);
                case "torch_off": return Torch(-1, name);
                case "torch_on1": return Torch(0, name);
                case "torch_on2": return Torch(1, name);
                case "goal_banner": return Banner(name);

                case "heart_full": return PixelArt.FromRows(Heart, HeartPalette(false), PixelArt.Center, name);
                case "heart_empty": return PixelArt.FromRows(Heart, HeartPalette(true), PixelArt.Center, name);

                case "bg_sky": return Sky(name);
                case "bg_clouds": return Clouds(name);
                case "bg_mountains": return Mountains(name);
                case "bg_hills": return Hills(name);
                case "bg_forest": return Forest(name);

                default:
                    Debug.LogWarning($"[Ironoak] No sprite named '{name}', using magenta placeholder.");
                    var buf = new PixelBuffer(16, 16);
                    for (int y = 0; y < 16; y++)
                        for (int x = 0; x < 16; x++)
                            buf.Set(x, y, ((x / 4 + y / 4) & 1) == 0 ? C("ff00ff") : C("000000"));
                    return buf.ToSprite(PixelArt.Center, name);
            }
        }

        // ---------- helpers ----------

        static uint Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                return h ^ (h >> 16);
            }
        }

        static float Rand(int x, int y, int seed) => (Hash(x, y, seed) & 0xFFFF) / 65535f;

        static Dictionary<char, Color32> HeartPalette(bool empty)
        {
            return new Dictionary<char, Color32>
            {
                { 'K', C("1d1424") },
                { 'R', empty ? C("4a3a4a") : C("e04848") },
                { 'W', empty ? C("5e4c5e") : C("ffd0d0") },
            };
        }

        // ---------- tiles ----------

        static Sprite Dirt(bool grass, string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 dirt = C("8a5a34"), dark = C("6b4226"), light = C("a8744a"), pebble = C("9a8f80");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float r = Rand(x, y, 7);
                    Color32 c = r < 0.12f ? dark : r > 0.92f ? light : dirt;
                    if (r > 0.985f) c = pebble;
                    b.Set(x, y, c);
                }

            if (grass)
            {
                Color32 g = C("5bb04a"), gDark = C("3f8f4a"), gLight = C("8fd36a");
                for (int x = 0; x < 16; x++)
                {
                    int depth = 3 + (Rand(x, 0, 3) > 0.55f ? 1 : 0) + (Rand(x, 1, 5) > 0.85f ? 1 : 0);
                    for (int d = 0; d < depth; d++)
                        b.Set(x, 15 - d, d == 0 ? gLight : d == depth - 1 ? gDark : g);
                }
            }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Stone(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 mortar = C("4f535c"), stone = C("8a8f99"), light = C("aab0ba"), dark = C("6d727c");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    int row = y / 4;
                    int off = (row % 2) * 4;
                    bool isMortar = y % 4 == 0 || (x + off) % 8 == 0;
                    Color32 c = isMortar ? mortar : y % 4 == 3 ? light : Rand(x, y, 9) > 0.85f ? dark : stone;
                    b.Set(x, y, c);
                }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Plank(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 top = C("d39a5c"), mid = C("b07a45"), edge = C("6b4226"), nail = C("3a2a20");
            for (int y = 10; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    Color32 c = y == 15 ? top : y == 10 ? edge : mid;
                    if (x % 8 == 7 && y > 10) c = edge;
                    b.Set(x, y, c);
                }
            b.Set(2, 13, nail);
            b.Set(10, 13, nail);
            // small support pegs under the plank
            for (int y = 7; y < 10; y++)
            {
                b.Set(3, y, edge);
                b.Set(12, y, edge);
            }
            return b.ToSprite(PixelArt.Center, name);
        }

        // ---------- props ----------

        static Sprite Coin(int frame, string name)
        {
            var b = new PixelBuffer(16, 16);
            float[] rx = { 5.5f, 4f, 1.5f, 4f };
            Color32 outline = C("9c6a12"), fill = C("e8b83a"), ring = C("b8861f"), shine = C("fff2a8");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float dx = (x - 7.5f) / rx[frame];
                    float dy = (y - 7.5f) / 6f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;
                    Color32 c = d > 0.6f ? outline : fill;
                    if (frame == 0 && d > 0.12f && d < 0.28f) c = ring;
                    if (frame != 2 && d <= 0.6f && x < 7 && y > 9) c = shine;
                    b.Set(x, y, c);
                }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Spikes(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 light = C("c89a62"), dark = C("7a5230"), tip = C("efd8a8"), band = C("5e3b22");
            for (int s = 0; s < 4; s++)
            {
                int x0 = 1 + s * 4;
                for (int y = 0; y < 11; y++)
                    for (int dx = 0; dx < 3; dx++)
                    {
                        bool on = y < 8 || dx == 1;
                        if (!on) continue;
                        Color32 c = y >= 9 ? tip : dx == 0 ? light : dark;
                        b.Set(x0 + dx, y, c);
                    }
            }
            for (int x = 0; x < 16; x++)
            {
                b.Set(x, 2, band);
                b.Set(x, 3, band);
            }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite Torch(int frame, string name)
        {
            var b = new PixelBuffer(16, 32);
            Color32 wood = C("9c6b3c"), woodDark = C("5e3b22"), metal = C("6d7380"), metalLight = C("aeb4bf");
            for (int y = 0; y < 18; y++)
            {
                b.Set(7, y, wood);
                b.Set(8, y, woodDark);
            }
            for (int x = 5; x <= 10; x++)
            {
                b.Set(x, 18, metal);
                b.Set(x, 19, metalLight);
                b.Set(x, 20, metal);
            }
            if (frame >= 0)
            {
                Color32 outer = C("f08a24"), inner = C("ffd84a"), core = C("fff6d0");
                for (int y = 21; y < 31; y++)
                {
                    float h = (y - 21) / 10f;
                    float w = 3.2f * (1f - h) + 0.4f;
                    float sway = Mathf.Sin(h * 3.2f) * 0.9f * (frame == 0 ? 1f : -1f);
                    float cx = 7.5f + sway;
                    for (int x = 0; x < 16; x++)
                    {
                        float dx = Mathf.Abs(x - cx);
                        if (dx > w) continue;
                        Color32 c = dx < w * 0.3f && h < 0.4f ? core : dx < w * 0.6f && h < 0.7f ? inner : outer;
                        b.Set(x, y, c);
                    }
                }
            }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite Banner(string name)
        {
            var b = new PixelBuffer(16, 48);
            Color32 wood = C("9c6b3c"), woodDark = C("5e3b22"), gold = C("e8b83a"), goldDark = C("9c6a12");
            Color32 cloth = C("2f7a3e"), clothDark = C("215a2d");
            for (int y = 0; y < 45; y++)
            {
                b.Set(2, y, wood);
                b.Set(3, y, woodDark);
            }
            for (int y = 44; y < 48; y++)
                for (int x = 1; x <= 4; x++)
                    b.Set(x, y, (x == 1 || y == 47) ? gold : goldDark);

            for (int y = 24; y < 43; y++)
                for (int x = 4; x < 16; x++)
                {
                    int notch = 3 - Mathf.Abs(x - 10);
                    if (notch > 0 && y < 24 + notch) continue; // swallowtail
                    Color32 c = y == 42 || x == 15 ? gold : (x + y) % 7 == 0 ? clothDark : cloth;
                    b.Set(x, y, c);
                }
            // emblem: a gold ring with a dot (an acorn-like mark)
            for (int y = 29; y <= 39; y++)
                for (int x = 5; x <= 15; x++)
                {
                    float d = Mathf.Sqrt((x - 10f) * (x - 10f) + (y - 34f) * (y - 34f));
                    if (d > 2.5f && d < 3.6f) b.Set(x, y, gold);
                    else if (d < 1.2f) b.Set(x, y, gold);
                }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        // ---------- backgrounds (480 px wide, seamless horizontally) ----------

        const int BgW = 480;
        static readonly float[] Bayer =
        {
            0, 8, 2, 10,
            12, 4, 14, 6,
            3, 11, 1, 9,
            15, 7, 13, 5
        };

        static Color32 Gradient(Color32[] stops, float t)
        {
            t = Mathf.Clamp01(t) * (stops.Length - 1);
            int i = Mathf.Min((int)t, stops.Length - 2);
            return Color32.Lerp(stops[i], stops[i + 1], t - i);
        }

        static Sprite Sky(string name)
        {
            const int H = 270, bands = 12;
            var stops = new[] { C("f6e3b4"), C("cfe6e6"), C("8cc4e8"), C("4a90c8") };
            var b = new PixelBuffer(BgW, H);
            for (int y = 0; y < H; y++)
            {
                float v = y / (float)(H - 1) * bands;
                int band = (int)v;
                float frac = v - band;
                for (int x = 0; x < BgW; x++)
                {
                    int bb = frac > Bayer[(x & 3) + (y & 3) * 4] / 16f ? band + 1 : band;
                    b.Set(x, y, Gradient(stops, Mathf.Min(bb, bands) / (float)bands));
                }
            }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Clouds(string name)
        {
            const int H = 96;
            var b = new PixelBuffer(BgW, H);
            Color32 white = C("fbfdff"), shade = C("d6e6f2");
            int[,] clouds = { { 40, 50, 10 }, { 150, 70, 7 }, { 250, 40, 12 }, { 360, 64, 8 }, { 440, 30, 6 } };
            for (int i = 0; i < clouds.GetLength(0); i++)
            {
                int cx = clouds[i, 0], cy = clouds[i, 1], r = clouds[i, 2];
                int[,] puffs = { { 0, 0, r }, { -r, -r / 3, r * 3 / 4 }, { r, -r / 3, r * 3 / 4 }, { r * 2, -r / 2, r / 2 }, { -r * 2, -r / 2, r / 2 } };
                for (int p = 0; p < puffs.GetLength(0); p++)
                {
                    int px = cx + puffs[p, 0], py = cy + puffs[p, 1], pr = puffs[p, 2];
                    for (int y = py - pr; y <= py + pr; y++)
                        for (int x = px - pr; x <= px + pr; x++)
                        {
                            int dx = x - px, dy = y - py;
                            if (dx * dx + dy * dy > pr * pr) continue;
                            b.SetWrapX(x, y, y < cy - r / 2 ? shade : white);
                        }
                }
            }
            return b.ToSprite(PixelArt.BottomLeft, name, TextureWrapMode.Repeat);
        }

        static float Ridge(int x, float baseH, float a1, int f1, float a2, int f2, float a3, int f3)
        {
            float fx = x / (float)BgW * Mathf.PI * 2f;
            return baseH + a1 * Mathf.Sin(f1 * fx) + a2 * Mathf.Sin(f2 * fx + 1.3f) + a3 * Mathf.Sin(f3 * fx + 0.4f);
        }

        static Sprite Mountains(string name)
        {
            const int H = 112;
            var b = new PixelBuffer(BgW, H);
            Color32 lit = C("8aa4c8"), shadow = C("7590b5"), snow = C("eef3f8"), snowShade = C("cdd9e8");
            for (int x = 0; x < BgW; x++)
            {
                float h = Ridge(x, 58, 26, 2, 14, 5, 6, 13);
                float slope = Ridge(x + 1, 58, 26, 2, 14, 5, 6, 13) - h;
                int top = (int)h;
                for (int y = 0; y < top; y++)
                {
                    bool isSnow = top > 78 && y > top - (top - 78) * 0.7f;
                    Color32 c = isSnow ? (slope < 0 ? snowShade : snow) : (slope < 0 ? shadow : lit);
                    b.Set(x, y, c);
                }
            }
            return b.ToSprite(PixelArt.BottomLeft, name, TextureWrapMode.Repeat);
        }

        static Sprite Hills(string name)
        {
            const int H = 80;
            var b = new PixelBuffer(BgW, H);
            Color32 fill = C("6b9a6a"), top = C("82b37a"), dark = C("5a8759");
            for (int x = 0; x < BgW; x++)
            {
                int h = (int)Ridge(x, 36, 10, 3, 5, 7, 2, 17);
                for (int y = 0; y < h; y++)
                {
                    Color32 c = y >= h - 2 ? top : (y < 12 && Bayer[(x & 3) + (y & 3) * 4] > y) ? dark : fill;
                    b.Set(x, y, c);
                }
            }
            return b.ToSprite(PixelArt.BottomLeft, name, TextureWrapMode.Repeat);
        }

        static Sprite Forest(string name)
        {
            // Tall dark forest floor so pits read as deep shadow instead of showing the sky.
            const int H = 104, Floor = 28;
            var b = new PixelBuffer(BgW, H);
            Color32 tree = C("2f5a3e"), edge = C("3e7050"), ground = C("27493a"), deep = C("1c3329");
            for (int x = 0; x < BgW; x++)
                for (int y = 0; y < Floor; y++)
                    b.Set(x, y, y < Floor - 10 && Bayer[(x & 3) + (y & 3) * 4] < (Floor - 10 - y) ? deep : ground);

            for (int i = 0; i < 24; i++)
            {
                int cx = i * 20 + (int)(Rand(i, 0, 11) * 10f);
                float height = 30f + Rand(i, 1, 11) * 34f;
                float halfBase = height * 0.3f;
                const int baseY = Floor - 4;
                for (int y = baseY; y < baseY + (int)height; y++)
                {
                    float t = (y - baseY) / height;
                    float w = halfBase * (1f - t);
                    if (((y / 4) & 1) == 0) w *= 0.8f; // layered branches
                    int iw = Mathf.CeilToInt(w);
                    for (int dx = -iw; dx <= iw; dx++)
                        b.SetWrapX(cx + dx, y, dx == -iw ? edge : tree);
                }
            }
            return b.ToSprite(PixelArt.BottomLeft, name, TextureWrapMode.Repeat);
        }
    }
}
