using System.Collections;
using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Listens for a plot flag and physically transforms the arena in response:
    /// shifts lighting, thickens fog, reveals a hidden path, and swaps the ambient audio.
    /// Handles instant application on scene reload when the flag is already set.
    /// </summary>
    public class ArenaEnvSwap : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Trigger")]
        [SerializeField, Tooltip("Flag ID that initiates the environment swap (e.g. 'Flag_BossDefeated').")]
        private string _watchFlag = "Flag_BossDefeated";

        [SerializeField, Tooltip("Apply swap immediately on Start if the flag is already set (handles scene reloads).")]
        private bool _instantIfFlagAlreadySet = true;

        [Header("Lighting")]
        [SerializeField, Tooltip("All arena lights whose colour will shift after the swap.")]
        private Light[] _arenaLights;

        [SerializeField, Tooltip("Target light colour — deep red to signal the boss's defeat.")]
        private Color _newLightColor = new Color(0.9f, 0.2f, 0.1f, 1f);

        [SerializeField, Tooltip("Duration of the light colour crossfade in seconds.")]
        private float _lightTransitionDuration = 3f;

        [Header("Fog")]
        [SerializeField, Tooltip("Apply fog density and colour changes alongside the light shift.")]
        private bool _modifyFog = true;

        [SerializeField, Tooltip("Target fog density. Values above the scene default thicken the atmosphere.")]
        private float _newFogDensity = 1.5f;

        [SerializeField, Tooltip("Target fog colour — dark red to match the lighting shift.")]
        private Color _newFogColor = new Color(0.4f, 0.05f, 0.05f, 1f);

        [SerializeField, Tooltip("Duration of the fog crossfade in seconds.")]
        private float _fogTransitionDuration = 4f;

        [Header("Hidden Path")]
        [SerializeField, Tooltip("Root GameObject revealed when the boss is defeated.")]
        private GameObject _hiddenPathRoot;

        [SerializeField, Tooltip("Root GameObject disabled when the boss is defeated (blocking wall).")]
        private GameObject _blockingWallRoot;

        [Header("Audio")]
        [SerializeField, Tooltip("AudioSource playing the arena ambient loop.")]
        private AudioSource _arenaAmbientSource;

        [SerializeField, Tooltip("Replacement ambient clip played after the swap.")]
        private AudioClip _newAmbientLoop;

        #endregion

        #region Private Variables

        private bool _swapped;
        private Color[] _originalLightColors;
        private float _originalFogDensity;
        private Color _originalFogColor;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_arenaLights != null)
            {
                _originalLightColors = new Color[_arenaLights.Length];
                for (int i = 0; i < _arenaLights.Length; i++)
                    _originalLightColors[i] = _arenaLights[i].color;
            }
            _originalFogDensity = RenderSettings.fogDensity;
            _originalFogColor = RenderSettings.fogColor;
        }

        private void Start()
        {
            if (PlotManager.Instance == null)
            {
                Debug.LogWarning("[ArenaEnvSwap] No PlotManager found — environment swap will not trigger.");
                return;
            }

            if (_instantIfFlagAlreadySet && PlotManager.Instance.CheckFlag(_watchFlag))
            {
                ApplySwapInstant();
                return;
            }

            PlotManager.Instance.OnFlagUpdated.AddListener(OnFlagUpdated);
        }

        private void OnDestroy()
        {
            if (PlotManager.Instance != null)
                PlotManager.Instance.OnFlagUpdated.RemoveListener(OnFlagUpdated);
        }

        #endregion

        #region State/Core Logic

        private IEnumerator ApplySwapAnimated()
        {
            _swapped = true;

            // Structural changes are instant — no ambiguity about which path is open.
            if (_hiddenPathRoot != null) _hiddenPathRoot.SetActive(true);
            if (_blockingWallRoot != null) _blockingWallRoot.SetActive(false);

            if (_arenaAmbientSource != null && _newAmbientLoop != null)
            {
                _arenaAmbientSource.clip = _newAmbientLoop;
                _arenaAmbientSource.Play();
            }

            float maxDuration = Mathf.Max(_lightTransitionDuration, _fogTransitionDuration);
            float t = 0f;

            int lightCount = _arenaLights?.Length ?? 0;
            Color[] startLightColors = new Color[lightCount];
            for (int i = 0; i < lightCount; i++)
                startLightColors[i] = _arenaLights[i].color;

            float startFogDensity = RenderSettings.fogDensity;
            Color startFogColor = RenderSettings.fogColor;

            while (t < maxDuration)
            {
                t += Time.deltaTime;
                float lightT = Mathf.Clamp01(t / _lightTransitionDuration);
                float fogT = Mathf.Clamp01(t / _fogTransitionDuration);

                if (_arenaLights != null)
                {
                    for (int i = 0; i < _arenaLights.Length; i++)
                        _arenaLights[i].color = Color.Lerp(startLightColors[i], _newLightColor, lightT);
                }

                if (_modifyFog)
                {
                    RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, _newFogDensity, fogT);
                    RenderSettings.fogColor = Color.Lerp(startFogColor, _newFogColor, fogT);
                }

                yield return null;
            }
        }

        private void ApplySwapInstant()
        {
            _swapped = true;

            if (_arenaLights != null)
            {
                foreach (var l in _arenaLights)
                    if (l != null) l.color = _newLightColor;
            }

            if (_modifyFog)
            {
                RenderSettings.fogDensity = _newFogDensity;
                RenderSettings.fogColor = _newFogColor;
            }

            if (_hiddenPathRoot != null) _hiddenPathRoot.SetActive(true);
            if (_blockingWallRoot != null) _blockingWallRoot.SetActive(false);

            if (_arenaAmbientSource != null && _newAmbientLoop != null)
            {
                _arenaAmbientSource.clip = _newAmbientLoop;
                _arenaAmbientSource.Play();
            }
        }

        #endregion

        #region Event Listeners/Callbacks

        private void OnFlagUpdated(string flagId)
        {
            if (flagId != _watchFlag) return;
            if (_swapped) return;
            StartCoroutine(ApplySwapAnimated());
        }

        #endregion
    }
}
