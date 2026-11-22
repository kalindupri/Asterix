using System.Collections.Generic;
using UnityEngine;

namespace Ironoak.Art
{
    /// <summary>Placeholder art for the M1 puzzle elements (original designs; replace with final art).</summary>
    public static partial class ProceduralArt
    {
        /// <summary>Every sprite name the game uses. The editor art baker writes any missing ones to PNG.</summary>
        public static readonly string[] AllSpriteNames =
        {
            "player_idle", "player_run1", "player_run2", "player_jump",
            "roman_walk1", "roman_walk2", "roman_squash",
            "tile_grass", "tile_dirt", "tile_stone", "tile_plank",
            "coin_0", "coin_1", "coin_2", "coin_3",
            "spikes", "torch_off", "torch_on1", "torch_on2", "goal_banner",
            "heart_full", "heart_empty",
            "bg_sky", "bg_clouds", "bg_mountains", "bg_hills", "bg_forest",
            // M1 puzzle set
            "sap_knot", "root_segment", "root_cap", "bell_post", "bell", "gate_bars", "bridge_plank", "rope",
            "plate_up", "plate_down", "crate", "wall_cracked", "secret_cover", "water",
            "item_ogham", "item_golden_torc", "item_shard", "item_druid_mark", "item_heartwood_torc",
            "boar_run1", "boar_run2", "cart_broken",
            "fx_dot", "fx_stone", "fx_particle",
        };

        static Sprite CreatePuzzleArt(string name)
        {
            switch (name)
            {
                case "sap_knot": return SapKnot(name);
                case "root_segment": return Root(false, name);
                case "root_cap": return Root(true, name);
                case "bell_post": return BellPost(name);
                case "bell": return BellSprite(name);
                case "gate_bars": return GateBars(name);
                case "bridge_plank": return BridgePlank(name);
                case "rope": return RopeSprite(name);
                case "plate_up": return Plate(false, name);
                case "plate_down": return Plate(true, name);
                case "crate": return CrateSprite(name);
                case "wall_cracked": return Cracked(name);
                case "secret_cover": return Cover(name);
                case "water": return Water(name);
                case "item_ogham": return Ogham(name);
                case "item_golden_torc": return TorcItem(C("e8b83a"), C("9c6a12"), name);
                case "item_heartwood_torc": return TorcItem(C("9c6b3c"), C("4a2e1a"), name, true);
                case "item_shard": return Shard(name);
                case "item_druid_mark": return DruidMark(name);
                case "boar_run1": return Boar(0, name);
                case "boar_run2": return Boar(1, name);
                case "cart_broken": return Cart(name);
                case "fx_dot": return Solid(2, 2, C("fff2c8"), name);
                case "fx_stone": return Stone4(name);
                case "fx_particle": return Solid(2, 2, C("ffffff"), name);
                default: return null;
            }
        }

        static Sprite Solid(int w, int h, Color32 c, string name)
        {
            var b = new PixelBuffer(w, h);
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) b.Set(x, y, c);
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Stone4(string name)
        {
            var b = new PixelBuffer(4, 4);
            Color32 o = C("3a3440"), m = C("9a948c"), l = C("d0cabe");
            string[] rows = { ".oo.", "oml o".Replace(" ", ""), "ommo", ".oo." };
            rows[1] = "olmo";
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                {
                    char ch = rows[3 - y][x];
                    if (ch == 'o') b.Set(x, y, o);
                    else if (ch == 'm') b.Set(x, y, m);
                    else if (ch == 'l') b.Set(x, y, l);
                }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite SapKnot(string name)
        {
            // Drawn white-ish so SapKnot can tint it (grey when dormant, glowing green-gold when awake).
            var b = new PixelBuffer(16, 8);
            Color32 rim = C("8a8a8a"), body = C("d8d8d8"), core = C("ffffff");
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 16; x++)
                {
                    float dx = (x - 7.5f) / 7f, dy = (y - 2.5f) / 3.5f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;
                    b.Set(x, y, d > 0.6f ? rim : d < 0.15f ? core : body);
                }
            // spiral hint
            b.Set(6, 3, rim); b.Set(7, 4, rim); b.Set(8, 4, rim); b.Set(9, 3, rim); b.Set(8, 2, rim);
            return b.ToSprite(new Vector2(0.5f, 0.375f), name);
        }

