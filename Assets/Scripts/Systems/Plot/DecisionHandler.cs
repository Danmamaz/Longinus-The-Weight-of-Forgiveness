using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlotBranching
{
    /// <summary>
    /// Handles presenting decisions to the player and processing their choice
    /// </summary>
    public class DecisionHandler : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject decisionPanel;
        public TextMeshProUGUI contextText;
        public Button choiceAButton;
        public Button choiceBButton;
        public TextMeshProUGUI choiceAText;
        public TextMeshProUGUI choiceBText;

        private DecisionNode currentDecision;

        private void Start()
        {
            // Hide panel initially
            if (decisionPanel != null)
            {
                decisionPanel.SetActive(false);
            }

            // Wire up button events
            if (choiceAButton != null)
            {
                choiceAButton.onClick.AddListener(() => OnChoiceSelected(true));
            }
            if (choiceBButton != null)
            {
                choiceBButton.onClick.AddListener(() => OnChoiceSelected(false));
            }
        }

        /// <summary>
        /// Presents a decision to the player
        /// </summary>
        public void PresentDecision(DecisionNode decision)
        {
            if (decision == null)
            {
                Debug.LogError("DecisionHandler: Cannot present null decision!");
                return;
            }

            // Check if conditions are met
            if (!PlotManager.Instance.AreConditionsMet(decision.conditions))
            {
                Debug.Log($"DecisionHandler: Conditions not met for decision '{decision.decisionName}'");
                return;
            }

            currentDecision = decision;

            // Update UI
            if (contextText != null)
            {
                contextText.text = decision.contextDescription;
            }
            if (choiceAText != null)
            {
                choiceAText.text = decision.choiceAText;
            }
            if (choiceBText != null)
            {
                choiceBText.text = decision.choiceBText;
            }

            // Show panel
            if (decisionPanel != null)
            {
                decisionPanel.SetActive(true);
            }

            // Pause game if needed
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Called when player selects a choice
        /// </summary>
        private void OnChoiceSelected(bool choseA)
        {
            if (currentDecision == null) return;

            // Register with PlotManager
            PlotManager.Instance.RegisterDecision(currentDecision, choseA);

            // Handle boss-specific logic (mini-game for spare)
            if (currentDecision.isBossFight && choseA && currentDecision.miniGamePrefab != null)
            {
                StartMiniGame(currentDecision.miniGamePrefab);
            }

            // Hide panel
            if (decisionPanel != null)
            {
                decisionPanel.SetActive(false);
            }

            // Resume game
            Time.timeScale = 1f;

            currentDecision = null;
        }

        /// <summary>
        /// Starts a mini-game (placeholder for your mini-game system)
        /// </summary>
        private void StartMiniGame(GameObject miniGamePrefab)
        {
            Debug.Log($"DecisionHandler: Starting mini-game '{miniGamePrefab.name}'");
            // TODO: Integrate with your mini-game system
            // Instantiate(miniGamePrefab);
            // MiniGameManager.Instance.StartMiniGame(miniGamePrefab);
        }
    }
}