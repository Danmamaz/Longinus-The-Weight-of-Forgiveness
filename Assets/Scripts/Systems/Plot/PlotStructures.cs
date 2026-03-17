using UnityEngine;
using System.Collections.Generic;

namespace PlotBranching
{
    // This file holds all the shared types so every other script can see them.

    [System.Serializable]
    public class DecisionCondition
    {
        public ConditionType type;
        public string targetID; // itemID, statName, etc.
        public ComparisonOperator comparison;
        public float value;
    }

    public enum ConditionType
    {
        HasItem,
        StatGreaterThan,
        StatLessThan,
        PreviousChoiceMade,
        KarmaThreshold
    }

    public enum ComparisonOperator
    {
        GreaterThan,
        LessThan,
        EqualTo,
        GreaterOrEqual,
        LessOrEqual
    }

    public enum WorldStateType
    {
        Normal,
        Gloomy,
        Hopeful
    }

    public enum NPCAttitude
    {
        Friendly,
        Neutral,
        Hostile
    }
    
    public enum DecisionType
    {
        HubNPCRequest,
        BossDefeat,
        WorldEvent
    }

    public enum Endings
    {
        Good,
        Neutral,
        Bad
    }

    [System.Serializable]
    public abstract class Consequence
    {
        public abstract void Apply(PlotManager context);
    }
}