using System;
using UnityEngine;
using Longinus.Interfaces;

namespace Longinus.Player
{
    /// <summary>
    /// Manages player health, stamina, mana, ultimate charge, and processes incoming damage.
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

        [Header("Magic & Ultimate Config")]
        [SerializeField, Tooltip("Maximum mana for spells/special abilities.")] 
        private float _maxMana = 50f;

        [SerializeField, Tooltip("Maximum charge needed to use the ultimate ability.")] 
        private float _maxUltimate = 100f;

        #endregion

        #region Private Variables
        
        private float _staminaRegenerationTimer;
        private bool _isDead;
        
        #endregion

        #region Public Properties

        public float MaxHealth => _maxHealth;
        public float CurrentHealth { get; private set; }
        
        public float MaxStamina => _maxStamina;
        public float CurrentStamina { get; private set; }

        public float MaxMana => _maxMana;
        public float CurrentMana { get; private set; }

        public float MaxUltimate => _maxUltimate;
        public float CurrentUltimate { get; private set; }
        
        #endregion

        #region Events
        
        public event Action<float> OnDamage;
        public event Action OnStaminaChange;
        public event Action OnManaChange;
        public event Action OnUltimateChange;
        public event Action OnDeath;
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            CurrentStamina = _maxStamina;
            CurrentMana = _maxMana;
            CurrentUltimate = 0f; // Ульта зазвичай накопичується з нуля
            
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
        /// Attempts to consume a specified amount of mana. Fails if insufficient.
        /// </summary>
        public bool TryConsumeMana(float amount)
        {
            if (_isDead || CurrentMana < amount) return false;

            CurrentMana -= amount;
            OnManaChange?.Invoke();
            
            return true;
        }

        /// <summary>
        /// Restores mana by a specific amount (e.g., from a potion).
        /// </summary>
        public void RestoreMana(float amount)
        {
            if (_isDead) return;

            CurrentMana = Mathf.Min(_maxMana, CurrentMana + amount);
            OnManaChange?.Invoke();
        }

        /// <summary>
        /// Adds charge to the ultimate meter (e.g., when dealing damage or taking damage).
        /// </summary>
        public void AddUltimateCharge(float amount)
        {
            if (_isDead || CurrentUltimate >= _maxUltimate) return;

            CurrentUltimate = Mathf.Min(_maxUltimate, CurrentUltimate + amount);
            OnUltimateChange?.Invoke();
        }

        /// <summary>
        /// Consumes the entire ultimate meter if it is fully charged.
        /// </summary>
        public bool TryUseUltimate()
        {
            if (_isDead || CurrentUltimate < _maxUltimate) return false;

            CurrentUltimate = 0f;
            OnUltimateChange?.Invoke();
            
            return true;
        }

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

        public void RestoreAll()
        {
            CurrentHealth = MaxHealth;
            CurrentStamina = MaxStamina;
            CurrentMana = MaxMana;
            CurrentUltimate = 0;
        }

        /// <summary>
        /// Restores the player's stats from a loaded save file. 
        /// Bypasses normal gameplay mechanics (like damage calculation or events).
        /// </summary>
        public void RestoreState(
            float maxHealth, float currentHealth, 
            float maxStamina, float currentStamina, 
            float maxMana, float currentMana, 
            float maxUltimate, float currentUltimate)
        {
            _maxHealth = maxHealth;
            _maxStamina = maxStamina;
            _maxMana = maxMana;
            _maxUltimate = maxUltimate;

            CurrentHealth = currentHealth;
            CurrentStamina = currentStamina;
            CurrentMana = currentMana;
            CurrentUltimate = currentUltimate;
            
            OnDamage?.Invoke(CurrentHealth);
            OnStaminaChange?.Invoke();
            OnManaChange?.Invoke();
            OnUltimateChange?.Invoke();
        }

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