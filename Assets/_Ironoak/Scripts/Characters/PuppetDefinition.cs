using System.Collections.Generic;
using UnityEngine;

namespace Ironoak.Characters
{
    /// <summary>
    /// Describes a cut-out puppet: which painted part images it uses, how they're joined, and
    /// their draw order. Every value can be tuned in the Inspector on
    /// Assets/_Ironoak/Resources/Characters/&lt;Name&gt;.asset without touching code.
    ///
    /// Conventions (see docs/design/CHARACTER_PIPELINE.md):
    /// - World units, character faces right, feet at y = 0.
    /// - Limb parts are drawn hanging straight down with the joint at the top.
    /// - <see cref="Part.pivot"/> is where the joint sits inside the image (0..1, from bottom-left).
    /// - <see cref="Part.height"/> is the part's on-screen height, so source images can be any
    ///   resolution: the rig scales each image to fit.
    /// </summary>
    [CreateAssetMenu(menuName = "Ironoak/Puppet Definition")]
    public class PuppetDefinition : ScriptableObject
    {
        [System.Serializable]
        public class Part
        {
            public string name;
            [Tooltip("Name of the parent part (empty = attached to the puppet root at the feet).")]
            public string parent;
            [Tooltip("Image name in Resources/Art/Painted/<folder>/")]
            public string sprite;
            [Tooltip("Joint position relative to the parent's joint, at rest (world units).")]
            public Vector2 offset;
            [Tooltip("Where the joint is inside the image (0..1 from bottom-left).")]
            public Vector2 pivot = new Vector2(0.5f, 0.9f);
            [Tooltip("On-screen height of the image (world units).")]
            public float height = 0.3f;
            [Tooltip("Draw order inside the puppet (higher = in front).")]
            public int order;
            [Tooltip("Tint; back limbs are darkened so the same image reads as 'further away'.")]
            public Color tint = Color.white;
        }

        public string spriteFolder = "Brenna";
        public List<Part> parts = new List<Part>();

        public static PuppetDefinition LoadOrDefault(string name)
        {
            var asset = Resources.Load<PuppetDefinition>("Characters/" + name);
            return asset != null ? asset : CreateBrenna();
        }

        static Part P(string name, string parent, string sprite, float ox, float oy, float px, float py, float h, int order, float shade = 1f) =>
            new Part
            {
                name = name, parent = parent, sprite = sprite,
                offset = new Vector2(ox, oy), pivot = new Vector2(px, py),
                height = h, order = order, tint = new Color(shade, shade, shade, 1f)
            };

        /// <summary>Brenna: 8 unique images, 14 parts (back limbs reuse the front images, darkened).</summary>
        public static PuppetDefinition CreateBrenna()
        {
            var d = CreateInstance<PuppetDefinition>();
            d.name = "Brenna";
            d.spriteFolder = "Brenna";
            const float back = 0.72f;
            d.parts = new List<Part>
            {
                P("pelvis",          "",                "brenna_pelvis",     0f,     0.60f,  0.5f,  0.88f, 0.26f, 10),
                P("torso",           "pelvis",          "brenna_torso",      0f,     0.01f,  0.5f,  0.06f, 0.42f, 11),
                P("head",            "torso",           "brenna_head",       0.02f,  0.36f,  0.42f, 0.06f, 0.52f, 12),
                P("braid1",          "head",            "brenna_braid",     -0.16f,  0.28f,  0.5f,  0.92f, 0.17f, 5),
                P("braid2",          "braid1",          "brenna_braid",      0f,    -0.13f,  0.5f,  0.92f, 0.16f, 4),
                P("braid3",          "braid2",          "brenna_braid",      0f,    -0.12f,  0.5f,  0.92f, 0.15f, 3),
                P("arm_back_upper",  "torso",           "brenna_arm_upper", -0.03f,  0.31f,  0.5f,  0.9f,  0.27f, 2, back),
                P("arm_back_lower",  "arm_back_upper",  "brenna_arm_lower",  0f,    -0.21f,  0.5f,  0.92f, 0.29f, 1, back),
                P("leg_back_upper",  "pelvis",          "brenna_leg_upper", -0.05f, -0.02f,  0.5f,  0.92f, 0.32f, 7, back),
                P("leg_back_lower",  "leg_back_upper",  "brenna_leg_lower",  0f,    -0.26f,  0.4f,  0.94f, 0.35f, 6, back),
                P("leg_front_upper", "pelvis",          "brenna_leg_upper",  0.05f, -0.02f,  0.5f,  0.92f, 0.32f, 9),
                P("leg_front_lower", "leg_front_upper", "brenna_leg_lower",  0f,    -0.26f,  0.4f,  0.94f, 0.35f, 8),
                P("arm_front_upper", "torso",           "brenna_arm_upper",  0.03f,  0.31f,  0.5f,  0.9f,  0.27f, 13),
                P("arm_front_lower", "arm_front_upper", "brenna_arm_lower",  0f,    -0.21f,  0.5f,  0.92f, 0.29f, 14),
            };
            return d;
        }
    }
}
