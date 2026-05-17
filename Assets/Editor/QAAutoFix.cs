using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LonginusEditor
{
    public static class QAAutoFix
    {
        [MenuItem("Longinus/QA/Auto-Fix Safe Issues")]
        public static void RunAutoFix()
        {
            int fixCount = 0;
            string[] files = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string original = File.ReadAllText(file);
                string patched  = original;

                patched = AnnotateHardcodedSceneIndices(patched);
                patched = RemoveDebugLogsFromUpdateMethods(patched);

                if (patched != original)
                {
                    File.WriteAllText(file, patched);
                    fixCount++;
                    Debug.Log($"[QAAutoFix] Patched: {Path.GetFileName(file)}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[QAAutoFix] Auto-fix complete. Files modified: {fixCount}");
            EditorUtility.DisplayDialog(
                "QA Auto-Fix Complete",
                $"Modified {fixCount} file(s).\n\nFix 1: Annotated hardcoded SceneManager.LoadScene(N) calls.\nFix 2: Removed Debug.Log calls from Update/FixedUpdate/LateUpdate.\n\nFix 3 (singleton null-checks) skipped — too risky for automated replacement.",
                "OK");
        }

        // Appends a TODO comment after SceneManager.LoadScene(<integer>) calls so they are
        // easy to find and replace with named constants.
        private static string AnnotateHardcodedSceneIndices(string source)
        {
            var pattern = new Regex(
                @"(SceneManager\.LoadScene\s*\(\s*\d+\s*\))(?!\s*/\*\s*TODO)");

            return pattern.Replace(source,
                "$1 /* TODO: replace with named constant */");
        }

        // Removes bare Debug.Log(...) statement lines from Update/FixedUpdate/LateUpdate bodies.
        // Only matches single-line calls (no block-spanning statements). Leaves LogWarning/LogError
        // intact — those are intentional diagnostic signals, not debug noise.
        private static string RemoveDebugLogsFromUpdateMethods(string source)
        {
            var updateBodyPattern = new Regex(
                @"((?:void|private void|protected void|public void)\s+" +
                @"(?:Update|FixedUpdate|LateUpdate)\s*\(\s*\)\s*\{)" +
                @"((?:[^}]|\{[^}]*\})*)\}",
                RegexOptions.Singleline);

            return updateBodyPattern.Replace(source, match =>
            {
                string header = match.Groups[1].Value;
                string body   = match.Groups[2].Value;

                // Remove lines that are purely a Debug.Log call (not LogWarning/LogError)
                string cleaned = Regex.Replace(
                    body,
                    @"[ \t]*Debug\.Log\s*\([^;]*\)\s*;\s*\r?\n",
                    string.Empty);

                return header + cleaned + "}";
            });
        }
    }
}
