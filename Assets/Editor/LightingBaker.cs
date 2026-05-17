using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace LonginusEditor
{
    public static class LightingBaker
    {
        private struct LightingPreset
        {
            public AmbientMode ambientMode;
            public Color       ambientSky;
            public Color       ambientEquator;
            public Color       ambientGround;
            public float       ambientIntensity;
            public bool        fog;
            public FogMode     fogMode;
            public Color       fogColor;
            public float       fogDensity;
            public float       sunIntensity;
            public Color       sunColor;
            public Vector3     sunRotation;
            public int         lightmapResolution;
        }

        private static readonly Dictionary<string, LightingPreset> PRESETS =
            new Dictionary<string, LightingPreset>
        {
            ["Main Menu"] = new LightingPreset
            {
                ambientMode       = AmbientMode.Trilight,
                ambientSky        = new Color(0.1f,  0.05f, 0.15f, 1f),
                ambientEquator    = new Color(0.05f, 0.05f, 0.1f,  1f),
                ambientGround     = new Color(0.02f, 0.02f, 0.02f, 1f),
                ambientIntensity  = 0.8f,
                fog               = true,
                fogMode           = FogMode.ExponentialSquared,
                fogColor          = new Color(0.1f, 0.05f, 0.15f, 1f),
                fogDensity        = 0.04f,
                sunIntensity      = 0.5f,
                sunColor          = new Color(0.4f, 0.3f, 0.6f, 1f),
                sunRotation       = new Vector3(45f, 30f, 0f),
                lightmapResolution = 20
            },
            ["Introduction Chapter"] = new LightingPreset
            {
                ambientMode       = AmbientMode.Trilight,
                ambientSky        = new Color(0.5f,  0.6f,  0.75f, 1f),
                ambientEquator    = new Color(0.4f,  0.4f,  0.45f, 1f),
                ambientGround     = new Color(0.15f, 0.12f, 0.1f,  1f),
                ambientIntensity  = 1.2f,
                fog               = true,
                fogMode           = FogMode.ExponentialSquared,
                fogColor          = new Color(0.7f, 0.75f, 0.8f, 1f),
                fogDensity        = 0.012f,
                sunIntensity      = 1.2f,
                sunColor          = new Color(1f, 0.95f, 0.85f, 1f),
                sunRotation       = new Vector3(50f, -30f, 0f),
                lightmapResolution = 40
            },
            ["Beach"] = new LightingPreset
            {
                ambientMode       = AmbientMode.Trilight,
                ambientSky        = new Color(0.3f,  0.35f, 0.45f, 1f),
                ambientEquator    = new Color(0.25f, 0.22f, 0.2f,  1f),
                ambientGround     = new Color(0.1f,  0.08f, 0.07f, 1f),
                ambientIntensity  = 0.9f,
                fog               = true,
                fogMode           = FogMode.ExponentialSquared,
                fogColor          = new Color(0.4f, 0.35f, 0.4f, 1f),
                fogDensity        = 0.025f,
                sunIntensity      = 0.8f,
                sunColor          = new Color(0.9f, 0.6f, 0.5f, 1f),
                sunRotation       = new Vector3(15f, 50f, 0f),
                lightmapResolution = 35
            },
            ["End Demo"] = new LightingPreset
            {
                ambientMode       = AmbientMode.Trilight,
                ambientSky        = new Color(0.8f, 0.7f,  0.5f, 1f),
                ambientEquator    = new Color(0.6f, 0.4f,  0.3f, 1f),
                ambientGround     = new Color(0.2f, 0.15f, 0.1f, 1f),
                ambientIntensity  = 1.5f,
                fog               = true,
                fogMode           = FogMode.ExponentialSquared,
                fogColor          = new Color(0.9f, 0.6f, 0.4f, 1f),
                fogDensity        = 0.02f,
                sunIntensity      = 1.5f,
                sunColor          = new Color(1f, 0.7f, 0.4f, 1f),
                sunRotation       = new Vector3(20f, -60f, 0f),
                lightmapResolution = 30
            }
        };

        [MenuItem("Longinus/Lighting/Apply Preset to Current Scene")]
        public static void ApplyToCurrentScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (!PRESETS.ContainsKey(sceneName))
            {
                Debug.LogError($"[Lighting] No preset for scene '{sceneName}'. " +
                               $"Known: {string.Join(", ", PRESETS.Keys)}");
                return;
            }

            ApplyPreset(PRESETS[sceneName]);
            Debug.Log($"[Lighting] Applied preset for '{sceneName}'");
        }

        [MenuItem("Longinus/Lighting/Bake All Scenes")]
        public static void BakeAll()
        {
            foreach (string sceneName in PRESETS.Keys)
            {
                string path = FindScenePath(sceneName);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarning($"[Lighting] Scene '{sceneName}' not found in AssetDatabase.");
                    continue;
                }

                EditorSceneManager.OpenScene(path);
                ApplyPreset(PRESETS[sceneName]);

                LightmapEditorSettings.maxAtlasSize  = 1024;
                LightmapEditorSettings.lightmapper   = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
                Lightmapping.giWorkflowMode          = Lightmapping.GIWorkflowMode.OnDemand;

                Debug.Log($"[Lighting] Starting bake for '{sceneName}'...");
                Lightmapping.Bake();

                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }

            Debug.Log("[Lighting] All scenes baked.");
        }

        private static void ApplyPreset(LightingPreset preset)
        {
            RenderSettings.ambientMode       = preset.ambientMode;
            RenderSettings.ambientSkyColor   = preset.ambientSky;
            RenderSettings.ambientEquatorColor = preset.ambientEquator;
            RenderSettings.ambientGroundColor = preset.ambientGround;
            RenderSettings.ambientIntensity  = preset.ambientIntensity;
            RenderSettings.fog               = preset.fog;
            RenderSettings.fogMode           = preset.fogMode;
            RenderSettings.fogColor          = preset.fogColor;
            RenderSettings.fogDensity        = preset.fogDensity;

            Light sun = GameObject.Find("Directional Light")?.GetComponent<Light>();
            if (sun == null)
            {
                var sunGO = new GameObject("Directional Light");
                sun           = sunGO.AddComponent<Light>();
                sun.type      = LightType.Directional;
            }
            sun.intensity             = preset.sunIntensity;
            sun.color                 = preset.sunColor;
            sun.transform.eulerAngles = preset.sunRotation;
            sun.shadows               = LightShadows.Soft;

            LightmapEditorSettings.realtimeResolution = preset.lightmapResolution / 10f;
            LightmapEditorSettings.bakeResolution     = preset.lightmapResolution;

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static string FindScenePath(string sceneName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName)
                    return path;
            }
            return null;
        }
    }
}
