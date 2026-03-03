using System;
using UnityEngine;

namespace Enemy.BaseEnemy
{
    public class EnemyStatsManager : MonoBehaviour, IDamageable
    {
        [Header("Config")]
        [SerializeField] private float maxHealth = 100f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public event Action OnDeath;
        public event Action<float, float> OnDamageTaken;

        private bool _isDead;

        private void OnEnable()
        {
            ResetHealth();
        }

#region Health
        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_isDead) return;

            CurrentHealth -= amount;

            OnDamageTaken?.Invoke(amount, CurrentHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            _isDead = false;
        }
#endregion

        private void Die()
        {
            if (_isDead) return;
            
            _isDead = true;
            CurrentHealth = 0;
            
            OnDeath?.Invoke();
        }
    }
}