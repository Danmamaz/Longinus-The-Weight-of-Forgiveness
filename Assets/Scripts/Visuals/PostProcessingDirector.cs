using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Longinus.PlotSystem;

namespace Longinus.Visuals
{
    [RequireComponent(typeof(Volume))]
    public class PostProcessingDirector : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField] private PostProcessingMode _initialMode = PostProcessingMode.Normal;

        public enum PostProcessingMode
        {
            Normal,
            BossArenaPhase1,
            BossArenaPhase2,
            PostBossKill,
            Death,
            Rest
        }

        private struct PostFXPreset
        {
            public Color colorFilter;
            public float contrast;
            public float saturation;
            public float postExposure;
            public float vignetteIntensity;
            public Color vignetteColor;
            public float bloomIntensity;
            public float bloomThreshold;
            public Color bloomTint;
            public Color fogColor;
            public float fogDensity;
            public float chromaticAberration;
        }

        private static readonly Dictionary<PostProcessingMode, PostFXPreset> PRESETS =
            new Dictionary<PostProcessingMode, PostFXPreset>
        {
            [PostProcessingMode.Normal] = new PostFXPreset
            {
                colorFilter         = Color.white,
                contrast            = 0f,
                saturation          = 0f,
                postExposure        = 0f,
                vignetteIntensity   = 0.25f,
                vignetteColor       = Color.black,
                bloomIntensity      = 0.4f,
                bloomThreshold      = 0.9f,
                bloomTint           = Color.white,
                fogColor            = new Color(0.7f, 0.75f, 0.8f, 1f),
                fogDensity          = 0.015f,
                chromaticAberration = 0.05f
            },
            [PostProcessingMode.BossArenaPhase1] = new PostFXPreset
            {
                colorFilter         = new Color(0.8f, 0.85f, 1f, 1f),
                contrast            = 15f,
                saturation          = -10f,
                postExposure        = -0.3f,
                vignetteIntensity   = 0.35f,
                vignetteColor       = new Color(0.05f, 0.05f, 0.1f, 1f),
                bloomIntensity      = 0.6f,
                bloomThreshold      = 0.85f,
                bloomTint           = new Color(0.8f, 0.9f, 1f, 1f),
                fogColor            = new Color(0.3f, 0.35f, 0.45f, 1f),
                fogDensity          = 0.025f,
                chromaticAberration = 0.1f
            },
            [PostProcessingMode.BossArenaPhase2] = new PostFXPreset
            {
                colorFilter         = new Color(1.1f, 0.85f, 0.8f, 1f),
                contrast            = 25f,
                saturation          = 5f,
                postExposure        = 0f,
                vignetteIntensity   = 0.45f,
                vignetteColor       = new Color(0.3f, 0f, 0f, 1f),
                bloomIntensity      = 1.2f,
                bloomThreshold      = 0.7f,
                bloomTint           = new Color(1f, 0.5f, 0.3f, 1f),
                fogColor            = new Color(0.4f, 0.15f, 0.1f, 1f),
                fogDensity          = 0.04f,
                chromaticAberration = 0.25f
            },
            [PostProcessingMode.PostBossKill] = new PostFXPreset
            {
                colorFilter         = new Color(1f, 0.6f, 0.5f, 1f),
                contrast            = 20f,
                saturation          = -5f,
                postExposure        = -0.2f,
                vignetteIntensity   = 0.5f,
                vignetteColor       = new Color(0.5f, 0.1f, 0f, 1f),
                bloomIntensity      = 1.5f,
                bloomThreshold      = 0.6f,
                bloomTint           = new Color(1f, 0.3f, 0.1f, 1f),
                fogColor            = new Color(0.6f, 0.1f, 0.05f, 1f),
                fogDensity          = 0.06f,
                chromaticAberration = 0.15f
            },
            [PostProcessingMode.Death] = new PostFXPreset
            {
                colorFilter         = new Color(0.5f, 0.5f, 0.5f, 1f),
                contrast            = -10f,
                saturation          = -80f,
                postExposure        = -1f,
                vignetteIntensity   = 0.6f,
                vignetteColor       = Color.black,
                bloomIntensity      = 0.2f,
                bloomThreshold      = 0.95f,
                bloomTint           = Color.white,
                fogColor            = new Color(0.2f, 0.2f, 0.2f, 1f),
                fogDensity          = 0.05f,
                chromaticAberration = 0.5f
            },
            [PostProcessingMode.Rest] = new PostFXPreset
            {
                colorFilter         = new Color(1f, 0.95f, 0.85f, 1f),
                contrast            = 5f,
                saturation          = 10f,
                postExposure        = 0.2f,
                vignetteIntensity   = 0.3f,
                vignetteColor       = new Color(0.3f, 0.15f, 0f, 1f),
                bloomIntensity      = 0.7f,
                bloomThreshold      = 0.8f,
                bloomTint           = new Color(1f, 0.85f, 0.6f, 1f),
                fogColor            = new Color(0.5f, 0.4f, 0.3f, 1f),
                fogDensity          = 0.018f,
                chromaticAberration = 0.03f
            }
        };

        #endregion

        #region Private Variables

        private Volume               _volume;
        private VolumeProfile        _profile;
        private ColorAdjustments     _colorAdjustments;
        private Bloom                _bloom;
        private Vignette             _vignette;
        private ChromaticAberration  _chromaticAberration;
        private PostProcessingMode   _currentMode;
        private Coroutine            _transitionCoroutine;

        #endregion

        #region Public Properties

        public static PostProcessingDirector Instance { get; private set; }
        public PostProcessingMode CurrentMode => _currentMode;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Instance = this;

            _volume  = GetComponent<Volume>();
            _profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _profile.name     = "Runtime_PostFX_Profile";
            _volume.profile   = _profile;
            _volume.isGlobal  = true;
            _volume.weight    = 1f;

            _colorAdjustments    = _profile.Add<ColorAdjustments>(true);
            _bloom               = _profile.Add<Bloom>(true);
            _vignette            = _profile.Add<Vignette>(true);
            _chromaticAberration = _profile.Add<ChromaticAberration>(true);

            // Explicit override enables in case Add(true) doesn't cover all parameters
            _colorAdjustments.colorFilter.overrideState    = true;
            _colorAdjustments.contrast.overrideState       = true;
            _colorAdjustments.saturation.overrideState     = true;
            _colorAdjustments.postExposure.overrideState   = true;
            _bloom.intensity.overrideState                 = true;
            _bloom.threshold.overrideState                 = true;
            _bloom.tint.overrideState                      = true;
            _vignette.intensity.overrideState              = true;
            _vignette.color.overrideState                  = true;
            _chromaticAberration.intensity.overrideState   = true;

            ApplyPresetInstant(_initialMode);
        }

        private void OnEnable()
        {
            if (PlotManager.Instance != null)
                PlotManager.Instance.OnFlagUpdated.AddListener(OnPlotFlagUpdated);
        }

        private void OnDisable()
        {
            if (PlotManager.Instance != null)
                PlotManager.Instance.OnFlagUpdated.RemoveListener(OnPlotFlagUpdated);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_profile != null) Destroy(_profile);
        }

        #endregion

        #region State / Core Logic

        public void TransitionTo(PostProcessingMode mode, float duration = 1.5f)
        {
            if (mode == _currentMode) return;

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            _transitionCoroutine = StartCoroutine(LerpToPreset(mode, duration));
        }

        private IEnumerator LerpToPreset(PostProcessingMode targetMode, float duration)
        {
            PostFXPreset start  = CaptureCurrent();
            PostFXPreset target = PRESETS[targetMode];
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                ApplyPresetLerp(start, target, Mathf.Clamp01(t / duration));
                yield return null;
            }

            _currentMode = targetMode;
            ApplyPresetInstant(targetMode);
        }

        private void ApplyPresetInstant(PostProcessingMode mode)
        {
            PostFXPreset p = PRESETS[mode];

            _colorAdjustments.colorFilter.value  = p.colorFilter;
            _colorAdjustments.contrast.value     = p.contrast;
            _colorAdjustments.saturation.value   = p.saturation;
            _colorAdjustments.postExposure.value = p.postExposure;

            _bloom.intensity.value  = p.bloomIntensity;
            _bloom.threshold.value  = p.bloomThreshold;
            _bloom.tint.value       = p.bloomTint;

            _vignette.intensity.value = p.vignetteIntensity;
            _vignette.color.value     = p.vignetteColor;

            _chromaticAberration.intensity.value = p.chromaticAberration;

            RenderSettings.fog        = true;
            RenderSettings.fogMode    = FogMode.Exponential;
            RenderSettings.fogColor   = p.fogColor;
            RenderSettings.fogDensity = p.fogDensity;

            _currentMode = mode;
        }

        private void ApplyPresetLerp(PostFXPreset a, PostFXPreset b, float t)
        {
            _colorAdjustments.colorFilter.value  = Color.Lerp(a.colorFilter, b.colorFilter, t);
            _colorAdjustments.contrast.value     = Mathf.Lerp(a.contrast, b.contrast, t);
            _colorAdjustments.saturation.value   = Mathf.Lerp(a.saturation, b.saturation, t);
            _colorAdjustments.postExposure.value = Mathf.Lerp(a.postExposure, b.postExposure, t);

            _bloom.intensity.value  = Mathf.Lerp(a.bloomIntensity, b.bloomIntensity, t);
            _bloom.threshold.value  = Mathf.Lerp(a.bloomThreshold, b.bloomThreshold, t);
            _bloom.tint.value       = Color.Lerp(a.bloomTint, b.bloomTint, t);

            _vignette.intensity.value = Mathf.Lerp(a.vignetteIntensity, b.vignetteIntensity, t);
            _vignette.color.value     = Color.Lerp(a.vignetteColor, b.vignetteColor, t);

            _chromaticAberration.intensity.value = Mathf.Lerp(a.chromaticAberration, b.chromaticAberration, t);

            RenderSettings.fogColor   = Color.Lerp(a.fogColor, b.fogColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(a.fogDensity, b.fogDensity, t);
        }

        private PostFXPreset CaptureCurrent()
        {
            return new PostFXPreset
            {
                colorFilter         = _colorAdjustments.colorFilter.value,
                contrast            = _colorAdjustments.contrast.value,
                saturation          = _colorAdjustments.saturation.value,
                postExposure        = _colorAdjustments.postExposure.value,
                vignetteIntensity   = _vignette.intensity.value,
                vignetteColor       = _vignette.color.value,
                bloomIntensity      = _bloom.intensity.value,
                bloomThreshold      = _bloom.threshold.value,
                bloomTint           = _bloom.tint.value,
                fogColor            = RenderSettings.fogColor,
                fogDensity          = RenderSettings.fogDensity,
                chromaticAberration = _chromaticAberration.intensity.value
            };
        }

        #endregion

        #region Event Listeners / Callbacks

        private void OnPlotFlagUpdated(string flagId)
        {
            if (flagId == "Flag_BossDefeated")
                TransitionTo(PostProcessingMode.PostBossKill);
        }

        #endregion
    }
}
