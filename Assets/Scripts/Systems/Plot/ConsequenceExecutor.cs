using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Listens to plot events and applies visual and audio changes to the world state.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ConsequenceExecutor : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Visual Settings - Normal")]
        [SerializeField, Tooltip("Skybox material used during the Normal world state.")]
        private Material _normalSkybox;
        
        [SerializeField, Tooltip("Ambient light color used during the Normal world state.")]
        private Color _normalAmbientLight = Color.white;

        [Header("Visual Settings - Gloomy")]
        [SerializeField, Tooltip("Skybox material used during the Gloomy world state.")]
        private Material _gloomySkybox;
        
        [SerializeField, Tooltip("Ambient light color used during the Gloomy world state.")]
        private Color _gloomyAmbientLight = new Color(0.3f, 0.3f, 0.4f);

        [Header("Visual Settings - Hopeful")]
        [SerializeField, Tooltip("Skybox material used during the Hopeful world state.")]
        private Material _hopefulSkybox;
        
        [SerializeField, Tooltip("Ambient light color used during the Hopeful world state.")]
        private Color _hopefulAmbientLight = new Color(1f, 0.95f, 0.8f);

        [Header("Audio Settings")]
        [SerializeField, Tooltip("Background music played during the Normal world state.")]
        private AudioClip _normalMusic;
        
        [SerializeField, Tooltip("Background music played during the Gloomy world state.")]
        private AudioClip _gloomyMusic;
        
        [SerializeField, Tooltip("Background music played during the Hopeful world state.")]
        private AudioClip _hopefulMusic;

        #endregion

        #region Private Variables

        private AudioSource _audioSource;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
        }

        private void Start()
        {
            if (PlotManager.Instance != null)
            {
                PlotManager.Instance.onWorldStateChanged.AddListener(OnWorldStateChanged);
            }
        }

        private void OnDestroy()
        {
            if (PlotManager.Instance != null)
            {
                PlotManager.Instance.onWorldStateChanged.RemoveListener(OnWorldStateChanged);
            }
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Applies the visual and audio settings for the Normal world state.
        /// </summary>
        private void ApplyNormalState()
        {
            if (_normalSkybox != null) RenderSettings.skybox = _normalSkybox;
            RenderSettings.ambientLight = _normalAmbientLight;
            RenderSettings.fog = false; // Pragmatic fix: reset fog from other states
            
            PlayMusic(_normalMusic);
        }

        /// <summary>
        /// Applies the visual and audio settings for the Gloomy world state, including fog.
        /// </summary>
        private void ApplyGloomyState()
        {
            if (_gloomySkybox != null) RenderSettings.skybox = _gloomySkybox;
            RenderSettings.ambientLight = _gloomyAmbientLight;
            
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.2f, 0.2f, 0.25f);
            RenderSettings.fogDensity = 0.02f;
            
            PlayMusic(_gloomyMusic);
        }

        /// <summary>
        /// Applies the visual and audio settings for the Hopeful world state.
        /// </summary>
        private void ApplyHopefulState()
        {
            if (_hopefulSkybox != null) RenderSettings.skybox = _hopefulSkybox;
            RenderSettings.ambientLight = _hopefulAmbientLight;
            RenderSettings.fog = false;
            
            PlayMusic(_hopefulMusic);
        }

        /// <summary>
        /// Safely plays the provided audio clip if it differs from the currently playing clip.
        /// </summary>
        private void PlayMusic(AudioClip clip)
        {
            if (_audioSource == null || clip == null || _audioSource.clip == clip) return;

            _audioSource.clip = clip;
            _audioSource.Play();
        }

        #endregion

        #region Event Listeners/Callbacks

        /// <summary>
        /// Triggered when the PlotManager broadcasts a change in the overall world state.
        /// </summary>
        private void OnWorldStateChanged(WorldStateType newState)
        {
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

        #endregion
    }
}