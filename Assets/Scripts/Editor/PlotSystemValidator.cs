using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

namespace PlotBranching.EditorScripts
{
#if UNITY_EDITOR
    public static class PlotSystemValidator
    {
        [MenuItem("Tools/Plot System/Validate All Nodes")]
        public static void ValidateAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:DecisionNode");
            
            HashSet<string> decisionIDs = new HashSet<string>();
            List<string> errors = new List<string>();
            int checkedNodesCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DecisionNode decision = AssetDatabase.LoadAssetAtPath<DecisionNode>(path);
                
                if (decision == null) continue;
                checkedNodesCount++;

                if (string.IsNullOrWhiteSpace(decision.decisionID))
                {
                    errors.Add($"[Empty ID] Decision '{decision.name}' at path '{path}'.");
                    continue;
                }
                
                if (!decisionIDs.Add(decision.decisionID))
                {
                    errors.Add($"[Duplicate ID] '{decision.decisionID}' is shared by '{decision.name}'.");
                }
                
                if (decision.isBossFight)
                {
                    if (decision.linkedBoss == null)
                    {
                        errors.Add($"[Missing Reference] '{decision.decisionID}' is marked as Boss Fight but lacks Boss Data!");
                    }
                    else if (string.IsNullOrWhiteSpace(decision.linkedBoss.bossID))
                    {
                        errors.Add($"[Broken Reference] '{decision.decisionID}' links to Boss Data '{decision.linkedBoss.name}' which has an empty Boss ID.");
                    }
                }
            }
            
            if (errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Plot System Validation FAILED with {errors.Count} critical errors:");
                foreach (string error in errors)
                {
                    sb.AppendLine($"  - {error}");
                }
                Debug.LogError(sb.ToString());
            }
            else
            {
                Debug.Log($"Plot System Validation PASSED. ({checkedNodesCount} nodes verified).");
            }
        }
    }
    
    [CustomEditor(typeof(DecisionNode))]
    public class DecisionNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            DecisionNode node = (DecisionNode)target;
            if (node == null) return;
            
            EditorGUILayout.Space();

            if (string.IsNullOrWhiteSpace(node.decisionID))
            {
                EditorGUILayout.HelpBox("CRITICAL: Decision ID is missing!", MessageType.Error);
            }
            
            if (node.isBossFight && node.linkedBoss == null)
            {
                EditorGUILayout.HelpBox("WARNING: Boss fight flag is active, but no Boss Data is assigned.", MessageType.Warning);
            }
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Unique ID"))
            {
                Undo.RecordObject(node, "Generate Decision ID");
                node.decisionID = $"decision_{System.Guid.NewGuid().ToString("N").Substring(0, 8)}";
                EditorUtility.SetDirty(node);
            }
        }
    }
#endif
}