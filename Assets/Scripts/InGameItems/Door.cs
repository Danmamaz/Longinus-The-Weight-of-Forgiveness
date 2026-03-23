using UnityEngine;
using Longinus.PlotSystem;
using System.Linq;

namespace Longinus.Environment
{
    /// <summary>
    /// Represents a physical barrier that responds to plot consequences (e.g., opens when a decision is made).
    /// </summary>
    public class Door : MonoBehaviour
    {
        #region Constants & Inspector Variables
        
        [Header("Configuration")]
        [SerializeField, Tooltip("Unique ID that must match the pathID in the ConsequenceSO.")]
        private string _pathId; 
        
        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (PlotManager.Instance == null) return;

            PlotManager.Instance.onPathOpened.AddListener(OnPathOpened);
            
            // Added defensive null-check for plotState
            if (PlotManager.Instance.PlotState != null && 
                PlotManager.Instance.PlotState.OpenedPathIDs.Contains(_pathId))
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (PlotManager.Instance != null)
            {
                PlotManager.Instance.onPathOpened.RemoveListener(OnPathOpened);
            }
        }

        #endregion

        #region Event Listeners/Callbacks

        /// <summary>
        /// Hides the door when the corresponding plot path is unlocked.
        /// </summary>
        private void OnPathOpened(string openedID)
        {
            if (_pathId == openedID)
            {
                gameObject.SetActive(false);
            }
        }
        
        #endregion
    }
}