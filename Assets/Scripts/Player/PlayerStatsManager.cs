using System;
using UnityEngine;
using Longinus.Interfaces;

namespace Longinus.Player
{
    /// <summary>
    /// Manages player health, stamina, and processes incoming damage.
    /// </summary>
    public class PlayerStatsManager : MonoBehaviour, IDamageable
    {
        #region Constants & Inspector Variables
        
        [Header("Health & Stamina Config")]
        [SerializeField, Tooltip("Maximum health points of the player.")] 
        private float _maxHealth = 100f;
        
        [SerializeField, Tooltip("Maximum stamina points available for actions.")] 
        private float _maxStamina = 100f;
        
        [SerializeField, Tooltip("Rate at which stamina recovers per second.")] 
        private float _staminaRegenRate = 10f;
        
        [SerializeField, Tooltip("Delay in seconds before stamina begins to regenerate after consumption.")] 
        private float _staminaRegenDelay = 1.2f;

        #endregion

        #region Private Variables
        
        private float _staminaRegenerationTimer;
        private bool _isDead;
        
        #endregion

        #region Public Properties

        public float MaxHealth => _maxHealth;
        public float CurrentHealth { get; private set; }
        public float CurrentStamina { get; private set; }
        
        #endregion

        #region Events
        
        // Switched from UnityEvent to standard C# Actions for performance and consistency
        public event Action<float> OnDamage;
        public event Action OnStaminaChange;
        public event Action OnDeath;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            CurrentStamina = _maxStamina;
            _staminaRegenerationTimer = 0f;
            _isDead = false;
        }

        private void Update()
        {
            if (_isDead) return;
            
            HandleStaminaRegen();
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Processes stamina regeneration over time after the delay has passed.
        /// </summary>
        private void HandleStaminaRegen()
        {
            if (_staminaRegenerationTimer < _staminaRegenDelay)
            {
                _staminaRegenerationTimer += Time.deltaTime;
                return;
            }

            if (CurrentStamina < _maxStamina)
            {
                CurrentStamina = Mathf.Min(_maxStamina, CurrentStamina + _staminaRegenRate * Time.deltaTime);
                OnStaminaChange?.Invoke();
            }
        }
        

        /// <summary>
        /// Attempts to consume a specified amount of stamina. Fails if insufficient.
        /// </summary>
        /// <param name="amount">Amount of stamina to consume.</param>
        /// <returns>True if successfully consumed, false otherwise.</returns>
        public bool TryConsumeStamina(float amount)
        {
            if (_isDead || CurrentStamina <= 0f) return false;

            CurrentStamina -= amount;
            if (CurrentStamina < 0f) CurrentStamina = 0f;

            _staminaRegenerationTimer = 0f; 
            OnStaminaChange?.Invoke();
            
            return true;
        }

        /// <summary>
        /// Applies damage to the player. Triggers death if health drops to or below zero.
        /// </summary>
        public void TakeDamage(float amount, float poiseDamage, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (_isDead) return;
            
            CurrentHealth -= amount;
            OnDamage?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Handles the irreversible death state of the player.
        /// </summary>
        private void Die()
        {
            if (_isDead) return;
            
            _isDead = true;
            CurrentHealth = 0f;
            OnDeath?.Invoke();
        }

        #endregion
    }
}