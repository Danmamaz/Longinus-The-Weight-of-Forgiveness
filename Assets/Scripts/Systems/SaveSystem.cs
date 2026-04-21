using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Longinus.Player;
using UnityEngine;
using Longinus.PlotSystem;

namespace Longinus.Save
{
    /// <summary>
    /// Handles saving and loading of the plot state to and from the disk.
    /// Utilizes XOR encryption and JSON serialization.
    /// </summary>
    public static class SaveSystem
    {
        #region Constants & Variables
        private const bool USE_ENCRYPTION = true;
        private const string ENCRYPTION_KEY = "Longinus_VerticalSlice_Key_2026"; 

        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.dat");
        private static string BackupPath => Path.Combine(Application.persistentDataPath, "save.backup");

        #endregion

        #region State/Core Logic

        /// <summary>
        /// Serializes and saves the current plot state to the persistent data path.
        /// </summary>
        /// <param name="state">The active plot state to save.</param>
        public static void SaveState(PlotState state, PlayerStatsManager stats, Vector3 saveLocation)
        {
            if (state == null)
            {
                Debug.LogError("[SaveSystem] Cannot save a null PlotState!");
                return;
            }

            try
            {
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupPath, true);
                }

                SaveData data = new SaveData(state, stats, saveLocation);
                string json = JsonUtility.ToJson(data, false);

