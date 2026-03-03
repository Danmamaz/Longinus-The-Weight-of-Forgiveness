using UnityEngine;

namespace PlotBranching
{
    [CreateAssetMenu(fileName = "New Boss Data", menuName = "Plot System/Boss Data")]
    public class BossData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique ID used for save files (e.g. 'BOSS_VEPAR')")]
        public string bossID;
        public string bossName;

        [Header("Assets")]
        public GameObject bossPrefab;
        public Sprite bossIcon;

        [TextArea]
        public string description;

    }
}