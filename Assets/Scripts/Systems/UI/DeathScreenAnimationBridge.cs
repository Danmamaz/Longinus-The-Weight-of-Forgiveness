using UnityEngine;
using UnityEngine.UI;

namespace Longinus.UI
{
    /// <summary>
    /// Bridges the death text reveal shader to the Animator by driving the _RevealProgress
    /// material property from a keyframeable inspector float. Instantiates the material
    /// to prevent modifying the shared project asset.
    /// </summary>
    public class DeathScreenAnimationBridge : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Range(0f, 1f)]
        [Tooltip("Keyframe this value in the Animation Window to drive the text reveal effect.")]
        public float revealProgress = 0f;

        [SerializeField]
        private Image deathTextImage;

        #endregion

        #region Private Variables

        private Material _deathMaterial;

        // Cached to avoid string lookup each frame
        private static readonly int RevealProgressID = Shader.PropertyToID("_RevealProgress");

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (deathTextImage != null && deathTextImage.material != null)
            {
                _deathMaterial = new Material(deathTextImage.material);
                deathTextImage.material = _deathMaterial;
                _deathMaterial.SetFloat(RevealProgressID, 0f);
            }
        }

        private void Update()
        {
            if (_deathMaterial != null)
            {
                _deathMaterial.SetFloat(RevealProgressID, revealProgress);
            }
        }

        private void OnDestroy()
        {
            if (_deathMaterial != null)
            {
                Destroy(_deathMaterial);
            }
        }

        #endregion
    }
}
