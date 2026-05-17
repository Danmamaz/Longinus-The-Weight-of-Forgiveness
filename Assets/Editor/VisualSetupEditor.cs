using UnityEngine;
using UnityEditor;
using System.IO;

namespace Longinus.Editor
{
    public static class VisualSetupEditor
    {
        [MenuItem("Longinus/Visuals/Create Dissolve Material")]
        private static void CreateDissolveMaterial()
        {
            Shader shader = Shader.Find("Longinus/Dissolve");
            if (shader == null)
            {
                Debug.LogError("[VisualSetup] Shader 'Longinus/Dissolve' not found. " +
                               "Ensure Assets/Shaders/Dissolve.shader has compiled without errors.");
                return;
            }

            Directory.CreateDirectory("Assets/Materials");

            var mat = new Material(shader);
            mat.SetColor("_EdgeColor", new Color(1f, 0.267f, 0f, 1f));   // #FF4400
            mat.SetFloat("_DissolveAmount", 0f);
            mat.SetFloat("_EdgeWidth", 0.05f);
            mat.SetFloat("_NoiseScale", 5f);
            mat.enableInstancing = true;

            AssetDatabase.CreateAsset(mat, "Assets/Materials/M_Dissolve.mat");
            AssetDatabase.SaveAssets();
            Debug.Log("[VisualSetup] Created Assets/Materials/M_Dissolve.mat");
        }

        [MenuItem("Longinus/Visuals/Create FoliageBend Material")]
        private static void CreateFoliageBendMaterial()
        {
            Shader shader = Shader.Find("Longinus/FoliageBend");
            if (shader == null)
            {
                Debug.LogError("[VisualSetup] Shader 'Longinus/FoliageBend' not found. " +
                               "Ensure Assets/Shaders/FoliageBend.shader has compiled without errors.");
                return;
            }

            Directory.CreateDirectory("Assets/Materials");

            var mat = new Material(shader);
            mat.SetFloat("_AlphaCutoff", 0.4f);
            mat.SetFloat("_WindStrength", 0.3f);
            mat.SetFloat("_WindSpeed", 1.5f);
            mat.SetFloat("_PlayerBendRadius", 3.0f);
            mat.SetFloat("_PlayerBendStrength", 0.5f);
            mat.enableInstancing = true;

            AssetDatabase.CreateAsset(mat, "Assets/Materials/M_FoliageBend.mat");
            AssetDatabase.SaveAssets();
            Debug.Log("[VisualSetup] Created Assets/Materials/M_FoliageBend.mat");
        }
    }
}
