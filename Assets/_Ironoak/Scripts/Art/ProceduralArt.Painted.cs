using UnityEngine;
using static Ironoak.Art.PaintCanvas;

namespace Ironoak.Art
{
    /// <summary>
    /// Code-painted placeholder parts for Brenna's cut-out puppet (original design). They follow
    /// the exact same canvas and joint conventions as the final art (docs/design/CHARACTER_PIPELINE.md),
    /// so they also serve as templates. Painted at 128 px per unit, centre pivot.
    /// </summary>
    public static partial class ProceduralArt
    {
        public const float PaintedPPU = 128f;

        public static readonly string[] BrennaParts =
        {
            "brenna_head", "brenna_torso", "brenna_pelvis", "brenna_arm_upper", "brenna_arm_lower",
            "brenna_leg_upper", "brenna_leg_lower", "brenna_braid",
        };

        static readonly Color Skin = new Color(0.96f, 0.77f, 0.62f);
        static readonly Color Hair = new Color(0.80f, 0.29f, 0.16f);
        static readonly Color HairDark = new Color(0.55f, 0.17f, 0.10f);
        static readonly Color Tunic = new Color(0.27f, 0.58f, 0.31f);
        static readonly Color TunicDark = new Color(0.19f, 0.42f, 0.23f);
        static readonly Color Leather = new Color(0.45f, 0.28f, 0.16f);
        static readonly Color Gold = new Color(0.95f, 0.75f, 0.25f);
        static readonly Color Trousers = new Color(0.52f, 0.38f, 0.24f);
        static readonly Color TrousersDark = new Color(0.40f, 0.28f, 0.17f);
        static readonly Color Boot = new Color(0.33f, 0.21f, 0.13f);

        static Color Checks(float x, float y) =>
            ((Mathf.FloorToInt(x / 5f) + Mathf.FloorToInt(y / 5f)) & 1) == 0 ? Trousers : TrousersDark;

        public static Sprite CreatePainted(string name)
        {
            switch (name)
            {
                case "brenna_head": return Head(name);
                case "brenna_torso": return Torso(name);
                case "brenna_pelvis": return Pelvis(name);
                case "brenna_arm_upper": return ArmUpper(name);
                case "brenna_arm_lower": return ArmLower(name);
                case "brenna_leg_upper": return LegUpper(name);
                case "brenna_leg_lower": return LegLower(name);
                case "brenna_braid": return Braid(name);
                default:
                    Debug.LogWarning($"[Ironoak] No painted part '{name}'.");
                    var c = new PaintCanvas(16, 16);
                    c.Shape(Circle(8, 8, 7), Color.magenta);
                    return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
            }
        }

        // Canvas sizes (px) — joint positions are documented in CHARACTER_PIPELINE.md.

        static Sprite Head(string name) // 72x80, neck joint at (0.42, 0.06)
        {
            var c = new PaintCanvas(72, 80);
            c.Shape(Capsule(30, 4, 30, 18, 7), Skin);                       // neck
            c.Shape(Ellipse(27, 47, 24, 26), HairDark, 2f, 0.15f);           // hair mass behind
            c.Shape(Ellipse(38, 42, 21, 23), Skin);                          // face
            c.Shape(Ellipse(30, 58, 24, 15), Hair);                          // crown of hair
            c.Shape(Ellipse(49, 57, 11, 8), Hair);                           // fringe sweeping forward
            c.Shape(Circle(29, 40, 4.5f), Skin, 1.5f);                       // ear
            c.Soft(Ellipse(50, 35, 4.5f, 3f), new Color(0.95f, 0.45f, 0.4f, 0.45f), 3f); // cheek
            c.Shape(Ellipse(47, 44, 4.5f, 5.5f), Color.white, 1.4f, 0f);     // eye
            c.Shape(Circle(49, 44, 2.6f), new Color(0.2f, 0.32f, 0.2f), 0f, 0f);
            c.Soft(Circle(49.8f, 45.3f, 0.9f), Color.white, 0.8f);
            c.Shape(Capsule(42, 52, 51, 53.5f, 1.4f), HairDark, 0f, 0f);     // brow
            c.Shape(Circle(57, 39, 3.2f), Skin, 1.5f);                       // nose
            c.Shape(Capsule(50, 30, 54, 31, 1.1f), new Color(0.6f, 0.22f, 0.2f), 0f, 0f); // mouth
            return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
        }

