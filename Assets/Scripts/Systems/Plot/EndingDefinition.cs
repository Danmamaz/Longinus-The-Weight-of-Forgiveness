using UnityEngine;                // Required for ScriptableObject, Header, etc.
using UnityEngine.Events;         // Required for UnityEvent
using System.Collections.Generic; // Required for List<>

namespace PlotBranching
{
    [CreateAssetMenu(fileName = "New Ending", menuName = "Plot System/Ending Definition")]
    public class EndingDefinition : ScriptableObject
    {
        [Header("Ending Identity")]
        public string endingID;
        public string endingName;
        [TextArea(3, 6)]
        public string endingDescription;

        [Header("Karma Threshold (Absolute)")]
        [Range(-100, 100)]
        public int karmaThreshold = 0; 
        
        [Header("Additional Conditions (Optional)")]
        // This now works because we defined DecisionCondition in PlotStructures.cs
        public List<DecisionCondition> additionalConditions = new List<DecisionCondition>();

        [Header("Cinematic/Scene")]
        public string endingSceneName;
        public GameObject endingCutscenePrefab;

        [Header("Callbacks")]
        public UnityEvent onEndingTriggered;
    }
}