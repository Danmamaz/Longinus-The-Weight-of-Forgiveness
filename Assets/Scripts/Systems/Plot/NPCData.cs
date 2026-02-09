using UnityEngine;

namespace PlotBranching
{
    [CreateAssetMenu(fileName = "New NPC Data", menuName = "Plot System/NPC Data")]
    public class NPCData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique ID used for save files (e.g. 'NPC_MERCHANT_01')")]
        public string npcID;
        public string npcName;
        
        [Header("Visuals")]
        public Sprite portrait;
        public GameObject npcPrefab;

        [Header("State")]
        public NPCAttitude defaultAttitude = NPCAttitude.Neutral;
    }
}