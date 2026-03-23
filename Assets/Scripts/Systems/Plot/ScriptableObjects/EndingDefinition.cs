using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Defines the requirements and assets for a specific game ending.
    /// </summary>
    [CreateAssetMenu(fileName = "New Ending", menuName = "Longinus/Plot System/Ending Definition")]
    public class EndingDefinition : ScriptableObject
    {
        #region Inspector Variables

        [Header("Ending Identity")]
        [SerializeField, Tooltip("Unique identifier for this ending.")]
        private string _endingID;
        
        [SerializeField, Tooltip("Display name of the ending.")]
        private string _endingName;
        
        [TextArea(3, 6), SerializeField, Tooltip("Internal or lore description of the ending.")]
        private string _endingDescription;

        [Header("Karma Threshold (Absolute)")]
        [Range(-100, 100)]
        [SerializeField, Tooltip("Minimum or maximum karma required to trigger this ending.")]
        private int _karmaThreshold = 0; 
        
        [Header("Additional Conditions (Optional)")]
        [SerializeField, Tooltip("Extra plot conditions that must be met.")]
        private List<DecisionCondition> _additionalConditions = new List<DecisionCondition>();

        [Header("Cinematic/Scene")]
        [SerializeField, Tooltip("Name of the scene to load for this ending.")]
        private string _endingSceneName;
        
        [SerializeField, Tooltip("Cutscene prefab to instantiate when the ending begins.")]
        private GameObject _endingCutscenePrefab;

        [Header("Callbacks")]
        [SerializeField, Tooltip("Events triggered immediately when this ending is selected.")]
        private UnityEvent _onEndingTriggered;

        [Header("Configuration")]
        [SerializeField, Tooltip("Categorization of the ending type.")]
        private Endings _endingType;

        #endregion

        #region Public Properties

        public string EndingID => _endingID;
        public string EndingName => _endingName;
        public string EndingDescription => _endingDescription;
        public int KarmaThreshold => _karmaThreshold;
        
        public IReadOnlyList<DecisionCondition> AdditionalConditions => _additionalConditions;
        
        public string EndingSceneName => _endingSceneName;
        public GameObject EndingCutscenePrefab => _endingCutscenePrefab;
        public UnityEvent OnEndingTriggered => _onEndingTriggered;
        public Endings EndingType => _endingType;

        #endregion
    }
}