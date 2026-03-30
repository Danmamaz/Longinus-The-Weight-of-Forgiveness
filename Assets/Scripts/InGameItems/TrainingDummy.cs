using UnityEngine;
using Longinus.EnemySystem; // Додано для доступу до EnemyStatsManager

namespace Longinus.InGameItems
{
    [RequireComponent(typeof(EnemyStatsManager))]
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private EnemyStatsManager _statsManager;

        private void Awake()
        {
            _statsManager = GetComponent<EnemyStatsManager>();
            _statsManager.OnDamageTaken += HandleDamage;
        }

        private void OnDestroy()
        {
            if (_statsManager != null)
                _statsManager.OnDamageTaken -= HandleDamage;
        }

        private void HandleDamage(float amount, float currentHealth)
        {
            if (animator != null) 
                animator.SetTrigger("gotHit");
                
            Debug.Log($"[TrainingDummy] Took {amount} damage. Remaining HP: {currentHealth}");
        }
    }
}