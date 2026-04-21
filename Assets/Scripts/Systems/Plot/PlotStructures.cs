using System.Collections.Generic;
using UnityEngine;

namespace Longinus.PlotSystem
{
    public enum ConditionCheckType { HasFlag, DoesNotHaveFlag, IntGreaterOrEqual }

    [System.Serializable]
    public struct PlotCondition
    {
        [Tooltip("What is checked")]
        public ConditionCheckType CheckType;
        
        [Tooltip("ID of the flag or key")]
        public string Key; 
        
        [Tooltip("Variable only for counters")]
        public int RequiredAmount; 
        
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

    [System.Serializable]
    public struct PlotConsequence
    {
        [Tooltip("If true, will set this flag")]
        public bool SetFlag;
        public string FlagToSet;
        
        [Tooltip("If true, will add this variables to counter")]
        public bool ModifyInt;
        public string IntKey;
        public int IntAmount;

        public void Apply(PlotState state)
        {
            if (SetFlag && !string.IsNullOrEmpty(FlagToSet)) 
                state.SetFlag(FlagToSet);
                
            if (ModifyInt && !string.IsNullOrEmpty(IntKey)) 
                state.AddToInt(IntKey, IntAmount);
        }
    }
}