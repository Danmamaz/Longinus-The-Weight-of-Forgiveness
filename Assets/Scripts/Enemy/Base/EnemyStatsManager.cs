using System;
using UnityEngine;

namespace Enemy.BaseEnemy
{
    public class EnemyStatsManager : MonoBehaviour, IDamageable
    {
        [Header("Config")]
        [SerializeField] private float maxHealth = 100f;

        [Header("Poise")]
        [SerializeField] private float maxPoise = 30f;
        [SerializeField] private float poiseRegenRate = 10f;
        [SerializeField] private float poiseRegenDelay = 2f;

        public bool IsInChoicePhase { get; private set; }
        public bool spareable;
        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public float CurrentPoise { get; private set; }
        public event Action OnPoiseBreak;
        public event Action OnDeath;
        public event Action<float, float> OnDamageTaken;
        public event Action OnSpareableDeath;
        public event Action OnChoicePhaseDamaged;

        private bool _isDead;
        private float _timeSinceLastHit;

        private void Update()
        {
            if (_isDead) return;

            _timeSinceLastHit += Time.deltaTime;
            if (_timeSinceLastHit >= poiseRegenDelay && CurrentPoise < maxPoise)
            {
                CurrentPoise = Mathf.Min(maxPoise, CurrentPoise + poiseRegenRate * Time.deltaTime);
            }
        }

        private void OnEnable()
        {
            ResetHealth();
        }


#region Health
        public void TakeDamage(float amount, float poiseDamage, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_isDead) return;

            // Перехоплення удару під час вікна вибору
            if (IsInChoicePhase)
            {
                OnChoicePhaseDamaged?.Invoke();
                return; // не рахуємо шкоду, не скидаємо poise
            }

            CurrentHealth -= amount;
            CurrentPoise -= poiseDamage;
            _timeSinceLastHit = 0f;
            OnDamageTaken?.Invoke(amount, CurrentHealth);

            if (CurrentPoise <= 0)
            {
                CurrentPoise = maxPoise;
                OnPoiseBreak?.Invoke();
            }

            if (CurrentHealth <= 0) Die();
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            CurrentPoise = maxPoise;
            _isDead = false;
        }
#endregion

        private void Die()
        {
            if (_isDead) return;
            CurrentHealth = 0;

            if (spareable)
            {
                CurrentHealth = 1f;
                IsInChoicePhase = true;
                OnSpareableDeath?.Invoke();
            }
            else
            {
                _isDead = true;
                OnDeath?.Invoke();
            }
        } 


        public void ExecuteFinalDeath()
        {
            IsInChoicePhase = false;
            _isDead = true;
            CurrentHealth = 0f;
            OnDeath?.Invoke();
        }
    }
}