using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Central hub for story state. Reads and writes PlotState flags and broadcasts changes
    /// to all listeners via OnFlagUpdated. Persists across scenes via DontDestroyOnLoad.
    /// </summary>
    public class PlotManager : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Configuration")]
        [SerializeField] private PlotState _plotState;

        [SerializeField, Tooltip("Registry of all PlotBranch assets. Evaluated after every state change.")]
        private PlotBranchRegistry _branchRegistry;

        [Header("Global Events")]
        [Tooltip("Fires whenever any flag is set. Subscribe to update UI, unlock doors, or trigger dialogue.")]
        public UnityEvent<string> OnFlagUpdated;

        #endregion

        #region Public Properties

        public static PlotManager Instance { get; private set; }
        public PlotState PlotState => _plotState;
        public PlotBranchRegistry BranchRegistry => _branchRegistry;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Returns true if every condition in the list is satisfied by the current plot state.
        /// An empty or null list is always considered met.
        /// </summary>
        public bool AreConditionsMet(List<PlotCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;

            foreach (var condition in conditions)
            {
                if (!condition.IsMet(_plotState)) return false;
            }
            return true;
        }

        /// <summary>
        /// Applies every consequence in the list to the current plot state and fires OnFlagUpdated
        /// for each newly set flag. Re-evaluates all branches afterwards.
        /// </summary>
        public void ApplyConsequences(List<PlotConsequence> consequences)
        {
            if (consequences == null || _plotState == null) return;

            foreach (var consequence in consequences)
            {
                consequence.Apply(_plotState);

                if (consequence.SetFlag && !string.IsNullOrEmpty(consequence.FlagToSet))
                {
                    OnFlagUpdated?.Invoke(consequence.FlagToSet);
                }
            }

            if (_branchRegistry != null) _branchRegistry.TryFireAll(_plotState);
        }

        /// <summary>
        /// Sets a flag, broadcasts it to all listeners, and re-evaluates all branches.
        /// </summary>
        public void TriggerFlag(string flagId)
        {
            if (_plotState == null || string.IsNullOrEmpty(flagId)) return;

            _plotState.SetFlag(flagId);
            OnFlagUpdated?.Invoke(flagId);
            if (_branchRegistry != null) _branchRegistry.TryFireAll(_plotState);
        }

        /// <summary>
        /// Finds the branch by ID and fires it if its conditions are met.
        /// Returns false if the branch is not found, cannot fire, or has already fired.
        /// </summary>
        public bool TryFireBranch(string branchId)
        {
            if (_branchRegistry == null) return false;
            var branch = _branchRegistry.GetById(branchId);
            if (branch == null) return false;
            if (!branch.CanFire(_plotState)) return false;
            branch.Fire(_plotState);
            OnFlagUpdated?.Invoke(branchId + "_fired");
            return true;
        }

        /// <summary>
        /// Returns true if the given flag has been set.
        /// </summary>
        public bool CheckFlag(string flagId)
        {
            return _plotState != null && _plotState.HasFlag(flagId);
        }

        #endregion
    }
}
