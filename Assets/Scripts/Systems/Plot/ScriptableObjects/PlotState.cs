using System.Collections.Generic;
using UnityEngine;

namespace Longinus.PlotSystem
{
    public enum KnightState { Alive, Killed, Spared }

    [System.Serializable]
    public struct IntVariable
    {
        public string Key;
        public int Value;
    }

    /// <summary>
    /// Plot state that controlls current plot.
    /// </summary>
    [CreateAssetMenu(fileName = "New Plot State", menuName = "Longinus/Plot System/Plot State")]
    public class PlotState : ScriptableObject
    {
        [Header("Boolean Flags")]
        [SerializeField, Tooltip("List of all performed actions")]
        private List<string> _activeFlags = new List<string>();

        [Header("Integer Variables")]
        [SerializeField, Tooltip("Counters")]
        private List<IntVariable> _intVariables = new List<IntVariable>();

        [Header("Specific States")]
        [SerializeField] private KnightState _knightState = KnightState.Alive;

        private HashSet<string> _runtimeFlags;
        private Dictionary<string, int> _runtimeInts;

        private void OnEnable()
        {
            RebuildRuntimeCaches();
        }

        /// <summary>
        /// Used after loading data from SaveSystem
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

        #region Boolean Flags Logic

        public void SetFlag(string flagId)
        {
            if (_runtimeFlags == null) RebuildRuntimeCaches();
            
            if (_runtimeFlags.Add(flagId))
            {
                _activeFlags.Add(flagId);
                Debug.Log($"[PlotState] Flag Set: {flagId}");
            }
        }

        public bool HasFlag(string flagId)
        {
            if (_runtimeFlags == null) RebuildRuntimeCaches();
            return _runtimeFlags.Contains(flagId);
        }

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

        public int GetInt(string key)
        {
            if (_runtimeInts == null) RebuildRuntimeCaches();
            return _runtimeInts.TryGetValue(key, out int val) ? val : 0;
        }

        public void AddToInt(string key, int amount)
        {
            SetInt(key, GetInt(key) + amount);
        }

        #endregion

        #region Specific Enums Logic

        public KnightState CurrentKnightState
        {
            get => _knightState;
            set => _knightState = value;
        }

        #endregion

        public void ResetState()
        {
            _activeFlags.Clear();
            _intVariables.Clear();
            _knightState = KnightState.Alive;
            RebuildRuntimeCaches();
        }
    }
}