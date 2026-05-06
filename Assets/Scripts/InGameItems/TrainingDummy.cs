using UnityEngine;
using Longinus.EnemySystem;

namespace Longinus.InGameItems
{
    /// <summary>
    /// A non-hostile training target that plays a hit reaction animation whenever it takes damage.
    /// Used in the tutorial area to teach the player combat without risk.
    /// </summary>
    [RequireComponent(typeof(EnemyStatsManager))]
    public class TrainingDummy : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("Animator driving the dummy's hit-reaction animation.")]
        private Animator animator;

        #endregion

        #region Private Variables

        private EnemyStatsManager _statsManager;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _statsManager = GetComponent<EnemyStatsManager>();
            _statsManager.OnDamageTaken += HandleDamage;
        }

        private void OnDestroy()
        {
            if (_statsManager != null)
            {
                _statsManager.OnDamageTaken -= HandleDamage;
            }
        }

        #endregion

        #region Event Listeners/Callbacks

        private void HandleDamage(float amount, float currentHealth)
        {
            if (animator != null)
            {
                animator.SetTrigger("gotHit");
            }

            Debug.Log($"[TrainingDummy] Took {amount} damage. Remaining HP: {currentHealth}");
        }

        #endregion
    }
}
