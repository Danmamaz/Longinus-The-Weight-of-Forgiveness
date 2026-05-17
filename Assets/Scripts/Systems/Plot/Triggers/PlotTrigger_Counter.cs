using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Increments a named integer counter in PlotState and re-evaluates all branches.
    /// Wire to UnityEvents on prefabs (enemy OnDeath, crow feed animation event, etc.).
    /// Use cases: BR-04 enemy kill counter, BR-10 player death counter, BR-12 crow feeding.
    /// </summary>
    public class PlotTrigger_Counter : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("PlotState integer key to increment (e.g. 'enemyKills', 'CrowFeedCount').")]
        private string _intKey;

        [SerializeField, Tooltip("Amount added on each Increment() call.")]
        private int _incrementBy = 1;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Increments the counter and re-evaluates all branches so threshold-based
        /// branches (e.g. enemyKills >= 10) can fire immediately.
        /// </summary>
        public void Increment()
        {
            if (PlotManager.Instance == null) return;
            PlotManager.Instance.PlotState.AddToInt(_intKey, _incrementBy);
            PlotManager.Instance.BranchRegistry?.TryFireAll(PlotManager.Instance.PlotState);
        }

        #endregion
    }
}
