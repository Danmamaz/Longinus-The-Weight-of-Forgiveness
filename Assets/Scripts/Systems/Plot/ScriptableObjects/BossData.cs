using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Contains static data defining a boss entity within the plot system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Boss Data", menuName = "Longinus/Plot System/Boss Data")]
    public class BossData : ScriptableObject
    {
        #region Inspector Variables

        [Header("Identity")]
        [SerializeField, Tooltip("Unique ID used for save files (e.g., 'BOSS_VEPAR').")]
        private string _bossID;
        
        [SerializeField, Tooltip("The localized display name of the boss.")]
        private string _bossName;

        [Header("Assets")]
        [SerializeField, Tooltip("The physical prefab instantiated for the boss fight.")]
        private GameObject _bossPrefab;
        
        [SerializeField, Tooltip("2D icon used in UI representations.")]
        private Sprite _bossIcon;

        [TextArea, SerializeField, Tooltip("Lore description or internal notes about the boss.")]
        private string _description;

        #endregion

        #region Public Properties

        public string BossID => _bossID;
        public string BossName => _bossName;
        public GameObject BossPrefab => _bossPrefab;
        public Sprite BossIcon => _bossIcon;
        public string Description => _description;

        #endregion
    }
}