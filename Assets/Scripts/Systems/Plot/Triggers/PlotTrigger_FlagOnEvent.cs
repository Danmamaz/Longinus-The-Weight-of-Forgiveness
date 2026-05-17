using UnityEngine;
using UnityEngine.Events;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Sets a plot flag and fires an optional UnityEvent for VFX/SFX.
    /// Wire Trigger() to existing OnDeath, OnPickup, or animation events.
    /// Use cases: BR-01 boss killing blow, BR-09 amulet loot event.
    /// </summary>
    public class PlotTrigger_FlagOnEvent : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("Flag set in PlotState when Trigger() is called.")]
        private string _flagToSet;

        [SerializeField, Tooltip("Optional VFX/SFX hook invoked after the flag is set.")]
        private UnityEvent _onFlagSet;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Sets the flag via PlotManager (which broadcasts OnFlagUpdated and evaluates all branches)
        /// then fires the local UnityEvent for scene-level feedback.
        /// </summary>
        public void Trigger()
        {
            if (PlotManager.Instance == null) return;
            PlotManager.Instance.TriggerFlag(_flagToSet);
            _onFlagSet?.Invoke();
        }

        #endregion
    }
}
