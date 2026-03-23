using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Central hub for tracking plot progression, making decisions, and broadcasting world state changes.
    /// </summary>
    public class PlotManager : MonoBehaviour
    {
        #region Constants & Inspector Variables

        [Header("Configuration")]
        [SerializeField, Tooltip("Reference to the active plot state data.")]
        private PlotState _plotState;

        [SerializeField, Tooltip("List of all possible endings the player can achieve.")]
        private List<EndingDefinition> _possibleEndings = new List<EndingDefinition>();

        [Header("Events")]
        [Tooltip("Triggered whenever the player's karma value changes.")]
        public UnityEvent<int> onKarmaChanged;
        
        [Tooltip("Triggered when a decision is registered. Passes the Decision ID.")]
        public UnityEvent<string> onChoiceMade;
        
        [Tooltip("Triggered when the global world state changes.")]
        public UnityEvent<WorldStateType> onWorldStateChanged;
        
        [Tooltip("Triggered when an ending condition is met and executed.")]
        public UnityEvent<EndingDefinition> onEndingTriggered;
        
        [Tooltip("Triggered when a physical path or door is unlocked. Passes the Path ID.")]
        public UnityEvent<string> onPathOpened;

        [Header("Plot Data")]
        [Tooltip("Drag all existing DecisionNode objects from the Project window here")]
        [SerializeField] private List<DecisionNode> allDecisionNodes = new List<DecisionNode>();
        private Dictionary<string, DecisionNode> _nodeDictionary;

        #endregion

        #region Public Properties

        public static PlotManager Instance { get; private set; }
        public PlotState PlotState => _plotState;

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

            if (_plotState == null)
            {
                Debug.LogError("[PlotManager] Critical Error: PlotState is not assigned!");
            }

            InitializeNodeDictionary();
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Registers a player's choice and executes all associated consequences.
        /// </summary>
        public void RegisterDecision(DecisionNode decision, bool choseA)
        {
            if (decision == null || _plotState == null) return;

            _plotState.AddDecision(decision.DecisionID, choseA ? "A" : "B");
            onChoiceMade?.Invoke(decision.DecisionID);

            var consequences = choseA ? decision.ChoiceAConsequences : decision.ChoiceBConsequences;
            
            if (consequences != null)
            {
                foreach (var consequence in consequences)
                {
                    consequence.Apply(this);
                }
            }
        }

        /// <summary>
        /// Modifies the player's karma and broadcasts the change.
        /// </summary>
        public void ChangeKarma(int amount)
        {
            if (_plotState == null) return;

            _plotState.ChangeKarma(amount);
            onKarmaChanged?.Invoke(_plotState.CurrentKarma);
        }

        /// <summary>
        /// Updates the global world state and broadcasts the change.
        /// </summary>
        public void ChangeWorldState(WorldStateType newState)
        {
            if (_plotState == null) return;

            _plotState.SetWorldState(newState);
            onWorldStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Unlocks a path or door and broadcasts the event.
        /// </summary>
        public void OpenPath(string pathId)
        {
            if (string.IsNullOrEmpty(pathId) || _plotState == null) return;

            _plotState.AddOpenedPath(pathId);
            onPathOpened?.Invoke(pathId);
        }

        /// <summary>
        /// Evaluates the current plot state and triggers the appropriate ending.
        /// </summary>
        public void TriggerEnding()
        {
            EndingDefinition ending = DetermineEnding();

            if (ending != null)
            {
                Debug.Log($"[PlotManager] Triggering ending: {ending.EndingName}");
                onEndingTriggered?.Invoke(ending);
                ending.OnEndingTriggered?.Invoke();
            }
            else
            {
                Debug.LogError("[PlotManager] No valid ending found based on current plot state!");
            }
        }

        /// <summary>
        /// Determines which ending should play based on karma and conditional logic.
        /// </summary>
        private EndingDefinition DetermineEnding()
        {
            if (_plotState == null || _possibleEndings == null) return null;

            if (_plotState.IsGoodEnding())
            {
                return _possibleEndings.FirstOrDefault(e => e.EndingType == Endings.Good);
            }
            
            if (_plotState.IsBadEnding())
            {
                return _possibleEndings.FirstOrDefault(e => e.EndingType == Endings.Bad);
            }

            // Fallback: evaluate absolute karma thresholds and specific conditions
            foreach (var ending in _possibleEndings.OrderByDescending(e => e.KarmaThreshold))
            {
                if (_plotState.CurrentKarma >= ending.KarmaThreshold && AreConditionsMet(ending.AdditionalConditions))
                {
                    return ending;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Checks if a set of plot conditions are satisfied.
        /// </summary>
        private bool AreConditionsMet(IReadOnlyList<DecisionCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0) return true;

            foreach (var condition in conditions)
            {
                if (!EvaluateCondition(condition)) return false;
            }
            
            return true;
        }

        /// <summary>
        /// Evaluates a single plot condition against the current PlotState.
        /// </summary>
        private bool EvaluateCondition(DecisionCondition condition)
        {
            if (_plotState == null) return false;

            switch (condition.ConditionType)
            {
                case ConditionType.KarmaAbove:
                    return _plotState.IsKarmaAbove(condition.TargetValue);
                
                case ConditionType.KarmaBelow:
                    return _plotState.IsKarmaBelow(condition.TargetValue);
                
                case ConditionType.DecisionMade:
                    return _plotState.MadeDecisionIDs.Contains(condition.TargetID);
                
                case ConditionType.DecisionNotMade:
                    return !_plotState.MadeDecisionIDs.Contains(condition.TargetID);
                
                case ConditionType.SpecificChoiceMade:
                    // Avoid LINQ or memory allocation by running a fast raw loop
                    for (int i = 0; i < _plotState.MadeDecisionIDs.Count; i++)
                    {
                        if (_plotState.MadeDecisionIDs[i] == condition.TargetID)
                        {
                            return _plotState.ChosenOptions[i] == condition.RequiredChoice;
                        }
                    }
                    return false;
                
                case ConditionType.HasItem:
                    // Pragmatic placeholder: To be implemented when Inventory System is ready
                    return false;
                
                case ConditionType.BossDefeated:
                    return _plotState.UnlockedBossIDs.Contains(condition.TargetID);
                
                default: 
                    return false;
            }
        }

        private void InitializeNodeDictionary()
        {
            _nodeDictionary = new Dictionary<string, DecisionNode>();

            foreach (var node in allDecisionNodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.DecisionID)) 
                {
                    if (!_nodeDictionary.ContainsKey(node.DecisionID))
                    {
                        _nodeDictionary.Add(node.DecisionID, node);
                    }
                    else
                    {
                        Debug.LogWarning($"[PlotManager] Dublicate ID detected: {node.DecisionID}. Ignoring.");
                    }
                }
            }
        }

        /// <summary>
        /// Returns DecisionNode by his ID.
        /// </summary>
        public DecisionNode GetNodeByID(string id)
        {
            if (string.IsNullOrEmpty(id)) 
                return null;

            if (_nodeDictionary.TryGetValue(id, out DecisionNode foundNode))
            {
                return foundNode;
            }

            Debug.LogError($"[PlotManager] Attempt to search nonexistent DecisionNode with ID: {id}");
            return null;
        }

        #endregion
    }
}