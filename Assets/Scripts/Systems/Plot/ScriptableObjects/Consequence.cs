using UnityEngine;
using UnityEngine.Events;

namespace PlotBranching
{
    [CreateAssetMenu(fileName = "New Consequence", menuName = "Plot System/Consequence")]
    public class Consequence : ScriptableObject
    {
        [Header("Consequence Identity")]
        public string consequenceID;
        [TextArea(2, 4)]
        public string description;

        [Header("Type and Values")]
        public ConsequenceType type;

        [Header("Karma")]
        public int karmaChange = 0;

        [Header("Player Stats")]
        public string statName;
        public float statModifierValue;
        public bool isPercentage = false;

        [Header("Buffs")]
        public string buffID;
        public GameObject buffPrefab;

        [Header("World State")]
        public WorldStateType newWorldState;

        [Header("Boss / NPC Interaction")]
        // REPLACED strings with Data Objects
        public BossData bossToUnlock;
        public NPCData npcToModify;
        
        public NPCAttitude newAttitude;

        [Header("Inventory")]
        public string itemID;
        public int itemQuantityChange;

        [Header("Callbacks")]
        public UnityEvent onConsequenceApplied;
    }
}