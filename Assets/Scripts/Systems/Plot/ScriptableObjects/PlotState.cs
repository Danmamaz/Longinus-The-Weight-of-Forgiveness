using System.Collections.Generic;
using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Represents the dynamic runtime state of the game's plot, choices, and world conditions.
    /// </summary>
    [CreateAssetMenu(fileName = "New Plot State", menuName = "Longinus/Plot System/Plot State")]
    public class PlotState : ScriptableObject
    {
        #region Constants & Inspector Variables

        public const int ABSOLUTE_MIN_KARMA = -100;
        public const int ABSOLUTE_MAX_KARMA = 100;

        [Header("Karma (Absolute Scale)")]
        [SerializeField, Tooltip("Current karma alignment of the player.")]
        private int _currentKarma = 0;
        
        [Header("Ending Thresholds")]
        [SerializeField, Tooltip("Karma >= this value triggers the Good ending.")]
        private int _goodEndingThreshold = 60;
        
        [SerializeField, Tooltip("Karma <= this value triggers the Bad ending.")]
        private int _badEndingThreshold = -40;

        [Header("Decision History")]
        [SerializeField] private List<string> _madeDecisionIDs = new List<string>();
        [SerializeField] private List<string> _chosenOptions = new List<string>(); // "A" or "B"

        [Header("World State")]
        [SerializeField] private WorldStateType _currentWorldState = WorldStateType.Normal;
        [SerializeField] private List<string> _openedPathIDs = new List<string>();

        [Header("Active Buffs")]
        [SerializeField] private List<string> _activeBuffIDs = new List<string>();

        [Header("Unlocked Bosses")]
        [SerializeField] private List<string> _unlockedBossIDs = new List<string>();

        // Note: Standard Dictionaries do not serialize in the Unity Inspector by default.
        // If you need to view this in the inspector, you will need a custom serializable dictionary wrapper.
        private Dictionary<string, NPCAttitude> _npcAttitudes = new Dictionary<string, NPCAttitude>();

        #endregion

        #region Public Properties

        public int CurrentKarma => _currentKarma;
        public int GoodEndingThreshold => _goodEndingThreshold;
        public int BadEndingThreshold => _badEndingThreshold;
        public WorldStateType CurrentWorldState => _currentWorldState;

        // IReadOnlyList ensures external scripts can iterate but cannot accidentally add/remove/clear data
        public IReadOnlyList<string> MadeDecisionIDs => _madeDecisionIDs;
        public IReadOnlyList<string> ChosenOptions => _chosenOptions;
        public IReadOnlyList<string> OpenedPathIDs => _openedPathIDs;
        public IReadOnlyList<string> ActiveBuffIDs => _activeBuffIDs;
        public IReadOnlyList<string> UnlockedBossIDs => _unlockedBossIDs;

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Changes karma with absolute limits and logs the variation.
        /// </summary>
        public void ChangeKarma(int amount)
        {
            int oldKarma = _currentKarma;
            _currentKarma = Mathf.Clamp(_currentKarma + amount, ABSOLUTE_MIN_KARMA, ABSOLUTE_MAX_KARMA);
            
            if (_currentKarma != oldKarma)
            {
                Debug.Log($"[PlotState] Karma: {oldKarma} → {_currentKarma} ({amount:+#;-#;0})");
            }
        }

        public bool IsGoodEnding() => _currentKarma >= _goodEndingThreshold;
        public bool IsBadEnding() => _currentKarma <= _badEndingThreshold;
        public bool IsNeutralEnding() => !IsGoodEnding() && !IsBadEnding();
        public bool IsKarmaAbove(int threshold) => _currentKarma >= threshold;
        public bool IsKarmaBelow(int threshold) => _currentKarma <= threshold;

        /// <summary>
        /// Registers a player's choice to the persistent plot state.
        /// </summary>
        public void AddDecision(string decisionId, string chosenOption)
        {
            if (!_madeDecisionIDs.Contains(decisionId))
            {
                _madeDecisionIDs.Add(decisionId);
                _chosenOptions.Add(chosenOption);
            }
        }

        /// <summary>
        /// Updates or adds the attitude of a specific NPC.
        /// </summary>
        public void SetNPCAttitude(string npcId, NPCAttitude attitude)
        {
            _npcAttitudes[npcId] = attitude;
        }

        /// <summary>
        /// Retrieves the current attitude of an NPC.
        /// </summary>
        public bool TryGetNPCAttitude(string npcId, out NPCAttitude attitude)
        {
            return _npcAttitudes.TryGetValue(npcId, out attitude);
        }

        /// <summary>
        /// Resets the plot state to default values (useful for New Game).
        /// </summary>
        public void ResetState()
        {
            _currentKarma = 0;
            _currentWorldState = WorldStateType.Normal;
            
            _madeDecisionIDs.Clear();
            _chosenOptions.Clear();
            _openedPathIDs.Clear();
            _activeBuffIDs.Clear();
            _unlockedBossIDs.Clear();
            _npcAttitudes.Clear();
        }

        public void SetWorldState(WorldStateType newState)
        {
            _currentWorldState = newState;
        }

        public void AddOpenedPath(string pathId)
        {
            if (!_openedPathIDs.Contains(pathId))
            {
                _openedPathIDs.Add(pathId);
            }
        }

        #endregion
    }
}