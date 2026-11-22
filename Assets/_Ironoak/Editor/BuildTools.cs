using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ironoak.EditorTools
{
    public static class BuildTools
    {
        const string Output = "Builds/Windows/Ironoak.exe";

        [MenuItem("Ironoak/Build Windows (Dev)", priority = 60)]
        public static void BuildWindowsDev()
        {
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = Output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Ironoak] Build {report.summary.result}: {Output} ({report.summary.totalSize / (1024 * 1024)} MB)");
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
