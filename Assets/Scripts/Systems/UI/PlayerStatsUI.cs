using UnityEngine;
using UnityEngine.UI;
using Longinus.Player;

namespace Longinus.UI
{
    /// <summary>
    /// Handles the visual representation of the player's core stats (Health, Stamina, Mana, Ultimate).
    /// </summary>
    public class PlayerStatsUI : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Manager Reference")]
        [SerializeField, Tooltip("Reference to the player's stats manager to listen for stat changes.")] 
        private PlayerStatsManager _statsManager;
        
        [Header("UI Images (Filled)")]
        [SerializeField, Tooltip("Image component representing the health bar fill.")] 
        private Image _healthBarFill;
        
        [SerializeField, Tooltip("Image component representing the stamina bar fill.")] 
        private Image _staminaBarFill;
        
        [SerializeField, Tooltip("Image component representing the mana bar fill.")] 
        private Image _manaBarFill;
        
        [SerializeField, Tooltip("Image component representing the ultimate ability charge fill.")] 
        private Image _ultimateBarFill;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            UpdateAllUI();
        }

        private void OnEnable()
        {
            if (_statsManager != null)
            {
                _statsManager.OnDamage += UpdateHealthUI;
                _statsManager.OnStaminaChange += UpdateStaminaUI;
                _statsManager.OnManaChange += UpdateManaUI;
                _statsManager.OnUltimateChange += UpdateUltimateUI;
                
                // Initialize UI state when enabled
                UpdateAllUI();
            }
            else
            {
                Debug.LogWarning("[PlayerStatsUI] PlayerStatsManager is missing!");
            }
        }

        private void OnDisable()
        {
            if (_statsManager != null)
            {
                // Unsubscribe to prevent memory leaks when the UI is disabled or destroyed
                _statsManager.OnDamage -= UpdateHealthUI;
                _statsManager.OnStaminaChange -= UpdateStaminaUI;
                _statsManager.OnManaChange -= UpdateManaUI;
                _statsManager.OnUltimateChange -= UpdateUltimateUI;
            }
        }

        #endregion

        #region UI Update Logic

        /// <summary>
        /// Forces an immediate update of all UI elements to match current player stats.
        /// </summary>
        private void UpdateAllUI()
        {
            if (_statsManager == null) return;

            UpdateHealthUI(_statsManager.CurrentHealth);
            UpdateStaminaUI();
            UpdateManaUI();
            UpdateUltimateUI();
        }

        /// <summary>
        /// Updates the health bar fill amount based on current health.
        /// </summary>
        /// <param name="currentHealth">The player's current health value.</param>
        private void UpdateHealthUI(float currentHealth)
        {
            if (_healthBarFill != null && _statsManager.MaxHealth > 0)
            {
                _healthBarFill.fillAmount = currentHealth / _statsManager.MaxHealth;
            }
        }

        /// <summary>
        /// Updates the stamina bar fill amount.
        /// </summary>
        private void UpdateStaminaUI()
        {
            if (_staminaBarFill != null && _statsManager.MaxStamina > 0)
            {
                _staminaBarFill.fillAmount = _statsManager.CurrentStamina / _statsManager.MaxStamina;
            }
        }

        /// <summary>
        /// Updates the mana bar fill amount.
        /// </summary>
        private void UpdateManaUI()
        {
            if (_manaBarFill != null && _statsManager.MaxMana > 0)
            {
                _manaBarFill.fillAmount = _statsManager.CurrentMana / _statsManager.MaxMana;
            }
        }

        /// <summary>
        /// Updates the ultimate ability bar fill amount.
        /// </summary>
        private void UpdateUltimateUI()
        {
            if (_ultimateBarFill != null && _statsManager.MaxUltimate > 0)
            {
                _ultimateBarFill.fillAmount = _statsManager.CurrentUltimate / _statsManager.MaxUltimate;
            }
        }

        #endregion
    }
}