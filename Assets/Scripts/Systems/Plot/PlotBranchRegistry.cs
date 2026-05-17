using System.Collections.Generic;
using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Holds all PlotBranch assets for the current playthrough.
    /// PlotManager holds a reference and calls TryFireAll after any state change.
    /// Create via Longinus/Plot System/Branch Registry.
    /// </summary>
    [CreateAssetMenu(fileName = "BranchRegistry", menuName = "Longinus/Plot System/Branch Registry")]
    public class PlotBranchRegistry : ScriptableObject
    {
        #region Constants & Inspector Variables

        [SerializeField, Tooltip("All plot branches evaluated by TryFireAll.")]
        private List<PlotBranch> _allBranches = new List<PlotBranch>();

        #endregion

        #region Public Properties

        public IReadOnlyList<PlotBranch> AllBranches => _allBranches;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Returns the branch with the given ID, or null if not found.
        /// </summary>
        public PlotBranch GetById(string branchId)
        {
            foreach (var b in _allBranches)
            {
                if (b != null && b.BranchId == branchId) return b;
            }
            return null;
        }

        /// <summary>
        /// Evaluates all registered branches and fires any that CanFire.
        /// Called by PlotManager after every state change. Bounded recursion is safe
        /// because each branch sets its _fired flag before calling ApplyConsequences.
        /// </summary>
        public void TryFireAll(PlotState state)
        {
            foreach (var b in _allBranches)
            {
                if (b != null && b.CanFire(state))
                    b.Fire(state);
            }
        }

        #endregion
    }
}
