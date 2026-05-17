using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Longinus.PlotSystem;

namespace LonginusEditor
{
    public class PreBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Force reset PlotState before every build to avoid
            // editor-state corruption shipping in builds
            string[] plotStates = AssetDatabase.FindAssets("t:PlotState");
            foreach (string guid in plotStates)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var ps = AssetDatabase.LoadAssetAtPath<PlotState>(path);
                if (ps != null)
                {
                    ps.ResetState();
                    EditorUtility.SetDirty(ps);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[PreBuild] PlotState assets reset to clean state");

            // Verify Player tag exists
            var tags = UnityEditorInternal.InternalEditorUtility.tags;
            bool hasPlayerTag = false;
            foreach (string t in tags)
                if (t == "Player") hasPlayerTag = true;

            if (!hasPlayerTag)
                throw new BuildFailedException("Player tag must exist in Tag Manager");
        }
    }
}