                if (USE_ENCRYPTION)
                {
                    json = ProcessEncryption(json);
                }

                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] State successfully saved to {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save state: {e.Message}");
            }
        }
        public static void SaveState(PlotState state, PlayerStatsManager stats, Vector3 saveLocation, int sceneIndex)
        {
            if (state == null)
            {
                Debug.LogError("[SaveSystem] Cannot save a null PlotState!");
                return;
            }

            try
            {
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupPath, true);
                }

                SaveData data = new SaveData(state, stats, saveLocation, sceneIndex);
                string json = JsonUtility.ToJson(data, false);

                if (USE_ENCRYPTION)
                {
                    json = ProcessEncryption(json);
                }

                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] State successfully saved to {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save state: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the plot state, player stats, and location from the persistent data path.
        /// </summary>
        public static bool LoadState(PlotState state, PlayerStatsManager stats, out Vector3 loadedLocation, out int loadedSceneIndex)
        {
            loadedLocation = Vector3.zero;
            loadedSceneIndex = -1;

            if (state == null || stats == null)
            {
                Debug.LogError("[SaveSystem] Cannot load into a null PlotState or PlayerStatsManager!");
                return false;
            }

            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("[SaveSystem] No save file found. Starting fresh.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(SavePath);

                if (USE_ENCRYPTION)
                {
                    json = ProcessEncryption(json);
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);

                JsonUtility.FromJsonOverwrite(json, state);

                stats.RestoreState(
                    data._maxHealth, data._currentHealth,
                    data._maxStamina, data._currentStamina,
                    data._maxMana, data._currentMana,
                    data._maxUltimate, data._currentUltimate
                );

                loadedLocation = data._location;
                loadedSceneIndex = data._buildSceneIndex;
                
                Debug.Log("[SaveSystem] State successfully loaded.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load state. Attempting to restore backup... Error: {e.Message}");
                return RestoreBackup(state, stats, out loadedLocation, out loadedSceneIndex);
            }
        }

        /// <summary>
        /// Attempts to load the backup save file if the primary file is corrupted.
        /// </summary>
        private static bool RestoreBackup(PlotState state, PlayerStatsManager stats, out Vector3 loadedLocation, out int loadedSceneIndex)
        {
            loadedLocation = Vector3.zero;
            loadedSceneIndex = -1;

            if (!File.Exists(BackupPath))
            {
                Debug.LogError("[SaveSystem] No backup file available to restore.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(BackupPath);
                
                if (USE_ENCRYPTION)
                {
                    json = ProcessEncryption(json);
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                JsonUtility.FromJsonOverwrite(json, state);

                stats.RestoreState(
                    data._maxHealth, data._currentHealth,
                    data._maxStamina, data._currentStamina,
                    data._maxMana, data._currentMana,
                    data._maxUltimate, data._currentUltimate
                );

                loadedLocation = data._location;
                loadedSceneIndex = data._buildSceneIndex;

                Debug.LogWarning("[SaveSystem] State successfully restored from backup.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Critical Failure! Backup is also corrupted: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Applies a simple XOR cipher to the string. Symmetrical for both encryption and decryption.
        /// </summary>
        private static string ProcessEncryption(string data)
        {
            if (string.IsNullOrEmpty(data)) return data;

            char[] result = new char[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (char)(data[i] ^ ENCRYPTION_KEY[i % ENCRYPTION_KEY.Length]);
            }
            return new string(result);
        }

        /// <summary>
        /// Reads only index of the scene, where the player was saved.
        /// </summary>
        public static int GetSavedSceneIndex()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("[SaveSystem] No save file found. Returning scene by default (1).");
                return 1;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                if (USE_ENCRYPTION)
                {
                    json = ProcessEncryption(json);
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return data._buildSceneIndex;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Error reading save data: {e.Message}");
                return 1;
            }
        }

        #endregion

        #region Data Structures

        /// <summary>
        /// DTO tailored to mirror PlotState's internal serialized fields.
        /// This allows JsonUtility.FromJsonOverwrite to map values directly into the ScriptableObject
        /// without exposing private setters in the PlotState class itself.
        /// </summary>
        [Serializable]
        private class SaveData
        {
            // General Data
            public int _buildSceneIndex;
            public Vector3 _location;

            public float _maxHealth;
            public float _currentHealth;
            
            public float _maxStamina;
            public float _currentStamina;

            public float _maxMana;
            public float _currentMana;

            public float _maxUltimate;
            public float _currentUltimate;

            // Plot Data
            public int _currentKarma;
            public WorldStateType _currentWorldState;
            public List<string> _madeDecisionIDs;
            public List<string> _chosenOptions;
            public List<string> _openedPathIDs;


            public SaveData(PlotState plot, PlayerStatsManager stats, Vector3 saveLocation)
            {
                _buildSceneIndex = SceneController.Instance.currentSceneIndex;
                _location = saveLocation;
                _maxHealth = stats.MaxHealth;
                _currentHealth = stats.CurrentHealth;
                
                _maxStamina = stats.MaxStamina;
                _currentStamina = stats.CurrentStamina;

                _maxMana = stats.MaxMana;
                _currentMana = stats.CurrentMana;

                _maxUltimate = stats.MaxUltimate;
                _currentUltimate = stats.CurrentUltimate;

                _currentKarma = plot.CurrentKarma;
                _currentWorldState = plot.CurrentWorldState;
                _madeDecisionIDs = plot.MadeDecisionIDs.ToList();
                _chosenOptions = plot.ChosenOptions.ToList();
                _openedPathIDs = plot.OpenedPathIDs.ToList();
            }
            public SaveData(PlotState plot, PlayerStatsManager stats, Vector3 saveLocation, int sceneIndex)
            {
                _buildSceneIndex = sceneIndex;
                _location = saveLocation;
                _maxHealth = stats.MaxHealth;
                _currentHealth = stats.CurrentHealth;
                
                _maxStamina = stats.MaxStamina;
                _currentStamina = stats.CurrentStamina;

                _maxMana = stats.MaxMana;
                _currentMana = stats.CurrentMana;

                _maxUltimate = stats.MaxUltimate;
                _currentUltimate = stats.CurrentUltimate;

                _currentKarma = plot.CurrentKarma;
                _currentWorldState = plot.CurrentWorldState;
                _madeDecisionIDs = plot.MadeDecisionIDs.ToList();
                _chosenOptions = plot.ChosenOptions.ToList();
                _openedPathIDs = plot.OpenedPathIDs.ToList();
            }
        }

        #endregion
    }
}