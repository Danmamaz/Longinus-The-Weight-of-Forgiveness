using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Stores persistent player-specific plot conditions and states.
    /// </summary>
    [CreateAssetMenu(fileName = "New Player Data", menuName = "Longinus/Plot System/Player Data", order = 1)]
    public class PlayerData : ScriptableObject
    {
        #region Inspector Variables
        
        [Header("Player State")]
        [SerializeField, Tooltip("Current active condition or status of the player affecting the plot.")]
        private ConditionType _conditionType;
        
        #endregion

        #region Public Properties
        
        public ConditionType CurrentConditionType => _conditionType;
        
        #endregion
    }
}