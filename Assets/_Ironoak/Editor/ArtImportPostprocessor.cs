using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// Any PNG dropped into Assets/_Ironoak/Resources/Art is imported as crisp pixel art
    /// (16 PPU, point filtering, no compression) with the pivot its name implies.
    /// This lets the Codex-generated art replace the placeholders with no manual setup.
    /// </summary>
    class ArtImportPostprocessor : AssetPostprocessor
    {
        const string ArtFolder = "/_Ironoak/Resources/Art/";
        const string PaintedFolder = "/_Ironoak/Resources/Art/Painted/";

        // Bump when the rules change: Unity then reimports every texture this processor handles.
        public override uint GetVersion() => 3;

        /// <summary>
        /// Painted (hi-res) art at 128 px per world unit, smooth filtering and mipmaps.
        /// Brenna's parts keep a centre pivot (the cut-out rig positions them by their joint).
        /// Props stand on their bottom-centre; terrain textures repeat (they are mapped in world
        /// space by PaintedTerrain); wide background layers get a 4K size limit.
        /// </summary>
        static void ImportPainted(TextureImporter ti, string path)
        {
            string file = Path.GetFileNameWithoutExtension(path);
            bool ui = path.Contains("/Painted/UI/");
            bool env = path.Contains("/Painted/Environment/");

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 128;
            ti.filterMode = FilterMode.Trilinear;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            ti.mipmapEnabled = !ui;
            ti.mipmapFilter = TextureImporterMipFilter.KaiserFilter;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = env ? 4096 : 2048;
            ti.wrapModeU = file.StartsWith("terrain_") && (file.Contains("_fill") || file.Contains("_top") || file.Contains("_bottom")) || file.StartsWith("bg_") || file.StartsWith("fg_")
                ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            ti.wrapModeV = file.StartsWith("terrain_") && (file.Contains("_fill") || file.Contains("_side")) || file == "root_segment"
                ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = PaintedPivotFor(file, path);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
        }

        public static Vector2 PaintedPivotFor(string file, string path)
        {
            if (path.Contains("/Painted/Props/"))
            {
                if (file == "sap_knot") return new Vector2(0.5f, 0.12f);   // the base sinks into the ground a little
                if (file.StartsWith("coin_") || file.Contains("glow") || file.StartsWith("item_") || file.StartsWith("fx_") ||
                    file.StartsWith("debris_") || file == "bell")
                    return file == "bell" ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0.5f);
                return new Vector2(0.5f, 0f);
            }
            if ((file.StartsWith("bg_") || file.StartsWith("fg_")) && file != "bg_sky") return Vector2.zero;
            return new Vector2(0.5f, 0.5f);
        }

        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (path.Contains(PaintedFolder))
            {
                ImportPainted((TextureImporter)assetImporter, path);
                return;
            }
            if (!path.Contains(ArtFolder)) return;

            string file = Path.GetFileNameWithoutExtension(path);
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.isReadable = false;
            ti.wrapMode = file.StartsWith("bg_") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = PivotFor(file);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
        }

        /// <summary>Must match the pivots used by ProceduralArt (see docs/ART_SPEC.md).</summary>
        public static Vector2 PivotFor(string name)
        {
            if (name == "bg_sky") return new Vector2(0.5f, 0.5f);
            if (name.StartsWith("bg_")) return Vector2.zero;
            if (name == "bell") return new Vector2(0.5f, 1f);
            if (name == "sap_knot") return new Vector2(0.5f, 0.375f);
            if (name.StartsWith("tile_") || name.StartsWith("coin_") || name.StartsWith("heart_") ||
                name.StartsWith("item_") || name.StartsWith("fx_") ||
                name == "rope" || name == "bridge_plank" || name == "secret_cover")
                return new Vector2(0.5f, 0.5f);
            return new Vector2(0.5f, 0f); // characters, props and tiled columns stand on their feet
        }
    }
}
