using System.Collections.Generic;
using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Represents a branching narrative choice within the plot system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Decision", menuName = "Longinus/Plot System/Decision Node")]
    public class DecisionNode : ScriptableObject
    {
        #region Inspector Variables

        [Header("Decision Identity")]
        [SerializeField, Tooltip("Unique identifier for this decision.")]
        public string _decisionID;
        
        [SerializeField, Tooltip("Display name of the decision.")]
        private string _decisionName;
        
        [TextArea(3, 6), SerializeField, Tooltip("Lore or context presented to the player.")]
        private string _contextDescription;

        [Header("Type")]
        [SerializeField, Tooltip("Categorization of the decision type.")]
        private DecisionType _type;

        [Header("Conditions (Optional)")]
        [SerializeField, Tooltip("Prerequisites that must be met for this decision to be available.")]
        private List<DecisionCondition> _conditions = new List<DecisionCondition>();

        [Header("Choice A (Good/Mercy)")]
        [SerializeField, Tooltip("Text shown on the UI for the positive choice.")]
        private string _choiceAText = "Spare / Help";
        
        [TextArea(2, 4), SerializeField, Tooltip("Flavor text describing the outcome of Choice A.")]
        private string _choiceADescription;

        [SerializeReference, Tooltip("List of mechanical consequences applied if Choice A is selected.")]
        private List<Consequence> _choiceAConsequences = new List<Consequence>();

        [Header("Choice B (Bad/Cruelty)")]
        [SerializeField, Tooltip("Text shown on the UI for the negative choice.")]
        private string _choiceBText = "Finish Off / Refuse";
        
        [TextArea(2, 4), SerializeField, Tooltip("Flavor text describing the outcome of Choice B.")]
        private string _choiceBDescription;
        
        [SerializeReference, Tooltip("List of mechanical consequences applied if Choice B is selected.")]
        private List<Consequence> _choiceBConsequences = new List<Consequence>();

        [Header("Linked Data (Optional)")]
        [SerializeField, Tooltip("Flag indicating if this decision involves a boss execution/sparing phase.")]
        private bool _isBossFight;
        
        [SerializeField, Tooltip("The boss data associated with this node.")]
        private BossData _linkedBoss; 
        
        [SerializeField, Tooltip("The NPC data associated with this node.")]
        private NPCData _linkedNPC;

        [SerializeField, Tooltip("Optional mini-game prefab to instantiate before resolving the decision.")]
        private GameObject _miniGamePrefab;

        #endregion

        #region Public Properties

        public string DecisionID 
        { 
            get => _decisionID; 
            set => _decisionID = value; // Setter allowed strictly for Editor ID Generation tools
        }
        
        public string DecisionName => _decisionName;
        public string ContextDescription => _contextDescription;
        public DecisionType Type => _type;
        
        // IReadOnlyList guarantees that external scripts can't accidentally add/remove items from the SO
        public IReadOnlyList<DecisionCondition> Conditions => _conditions;

        public string ChoiceAText => _choiceAText;
        public string ChoiceADescription => _choiceADescription;
        public IReadOnlyList<Consequence> ChoiceAConsequences => _choiceAConsequences;

        public string ChoiceBText => _choiceBText;
        public string ChoiceBDescription => _choiceBDescription;
        public IReadOnlyList<Consequence> ChoiceBConsequences => _choiceBConsequences;

        public bool IsBossFight => _isBossFight;
        public BossData LinkedBoss => _linkedBoss;
        public NPCData LinkedNPC => _linkedNPC;
        public GameObject MiniGamePrefab => _miniGamePrefab;

        #endregion
    }
}