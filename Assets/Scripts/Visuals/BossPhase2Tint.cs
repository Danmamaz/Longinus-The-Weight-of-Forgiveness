using System.Collections;
using UnityEngine;
using Longinus.EnemySystem;

namespace Longinus.Visuals
{
    [RequireComponent(typeof(Renderer))]
    public class BossPhase2Tint : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField] private float _transitionDuration = 1.5f;

        // R > 1 so the bloom post-process picks up the emissive boost
        private static readonly Color PHASE2_TINT_COLOR = new Color(1.4f, 0.6f, 0.5f, 1f);
        private static readonly Color PHASE2_EMISSION   = new Color(2f, 0.3f, 0.1f, 1f);

        private static readonly int BaseColorID      = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorID  = Shader.PropertyToID("_EmissionColor");

        #endregion

        #region Private Variables

        private Renderer[]          _renderers;
        private Color[]             _originalBaseColors;
        private Color[]             _originalEmissionColors;
        private MaterialPropertyBlock _block;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _block     = new MaterialPropertyBlock();

            _originalBaseColors     = new Color[_renderers.Length];
            _originalEmissionColors = new Color[_renderers.Length];

            for (int i = 0; i < _renderers.Length; i++)
            {
                Material mat = _renderers[i].sharedMaterial;

                _originalBaseColors[i] = (mat != null && mat.HasProperty(BaseColorID))
                    ? mat.GetColor(BaseColorID)
                    : Color.white;

                _originalEmissionColors[i] = (mat != null && mat.HasProperty(EmissionColorID))
                    ? mat.GetColor(EmissionColorID)
                    : Color.black;
            }
        }

        private void OnEnable()
        {
            if (TryGetComponent(out BossController boss))
                boss.OnPhaseChanged += HandlePhaseChange;
        }

        private void OnDisable()
        {
            if (TryGetComponent(out BossController boss))
                boss.OnPhaseChanged -= HandlePhaseChange;
        }

        #endregion

        #region Event Listeners / Callbacks

        private void HandlePhaseChange(BossController.BossPhase phase)
        {
            if (phase == BossController.BossPhase.Phase2)
                StartCoroutine(TintTransition());
        }

        #endregion

        #region State / Core Logic

        private IEnumerator TintTransition()
        {
            float t = 0f;
            while (t < _transitionDuration)
            {
                t += Time.deltaTime;
                float k = t / _transitionDuration;

                for (int i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].GetPropertyBlock(_block);
                    _block.SetColor(BaseColorID,     Color.Lerp(_originalBaseColors[i],     PHASE2_TINT_COLOR, k));
                    _block.SetColor(EmissionColorID, Color.Lerp(_originalEmissionColors[i], PHASE2_EMISSION,   k));
                    _renderers[i].SetPropertyBlock(_block);
                }

                yield return null;
            }
        }

        #endregion
    }
}
