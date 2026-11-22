using System.Collections.Generic;
using UnityEngine;

namespace Ironoak.Core
{
    public enum ParallaxAnchor
    {
        /// <summary>Scaled to cover the whole view, centred on the camera (skies).</summary>
        Cover,
        /// <summary>Bottom edge sits at the bottom of the view (plus yOffset), sinking as the camera climbs.</summary>
        ViewBottom,
        /// <summary>Top edge sits at the top of the view (plus yOffset). Foreground canopies.</summary>
        ViewTop
    }

    [System.Serializable]
    public class ParallaxLayerDef
    {
        public string name = "layer";
        [Tooltip("Path under Resources/Art/, e.g. Painted/Environment/Forest/bg_far_hills, or a pixel sprite name like bg_hills.")]
        public string sprite;
        [Tooltip("Horizontal: 1 = fixed to the camera (far away), 0 = moves with the world, negative = foreground (moves faster than the world).")]
        [Range(-1f, 1f)] public float factor = 0.5f;
        [Tooltip("Vertical follow: 1 = fixed to the view, 0 = fixed in the world. ViewBottom layers only.")]
        [Range(0f, 1f)] public float verticalFactor = 0.8f;
        public ParallaxAnchor anchor = ParallaxAnchor.ViewBottom;
        [Tooltip("World units above the view bottom (ViewBottom) or above the view top (ViewTop).")]
        public float yOffset;
        public float scale = 1f;
        [Tooltip("For art whose top edge is cut off (tall trunks): never let that edge come into view.")]
        public bool keepTopAboveView;
        public int sortingOrder = -60;
        public Color tint = Color.white;
    }

    /// <summary>
    /// A level's background (and foreground) layers. Assets live in Resources/Parallax/ and are
    /// picked by name from LevelRoot.look.parallaxSet; built-in presets are used if none exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Ironoak/Parallax Set", fileName = "ParallaxSet")]
    public class ParallaxSet : ScriptableObject
    {
        public Color clearColor = new Color32(0x9c, 0xc8, 0xe8, 0xff);
        public List<ParallaxLayerDef> layers = new List<ParallaxLayerDef>();

        const string ForestDir = "Painted/Environment/Forest/";

        /// <summary>Loads Resources/Parallax/&lt;name&gt;, else a built-in preset; falls back to Pixel if the art is missing.</summary>
        public static ParallaxSet Load(string name)
        {
            if (IronoakConfig.PixelPerfect) return Pixel();
            var asset = string.IsNullOrEmpty(name) ? null : Resources.Load<ParallaxSet>("Parallax/" + name);
            if (asset != null) return asset;
            if (name == "Forest" && Resources.Load<Sprite>("Art/" + ForestDir + "bg_sky") != null) return Forest();
            return Pixel();
        }

        static ParallaxLayerDef L(string name, string sprite, float factor, float vFactor, ParallaxAnchor anchor, float yOffset, int order,
                                  Color? tint = null, float scale = 1f, bool keepTop = false) =>
            new ParallaxLayerDef
            {
                name = name, sprite = sprite, factor = factor, verticalFactor = vFactor, anchor = anchor,
                yOffset = yOffset, sortingOrder = order, tint = tint ?? Color.white, scale = scale, keepTopAboveView = keepTop
            };

        /// <summary>Oak Road forest, painted (Batch 1).</summary>
        public static ParallaxSet Forest()
        {
            var set = CreateInstance<ParallaxSet>();
            set.name = "Forest";
            set.clearColor = new Color32(0xbf, 0xdc, 0xee, 0xff);
            set.layers = new List<ParallaxLayerDef>
            {
                L("sky", ForestDir + "bg_sky", 1f, 1f, ParallaxAnchor.Cover, 0f, -100),
                L("far hills", ForestDir + "bg_far_hills", 0.93f, 0.95f, ParallaxAnchor.ViewBottom, 2.6f, -90),
                L("far forest", ForestDir + "bg_far_forest", 0.82f, 0.88f, ParallaxAnchor.ViewBottom, 0.9f, -80),
                L("mid forest", ForestDir + "bg_mid_forest", 0.66f, 0.75f, ParallaxAnchor.ViewBottom, 0.2f, -70, new Color(0.84f, 0.88f, 0.84f)),
                L("near trunks", ForestDir + "bg_near_trunks", 0.45f, 0.6f, ParallaxAnchor.ViewBottom, -1.6f, -60, new Color(0.52f, 0.52f, 0.5f), 0.9f, true),
                L("fg leaves", ForestDir + "fg_leaves_top", -0.25f, 1f, ParallaxAnchor.ViewTop, 0.6f, 35, new Color(0.55f, 0.6f, 0.5f)),
                L("fg bushes", ForestDir + "fg_bushes_bottom", -0.35f, 1f, ParallaxAnchor.ViewBottom, -0.9f, 35, new Color(0.42f, 0.48f, 0.38f)),
            };
            return set;
        }

        /// <summary>The original 16 px pixel-art backdrop.</summary>
        public static ParallaxSet Pixel()
        {
            var set = CreateInstance<ParallaxSet>();
            set.name = "Pixel";
            set.clearColor = new Color32(0x4a, 0x90, 0xc8, 0xff);
            set.layers = new List<ParallaxLayerDef>
            {
                L("sky", "bg_sky", 1f, 1f, ParallaxAnchor.Cover, 0f, -100),
                L("clouds", "bg_clouds", 0.92f, 1f, ParallaxAnchor.ViewBottom, 10.2f, -90),
                L("mountains", "bg_mountains", 0.85f, 1f, ParallaxAnchor.ViewBottom, 1.0f, -80),
                L("hills", "bg_hills", 0.7f, 1f, ParallaxAnchor.ViewBottom, 0.8f, -70),
                L("forest", "bg_forest", 0.45f, 1f, ParallaxAnchor.ViewBottom, 0f, -60),
            };
            return set;
        }
    }
}
