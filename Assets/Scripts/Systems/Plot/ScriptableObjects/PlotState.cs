using System.Collections.Generic;
using UnityEngine;

namespace Longinus.PlotSystem
{
    /// <summary>
    /// Tracks the current story state of a specific named NPC/boss.
    /// </summary>
    public enum KnightState { Alive, Killed, Spared }

    /// <summary>
    /// A key-value pair for a named integer counter stored in the plot state.
    /// </summary>
    [System.Serializable]
    public struct IntVariable
    {
        public string Key;
        public int Value;
    }

    /// <summary>
    /// ScriptableObject that holds all boolean story flags, integer counters, and specific enum states.
    /// Maintains both serialized lists (for saving) and runtime HashSet/Dictionary caches (for O(1) lookup).
    /// After loading from SaveSystem, always call RebuildRuntimeCaches() before querying.
    /// </summary>
    [CreateAssetMenu(fileName = "New Plot State", menuName = "Longinus/Plot System/Plot State")]
    public class PlotState : ScriptableObject
    {
        #region Constants & Inspector Variables

        [Header("Save Format")]
        [SerializeField, Tooltip("Incremented when the serialized layout changes in a breaking way.")]
        private int _saveFormatVersion = 1;

        [Header("Boolean Flags")]
        [SerializeField, Tooltip("List of all flags that have been set during this playthrough.")]
        private List<string> _activeFlags = new List<string>();

        [Header("Integer Variables")]
        [SerializeField, Tooltip("Named integer counters (kill counts, item quantities, etc.).")]
        private List<IntVariable> _intVariables = new List<IntVariable>();

        [Header("Specific States")]
        [SerializeField] private KnightState _knightState = KnightState.Alive;

        #endregion

        #region Private Variables

        private HashSet<string> _runtimeFlags;
        private Dictionary<string, int> _runtimeInts;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            RebuildRuntimeCaches();
        }

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Rebuilds the runtime HashSet and Dictionary from the serialized lists.
        /// Must be called after deserializing from SaveSystem.
        /// </summary>
        public void RebuildRuntimeCaches()
        {
            _runtimeFlags = new HashSet<string>(_activeFlags);

            _runtimeInts = new Dictionary<string, int>();
            foreach (var iv in _intVariables)
            {
                _runtimeInts[iv.Key] = iv.Value;
            }
        }

        /// <summary>
        /// Clears all flags, counters, and enum states and rebuilds caches.
        /// </summary>
        public void ResetState()
        {
            _activeFlags.Clear();
            _intVariables.Clear();
            _knightState = KnightState.Alive;
            RebuildRuntimeCaches();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ResetAllPlotStatesOnPlay(
            UnityEditor.EnterPlayModeOptions options)
        {
            // Find all PlotState assets and reset them when entering play mode
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:PlotState");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                PlotState ps = UnityEditor.AssetDatabase.LoadAssetAtPath<PlotState>(path);
                if (ps != null) ps.ResetState();
            }
        }
#endif

        #endregion

        #region Boolean Flags Logic

        /// <summary>
        /// Sets a flag permanently. No-op if already set.
        /// </summary>
        public void SetFlag(string flagId)
        {
            if (_runtimeFlags == null) RebuildRuntimeCaches();

            if (_runtimeFlags.Add(flagId))
            {
                _activeFlags.Add(flagId);
                Debug.Log($"[PlotState] Flag Set: {flagId}");
            }
        }

        /// <summary>
        /// Returns true if the flag has been set.
        /// </summary>
        public bool HasFlag(string flagId)
        {
            if (_runtimeFlags == null) RebuildRuntimeCaches();
            return _runtimeFlags.Contains(flagId);
        }

        /// <summary>
        /// Removes a flag. No-op if not set.
        /// </summary>
        public void RemoveFlag(string flagId)
        {
            if (_runtimeFlags == null) RebuildRuntimeCaches();

            if (_runtimeFlags.Remove(flagId))
            {
                _activeFlags.Remove(flagId);
                Debug.Log($"[PlotState] Flag Removed: {flagId}");
            }
        }

        #endregion

        #region Integer Counters Logic

        /// <summary>
        /// Sets a named integer counter to an explicit value.
        /// </summary>
        public void SetInt(string key, int value)
        {
            if (_runtimeInts == null) RebuildRuntimeCaches();

            _runtimeInts[key] = value;

            int index = _intVariables.FindIndex(v => v.Key == key);
            if (index >= 0)
            {
                _intVariables[index] = new IntVariable { Key = key, Value = value };
            }
            else
            {
                _intVariables.Add(new IntVariable { Key = key, Value = value });
            }

            Debug.Log($"[PlotState] Variable Updated: {key} = {value}");
        }

        /// <summary>
        /// Returns the value of a named counter, or 0 if it has never been set.
        /// </summary>
        public int GetInt(string key)
        {
            if (_runtimeInts == null) RebuildRuntimeCaches();
            return _runtimeInts.TryGetValue(key, out int val) ? val : 0;
        }

        /// <summary>
        /// Adds an amount to a named counter (creates it at 0 if it does not exist).
        /// </summary>
        public void AddToInt(string key, int amount)
        {
            SetInt(key, GetInt(key) + amount);
        }

        #endregion

        #region Public Properties

        public int SaveFormatVersion => _saveFormatVersion;

        /// <summary>
        /// Returns false if a save file was written with a newer format version than this asset supports.
        /// </summary>
        public bool IsCompatibleVersion(int loadedVersion) => loadedVersion <= _saveFormatVersion;

        #endregion

        #region Specific Enums Logic

        public KnightState CurrentKnightState
        {
            get => _knightState;
            set => _knightState = value;
        }

        #endregion
    }
}
