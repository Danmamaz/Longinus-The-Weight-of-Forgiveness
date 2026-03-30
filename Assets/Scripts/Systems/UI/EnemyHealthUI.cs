using UnityEngine;
using UnityEngine.UI;
using Longinus.EnemySystem;

namespace Longinus.UI
{
    public class EnemyHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Посилання на EnemyStatsManager ворога")] 
        private EnemyStatsManager _enemyStats;
        
        [SerializeField, Tooltip("Слайдер здоров'я ворога")] 
        private Slider _healthSlider;
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            if (_enemyStats == null || _healthSlider == null || _animator == null)
            {
                Debug.LogError("[EnemyHealthUI] Не призначені посилання в інспекторі!");
                return;
            }

            // Налаштовуємо максимуми слайдера під параметри ворога
            _healthSlider.maxValue = _enemyStats.MaxHealth;
            _healthSlider.value = _enemyStats.MaxHealth; // При старті здоров'я повне

            // Підписуємось на подію отримання шкоди
            _enemyStats.OnDamageTaken += UpdateHealthBar;
        }

        private void OnDestroy()
        {
            if (_enemyStats != null)
            {
                _enemyStats.OnDamageTaken -= UpdateHealthBar;
            }
        }

        // Подія OnDamageTaken передає кількість шкоди і поточне здоров'я. Нам треба тільки друге.
        private void UpdateHealthBar(float damageAmount, float currentHealth)
        {
            _healthSlider.value = currentHealth;
            if (currentHealth <= 0) _animator.SetTrigger("Flash");
        }
    }
}