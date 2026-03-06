using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace PlotBranching
{
    public class PlotManager : MonoBehaviour
    {
        public static PlotManager Instance { get; private set; }

        [Header("Configuration")]
        public PlotState plotState;
        public List<EndingDefinition> possibleEndings = new List<EndingDefinition>();

        [Header("Events")]
        public UnityEvent<int> onKarmaChanged;
        public UnityEvent<string> onChoiceMade;
        public UnityEvent<WorldStateType> onWorldStateChanged;
        public UnityEvent<EndingDefinition> onEndingTriggered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (plotState == null)
            {
                Debug.LogError("PlotManager: PlotState is not assigned!");
            }
        }

        public void RegisterDecision(DecisionNode decision, bool choseA)
        {
            if (decision == null) return;

            // Track decision
            plotState.madeDecisionIDs.Add(decision.decisionID);
            plotState.chosenOptions.Add(choseA ? "A" : "B");

            // Apply consequences
            List<Consequence> consequences = choseA ? decision.choiceAConsequences : decision.choiceBConsequences;
            foreach (var consequence in consequences)
            {
                ApplyConsequence(consequence);
            }

            onChoiceMade?.Invoke(decision.decisionID);
            Debug.Log($"PlotManager: Decision '{decision.decisionName}' made. Choice: {(choseA ? "A" : "B")}. Karma: {plotState.currentKarma}");
        }

        private void ApplyConsequence(Consequence consequence)
        {
            if (consequence == null) return;

            switch (consequence.type)
            {
                case ConsequenceType.KarmaChange:
                    ChangeKarma(consequence.karmaChange);
                    break;

                case ConsequenceType.StatModifier:
                    // TODO: Add your stat system integration here
                    break;

                case ConsequenceType.BuffAdd:
                    // TODO: Add buff system integration here
                    break;

                case ConsequenceType.BuffRemove:
                    // TODO: Add buff removal here
                    break;
                
                case ConsequenceType.WorldStateChange:
                    ChangeWorldState(consequence.newWorldState);
                    break;

                case ConsequenceType.UnlockBoss:
                    // FIX: Check if the BossData object is assigned, then get its ID
                    if (consequence.bossToUnlock != null)
                    {
                        UnlockBoss(consequence.bossToUnlock.bossID);
                    }
                    else
                    {
                        Debug.LogError($"Consequence '{consequence.name}' tries to unlock a boss, but no BossData is assigned!");
                    }
                    break;
                    
                case ConsequenceType.NPCAttitudeChange:
                    // FIX: Check if the NPCData object is assigned, then get its ID
                    if (consequence.npcToModify != null)
                    {
                        ChangeNPCAttitude(consequence.npcToModify.npcID, consequence.newAttitude);
                    }
                    else
                    {
                        Debug.LogError($"Consequence '{consequence.name}' tries to change NPC attitude, but no NPCData is assigned!");
                    }
                    break;
                
                case ConsequenceType.InventoryModify:
                    // TODO: Add inventory system integration
                    break;
                
                case ConsequenceType.OpenPath:
                    consequence.objectToRemove.SetActive(false);
                    break;
            }

            consequence.onConsequenceApplied?.Invoke();
        }

        public void ChangeKarma(int amount)
        {
            // Use the absolute constants from the new PlotState
            plotState.ChangeKarma(amount);
            onKarmaChanged?.Invoke(plotState.currentKarma);
        }

        private void ChangeWorldState(WorldStateType newState)
        {
            plotState.currentWorldState = newState;
            onWorldStateChanged?.Invoke(newState);
        }

        private void UnlockBoss(string bossID)
        {
            if (!plotState.unlockedBossIDs.Contains(bossID))
            {
                plotState.unlockedBossIDs.Add(bossID);
            }
        }
        
        private void ChangeNPCAttitude(string npcID, NPCAttitude attitude)
        {
            if (plotState.npcAttitudes.ContainsKey(npcID))
                plotState.npcAttitudes[npcID] = attitude;
            else
                plotState.npcAttitudes.Add(npcID, attitude);
        }

        public bool AreConditionsMet(List<DecisionCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;
            foreach (var condition in conditions)
            {
                if (!IsConditionMet(condition)) return false;
            }
            return true;
        }

        private bool IsConditionMet(DecisionCondition condition)
        {
            switch (condition.type)
            {
                case ConditionType.PreviousChoiceMade:
                    return plotState.madeDecisionIDs.Contains(condition.targetID);

                case ConditionType.KarmaThreshold:
                    return CompareValue(plotState.currentKarma, condition.comparison, condition.value);

                default:
                    return true; // Placeholder for other conditions
            }
        }

        private bool CompareValue(float actual, ComparisonOperator op, float target)
        {
            switch (op)
            {
                case ComparisonOperator.GreaterThan: return actual > target;
                case ComparisonOperator.LessThan: return actual < target;
                case ComparisonOperator.EqualTo: return Mathf.Approximately(actual, target);
                case ComparisonOperator.GreaterOrEqual: return actual >= target;
                case ComparisonOperator.LessOrEqual: return actual <= target;
                default: return false;
            }
        }

        public void TriggerEnding()
        {
            EndingDefinition ending = DetermineEnding();

            if (ending != null)
            {
                Debug.Log($"PlotManager: Triggering ending '{ending.endingName}'");
                onEndingTriggered?.Invoke(ending);
                ending.onEndingTriggered?.Invoke();
            }
            else
            {
                Debug.LogError("PlotManager: No valid ending found!");
            }
        }

        private EndingDefinition DetermineEnding()
        {
            // Simple check: Good -> Bad -> Neutral
            // This replaces the old "percentage" check
            
            if (plotState.IsGoodEnding())
            {
                return possibleEndings.FirstOrDefault(e => e.endingID.Contains("Good") || e.endingID.Contains("Saint"));
            }
            
            if (plotState.IsBadEnding())
            {
                return possibleEndings.FirstOrDefault(e => e.endingID.Contains("Bad") || e.endingID.Contains("Heretic"));
            }

            // Fallback to finding one that matches the threshold
            foreach (var ending in possibleEndings.OrderByDescending(e => e.karmaThreshold))
            {
                if (plotState.currentKarma >= ending.karmaThreshold && 
                    AreConditionsMet(ending.additionalConditions))
                {
                    return ending;
                }
            }
            
            return null;
        }
    }
}