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

        /// <summary>
        /// Painted (hi-res) art: smooth filtering, centre pivot. The cut-out rig scales each part to
        /// its configured on-screen height and positions it by its joint, so the source resolution
        /// and pivot don't matter.
        /// </summary>
        static void ImportPainted(TextureImporter ti)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 128;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.maxTextureSize = 2048;
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
        }

        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (path.Contains(PaintedFolder))
            {
                ImportPainted((TextureImporter)assetImporter);
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