        static Sprite Torso(string name) // 56x60, waist joint at (0.5, 0.06)
        {
            var c = new PaintCanvas(56, 60);
            c.Shape(RoundBox(28, 32, 17, 27, 10), Tunic);                    // tunic body
            c.Soft(Capsule(31, 55, 35, 45, 4.5f), Skin, 1.5f);               // V-neck
            c.Shape(Capsule(14, 42, 42, 42, 1.3f), Gold, 0f, 0f);            // embroidered trim
            c.Shape(RoundBox(28, 9, 18, 4.5f, 2), Leather);                  // belt
            c.Shape(RoundBox(35, 9, 4.5f, 4.5f, 1.5f), Gold, 1.5f);          // buckle
            return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
        }

        static Sprite Pelvis(string name) // 60x36, hip joint at (0.5, 0.88)
        {
            var c = new PaintCanvas(60, 36);
            c.Shape(RoundBox(30, 19, 20, 15, 7), TunicDark);                 // tunic skirt
            c.Shape(Capsule(12, 7, 48, 7, 2.2f), Gold, 1.2f);                // hem
            return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
        }

        static Sprite ArmUpper(string name) // 22x38, shoulder joint at (0.5, 0.9)
        {
            var c = new PaintCanvas(22, 38);
            c.Shape(Capsule(11, 33, 11, 9, 8.5f, 7f), Tunic);                // sleeve
            c.Shape(Capsule(4, 10, 18, 10, 1.4f), Gold, 0f, 0f);             // cuff trim
            return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
        }

        static Sprite ArmLower(string name) // 22x40, elbow joint at (0.5, 0.92)
        {
            var c = new PaintCanvas(22, 40);
            c.Shape(Capsule(11, 35, 11, 12, 6f, 5f), Skin);                  // forearm
            c.Shape(RoundBox(11, 19, 6.5f, 4.5f, 2), Leather);               // bracer
            c.Shape(Circle(12, 8, 6.5f), Skin);                              // hand
            c.Shape(Capsule(14, 5, 17, 9, 2f), Skin, 1.2f);                  // thumb
            return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
        }

        static Sprite LegUpper(string name) // 26x44, hip joint at (0.5, 0.92)
        {
            var c = new PaintCanvas(26, 44);
            c.Shape(Capsule(13, 39, 13, 9, 10f, 8.5f), Trousers, 2f, 0.22f, Checks);
            return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
        }

        static Sprite LegLower(string name) // 34x50, knee joint at (0.4, 0.94)
        {
            var c = new PaintCanvas(34, 50);
            c.Shape(Capsule(13.6f, 46, 13.6f, 22, 8.5f, 7.5f), Trousers, 2f, 0.22f, Checks);
            c.Shape(Union(RoundBox(15, 11, 10, 9, 5), Capsule(15, 6, 27, 6, 5)), Boot); // boot with toe
            c.Shape(Capsule(5, 19, 23, 19, 3f), Leather, 1.5f);              // boot cuff
            return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
        }

        static Sprite Braid(string name) // 20x28, top joint at (0.5, 0.92)
        {
            var c = new PaintCanvas(20, 28);
            c.Shape(Capsule(10, 24, 10, 6, 7f, 5f), Hair);
            c.Shape(Capsule(6, 20, 13, 15, 1.2f), HairDark, 0f, 0f);         // plait strands
            c.Shape(Capsule(7, 13, 13, 9, 1.2f), HairDark, 0f, 0f);
            c.Shape(RoundBox(10, 5, 5.5f, 2.5f, 1), Gold, 1.2f);             // band
            return c.ToSprite(new Vector2(0.5f, 0.5f), PaintedPPU, name);
        }
    }
}
