using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LonginusEditor
{
    public static class QAScanner
    {
        [MenuItem("Longinus/QA/Run Full Scan")]
        public static void RunFullScan()
        {
            var report = new List<string>();
            report.Add("===== LONGINUS QA REPORT =====");
            report.Add($"Generated: {System.DateTime.Now}");
            report.Add("");

            ScanForDebugLogsInUpdate(report);
            ScanForMissingNullChecks(report);
            ScanForUnreferencedPublicFields(report);
            ScanForHardcodedSceneIndices(report);
            ScanForMissingTagDefinitions(report);
            ScanForEventUnsubscribeMistakes(report);
            ScanForFindObjectInUpdate(report);
            ScanForUnusedSerializedFields(report);

            const string path = "Assets/Documentation/QA_Report.md";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, report);
            AssetDatabase.Refresh();
            Debug.Log($"[QA] Report saved to {path}. Lines written: {report.Count}");
        }

        private static void ScanForDebugLogsInUpdate(List<string> report)
        {
            report.Add("## Debug.Log calls inside Update/FixedUpdate/LateUpdate");
            string[] files = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            var updatePattern = new Regex(
                @"(?:void|private void|protected void|public void)\s+" +
                @"(?:Update|FixedUpdate|LateUpdate)\s*\(\s*\)\s*\{" +
                @"([^}]*(?:\{[^}]*\}[^}]*)*)\}",
                RegexOptions.Singleline);

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                foreach (Match m in updatePattern.Matches(content))
                {
                    if (m.Groups[1].Value.Contains("Debug.Log"))
                        report.Add($"  ❌ {Path.GetFileName(file)}: Debug.Log inside Update/FixedUpdate/LateUpdate");
                }
            }
            report.Add("");
        }

        private static void ScanForMissingNullChecks(List<string> report)
        {
            report.Add("## Potential NullReferenceException — singletons accessed without null check");
            string[] files = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            string[] singletons =
            {
                "PlayerController.Instance", "PlotManager.Instance",
                "SceneController.Instance",  "AudioDirector.Instance",
                "PostProcessingDirector.Instance", "HitImpactPool.Instance"
            };

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                string[] lines = content.Split('\n');

                foreach (string singleton in singletons)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (!line.Contains(singleton + ".")) continue;
                        if (line.Contains($"{singleton} ==") ||
                            line.Contains($"{singleton} !=") ||
                            line.Contains("?.")) continue;

                        bool hasGuard = false;
                        for (int j = System.Math.Max(0, i - 5); j < i; j++)
                        {
                            if (lines[j].Contains($"{singleton} ==") ||
                                lines[j].Contains($"{singleton} !="))
                            {
                                hasGuard = true;
                                break;
                            }
                        }

                        if (!hasGuard)
                            report.Add($"  ⚠️ {Path.GetFileName(file)}:{i + 1} — {singleton} accessed without null check");
                    }
                }
            }
            report.Add("");
        }

        private static void ScanForUnreferencedPublicFields(List<string> report)
        {
            report.Add("## Public fields that might be unintentionally exposed (consider [SerializeField] private)");
            string[] files = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            var pattern = new Regex(
                @"^\s+public\s+(?!class|struct|enum|interface|static|abstract|virtual|override|new)" +
                @"([\w\.<>,\[\]]+)\s+(\w+)\s*;",
                RegexOptions.Multiline);

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                foreach (Match m in pattern.Matches(content))
                {
                    string typeName  = m.Groups[1].Value;
                    string fieldName = m.Groups[2].Value;
                    if (!typeName.Contains("event") && !typeName.Contains("delegate"))
                        report.Add($"  ⚠️ {Path.GetFileName(file)}: public {typeName} {fieldName} — consider [SerializeField] private");
                }
            }
            report.Add("");
        }

        private static void ScanForHardcodedSceneIndices(List<string> report)
        {
            report.Add("## Hardcoded scene indices (use enum or named constants)");
            string[] files = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            var pattern = new Regex(@"SceneManager\.LoadScene\s*\(\s*(\d+)\s*\)");

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                foreach (Match m in pattern.Matches(content))
                    report.Add($"  ⚠️ {Path.GetFileName(file)}: SceneManager.LoadScene({m.Groups[1].Value}) — use serialized field or named const");
            }
            report.Add("");
        }

        private static void ScanForMissingTagDefinitions(List<string> report)
        {
            report.Add("## CompareTag usages — verify these tags exist in Tag Manager");
            string[] files = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            var foundTags = new HashSet<string>();
            var pattern   = new Regex(@"CompareTag\s*\(\s*""(\w+)""\s*\)");

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                foreach (Match m in pattern.Matches(content))
                    foundTags.Add(m.Groups[1].Value);
            }

            foreach (string tag in foundTags)
                report.Add($"  ✓ Tag used: {tag}");

            report.Add("  → Manually verify in Edit → Project Settings → Tags and Layers");
            report.Add("");
        }

        private static void ScanForEventUnsubscribeMistakes(List<string> report)
        {
            report.Add("## Event subscriptions without matching unsubscribe");
            string[] files = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            var subscribePattern   = new Regex(@"(\w+(?:\.\w+)*)\s*\+=\s*(\w+)");
            var unsubscribePattern = new Regex(@"(\w+(?:\.\w+)*)\s*-=\s*(\w+)");

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                var subs   = new HashSet<string>();
                var unsubs = new HashSet<string>();

                foreach (Match m in subscribePattern.Matches(content))
                    subs.Add($"{m.Groups[1].Value}::{m.Groups[2].Value}");

                foreach (Match m in unsubscribePattern.Matches(content))
                    unsubs.Add($"{m.Groups[1].Value}::{m.Groups[2].Value}");

                foreach (string sub in subs)
                {
                    if (!unsubs.Contains(sub))
                        report.Add($"  ⚠️ {Path.GetFileName(file)}: {sub} subscribed but never unsubscribed");
                }
            }
            report.Add("");
        }

        private static void ScanForFindObjectInUpdate(List<string> report)
        {
            report.Add("## FindObjectOfType / GameObject.Find in Update (performance hit)");
            string[] expensive = { "FindObjectOfType", "FindObjectsOfType", "FindFirstObjectByType", "GameObject.Find" };
            string[] files     = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            var updatePattern  = new Regex(
                @"void\s+(?:Update|FixedUpdate|LateUpdate)\s*\(\s*\)\s*\{" +
                @"([^}]*(?:\{[^}]*\}[^}]*)*)\}",
                RegexOptions.Singleline);

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                foreach (Match m in updatePattern.Matches(content))
                {
                    foreach (string call in expensive)
                    {
                        if (m.Groups[1].Value.Contains(call))
                            report.Add($"  ❌ {Path.GetFileName(file)}: {call} inside Update loop");
                    }
                }
            }
            report.Add("");
        }

        private static void ScanForUnusedSerializedFields(List<string> report)
        {
            report.Add("## [SerializeField] fields that may be unused (heuristic — verify manually)");
            string[] files  = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            var pattern = new Regex(
                @"\[SerializeField\][^\n]*?\s+(?:private\s+)?[\w\.<>,\[\]]+\s+(_\w+)\s*[;=]",
                RegexOptions.Multiline);

            foreach (string file in files)
            {
                string content = File.ReadAllText(file);
                foreach (Match m in pattern.Matches(content))
                {
                    string fieldName = m.Groups[1].Value;
                    int count = Regex.Matches(content, @"\b" + Regex.Escape(fieldName) + @"\b").Count;
                    if (count == 1)
                        report.Add($"  ⚠️ {Path.GetFileName(file)}: {fieldName} declared but never referenced");
                }
            }
            report.Add("");
        }
    }
}
