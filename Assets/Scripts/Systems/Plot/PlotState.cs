using UnityEngine;
using System.Collections.Generic;

namespace PlotBranching
{
    [CreateAssetMenu(fileName = "PlotState", menuName = "Plot System/Plot State")]
    public class PlotState : ScriptableObject
    {
        [Header("Karma (Absolute Scale)")]
        public int currentKarma = 0;
        
        // Hard-coded thresholds (Souls-like opacity)
        public const int ABSOLUTE_MIN_KARMA = -100;
        public const int ABSOLUTE_MAX_KARMA = 100;
        
        // Ending thresholds (designer-tunable via inspector)
        [Header("Ending Thresholds")]
        [Tooltip("Karma >= this value triggers Good ending")]
        public int goodEndingThreshold = 60;
        
        [Tooltip("Karma <= this value triggers Bad ending")]
        public int badEndingThreshold = -40;
        
        // Between badEndingThreshold and goodEndingThreshold = Neutral ending

        [Header("Decision History")]
        public List<string> madeDecisionIDs = new List<string>();
        public List<string> chosenOptions = new List<string>(); // "A" or "B"

        [Header("World State")]
        public WorldStateType currentWorldState = WorldStateType.Normal;

        [Header("Active Buffs")]
        public List<string> activeBuffIDs = new List<string>();

        [Header("Unlocked Bosses")]
        public List<string> unlockedBossIDs = new List<string>();

        [Header("NPC Attitudes")]
        public Dictionary<string, NPCAttitude> npcAttitudes = new Dictionary<string, NPCAttitude>();

        /// <summary>
        /// Changes karma with absolute limits
        /// </summary>
        public void ChangeKarma(int amount)
        {
            int oldKarma = currentKarma;
            currentKarma = Mathf.Clamp(currentKarma + amount, ABSOLUTE_MIN_KARMA, ABSOLUTE_MAX_KARMA);
            
            if (currentKarma != oldKarma)
            {
                Debug.Log($"Karma: {oldKarma} → {currentKarma} ({amount:+#;-#;0})");
            }
        }

        /// <summary>
        /// Checks if karma meets threshold for good ending
        /// </summary>
        public bool IsGoodEnding() => currentKarma >= goodEndingThreshold;

        /// <summary>
        /// Checks if karma meets threshold for bad ending
        /// </summary>
        public bool IsBadEnding() => currentKarma <= badEndingThreshold;

        /// <summary>
        /// Neutral ending is everything in between
        /// </summary>
        public bool IsNeutralEnding() => !IsGoodEnding() && !IsBadEnding();

        /// <summary>
        /// Generic threshold check
        /// </summary>
        public bool IsKarmaAbove(int threshold) => currentKarma >= threshold;
        public bool IsKarmaBelow(int threshold) => currentKarma <= threshold;

        /// <summary>
        /// Resets state to default (for new game)
        /// </summary>
        public void ResetState()
        {
            currentKarma = 0;
            madeDecisionIDs.Clear();
            chosenOptions.Clear();
            currentWorldState = WorldStateType.Normal;
            activeBuffIDs.Clear();
            unlockedBossIDs.Clear();
            npcAttitudes.Clear();
        }
    }
}