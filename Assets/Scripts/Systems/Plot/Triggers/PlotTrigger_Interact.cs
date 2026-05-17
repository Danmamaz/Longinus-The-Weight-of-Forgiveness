using UnityEngine;
using Longinus.Interfaces;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Drop on any world prop that should fire a PlotBranch when the player interacts with it.
    /// Use cases: BR-02 altar, BR-03 sealed door, BR-07 bonfire, BR-08 Scribe NPC,
    /// BR-11 chapel bell, BR-12 crow feed prop.
    /// </summary>
    public class PlotTrigger_Interact : MonoBehaviour, IInteractable
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("ID of the PlotBranch to fire on interaction (e.g. 'BR-07').")]
        private string _branchIdToFire;

        [SerializeField, Tooltip("Prompt text shown in the interaction UI.")]
        private string _interactionText = "Interact";

        [SerializeField, Tooltip("When true, disables the Collider after the branch fires so the player cannot re-trigger it.")]
        private bool _consumeOnFire = true;

        #endregion

        #region State/Core Logic

        public string GetInteractionText() => _interactionText;

        public void Interact()
        {
            if (PlotManager.Instance == null) return;
            if (!PlotManager.Instance.TryFireBranch(_branchIdToFire)) return;
            if (_consumeOnFire)
            {
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        #endregion
    }
}
