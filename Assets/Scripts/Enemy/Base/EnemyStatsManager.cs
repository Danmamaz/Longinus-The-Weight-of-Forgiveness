using System;
using UnityEngine;
using Longinus.Interfaces;

namespace Longinus.EnemySystem
{
    /// <summary>
    /// Manages enemy health, poise (stagger), and death states, including the boss spare/kill choice phase.
    /// </summary>
    public class EnemyStatsManager : MonoBehaviour, IDamageable 
    {
        #region Constants & Inspector Variables
        
        [Header("Config")]
        [SerializeField, Tooltip("Maximum health points of the entity.")] 
        private float _maxHealth = 100f;

        [SerializeField, Tooltip("Determines if this entity enters a vulnerable choice phase instead of dying immediately.")]
        private bool _isSpareable;

        [Header("Poise")]
        [SerializeField, Tooltip("Maximum poise. Reaching 0 triggers a stagger.")] 
        private float _maxPoise = 30f;
        
        [SerializeField, Tooltip("Rate at which poise recovers per second after the delay.")] 
        private float _poiseRegenRate = 10f;
        
        [SerializeField, Tooltip("Time in seconds without taking damage before poise begins to regenerate.")] 
        private float _poiseRegenDelay = 2f;

        #endregion

        #region Private Variables
        
        private bool _isDead;
        private float _timeSinceLastHit;

        #endregion

        #region Public Properties
        
        public bool IsInChoicePhase { get; private set; }
        public bool IsSpareable => _isSpareable;
        public float CurrentHealth { get; private set; }
        public float MaxHealth => _maxHealth;
        public float CurrentPoise { get; private set; }
        
        #endregion

        #region Events
        
        public event Action OnPoiseBreak;
        public event Action OnDeath;
        public event Action<float, float> OnDamageTaken;
        public event Action OnSpareableDeath;
        public event Action OnChoicePhaseDamaged;
        
        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            ResetHealth();
        }

        private void Update()
        {
            if (_isDead) return;

            _timeSinceLastHit += Time.deltaTime;
            
            if (_timeSinceLastHit >= _poiseRegenDelay && CurrentPoise < _maxPoise)
            {
                CurrentPoise = Mathf.Min(_maxPoise, CurrentPoise + _poiseRegenRate * Time.deltaTime);
            }
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Applies damage and poise damage to the enemy. Handles stagger and death transitions.
        /// </summary>
        public void TakeDamage(float amount, float poiseDamage, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_isDead) return;

            // Intercept hits during the choice phase to trigger the final execution rather than standard damage calculation.
            if (IsInChoicePhase)
            {
                OnChoicePhaseDamaged?.Invoke();
                return;
            }

            CurrentHealth -= amount;
            CurrentPoise -= poiseDamage;
            _timeSinceLastHit = 0f;
            
            OnDamageTaken?.Invoke(amount, CurrentHealth);

            if (CurrentPoise <= 0)
            {
                CurrentPoise = _maxPoise;
                OnPoiseBreak?.Invoke();
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Resets health, poise, and state flags to their defaults. Essential for object pooling.
        /// </summary>
        public void ResetHealth()
        {
            CurrentHealth = _maxHealth;
            CurrentPoise = _maxPoise;
            _isDead = false;
            IsInChoicePhase = false;
            _timeSinceLastHit = 0f;
        }

        /// <summary>
        /// Evaluates whether the enemy dies permanently or enters the interactive spare/kill phase.
        /// </summary>
        private void Die()
        {
            if (_isDead) return;
            CurrentHealth = 0;

            if (_isSpareable)
            {
                CurrentHealth = 1f; // Leave 1 HP for the choice phase visually/mechanically
                IsInChoicePhase = true;
                OnSpareableDeath?.Invoke();
            }
            else
            {
                _isDead = true;
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// Forces the final death state, bypassing any choice mechanics. Used for executions or when mercy is granted.
        /// </summary>
        public void ExecuteFinalDeath()
        {
            IsInChoicePhase = false;
            _isDead = true;
            CurrentHealth = 0f;
            OnDeath?.Invoke();
        }

        #endregion
    }
}