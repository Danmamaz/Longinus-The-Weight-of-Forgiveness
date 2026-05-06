using UnityEngine;
using UnityEngine.UI;
using Longinus.EnemySystem;

namespace Longinus.UI
{
    /// <summary>
    /// Displays the enemy's current health on a Slider and triggers a flash animation on death.
    /// </summary>
    public class EnemyHealthUI : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("References")]
        [SerializeField, Tooltip("EnemyStatsManager of the enemy this UI belongs to.")]
        private EnemyStatsManager _enemyStats;

        [SerializeField, Tooltip("Slider representing the enemy's current health.")]
        private Slider _healthSlider;

        #endregion

        #region Private Variables

        private Animator _animator;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (_enemyStats == null || _healthSlider == null || _animator == null)
            {
                Debug.LogError("[EnemyHealthUI] Missing references in Inspector!");
                return;
            }

            _healthSlider.maxValue = _enemyStats.MaxHealth;
            _healthSlider.value = _enemyStats.MaxHealth;

            _enemyStats.OnDamageTaken += UpdateHealthBar;
        }

        private void OnDestroy()
        {
            if (_enemyStats != null)
            {
                _enemyStats.OnDamageTaken -= UpdateHealthBar;
            }
        }

        #endregion

        #region Event Listeners/Callbacks

        private void UpdateHealthBar(float damageAmount, float currentHealth)
        {
            _healthSlider.value = currentHealth;
            if (currentHealth <= 0) _animator.SetTrigger("Flash");
        }

        #endregion
    }
}
