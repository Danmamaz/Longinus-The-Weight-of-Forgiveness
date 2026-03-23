using UnityEngine;
using Longinus.PlotSystem;
using Longinus.Interfaces;

namespace Longinus.InGameItems
{
    /// <summary>
    /// An interactable object that triggers a plot decision when the player engages with it.
    /// </summary>
    public class DecisionInteractable : MonoBehaviour, IInteractable
    {
        #region Constants & Inspector Variables
        
        [Header("Configuration")]
        [SerializeField, Tooltip("The specific decision node this object will trigger.")]
        private DecisionNode _decisionToTrigger;
        
        [SerializeField, Tooltip("Reference to the handler that manages the UI and logic for this decision.")] 
        private DecisionHandler _decisionHandler; 
        
        #endregion

        #region Event Listeners/Callbacks

        /// <summary>
        /// Executes the interaction logic, presenting the decision to the player.
        /// </summary>
        public void Interact()
        {
            if (_decisionHandler != null && _decisionToTrigger != null)
            {
                _decisionHandler.PresentDecision(_decisionToTrigger);
            }
            else
            {
                Debug.LogError($"[DecisionInteractable] Missing DecisionHandler or DecisionNode assignment on {gameObject.name}.");
            }
        }

        /// <summary>
        /// Provides the UI text prompt for this interaction.
        /// </summary>
        public string GetInteractionText()
        {
            return "Press E to Decide";
        }
        
        #endregion
    }
}