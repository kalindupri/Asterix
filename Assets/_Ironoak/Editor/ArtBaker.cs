using System.IO;
using Ironoak.Art;
using UnityEditor;
using UnityEngine;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// Writes a PNG for every sprite the game uses that has no file yet in Resources/Art,
    /// from the procedural placeholder. Existing files (e.g. Codex art) are never touched.
    /// Real asset files are needed so prefabs and scenes can reference the sprites.
    /// </summary>
    public static class ArtBaker
    {
        public const string ArtDir = "Assets/_Ironoak/Resources/Art";

        [MenuItem("Ironoak/Pipeline/1. Bake Missing Placeholder Art", priority = 100)]
        public static void BakeMissing()
        {
            Directory.CreateDirectory(ArtDir);
            int written = 0;
            foreach (var name in ProceduralArt.AllSpriteNames)
            {
                string path = $"{ArtDir}/{name}.png";
                if (File.Exists(path)) continue;
                var sprite = ProceduralArt.Create(name);
                File.WriteAllBytes(path, sprite.texture.EncodeToPNG());
                Object.DestroyImmediate(sprite.texture);
                Object.DestroyImmediate(sprite);
                written++;
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"[Ironoak] Art baker: wrote {written} placeholder PNG(s).");
        }
    }
}
