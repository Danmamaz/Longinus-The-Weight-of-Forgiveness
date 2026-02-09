using UnityEngine;
using System.Collections.Generic;

namespace PlotBranching
{
    [CreateAssetMenu(fileName = "New Decision", menuName = "Plot System/Decision Node")]
    public class DecisionNode : ScriptableObject
    {
        [Header("Decision Identity")]
        public string decisionID;
        public string decisionName;
        [TextArea(3, 6)]
        public string contextDescription;

        [Header("Type")]
        public DecisionType type; 

        [Header("Conditions (Optional)")]
        public List<DecisionCondition> conditions = new List<DecisionCondition>();

        [Header("Choice A (Good/Mercy)")]
        public string choiceAText = "Spare / Help";
        [TextArea(2, 4)]
        public string choiceADescription;
        public List<Consequence> choiceAConsequences = new List<Consequence>();

        [Header("Choice B (Bad/Cruelty)")]
        public string choiceBText = "Finish Off / Refuse";
        [TextArea(2, 4)]
        public string choiceBDescription;
        public List<Consequence> choiceBConsequences = new List<Consequence>();

        [Header("Linked Data (Optional)")]
        public bool isBossFight = false;
        
        // REPLACED string with actual Data Object
        public BossData linkedBoss; 
        
        // Added NPC link
        public NPCData linkedNPC;

        public GameObject miniGamePrefab; 
    }
}