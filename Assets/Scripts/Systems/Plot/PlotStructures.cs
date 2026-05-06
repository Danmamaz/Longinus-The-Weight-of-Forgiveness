using System.Collections.Generic;
using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Defines the type of check a PlotCondition evaluates against the current PlotState.
    /// </summary>
    public enum ConditionCheckType
    {
        HasFlag,
        DoesNotHaveFlag,
        IntGreaterOrEqual
    }

    /// <summary>
    /// A serializable condition evaluated against a PlotState.
    /// Used in Inspector-assignable lists to gate dialogue, doors, and events.
    /// </summary>
    [System.Serializable]
    public struct PlotCondition
    {
        [Tooltip("The type of check to perform.")]
        public ConditionCheckType CheckType;

        [Tooltip("ID of the flag or counter key to evaluate.")]
        public string Key;

        [Tooltip("Threshold value used only for IntGreaterOrEqual checks.")]
        public int RequiredAmount;

        /// <summary>
        /// Evaluates this condition against the provided state.
        /// </summary>
        public bool IsMet(PlotState state)
        {
            if (state == null) return false;

            switch (CheckType)
            {
                case ConditionCheckType.HasFlag:
                    return state.HasFlag(Key);
                case ConditionCheckType.DoesNotHaveFlag:
                    return !state.HasFlag(Key);
                case ConditionCheckType.IntGreaterOrEqual:
                    return state.GetInt(Key) >= RequiredAmount;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// A serializable side-effect applied to a PlotState, such as setting a flag or incrementing a counter.
    /// Used in Inspector-assignable lists to drive story progression.
    /// </summary>
    [System.Serializable]
    public struct PlotConsequence
    {
        [Tooltip("If true, sets the flag specified by FlagToSet.")]
        public bool SetFlag;

        [Tooltip("Flag ID to set when SetFlag is true.")]
        public string FlagToSet;

        [Tooltip("If true, adds IntAmount to the counter at IntKey.")]
        public bool ModifyInt;

        [Tooltip("Counter key to modify when ModifyInt is true.")]
        public string IntKey;

        [Tooltip("Amount to add to the counter. Can be negative.")]
        public int IntAmount;

        /// <summary>
        /// Applies this consequence to the provided state.
        /// </summary>
        public void Apply(PlotState state)
        {
            if (SetFlag && !string.IsNullOrEmpty(FlagToSet))
                state.SetFlag(FlagToSet);

            if (ModifyInt && !string.IsNullOrEmpty(IntKey))
                state.AddToInt(IntKey, IntAmount);
        }
    }
}
