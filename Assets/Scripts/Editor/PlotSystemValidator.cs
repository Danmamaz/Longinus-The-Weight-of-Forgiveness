using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Longinus.PlotSystem.Editor
{
#if UNITY_EDITOR
    /// <summary>
    /// Provides editor tools to validate the integrity of plot-related data like DecisionNodes.
    /// </summary>
    public static class PlotSystemValidator
    {
        #region State/Core Logic

        /// <summary>
        /// Finds and validates all DecisionNode assets in the project.
        /// Checks for missing IDs, duplicate IDs, and broken boss data references.
        /// </summary>
        [MenuItem("Tools/Longinus/Plot System/Validate All Nodes")]
        public static void ValidateAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:DecisionNode");
            
            HashSet<string> DecisionIDs = new HashSet<string>();
            List<string> errors = new List<string>();
            int checkedNodesCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DecisionNode decision = AssetDatabase.LoadAssetAtPath<DecisionNode>(path);
                
                if (decision == null) continue;
                checkedNodesCount++;

                if (string.IsNullOrWhiteSpace(decision.DecisionID))
                {
                    errors.Add($"[Empty ID] Decision '{decision.name}' at path '{path}'.");
                    continue;
                }
                
                if (!DecisionIDs.Add(decision.DecisionID))
                {
                    errors.Add($"[Duplicate ID] '{decision.DecisionID}' is shared by '{decision.name}'.");
                }
                
                if (decision.IsBossFight)
                {
                    if (decision.LinkedBoss == null)
                    {
                        errors.Add($"[Missing Reference] '{decision.DecisionID}' is marked as Boss Fight but lacks Boss Data!");
                    }
                    else if (string.IsNullOrWhiteSpace(decision.LinkedBoss.BossID))
                    {
                        errors.Add($"[Broken Reference] '{decision.DecisionID}' links to Boss Data '{decision.LinkedBoss.name}' which has an empty Boss ID.");
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

        #endregion
    }
    
    /// <summary>
    /// Custom inspector for DecisionNode to provide quick validation warnings and ID generation.
    /// </summary>
    [CustomEditor(typeof(DecisionNode))]
    public class DecisionNodeEditor : UnityEditor.Editor
    {
        #region Unity Lifecycle

        /// <summary>
        /// Draws the custom inspector GUI, including validation warnings and utility buttons.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            DecisionNode node = (DecisionNode)target;
            if (node == null) return;
            
            EditorGUILayout.Space();

            if (string.IsNullOrWhiteSpace(node.DecisionID))
            {
                EditorGUILayout.HelpBox("CRITICAL: Decision ID is missing!", MessageType.Error);
            }
            
            if (node.IsBossFight && node.LinkedBoss == null)
            {
                EditorGUILayout.HelpBox("WARNING: Boss fight flag is active, but no Boss Data is assigned.", MessageType.Warning);
            }
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Unique ID"))
            {
                Undo.RecordObject(node, "Generate Decision ID");
                node.DecisionID = $"decision_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                EditorUtility.SetDirty(node);
            }
        }

        #endregion
    }
#endif
}