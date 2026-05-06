using UnityEngine;
using Longinus.PlotSystem;

namespace Longinus.InGameItems
{
    /// <summary>
    /// A world-space door that opens when a required plot flag is set.
    /// Registers to PlotManager's global flag event to react to story progression in real time.
    /// </summary>
    public class Door : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Plot Settings")]
        [SerializeField, Tooltip("Flag ID that must be set for this door to open.")]
        public string requiredFlagID;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (PlotManager.Instance == null) return;

            if (PlotManager.Instance.CheckFlag(requiredFlagID))
            {
                OpenDoorInstantly();
                return;
            }

            PlotManager.Instance.OnFlagUpdated.AddListener(OnGlobalFlagUpdated);
        }

        private void OnDestroy()
        {
            if (PlotManager.Instance != null)
            {
                PlotManager.Instance.OnFlagUpdated.RemoveListener(OnGlobalFlagUpdated);
            }
        }

        #endregion

        #region State/Core Logic

        private void OnGlobalFlagUpdated(string updatedFlagID)
        {
            if (updatedFlagID == requiredFlagID)
            {
                OpenDoorWithAnimation();
            }
        }

        private void OpenDoorInstantly()
        {
            Debug.Log($"[Door] {name} is already open.");
        }

        private void OpenDoorWithAnimation()
        {
            Debug.Log($"[Door] {name} is opening now!");
        }

        #endregion
    }
}
