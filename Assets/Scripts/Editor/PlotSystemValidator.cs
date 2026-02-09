using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace PlotBranching
{
#if UNITY_EDITOR
    /// <summary>
    /// Validates all decision nodes for typos and broken references
    /// </summary>
    public class PlotSystemValidator : EditorWindow
    {
        [MenuItem("Tools/Plot System/Validate All Nodes")]
        public static void ValidateAll()
        {
            // Find all DecisionNode assets in the project (even if not in scene)
            var allDecisions = Resources.FindObjectsOfTypeAll<DecisionNode>();
            
            HashSet<string> decisionIDs = new HashSet<string>();
            List<string> errors = new List<string>();
            
            // Check for duplicate IDs
            foreach (var decision in allDecisions)
            {
                // Skip internal Unity assets or temporary objects
                if (decision == null) continue;
                if (AssetDatabase.Contains(decision) == false) continue; 

                // 1. Check ID is present
                if (string.IsNullOrEmpty(decision.decisionID))
                {
                    errors.Add($"Decision '{decision.name}' has empty ID");
                    continue;
                }
                
                // 2. Check for Duplicates
                if (decisionIDs.Contains(decision.decisionID))
                {
                    errors.Add($"Duplicate decision ID: '{decision.decisionID}' found in '{decision.name}'");
                }
                else
                {
                    decisionIDs.Add(decision.decisionID);
                }
                
                // 3. Validate Boss Links
                // We now check the object reference (linkedBoss) instead of the old string (bossID)
                if (decision.isBossFight)
                {
                    if (decision.linkedBoss == null)
                    {
                        errors.Add($"Decision '{decision.decisionID}' is marked as Boss Fight but has no Boss Data assigned!");
                    }
                    else if (string.IsNullOrEmpty(decision.linkedBoss.bossID))
                    {
                        errors.Add($"Decision '{decision.decisionID}' links to Boss Data '{decision.linkedBoss.name}' which has an EMPTY Boss ID!");
                    }
                }
            }
            
            // Report results
            if (errors.Count > 0)
            {
                Debug.LogError($"Plot System Validation FAILED with {errors.Count} errors:");
                foreach (var error in errors)
                {
                    Debug.LogError($"  • {error}");
                }
            }
            else
            {
                Debug.Log($"✓ Plot System Validation PASSED ({allDecisions.Length} decisions checked)");
            }
        }
    }
    
    /// <summary>
    /// Custom inspector for DecisionNode with validation warnings
    /// </summary>
    [CustomEditor(typeof(DecisionNode))]
    public class DecisionNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            DecisionNode node = (DecisionNode)target;
            
            // Real-time validation
            if (string.IsNullOrEmpty(node.decisionID))
            {
                EditorGUILayout.HelpBox("Decision ID is required!", MessageType.Error);
            }
            
            // Updated check: Look for the linked object, not the string
            if (node.isBossFight && node.linkedBoss == null)
            {
                EditorGUILayout.HelpBox("Boss fight marked: Please assign a Boss Data object!", MessageType.Warning);
            }
            
            if (GUILayout.Button("Generate Unique ID"))
            {
                node.decisionID = $"decision_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
                EditorUtility.SetDirty(node);
            }
        }
    }
#endif
}