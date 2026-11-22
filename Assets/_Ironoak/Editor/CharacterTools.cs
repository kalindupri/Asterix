using System.IO;
using Ironoak.Art;
using Ironoak.Characters;
using Ironoak.Core;
using UnityEditor;
using UnityEngine;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// Cut-out character pipeline helpers:
    /// - bake code-painted placeholder parts (only where no final art exists yet),
    /// - write annotated templates (4x size, joint marked) to docs/character-templates for
    ///   Codex prompts and cleanup,
    /// - create the editable rig definition and the project config asset.
    /// </summary>
    public static class CharacterTools
    {
        public const string PaintedDir = "Assets/_Ironoak/Resources/Art/Painted/Brenna";
        const string DefinitionPath = "Assets/_Ironoak/Resources/Characters/Brenna.asset";
        const string ConfigPath = "Assets/_Ironoak/Resources/IronoakConfig.asset";
        const string TemplateDir = "docs/character-templates";

        [MenuItem("Ironoak/Characters/Bake Brenna Placeholder Parts", priority = 200)]
        public static void BakePlaceholders()
        {
            Directory.CreateDirectory(PaintedDir);
            int written = 0;
            foreach (var part in ProceduralArt.BrennaParts)
            {
                string path = $"{PaintedDir}/{part}.png";
                if (File.Exists(path)) continue;
                var s = ProceduralArt.CreatePainted(part);
                File.WriteAllBytes(path, s.texture.EncodeToPNG());
                Object.DestroyImmediate(s.texture);
                Object.DestroyImmediate(s);
                written++;
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"[Ironoak] Brenna placeholders: wrote {written} part(s).");
        }

        [MenuItem("Ironoak/Characters/Write Part Templates (docs)", priority = 201)]
        public static void WriteTemplates()
        {
            Directory.CreateDirectory(TemplateDir);
            var def = PuppetDefinition.CreateBrenna();
            const int scale = 4;
            foreach (var partName in ProceduralArt.BrennaParts)
            {
                var src = ProceduralArt.CreatePainted(partName);
                var tex = src.texture;
                int w = tex.width * scale, h = tex.height * scale;
                var outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                var checker = new Color[w * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        // checkerboard = transparent area; part bilinear-upscaled on top
                        Color bg = ((x / 16 + y / 16) & 1) == 0 ? new Color(0.86f, 0.86f, 0.86f) : new Color(0.74f, 0.74f, 0.74f);
                        Color fg = tex.GetPixelBilinear((x + 0.5f) / w, (y + 0.5f) / h);
                        checker[y * w + x] = Color.Lerp(bg, new Color(fg.r, fg.g, fg.b, 1f), fg.a);
                    }

                // Joint marker (magenta cross) from the rig definition.
                var part = def.parts.Find(p => p.sprite == partName);
                if (part != null)
                {
                    int jx = Mathf.RoundToInt(part.pivot.x * w), jy = Mathf.RoundToInt(part.pivot.y * h);
                    for (int i = -14; i <= 14; i++)
                        for (int t = -1; t <= 1; t++)
                        {
                            Set(checker, w, h, jx + i, jy + t, Color.magenta);
                            Set(checker, w, h, jx + t, jy + i, Color.magenta);
                        }
                }
                outTex.SetPixels(checker);
                outTex.Apply();
                File.WriteAllBytes($"{TemplateDir}/{partName}_template.png", outTex.EncodeToPNG());
                Object.DestroyImmediate(outTex);
                Object.DestroyImmediate(src.texture);
                Object.DestroyImmediate(src);
            }
            Debug.Log("[Ironoak] Character templates written to " + TemplateDir);
        }

        static void Set(Color[] px, int w, int h, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            px[y * w + x] = c;
        }

        [MenuItem("Ironoak/Characters/Create Brenna Rig Asset", priority = 202)]
        public static void CreateDefinitionAsset()
        {
            if (File.Exists(DefinitionPath)) return;
            Directory.CreateDirectory(Path.GetDirectoryName(DefinitionPath));
            AssetDatabase.CreateAsset(PuppetDefinition.CreateBrenna(), DefinitionPath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Ironoak/Create Config Asset", priority = 21)]
        public static void CreateConfigAsset()
        {
            if (File.Exists(ConfigPath)) return;
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<IronoakConfig>(), ConfigPath);
            AssetDatabase.SaveAssets();
        }

        public static void RunAll()
        {
            CreateConfigAsset();
            BakePlaceholders();
            CreateDefinitionAsset();
            WriteTemplates();
        }
    }
}
