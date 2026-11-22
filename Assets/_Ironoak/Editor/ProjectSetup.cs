using System.IO;
using Ironoak.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// One-time project bootstrap: creates the Main scene (a single IronoakGame object),
    /// registers it in Build Settings and applies player settings. Also available from
    /// the "Ironoak" menu.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        /// <summary>Entry point for command-line setup: -executeMethod Ironoak.EditorTools.ProjectSetup.BatchSetup</summary>
        public static void BatchSetup()
        {
            ApplyProjectSettings();
            Directory.CreateDirectory(SceneDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("IronoakGame").AddComponent<IronoakGame>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[Ironoak] Batch setup complete.");
        }

        const string SceneDir = "Assets/_Ironoak/Scenes";
        const string ScenePath = SceneDir + "/Main.unity";
        const string SessionKey = "Ironoak.SetupChecked";

        static ProjectSetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        static void AutoSetup()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (!File.Exists(ScenePath))
            {
                ApplyProjectSettings();
                CreateMainScene();
                Debug.Log("[Ironoak] Created Main scene. Press Play to run the game.");
            }
            EnsureBuildSettings();
        }

        [MenuItem("Ironoak/Open Main Scene", priority = 0)]
        static void OpenMainScene()
        {
            if (!File.Exists(ScenePath)) CreateMainScene();
            else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Ironoak/Rebuild Main Scene", priority = 1)]
        static void RebuildMainScene()
        {
            CreateMainScene();
            EnsureBuildSettings();
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
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(SceneDir);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("IronoakGame").AddComponent<IronoakGame>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
        }

        static void EnsureBuildSettings()
        {
            if (!File.Exists(ScenePath)) return;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
