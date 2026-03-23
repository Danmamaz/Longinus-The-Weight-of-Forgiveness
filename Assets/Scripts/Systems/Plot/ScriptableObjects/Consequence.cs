using UnityEngine;
using UnityEngine.Events;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Defines a specific outcome or mechanical effect triggered by a player's plot decision.
    /// </summary>
    [CreateAssetMenu(fileName = "New Consequence", menuName = "Longinus/Plot System/Consequence")]
    public class ConsequenceSO : ScriptableObject
    {
        #region Inspector Variables

        [Header("Consequence Identity")]
        [SerializeField, Tooltip("Unique identifier for this specific consequence.")]
        private string _consequenceID;
        
        [TextArea(2, 4), SerializeField, Tooltip("Internal description for design purposes.")]
        private string _description;

        [Header("Type and Values")]
        [SerializeField, Tooltip("Defines what mechanical system this consequence affects.")]
        private Consequence _type;

        [Header("Karma")]
        [SerializeField, Tooltip("Amount of karma gained or lost.")]
        private int _karmaChange = 0;

        [Header("Player Stats")]
        [SerializeField, Tooltip("Name of the stat to modify.")]
        private string _statName;
        
        [SerializeField, Tooltip("Value to add, subtract, or multiply.")]
        private float _statModifierValue;
        
        [SerializeField, Tooltip("If true, the modifier is applied as a percentage.")]
        private bool _isPercentage;

        [Header("Buffs")]
        [SerializeField, Tooltip("Identifier for the status effect to apply.")]
        private string _buffID;
        
        [SerializeField, Tooltip("Prefab for the visual/mechanical buff effect.")]
        private GameObject _buffPrefab;

        [Header("World State")]
        [SerializeField, Tooltip("The new world state to transition into.")]
        private WorldStateType _newWorldState;

        [Header("Boss / NPC Interaction")]
        [SerializeField, Tooltip("Boss entity unlocked or affected by this consequence.")]
        private BossData _bossToUnlock;
        
        [SerializeField, Tooltip("NPC entity affected by this consequence.")]
        private NPCData _npcToModify;
        
        [SerializeField, Tooltip("The new attitude the NPC will have towards the player.")]
        private NPCAttitude _newAttitude;

        [Header("Inventory")]
        [SerializeField, Tooltip("ID of the item to add or remove.")]
        private string _itemID;
        
        [SerializeField, Tooltip("Amount of the item to add or remove.")]
        private int _itemQuantityChange;

        [Header("Open Path")]
        [SerializeField, Tooltip("ID of the physical path, door, or barrier to unlock.")]
        private string _pathID;

        [Header("Callbacks")]
        [SerializeField, Tooltip("Custom events triggered when this consequence executes.")]
        private UnityEvent _onConsequenceApplied;

        #endregion

        #region Public Properties

        public string ConsequenceID => _consequenceID;
        public string Description => _description;
        public Consequence Type => _type;
        public int KarmaChange => _karmaChange;
        public string StatName => _statName;
        public float StatModifierValue => _statModifierValue;
        public bool IsPercentage => _isPercentage;
        public string BuffID => _buffID;
        public GameObject BuffPrefab => _buffPrefab;
        public WorldStateType NewWorldState => _newWorldState;
        public BossData BossToUnlock => _bossToUnlock;
        public NPCData NpcToModify => _npcToModify;
        public NPCAttitude NewAttitude => _newAttitude;
        public string ItemID => _itemID;
        public int ItemQuantityChange => _itemQuantityChange;
        public string PathID => _pathID;
        public UnityEvent OnConsequenceApplied => _onConsequenceApplied;

        #endregion
    }
}