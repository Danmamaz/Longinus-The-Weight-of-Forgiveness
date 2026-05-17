using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LonginusEditor
{
    public static class VerticalSliceBuilder
    {
        private const string BUILD_VERSION    = "1.0.0-vertical-slice";
        private const string BUILD_OUTPUT_DIR = "Builds/VerticalSlice";
        private const string BUILD_EXE_NAME   = "Longinus.exe";

        private static readonly string[] REQUIRED_SCENES =
        {
            "Assets/Scenes/Main Menu/Main Menu.unity",
            "Assets/Scenes/Introduction Chapter/Introduction Chapter.unity",
            "Assets/Scenes/Beach/Beach.unity",
            "Assets/Scenes/End Demo/End Demo.unity",
        };

        [MenuItem("Longinus/Build/Vertical Slice — Full Pipeline")]
        public static void BuildVerticalSlice()
        {
            Debug.Log("===== VERTICAL SLICE BUILD START =====");

            if (!PreBuildValidation())
            {
                EditorUtility.DisplayDialog("Build Failed",
                    "Pre-build validation failed. See Console.", "OK");
                return;
            }

            if (!ConfigureBuildSettings())
                return;

            if (!RunFinalQACheck())
            {
                bool proceed = EditorUtility.DisplayDialog("QA Warnings",
                    "QA scan found warnings. Build anyway?", "Build", "Cancel");
                if (!proceed) return;
            }

            BuildReport report = ExecuteBuild();
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[Build] FAILED: {report.summary.result}");
                return;
            }

            PostBuildSteps(report);

            Debug.Log("===== VERTICAL SLICE BUILD COMPLETE =====");
            EditorUtility.RevealInFinder(Path.Combine(BUILD_OUTPUT_DIR, BUILD_EXE_NAME));
        }

        private static bool PreBuildValidation()
        {
            Debug.Log("[Build] Validating scenes...");
            foreach (string scenePath in REQUIRED_SCENES)
            {
                if (!File.Exists(scenePath))
                {
                    Debug.LogError($"[Build] Required scene missing: {scenePath}");
                    return false;
                }
            }

            Debug.Log("[Build] Validating PlotState assets...");
            string[] plotStates = AssetDatabase.FindAssets("t:PlotState");
            if (plotStates.Length == 0)
            {
                Debug.LogError("[Build] No PlotState assets found");
                return false;
            }

            Debug.Log("[Build] Validating branch registry...");
            string[] registries = AssetDatabase.FindAssets("t:PlotBranchRegistry");
            if (registries.Length == 0)
            {
                Debug.LogError("[Build] No PlotBranchRegistry asset found");
                return false;
            }

            Debug.Log("[Build] Pre-build validation passed ✓");
            return true;
        }

        private static bool ConfigureBuildSettings()
        {
            Debug.Log("[Build] Configuring build settings...");

            var editorScenes = new List<EditorBuildSettingsScene>();
            foreach (string path in REQUIRED_SCENES)
                editorScenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = editorScenes.ToArray();

            PlayerSettings.bundleVersion = BUILD_VERSION;
            PlayerSettings.productName   = "Longinus";
            PlayerSettings.companyName   = "Longinus Studios";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,
                ScriptingImplementation.Mono2x);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone,
                ApiCompatibilityLevel.NET_Standard_2_0);

            EditorUserBuildSettings.development      = false;
            EditorUserBuildSettings.allowDebugging   = false;
            EditorUserBuildSettings.connectProfiler  = false;

            Debug.Log("[Build] Build settings configured ✓");
            return true;
        }

        private static bool RunFinalQACheck()
        {
            bool hasCritical = false;
            string[] files = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                if (content.Contains("TODO_BLOCKING") || content.Contains("FIXME_BLOCKING"))
                {
                    Debug.LogError($"[Build QA] Blocking marker in {file}");
                    hasCritical = true;
                }
            }
            return !hasCritical;
        }

        private static BuildReport ExecuteBuild()
        {
            Debug.Log("[Build] Compiling...");
            Directory.CreateDirectory(BUILD_OUTPUT_DIR);

            var options = new BuildPlayerOptions
            {
                scenes           = REQUIRED_SCENES,
                locationPathName = Path.Combine(BUILD_OUTPUT_DIR, BUILD_EXE_NAME),
                target           = BuildTarget.StandaloneWindows64,
                targetGroup      = BuildTargetGroup.Standalone,
                options          = BuildOptions.None,
            };

            return BuildPipeline.BuildPlayer(options);
        }

        private static void PostBuildSteps(BuildReport report)
        {
            GenerateBuildManifest(report);
            GenerateReleaseNotes(report);
            CopyDocumentation();

            long mb = (long)(report.summary.totalSize / 1024 / 1024);
            Debug.Log($"[Build] Size: {mb} MB, Time: {report.summary.totalTime}");
        }

        private static void GenerateBuildManifest(BuildReport report)
        {
            string manifestPath = Path.Combine(BUILD_OUTPUT_DIR, "BuildManifest.txt");
            var lines = new List<string>
            {
                "===== LONGINUS BUILD MANIFEST =====",
                $"Version: {BUILD_VERSION}",
                $"Build Date: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Unity Version: {Application.unityVersion}",
                $"Platform: {report.summary.platform}",
                $"Size: {report.summary.totalSize / 1024 / 1024} MB",
                $"Build Time: {report.summary.totalTime}",
                $"Result: {report.summary.result}",
                "",
                "===== INCLUDED SCENES =====",
            };
            foreach (string s in REQUIRED_SCENES)
                lines.Add($"  - {Path.GetFileName(s)}");

            File.WriteAllLines(manifestPath, lines);
            Debug.Log($"[Build] Manifest written: {manifestPath}");
        }

        private static void GenerateReleaseNotes(BuildReport report)
        {
            string notesPath = Path.Combine(BUILD_OUTPUT_DIR, "ReleaseNotes.md");
            var notes = new List<string>
            {
                "# Longinus — Vertical Slice Release Notes",
                $"**Version**: {BUILD_VERSION}  ",
                $"**Date**: {System.DateTime.Now:yyyy-MM-dd}",
                "",
                "## What's Included",
                "",
                "### Combat System",
                "- Player melee combo system with stamina management",
                "- Roll/dodge with i-frames",
                "- Lock-on targeting with Q/E switching",
                "- Custom camera orbit during lock-on",
                "",
                "### Enemies",
                "- Melee enemy with state machine (Idle, Chase, Attack, Patrol, Search, Strafe)",
                "- Ranged enemy with kiting and projectiles",
                "- Boss encounter with 2-phase progression",
                "- Per-bone damage colliders on boss attacks",
                "",
                "### Plot System",
                "- 12 plot branches (BR-01 through BR-12)",
                "- Major branches with environmental consequences",
                "- Save/load with full PlotState serialization",
                "- BR-01 (Boss Defeated) triggers arena env swap",
                "",
                "### Visuals",
                "- URP post-processing with 6 presets (auto-switched by game state)",
                "- Dissolve shader on enemy death",
                "- Foliage bend reacts to player position",
                "- Boss Phase 2 tint effect",
                "- Procedural particles: hit impacts, fog, ambient dust",
                "- Programmatic lighting presets per scene",
                "",
                "### Audio",
                "- Music director with smooth crossfades",
                "- Auto-wired SFX for combat, checkpoints, UI",
                "- Ambient audio per scene",
                "",
                "## Known Issues",
                "- (none blocking)",
                "",
                "## Controls",
                "- **WASD**: Move",
                "- **Shift**: Roll/Dodge",
                "- **LMB**: Attack",
                "- **F**: Interact",
                "- **MMB**: Toggle Lock-On",
                "- **Q/E**: Switch Lock-On Target",
                "- **Esc**: Pause Menu",
                "",
                "## Credits",
                "Diploma project — Longinus Studios",
            };

            File.WriteAllLines(notesPath, notes);
            Debug.Log($"[Build] Release notes written: {notesPath}");
        }

        private static void CopyDocumentation()
        {
            string docsSource = "Assets/Documentation";
            string docsTarget = Path.Combine(BUILD_OUTPUT_DIR, "Documentation");
            if (!Directory.Exists(docsSource)) return;
            Directory.CreateDirectory(docsTarget);
            foreach (string file in Directory.GetFiles(docsSource))
            {
                string dest = Path.Combine(docsTarget, Path.GetFileName(file));
                File.Copy(file, dest, overwrite: true);
            }
            Debug.Log("[Build] Documentation copied to build folder");
        }
    }
}
