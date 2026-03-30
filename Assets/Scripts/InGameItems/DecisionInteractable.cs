using UnityEngine;
using Longinus.EnemySystem;

namespace Longinus.InGameItems
{
    [RequireComponent(typeof(EnemyStatsManager))]
    public class DecisionInteractable : MonoBehaviour
    {
        [Header("Двері для вибору")]
        [SerializeField] private Animator _leftDoorAnimator;
        [SerializeField] private Animator _rightDoorAnimator;
        [SerializeField] private GameObject _blueFlash;
        [SerializeField] private GameObject _redFlash;

        private EnemyStatsManager _enemyStats;
        private bool _isWaitingForChoice = false;
        private Collider _collider;
        private int _originalLayer;

        private void Awake()
        {
            _enemyStats = GetComponent<EnemyStatsManager>();
            _collider = GetComponent<Collider>();
            _originalLayer = gameObject.layer; // Запам'ятовуємо шар (Enemy)

            _enemyStats.OnSpareableDeath += StartChoicePhase;
            _enemyStats.OnDamageTaken += HandleFollowUpHit;
        }

        private void OnDestroy()
        {
            if (_enemyStats != null)
            {
                _enemyStats.OnSpareableDeath -= StartChoicePhase;
                _enemyStats.OnDamageTaken -= HandleFollowUpHit;
            }
        }

        private void StartChoicePhase()
        {
            _isWaitingForChoice = true;
            
            // 1. Таймер через Invoke не залежить від Update()
            Invoke(nameof(ExecuteSpareChoice), 5f); 

            // 2. ХАК: Даємо машині станів 0.1 сек на вимкнення коллайдера, а потім брутально вмикаємо його назад
            Invoke(nameof(ForceEnableHitbox), 0.1f);
        }

        private void ForceEnableHitbox()
        {
            if (_collider != null) _collider.enabled = true;
            gameObject.layer = _originalLayer; // Повертаємо шар "Enemy"
            Debug.Log("[Decision] Коллайдер і шар примусово відновлено. Бий його!");
        }

        private void HandleFollowUpHit(float damage, float currentHealth)
        {
            if (_isWaitingForChoice)
            {
                CancelInvoke(nameof(ExecuteSpareChoice)); // Зупиняємо таймер 5 секунд
                ExecuteKillChoice();
            }
        }

        private void ExecuteKillChoice()
        {
            _isWaitingForChoice = false;
            Debug.Log("[Decision] Вибір: ДОБИТИ. Відкриваємо ліві двері.");
            _redFlash.SetActive(true);
            _enemyStats.ExecuteFinalDeath(); 
            if (_leftDoorAnimator != null) _leftDoorAnimator.SetTrigger("Open");
        }

        private void ExecuteSpareChoice()
        {
            if (!_isWaitingForChoice) return;
            
            _blueFlash.SetActive(true);
            _isWaitingForChoice = false;
            Debug.Log("[Decision] Час вийшов. Вибір: ПОЩАДА. Відкриваємо праві двері.");
            
            if (_rightDoorAnimator != null) _rightDoorAnimator.SetTrigger("Open");
        }
    }
}