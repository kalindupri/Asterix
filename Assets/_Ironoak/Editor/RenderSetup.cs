using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// Creates and activates the URP pipeline with the 2D Renderer (2D lights, post-processing)
    /// for every quality level. Safe to run repeatedly: existing assets are reused.
    /// </summary>
    public static class RenderSetup
    {
        const string Dir = "Assets/_Ironoak/Settings";
        const string RendererPath = Dir + "/Ironoak_Renderer2D.asset";
        const string PipelinePath = Dir + "/Ironoak_URP.asset";

        [MenuItem("Ironoak/Apply Render Pipeline (URP 2D)", priority = 21)]
        public static void Apply()
        {
            Directory.CreateDirectory(Dir);

            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(RendererPath);
            if (renderer == null) renderer = CreateRenderer2D(RendererPath);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            // Painted art: HDR for bloom, no MSAA (sprites are already anti-aliased), full render scale.
            pipeline.supportsHDR = true;
            pipeline.msaaSampleCount = 1;
            pipeline.renderScale = 1f;
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(current, false);

            AssetDatabase.SaveAssets();
            Debug.Log("[Ironoak] URP 2D pipeline active: " + PipelinePath);
        }

        /// <summary>
        /// Uses URP's own (internal) creation path so all default shaders, materials and
        /// post-processing resources are filled in exactly as the Create menu does.
        /// </summary>
        static Renderer2DData CreateRenderer2D(string path)
        {
            var menus = Type.GetType("UnityEditor.Rendering.Universal.Renderer2DMenus, Unity.RenderPipelines.Universal.Editor");
            var create = menus?.GetMethod("CreateRendererAsset", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (create != null)
            {
                var data = create.Invoke(null, new object[] { path, RendererType._2DRenderer, false, "Renderer" }) as Renderer2DData;
                if (data != null) return data;
            }

            Debug.LogWarning("[Ironoak] URP internal 2D renderer factory not found; creating a bare Renderer2DData.");
            var fallback = ScriptableObject.CreateInstance<Renderer2DData>();
            AssetDatabase.CreateAsset(fallback, path);
            return fallback;
        }
    }
}
