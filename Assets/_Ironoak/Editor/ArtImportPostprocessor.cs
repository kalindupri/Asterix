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

        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
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
            ti.wrapMode = file.StartsWith("bg_") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = PivotFor(file);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
        }

        /// <summary>Must match the pivots used by ProceduralArt (see docs/ART_SPEC.md).</summary>
        static Vector2 PivotFor(string name)
        {
            if (name == "bg_sky") return new Vector2(0.5f, 0.5f);
            if (name.StartsWith("bg_")) return Vector2.zero;
            if (name.StartsWith("tile_") || name.StartsWith("coin_") || name.StartsWith("heart_"))
                return new Vector2(0.5f, 0.5f);
            return new Vector2(0.5f, 0f); // characters & props stand on their feet
        }
    }
}
