using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Handles presenting decisions to the player and processing their choice via UI.
    /// </summary>
    public class DecisionHandler : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("UI References")]
        [SerializeField, Tooltip("Main UI panel containing the decision elements.")]
        private GameObject _decisionPanel;
        
        [SerializeField, Tooltip("Text component displaying the context or lore of the decision.")]
        private TextMeshProUGUI _contextText;
        
        [SerializeField, Tooltip("Button for the positive/mercy choice.")]
        private Button _choiceAButton;
        
        [SerializeField, Tooltip("Button for the negative/cruelty choice.")]
        private Button _choiceBButton;
        
        [SerializeField, Tooltip("Text component for the positive choice label.")]
        private TextMeshProUGUI _choiceAText;
        
        [SerializeField, Tooltip("Text component for the negative choice label.")]
        private TextMeshProUGUI _choiceBText;

        #endregion

        #region Private Variables

        private DecisionNode _currentDecision;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (_decisionPanel != null)
            {
                _decisionPanel.SetActive(false);
            }

            if (_choiceAButton != null)
            {
                _choiceAButton.onClick.AddListener(() => OnChoiceSelected(true));
            }
            
            if (_choiceBButton != null)
            {
                _choiceBButton.onClick.AddListener(() => OnChoiceSelected(false));
            }
        }

        #endregion

        #region State/Core Logic

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

            _currentDecision = decision;

            if (_contextText != null) _contextText.text = decision.ContextDescription;
            if (_choiceAText != null) _choiceAText.text = decision.ChoiceAText;
            if (_choiceBText != null) _choiceBText.text = decision.ChoiceBText;

            if (_decisionPanel != null)
            {
                _decisionPanel.SetActive(true);
            }
        }

        #endregion

        #region Event Listeners/Callbacks

        /// <summary>
        /// Processes the player's choice, registers it with the PlotManager, and handles subsequent logic.
        /// </summary>
        /// <param name="choseA">True if the positive choice was selected, false otherwise.</param>
        private void OnChoiceSelected(bool choseA)
        {
            if (_currentDecision == null) return;

            if (PlotManager.Instance != null)
            {
                PlotManager.Instance.RegisterDecision(_currentDecision, choseA);
            }

            if (_currentDecision.IsBossFight && choseA && _currentDecision.MiniGamePrefab != null)
            {
                StartMiniGame(_currentDecision.MiniGamePrefab);
            }

            if (_decisionPanel != null)
            {
                _decisionPanel.SetActive(false);
            }

            _currentDecision = null;
        }

        /// <summary>
        /// Initializes the associated mini-game sequence for specific choices.
        /// </summary>
        private void StartMiniGame(GameObject miniGamePrefab)
        {
            if (miniGamePrefab == null) return;
            
            // Pragmatic placeholder for future mini-game integration
            Debug.Log($"[DecisionHandler] Triggering mini-game: {miniGamePrefab.name}");
        }

        #endregion
    }
}