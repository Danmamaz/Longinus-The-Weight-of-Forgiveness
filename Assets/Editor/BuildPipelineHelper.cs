using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Editor utility: Longinus → Build
/// One-click Build #3 pipeline and scene-order verification.
/// </summary>
public static class BuildPipelineHelper
{
    private const string BUILD_VERSION    = "0.3.0-week3-build3";
    private const string BUILD_OUTPUT_DIR = "Builds/Build3";
    private const string BUILD_EXE_NAME   = "Longinus.exe";

    // Expected scene order in Build Settings (must match exactly)
    private static readonly string[] EXPECTED_SCENE_NAMES =
    {
        "Main Menu",            // index 0
        "Introduction Chapter", // index 1
        "Beach",                // index 2
        "End Demo",             // index 3
    };

    // ── Menu Items ───────────────────────────────────────────────────────────

    [MenuItem("Longinus/Build/Build #3 PC Player")]
    public static void BuildBuild3PC()
    {
        if (!VerifySceneOrderInternal())
        {
            Debug.LogError("[BuildPipelineHelper] Scene order check failed. Fix Build Settings before building.");
            return;
        }

        string[] scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        PlayerSettings.bundleVersion = BUILD_VERSION;

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenePaths,
            locationPathName = $"{BUILD_OUTPUT_DIR}/{BUILD_EXE_NAME}",
            target           = BuildTarget.StandaloneWindows64,
            options          = BuildOptions.None,
        });

        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"[BuildPipelineHelper] Build #3 succeeded → {BUILD_OUTPUT_DIR}/{BUILD_EXE_NAME}  " +
                      $"({report.summary.totalSize / 1024 / 1024} MB)");
        else
            Debug.LogError($"[BuildPipelineHelper] Build #3 FAILED with {report.summary.totalErrors} error(s). " +
                           "Check the Build Report window.");
    }

    [MenuItem("Longinus/Build/Verify Scene Order")]
    public static void VerifySceneOrder()
    {
        bool ok = VerifySceneOrderInternal();
        if (ok)
            Debug.Log("[BuildPipelineHelper] Scene order verified — all scenes present and in correct order.");
    }

    // ── Internal Helpers ─────────────────────────────────────────────────────

    private static bool VerifySceneOrderInternal()
    {
        var enabledScenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .ToArray();

        bool allGood = true;

        if (enabledScenes.Length < EXPECTED_SCENE_NAMES.Length)
        {
            Debug.LogError($"[BuildPipelineHelper] Expected {EXPECTED_SCENE_NAMES.Length} enabled scenes " +
                           $"but found {enabledScenes.Length}.");
            allGood = false;
        }

        for (int i = 0; i < EXPECTED_SCENE_NAMES.Length && i < enabledScenes.Length; i++)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(enabledScenes[i].path);
            if (sceneName != EXPECTED_SCENE_NAMES[i])
            {
                Debug.LogError($"[BuildPipelineHelper] Scene index {i}: expected '{EXPECTED_SCENE_NAMES[i]}' " +
                               $"but found '{sceneName}'.");
                allGood = false;
            }
        }

        return allGood;
    }
}
