using UnityEngine;
using System.Collections.Generic;

namespace PlotBranching
{
    /// <summary>
    /// Listens to plot events and applies visual/audio changes to the world
    /// </summary>
    public class ConsequenceExecutor : MonoBehaviour
    {
        [Header("Visual Settings")]
        public Material normalSkybox;
        public Material gloomySkybox;
        public Material hopefulSkybox;
        public Color normalAmbientLight = Color.white;
        public Color gloomyAmbientLight = new Color(0.3f, 0.3f, 0.4f);
        public Color hopefulAmbientLight = new Color(1f, 0.95f, 0.8f);

        [Header("Audio")]
        public AudioClip normalMusic;
        public AudioClip gloomyMusic;
        public AudioClip hopefulMusic;
        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.loop = true;
            }

            // Subscribe to events
            if (PlotManager.Instance != null)
            {
                PlotManager.Instance.onWorldStateChanged.AddListener(OnWorldStateChanged);
            }
        }

        /// <summary>
        /// Called when world state changes
        /// </summary>
        private void OnWorldStateChanged(WorldStateType newState)
        {
            Debug.Log($"ConsequenceExecutor: Applying world state '{newState}'");

            switch (newState)
            {
                case WorldStateType.Normal:
                    ApplyNormalState();
                    break;
                case WorldStateType.Gloomy:
                    ApplyGloomyState();
                    break;
                case WorldStateType.Hopeful:
                    ApplyHopefulState();
                    break;
            }
        }

        private void ApplyNormalState()
        {
            if (normalSkybox != null) RenderSettings.skybox = normalSkybox;
            RenderSettings.ambientLight = normalAmbientLight;
            if (normalMusic != null) PlayMusic(normalMusic);
        }

        private void ApplyGloomyState()
        {
            if (gloomySkybox != null) RenderSettings.skybox = gloomySkybox;
            RenderSettings.ambientLight = gloomyAmbientLight;
            if (gloomyMusic != null) PlayMusic(gloomyMusic);
            
            // Additional effects: fog, post-processing, etc.
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.2f, 0.2f, 0.25f);
            RenderSettings.fogDensity = 0.02f;
        }

        private void ApplyHopefulState()
        {
            if (hopefulSkybox != null) RenderSettings.skybox = hopefulSkybox;
            RenderSettings.ambientLight = hopefulAmbientLight;
            if (hopefulMusic != null) PlayMusic(hopefulMusic);

            RenderSettings.fog = false;
        }

        private void PlayMusic(AudioClip clip)
        {
            if (audioSource == null || clip == null) return;
            if (audioSource.clip == clip) return;

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}