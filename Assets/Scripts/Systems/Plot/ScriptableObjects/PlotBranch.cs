using System.Collections.Generic;
using UnityEngine;

namespace Longinus.PlotSystem
{
    public enum BranchType { Major, Medium, Minor }

    /// <summary>
    /// A data-driven story branch. Defines the conditions that must be met for it to fire
    /// and the consequences applied when it does. Fire-once: the BranchId + "_fired" flag
    /// is set on first execution and blocks all subsequent attempts.
    /// Create via Longinus/Plot System/Plot Branch.
    /// </summary>
    [CreateAssetMenu(fileName = "BR-XX New Branch", menuName = "Longinus/Plot System/Plot Branch")]
    public class PlotBranch : ScriptableObject
    {
        #region Constants & Inspector Variables

        [Header("Identity")]
        [SerializeField, Tooltip("Unique identifier for this branch (e.g. 'BR-01').")]
        private string _branchId;

        [SerializeField, Tooltip("Human-readable name shown in logs and the editor.")]
        private string _displayName;

        [TextArea(2, 4)]
        [SerializeField, Tooltip("Design notes — trigger context, expected player action.")]
        private string _description;

        [SerializeField]
        private BranchType _type;

        [Header("Logic")]
        [SerializeField, Tooltip("ALL conditions must be met for this branch to fire (AND logic). Empty = always met.")]
        private List<PlotCondition> _conditions = new List<PlotCondition>();

        [SerializeField, Tooltip("Applied to PlotState when this branch fires.")]
        private List<PlotConsequence> _consequences = new List<PlotConsequence>();

        #endregion

        #region Public Properties

        public string BranchId => _branchId;
        public BranchType Type => _type;
        public IReadOnlyList<PlotCondition> Conditions => _conditions;
        public IReadOnlyList<PlotConsequence> Consequences => _consequences;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Returns true when all conditions are met and this branch has not already fired.
        /// </summary>
        public bool CanFire(PlotState state)
        {
            if (PlotManager.Instance == null || state == null) return false;
            return PlotManager.Instance.AreConditionsMet(_conditions)
                && !state.HasFlag(_branchId + "_fired");
        }

        /// <summary>
        /// Marks this branch as fired, applies its consequences, and logs the event.
        /// The fired flag is set BEFORE ApplyConsequences to prevent re-entry when
        /// ApplyConsequences triggers TryFireAll recursively.
        /// </summary>
        public void Fire(PlotState state)
        {
            if (!CanFire(state)) return;

            state.SetFlag(_branchId + "_fired");
            PlotManager.Instance.ApplyConsequences(_consequences);
            Debug.Log($"[PlotBranch] {_branchId} fired: {_displayName}");
        }

        #endregion
    }
}
