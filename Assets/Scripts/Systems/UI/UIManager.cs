using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using Longinus.Player;
using Longinus.Interfaces;
using Longinus.PlotSystem;

namespace Longinus.UI
{
    /// <summary>
    /// Manages the core in-game user interface, including player stats, interactable prompts, and the pause menu.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("System References")]
        [SerializeField, Tooltip("Reference to the player's stats manager.")]
        private PlayerStatsManager _playerStats;
        [SerializeField] private DecisionHandler dh;
        
        [SerializeField, Tooltip("Reference to the player's interaction system.")]
        private InteractionSystem _interactionSystem;
        
        [Header("Pause Menu")]
        [SerializeField, Tooltip("The root game object of the pause menu UI.")]
        private GameObject _pauseMenu;
        
        [Header("Player Stats UI")]
        [SerializeField, Tooltip("Text component displaying current health.")]
        private TMP_Text _healthText; 
        
        [SerializeField, Tooltip("Text component displaying current stamina.")]
        private TMP_Text _staminaText;
        
        [Header("Interactables UI")]
        [SerializeField, Tooltip("Text component displaying interaction prompts.")]
        private TMP_Text _interactableText;

        #endregion

        #region Private Variables
        
        private bool _isPaused;
        private readonly StringBuilder _interactableStringBuilder = new StringBuilder();
        
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _isPaused = false;
            if (_pauseMenu != null) _pauseMenu.SetActive(false);
        }

        private void OnEnable()
        {
            if (_playerStats != null)
            {
                // Subscribing to the refactored standard C# Actions
                _playerStats.OnDamage += UpdateHealthUI;
                _playerStats.OnStaminaChange += UpdateStaminaUI;
                
                // Initialize UI with current values
                UpdateHealthUI(_playerStats.CurrentHealth);
                UpdateStaminaUI();
            }

            if (_interactionSystem != null)
            {
                // Subscribing to the refactored single UnityEvent
                _interactionSystem.OnInteractablesChanged.AddListener(UpdateInteractableUI);
            }
        }

        private void OnDisable()
        {
            if (_playerStats != null)
            {
                _playerStats.OnDamage -= UpdateHealthUI;
                _playerStats.OnStaminaChange -= UpdateStaminaUI;
            }

            if (_interactionSystem != null)
            {
                _interactionSystem.OnInteractablesChanged.RemoveListener(UpdateInteractableUI);
            }
            
            // Safety net: ensure time is unpaused if the UI object is destroyed during a scene transition
            Time.timeScale = 1f;
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Toggles the game pause state, managing the UI panel and time scale.
        /// </summary>
        /// <returns>True if the game is now paused, false otherwise.</returns>
        public bool TogglePauseMenu()
        {
            if (_pauseMenu == null) return false;
            
            _isPaused = !_isPaused;
            _pauseMenu.SetActive(_isPaused);
            Time.timeScale = _isPaused ? 0f : 1f;

            return _isPaused;
        }

        /// <summary>
        /// Presents a specific decision node to the player, updating UI elements accordingly.
        /// </summary>
        /// <param name="decision">The decision node data to display.</param>
        public void PresentDecision(DecisionNode decision)
        {
            if (decision == null)
            {
                Debug.LogError("[DecisionHandler] Cannot present null decision!");
                return;
            }

            dh._currentDecision = decision;

            if (dh._contextText != null) dh._contextText.text = decision.ContextDescription;
            if (dh._choiceAText != null) dh._choiceAText.text = decision.ChoiceAText;
            if (dh._choiceBText != null) dh._choiceBText.text = decision.ChoiceBText;

            if (dh._decisionPanel != null)
            {
                dh._decisionPanel.SetActive(true);
            }
        }

        #endregion
        
        #region Event Listeners/Callbacks

        /// <summary>
        /// Updates the health display. Triggered when the player takes damage.
        /// </summary>
        private void UpdateHealthUI(float currentHealth)
        {
            if (_healthText != null)
            {
                _healthText.text = $"Health: {Mathf.CeilToInt(currentHealth)}";
            }
        }

        /// <summary>
        /// Updates the stamina display. Triggered when stamina is consumed or regenerated.
        /// </summary>
        private void UpdateStaminaUI()
        {
            if (_staminaText != null && _playerStats != null)
            {
                _staminaText.text = $"Stamina: {Mathf.CeilToInt(_playerStats.CurrentStamina)}";
            }
        }

        /// <summary>
        /// Updates the interaction prompt text based on objects currently in range.
        /// </summary>
        private void UpdateInteractableUI(List<IInteractable> interactables)
        {
            if (_interactableText == null) return;

            if (interactables == null || interactables.Count == 0)
            {
                _interactableText.text = string.Empty;
                return;
            }

            _interactableStringBuilder.Clear();
            
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    _interactableStringBuilder.AppendLine(interactable.GetInteractionText());
                }
            }
            
            _interactableText.text = _interactableStringBuilder.ToString().TrimEnd();
        }

        #endregion

        #region Buttons

        /// <summary>
        /// Functionality of a resume button
        /// </summary>
        public void ResumeButton() {TogglePauseMenu();}

        #endregion
    }
}