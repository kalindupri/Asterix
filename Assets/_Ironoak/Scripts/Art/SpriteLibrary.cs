using System.Collections.Generic;
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

        public static Sprite Get(string name)
        {
            if (Cache.TryGetValue(name, out var cached) && cached != null) return cached;

            var sprite = Resources.Load<Sprite>("Art/" + name);
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

        public static Sprite[] GetFrames(params string[] names)
        {
            var frames = new Sprite[names.Length];
            for (int i = 0; i < names.Length; i++) frames[i] = Get(names[i]);
            return frames;
        }
    }
}
