using System.IO;
using Ironoak.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// Project bootstrap: bakes art, generates prefabs, builds the greybox level, creates the
    /// Main scene (a single IronoakGame object) and registers scenes in Build Settings.
    /// Runs automatically the first time the project is opened; also on the Ironoak menu.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        const string SceneDir = "Assets/_Ironoak/Scenes";
        public const string MainScenePath = SceneDir + "/Main.unity";
        const string SessionKey = "Ironoak.SetupChecked";

        static ProjectSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        /// <summary>Command line: -executeMethod Ironoak.EditorTools.ProjectSetup.BatchSetup</summary>
        public static void BatchSetup()
        {
            ApplyProjectSettings();
            RunPipeline(rebuildLevel: true);
            Debug.Log("[Ironoak] Batch setup complete.");
        }

        static void AutoSetup()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (!File.Exists(MainScenePath) || !File.Exists(Level1Blueprint.ScenePath))
            {
                ApplyProjectSettings();
                RunPipeline(rebuildLevel: !File.Exists(Level1Blueprint.ScenePath));
                EditorSceneManager.OpenScene(MainScenePath);
                Debug.Log("[Ironoak] Project set up. Press Play in the Main scene.");
            }
            EnsureBuildSettings();
        }

        static void RunPipeline(bool rebuildLevel)
        {
            ArtBaker.BakeMissing();
            CharacterTools.RunAll();
            PrefabFactory.BuildAll();
            if (rebuildLevel) Level1Blueprint.Build();
            CreateMainScene();
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Ironoak/Open Main Scene", priority = 0)]
        static void OpenMainScene()
        {
            if (!File.Exists(MainScenePath)) CreateMainScene();
            else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(MainScenePath);
        }

        [MenuItem("Ironoak/Open Level 1 Scene", priority = 1)]
        static void OpenLevel1()
        {
            if (File.Exists(Level1Blueprint.ScenePath) && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(Level1Blueprint.ScenePath);
        }

        [MenuItem("Ironoak/Apply Project Settings", priority = 20)]
        static void ApplyProjectSettings()
        {
            PlayerSettings.productName = "Ironoak";
            PlayerSettings.companyName = "Ironoak";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            RenderSetup.Apply();
        }

        [MenuItem("Ironoak/Reset Best Score", priority = 40)]
        static void ResetBest()
        {
            PlayerPrefs.DeleteKey("ironoak.best.coins");
            PlayerPrefs.DeleteKey("ironoak.best.time");
            PlayerPrefs.Save();
        }

        static void CreateMainScene()
        {
            Directory.CreateDirectory(SceneDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("IronoakGame").AddComponent<IronoakGame>();
            EditorSceneManager.SaveScene(scene, MainScenePath);
            AssetDatabase.Refresh();
        }

        static void EnsureBuildSettings()
        {
            if (!File.Exists(MainScenePath)) return;
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true),
                new EditorBuildSettingsScene(Level1Blueprint.ScenePath, File.Exists(Level1Blueprint.ScenePath)),
            };
        }
    }
}