        static Sprite Root(bool cap, string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 dark = C("4a2e1a"), mid = C("7a5030"), light = C("a8774a"), moss = C("6fa046");
            for (int y = 0; y < 16; y++)
                for (int x = 2; x < 14; x++)
                {
                    bool edge = x == 2 || x == 13;
                    int twist = (x + y / 2) % 6;
                    Color32 c = edge ? dark : twist == 0 ? dark : twist < 3 ? mid : light;
                    if (cap && y > 11)
                    {
                        float w = (15 - y) * 1.6f + 1f; // tapering crown
                        if (Mathf.Abs(x - 7.5f) > w) continue;
                    }
                    b.Set(x, y, c);
                }
            if (cap)
            {
                b.Set(5, 13, moss); b.Set(6, 14, moss); b.Set(9, 12, moss); b.Set(10, 13, moss);
                for (int x = 3; x < 13; x++) if (Rand(x, 1, 21) > 0.5f) b.Set(x, 12, moss);
            }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite BellPost(string name)
        {
            var b = new PixelBuffer(16, 48);
            Color32 wood = C("9c6b3c"), dark = C("5e3b22");
            for (int y = 0; y < 46; y++) { b.Set(3, y, wood); b.Set(4, y, dark); }
            for (int x = 3; x < 14; x++) { b.Set(x, 45, wood); b.Set(x, 44, dark); }
            for (int i = 0; i < 4; i++) b.Set(5 + i, 40 + i, dark); // brace
            b.Set(11, 43, dark); b.Set(11, 42, dark); // hook
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite BellSprite(string name)
        {
            var b = new PixelBuffer(12, 12);
            Color32 o = C("2b1d2e"), bronze = C("c8862a"), light = C("f0c060"), dark = C("8a5418");
            for (int y = 0; y < 12; y++)
                for (int x = 0; x < 12; x++)
                {
                    float top = 10.5f;
                    float halfW = y < 2 ? 5.5f : 2.5f + (top - y) * -0.0f + (10f - y) * 0.3f;
                    halfW = y <= 1 ? 5.5f : Mathf.Lerp(5f, 2f, (y - 1) / 9f);
                    if (y > 10 || Mathf.Abs(x - 5.5f) > halfW) continue;
                    bool edge = Mathf.Abs(x - 5.5f) > halfW - 1f || y == 0 || y == 10;
                    b.Set(x, y, edge ? o : x < 5 ? light : y < 3 ? dark : bronze);
                }
            b.Set(5, 11, o); b.Set(6, 11, o);
            return b.ToSprite(new Vector2(0.5f, 1f), name);
        }

        static Sprite GateBars(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 wood = C("8a5a34"), dark = C("4a2e1a"), iron = C("6d7380"), ironL = C("aeb4bf");
            for (int y = 0; y < 16; y++)
                for (int x = 1; x < 15; x++)
                {
                    bool bar = x % 4 == 1 || x % 4 == 2;
                    if (bar) b.Set(x, y, x % 4 == 1 ? wood : dark);
                }
            for (int x = 1; x < 15; x++) { b.Set(x, 6, iron); b.Set(x, 7, ironL); }
            for (int x = 1; x < 15; x += 4) b.Set(x, 0, ironL);
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite BridgePlank(string name)
        {
            var b = new PixelBuffer(16, 6);
            Color32 top = C("d39a5c"), mid = C("b07a45"), edge = C("6b4226");
            for (int y = 0; y < 6; y++)
                for (int x = 0; x < 16; x++)
                    b.Set(x, y, y == 5 ? top : y == 0 || x == 15 ? edge : mid);
            b.Set(3, 3, edge); b.Set(12, 3, edge);
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite RopeSprite(string name)
        {
            var b = new PixelBuffer(4, 16);
            Color32 a = C("c8a878"), d = C("8a6a40");
            for (int y = 0; y < 16; y++)
            {
                b.Set(1, y, (y % 4) < 2 ? a : d);
                b.Set(2, y, (y % 4) < 2 ? d : a);
            }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Plate(bool down, string name)
        {
            var b = new PixelBuffer(16, 6);
            Color32 base_ = C("5a5e66"), top = C("aab0ba"), glow = C("ffd84a");
            int h = down ? 2 : 4;
            for (int x = 0; x < 16; x++) { b.Set(x, 0, base_); b.Set(x, 1, base_); }
            for (int y = 2; y < 2 + h - 1; y++) for (int x = 2; x < 14; x++) b.Set(x, y, top);
            for (int x = 3; x < 13; x++) b.Set(x, 1 + h - 1, down ? glow : top);
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite CrateSprite(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 o = C("3a2414"), wood = C("b07a45"), light = C("d39a5c"), dark = C("7a5030");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    bool border = x < 2 || x > 13 || y < 2 || y > 13;
                    bool diag = Mathf.Abs(x - y) <= 1 || Mathf.Abs(x - (15 - y)) <= 0;
                    Color32 c = border ? dark : diag ? dark : (y % 4 == 0 ? light : wood);
                    if (x == 0 || x == 15 || y == 0 || y == 15) c = o;
                    b.Set(x, y, c);
                }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite Cracked(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 mortar = C("4f535c"), stone = C("8a8f99"), light = C("aab0ba"), crack = C("23202a");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    bool isMortar = y % 8 == 0 || (x + (y / 8) * 8) % 16 == 0;
                    b.Set(x, y, isMortar ? mortar : y % 8 == 7 ? light : stone);
                }
            int cx = 7;
            for (int y = 15; y >= 0; y--)
            {
                b.Set(cx, y, crack);
                if (y % 3 == 0) cx += (y % 6 == 0) ? 1 : -1;
                if (y == 9) { b.Set(cx + 1, y, crack); b.Set(cx + 2, y - 1, crack); b.Set(cx + 3, y - 1, crack); }
                if (y == 4) { b.Set(cx - 1, y, crack); b.Set(cx - 2, y + 1, crack); }
            }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite Cover(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 a = C("3b3540"), d = C("2e2933"), l = C("4a4350");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float r = Rand(x, y, 31);
                    b.Set(x, y, r < 0.2f ? d : r > 0.9f ? l : a);
                }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Water(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 deep = new Color32(0x2a, 0x6a, 0xa8, 0xd8), mid = new Color32(0x3a, 0x88, 0xc8, 0xd8), foam = new Color32(0xd8, 0xf0, 0xff, 0xf0);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    b.Set(x, y, ((x + y * 3) % 11 == 0) ? mid : deep);
            for (int x = 0; x < 16; x++)
            {
                b.Set(x, 15, foam);
                if (x % 5 < 2) b.Set(x, 14, foam);
            }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite Ogham(string name)
        {
            var b = new PixelBuffer(12, 16);
            Color32 o = C("2b1d2e"), stone = C("8a8f99"), light = C("b8bec8"), rune = C("7fe0ff");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 12; x++)
                {
                    float w = y > 12 ? 4.5f - (y - 12) : 5.5f;
                    if (Mathf.Abs(x - 5.5f) > w) continue;
                    bool edge = Mathf.Abs(x - 5.5f) > w - 1f || y == 0;
                    b.Set(x, y, edge ? o : x < 5 ? light : stone);
                }
            for (int y = 2; y < 13; y++) b.Set(6, y, rune);
            for (int i = 0; i < 4; i++) { b.Set(4, 3 + i * 3, rune); b.Set(5, 3 + i * 3, rune); if (i % 2 == 0) { b.Set(7, 4 + i * 3, rune); b.Set(8, 4 + i * 3, rune); } }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite TorcItem(Color32 metal, Color32 shade, string name, bool heartwood = false)
        {
            var b = new PixelBuffer(16, 16);
            Color32 o = C("2b1d2e"), shine = C("fff2a8"), gem = heartwood ? C("8fd36a") : C("e04848");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float dx = (x - 7.5f) / 6.5f, dy = (y - 8f) / 5.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > 1f || d < 0.62f) continue;
                    if (y < 5 && Mathf.Abs(x - 7.5f) < 2.5f) continue; // open ring
                    b.Set(x, y, d > 0.93f || d < 0.68f ? o : (x < 7 && y > 8) ? shine : (x + y) % 3 == 0 ? shade : metal);
                }
            // terminals
            b.Set(4, 4, gem); b.Set(5, 4, gem); b.Set(10, 4, gem); b.Set(11, 4, gem);
            b.Set(4, 3, o); b.Set(5, 3, o); b.Set(10, 3, o); b.Set(11, 3, o);
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Shard(string name)
        {
            var b = new PixelBuffer(10, 14);
            Color32 o = C("1d3a5a"), c = C("7fe0ff"), l = C("e8fbff"), d = C("3aa0d8");
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 10; x++)
                {
                    float w = y < 7 ? y * 0.7f : (13 - y) * 0.7f;
                    if (Mathf.Abs(x - 4.5f) > w + 0.3f) continue;
                    bool edge = Mathf.Abs(x - 4.5f) > w - 0.7f;
                    b.Set(x, y, edge ? o : x < 4 ? l : (y % 3 == 0 ? d : c));
                }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite DruidMark(string name)
        {
            var b = new PixelBuffer(16, 16);
            Color32 o = C("2a1440"), p = C("b070ff"), l = C("e8c8ff");
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    float d = Mathf.Sqrt((x - 7.5f) * (x - 7.5f) + (y - 7.5f) * (y - 7.5f));
                    if (d > 7f) continue;
                    b.Set(x, y, d > 6f ? o : d < 2f ? l : p);
                }
            // three-armed spiral mark (generic Celtic motif)
            for (int a = 0; a < 3; a++)
                for (int i = 0; i < 5; i++)
                {
                    float ang = a * 2.094f + i * 0.5f;
                    float r = 1.5f + i * 0.9f;
                    b.Set(Mathf.RoundToInt(7.5f + Mathf.Cos(ang) * r), Mathf.RoundToInt(7.5f + Mathf.Sin(ang) * r), o);
                }
            return b.ToSprite(PixelArt.Center, name);
        }

        static Sprite Boar(int frame, string name)
        {
            var b = new PixelBuffer(24, 16);
            Color32 o = C("2b1d2e"), fur = C("6b4a32"), furL = C("8a6448"), tusk = C("f2ead8"), eye = C("ff6040");
            // body
            for (int y = 3; y < 13; y++)
                for (int x = 2; x < 20; x++)
                {
                    float dx = (x - 10.5f) / 8.5f, dy = (y - 7.5f) / 4.5f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;
                    b.Set(x, y, d > 0.8f ? o : y > 9 ? furL : fur);
                }
            // head & snout (facing right)
            for (int y = 4; y < 11; y++)
                for (int x = 17; x < 24; x++)
                {
                    if (x > 21 && (y < 5 || y > 8)) continue;
                    bool edge = x == 23 || y == 4 || y == 10;
                    b.Set(x, y, edge ? o : fur);
                }
            b.Set(20, 9, eye);
            b.Set(22, 5, tusk); b.Set(23, 6, tusk);
            // bristles
            for (int x = 5; x < 16; x += 2) b.Set(x, 13, o);
            // legs (two poses)
            int[] legX = frame == 0 ? new[] { 5, 8, 14, 17 } : new[] { 6, 9, 13, 16 };
            for (int i = 0; i < legX.Length; i++)
            {
                int lx = legX[i] + (frame == 0 ? (i % 2 == 0 ? -1 : 1) : 0);
                for (int y = 0; y < 4; y++) b.Set(lx, y, o);
            }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }

        static Sprite Cart(string name)
        {
            var b = new PixelBuffer(32, 24);
            Color32 o = C("2b1d2e"), wood = C("9c6b3c"), dark = C("5e3b22"), iron = C("6d7380");
            // tilted bed
            for (int x = 2; x < 28; x++)
            {
                int y0 = 8 + (x / 6);
                for (int y = y0; y < y0 + 6; y++) b.Set(x, y, y == y0 || y == y0 + 5 ? dark : (x % 6 == 0 ? dark : wood));
            }
            // one wheel upright, one broken flat
            for (int y = 0; y < 12; y++)
                for (int x = 0; x < 12; x++)
                {
                    float d = Mathf.Sqrt((x - 5.5f) * (x - 5.5f) + (y - 5.5f) * (y - 5.5f));
                    if (d > 5.5f) continue;
                    if (d > 4.5f) b.Set(x + 3, y, iron);
                    else if (x == 5 || y == 5 || Mathf.Abs(x - y) < 1) b.Set(x + 3, y, dark);
                }
            for (int x = 20; x < 31; x++) { b.Set(x, 0, o); b.Set(x, 1, dark); }
            return b.ToSprite(PixelArt.BottomCenter, name);
        }
    }
}
