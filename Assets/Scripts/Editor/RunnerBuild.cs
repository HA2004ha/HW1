using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// Entry points for the whole pipeline. Each stage is idempotent and reads its inputs from disk,
    /// so "Setup Project" on a fresh clone produces the same game as the committed assets.
    /// </summary>
    public static class RunnerBuild
    {
        private const string ScenePath = "Assets/Scenes/Game.unity";
        private const string PlayerPath = "Build/Linux/NeonRunner";

        [MenuItem("Runner/Setup Project (everything)")]
        public static void Setup()
        {
            ApplyProjectSettings();
            GenerateArt();
            GenerateAnimations();
            GeneratePrefabs();
            BuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Runner pipeline finished. Open " + ScenePath + " and press Play.");
        }

        [MenuItem("Runner/Stage 1 - Generate Art")]
        public static void GenerateArt()
        {
            RunnerEnvironmentArt.Build();
            RunnerCharacterArt.Build();
            RunnerPropArt.Build();
            AssetDatabase.Refresh();
        }

        [MenuItem("Runner/Stage 2 - Generate Animations")]
        public static void GenerateAnimations()
        {
            RunnerCharacterArt.Build();
            RunnerAnimations.Build();
            AssetDatabase.Refresh();
        }

        [MenuItem("Runner/Stage 3 - Generate Prefabs")]
        public static void GeneratePrefabs()
        {
            RunnerPropArt.Build();
            RunnerPrefabs.Build();
            AssetDatabase.Refresh();
        }

        [MenuItem("Runner/Stage 4 - Build Scene")]
        public static void BuildScene()
        {
            RunnerCharacterArt.Build();
            RunnerPropArt.Build();
            RunnerEnvironmentArt.Build();
            RunnerAnimations.Build();
            RunnerPrefabs.Build();
            RunnerAssets.Invalidate();
            RunnerSceneBuilder.Build();

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Runner/Stage 5 - Build Linux Player")]
        public static void BuildPlayer()
        {
            ApplyProjectSettings();

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = PlayerPath,
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var result = report.summary.result;
            Debug.Log($"Player build: {result} -> {report.summary.outputPath}");
            if (result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError(report.summary.result + "; see the build log for errors.");
            }
        }

        private static void ApplyProjectSettings()
        {
            PlayerSettings.companyName = "IUST";
            PlayerSettings.productName = "Neon Runner";
            PlayerSettings.runInBackground = true;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        // Batch mode hooks: Unity -batchmode -executeMethod Runner.EditorTools.RunnerBuild.SetupFromBatch
        public static void SetupFromBatch()
        {
            RunInBatch(Setup);
        }

        public static void BuildFromBatch()
        {
            RunInBatch(BuildPlayer);
        }

        private static void RunInBatch(Action stage)
        {
            try
            {
                stage();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }
    }
}
