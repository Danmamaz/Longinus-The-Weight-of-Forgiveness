using System;
using UnityEngine;

namespace Longinus.PlotSystem
{
    #region Enums

    /// <summary>
    /// Defines the type of condition required to unlock a decision or ending.
    /// </summary>
    public enum ConditionType
    {
        KarmaAbove,
        KarmaBelow,
        DecisionMade,
        DecisionNotMade,
        SpecificChoiceMade,
        HasItem,
        BossDefeated
    }

    /// <summary>
    /// Defines mathematical operators for evaluating numerical conditions.
    /// </summary>
    public enum ComparisonOperator
    {
        GreaterThan,
        LessThan,
        EqualTo,
        GreaterOrEqual,
        LessOrEqual
    }

    /// <summary>
    /// Represents the current visual and narrative state of the game world.
    /// </summary>
    public enum WorldStateType
    {
        Normal,
        Gloomy,
        Hopeful
    }

    /// <summary>
    /// Represents how an NPC reacts to the player based on previous decisions.
    /// </summary>
    public enum NPCAttitude
    {
        Friendly,
        Neutral,
        Hostile
    }
    
    /// <summary>
    /// Categorizes the nature of a plot decision for UI or tracking purposes.
    /// </summary>
    public enum DecisionType
    {
        HubNPCRequest,
        BossDefeat,
        WorldEvent
    }

    /// <summary>
    /// Categorizes the alignment of a game ending.
    /// </summary>
    public enum Endings
    {
        Good,
        Neutral,
        Bad
    }

    #endregion

    #region Data Structures

    /// <summary>
    /// A serializable structure defining a prerequisite condition for a plot node.
    /// </summary>
    [Serializable]
    public class DecisionCondition
    {
        [SerializeField, Tooltip("The specific rule type to evaluate.")]
        private ConditionType _conditionType;
        
        [SerializeField, Tooltip("The ID of the item, boss, or decision to check against.")]
        private string _targetID;
        
        [SerializeField, Tooltip("Operator used if comparing numerical values (like stats).")]
        private ComparisonOperator _comparison;
        
        [SerializeField, Tooltip("The numerical threshold required (e.g., Karma amount).")]
        private int _targetValue;
        
        [SerializeField, Tooltip("The specific choice ('A' or 'B') required if checking previous decisions.")]
        private string _requiredChoice;

        public ConditionType ConditionType => _conditionType;
        public string TargetID => _targetID;
        public ComparisonOperator Comparison => _comparison;
        public int TargetValue => _targetValue;
        public string RequiredChoice => _requiredChoice;
    }

    #endregion

    #region Abstract Classes

    /// <summary>
    /// Base class for all plot-related outcomes and mechanical effects.
    /// </summary>
    [Serializable]
    public abstract class Consequence
    {
        /// <summary>
        /// Executes the specific logic of this consequence via the PlotManager.
        /// </summary>
        /// <param name="context">The active PlotManager instance.</param>
        public abstract void Apply(PlotManager context);
    }

    #endregion
}