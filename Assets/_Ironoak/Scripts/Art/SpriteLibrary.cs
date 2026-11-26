using System.Collections.Generic;
using Ironoak.Core;
using UnityEngine;

namespace Ironoak.Art
{
    /// <summary>
    /// Central sprite lookup. Final art dropped into Assets/_Ironoak/Resources/Art/&lt;name&gt;.png
    /// automatically overrides the procedural placeholder of the same name, with no code
    /// changes needed. See docs/ART_SPEC.md for the names, sizes and pivots.
    /// </summary>
    public static class SpriteLibrary
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Gameplay sprite names that have a painted (hi-res) replacement. In the Native display
        /// mode, a painted file that exists wins over the pixel sprite of the same gameplay name.
        /// </summary>
        static readonly Dictionary<string, string> PaintedAliases = new Dictionary<string, string>
        {
            { "crate", "Props/crate" },
            { "sap_knot", "Props/sap_knot" },
            { "root_segment", "Props/root_segment" },
            { "root_cap", "Props/root_crown" },
            { "coin_0", "Props/coin_gold" },
            { "heart_full", "UI/ui_heart_full" },
            { "heart_empty", "UI/ui_heart_empty" },
            { "ui_coin", "UI/ui_coin" },
            // Batch 2: Level 1 puzzle pieces
            { "bell", "Props/bell" },
            { "bell_post", "Props/bell_post" },
            { "gate_bars", "Props/gate_portcullis" },
            { "bridge_plank", "Props/bridge_plank" },
            { "rope", "Props/rope" },
            { "plate_up", "Props/plate_up" },
            { "plate_down", "Props/plate_down" },
            { "wall_cracked", "Props/wall_cracked" },
            { "water", "Props/water_body" },
            { "spikes", "Props/spikes_stakes" },
            { "torch_off", "Props/torch_post_off" },
            { "torch_on1", "Props/torch_post_on" },
            { "torch_on2", "Props/torch_post_on" },
            { "goal_banner", "Props/goal_banner" },
            { "cart_broken", "Props/cart_broken" },
            { "item_ogham", "Props/item_ogham" },
            { "item_golden_torc", "Props/item_golden_torc" },
            { "item_heartwood_torc", "Props/item_heartwood_torc" },
            { "item_shard", "Props/item_shard" },
            { "item_druid_mark", "Props/item_druid_mark" },
            { "secret_cover", "Props/secret_cover_rock" },
        };

        /// <summary>True when painted art should be preferred (Native display mode).</summary>
        public static bool PreferPainted => !IronoakConfig.PixelPerfect;

        public static Sprite Get(string name)
        {
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;

            Sprite sprite = null;
            if (PreferPainted && PaintedAliases.TryGetValue(name, out var painted))
                sprite = Resources.Load<Sprite>("Art/Painted/" + painted);
            if (sprite == null) sprite = Resources.Load<Sprite>("Art/" + name);
            if (sprite == null) sprite = ProceduralArt.Create(name);
            Cache[name] = sprite;
            return sprite;
        }

        /// <summary>
        /// Painted (high-resolution) art: Resources/Art/Painted/&lt;folder&gt;/&lt;name&gt;.png, falling back to
        /// a code-painted placeholder.
        /// </summary>
        public static Sprite GetPainted(string folder, string name)
        {
            string key = "painted/" + folder + "/" + name;
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var sprite = Resources.Load<Sprite>($"Art/Painted/{folder}/{name}");
            if (sprite == null) sprite = ProceduralArt.CreatePainted(name);
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>The painted sprite Resources/Art/Painted/&lt;folder&gt;/&lt;name&gt;.png, or null if it isn't there.</summary>
        public static Sprite GetPaintedOrNull(string folder, string name)
        {
            string key = "painted?/" + folder + "/" + name;
            if (Cache.TryGetValue(key, out var cached)) return cached;
            var sprite = Resources.Load<Sprite>($"Art/Painted/{folder}/{name}");
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>True if the sprite is hi-res painted art rather than 16 px pixel art.</summary>
        public static bool IsPainted(Sprite s) => s != null && s.pixelsPerUnit > 32f;

        public static Sprite[] GetFrames(params string[] names)
        {
            var frames = new Sprite[names.Length];
            for (int i = 0; i < names.Length; i++) frames[i] = Get(names[i]);
            return frames;
        }
    }
}
