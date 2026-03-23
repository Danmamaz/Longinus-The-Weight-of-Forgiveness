using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Contains static data defining a non-playable character (NPC) within the plot system.
    /// </summary>
    [CreateAssetMenu(fileName = "New NPC Data", menuName = "Longinus/Plot System/NPC Data")]
    public class NPCData : ScriptableObject
    {
        #region Inspector Variables

        [Header("Identity")]
        [SerializeField, Tooltip("Unique ID used for save files (e.g., 'NPC_MERCHANT_01').")]
        private string _npcID;
        
        [SerializeField, Tooltip("The localized display name of the NPC.")]
        private string _npcName;
        
        [Header("Visuals")]
        [SerializeField, Tooltip("2D portrait used in dialog UI.")]
        private Sprite _portrait;
        
        [SerializeField, Tooltip("The physical prefab instantiated in the world.")]
        private GameObject _npcPrefab;

        [Header("State")]
        [SerializeField, Tooltip("The baseline attitude this NPC has towards the player before any interactions.")]
        private NPCAttitude _defaultAttitude = NPCAttitude.Neutral;

        #endregion

        #region Public Properties

        public string NpcID => _npcID;
        public string NpcName => _npcName;
        public Sprite Portrait => _portrait;
        public GameObject NpcPrefab => _npcPrefab;
        public NPCAttitude DefaultAttitude => _defaultAttitude;

        #endregion
    }
}