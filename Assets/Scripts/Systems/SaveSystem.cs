using System;
using System.IO;
using UnityEngine;
using Longinus.Player;
using Longinus.PlotSystem;
using Longinus.Systems;

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

        public static void SaveState(PlotState state, PlayerStatsManager stats, Vector3 saveLocation)
        {
            // Fallback до поточного індексу сцени, якщо він не переданий явно
            SaveState(state, stats, saveLocation, SceneController.Instance != null ? SceneController.Instance.currentSceneIndex : 1);
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

                // Restore PlotState from embedded JSON
                if (!string.IsNullOrEmpty(data._plotStateJson))
                {
                    JsonUtility.FromJsonOverwrite(data._plotStateJson, state);
                    state.RebuildRuntimeCaches();

                    if (!state.IsCompatibleVersion(data._saveFormatVersion))
                    {
                        Debug.LogWarning($"[SaveSystem] Save format version {data._saveFormatVersion} " +
                                         $"is newer than PlotState version {state.SaveFormatVersion}. " +
                                         "Some flags may be unrecognized.");
                    }
                }

                stats.RestoreState(
                    data._maxHealth, data._currentHealth,
                    data._maxStamina, data._currentStamina,
                    data._maxMana, data._currentMana,
                    data._maxUltimate, data._currentUltimate
                );

                loadedLocation = data._location;
                loadedSceneIndex = data._buildSceneIndex;

                // Re-evaluate auto-detected branches whose conditions may already be met
                // from counters restored above (e.g. enemyKills >= 10 → BR-04).
                if (PlotManager.Instance != null && PlotManager.Instance.BranchRegistry != null)
                    PlotManager.Instance.BranchRegistry.TryFireAll(state);

                Debug.Log("[SaveSystem] State successfully loaded.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load state. Attempting to restore backup... Error: {e.Message}");
                return RestoreBackup(state, stats, out loadedLocation, out loadedSceneIndex);
            }
        }

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
                
                if (!string.IsNullOrEmpty(data._plotStateJson))
                {
                    JsonUtility.FromJsonOverwrite(data._plotStateJson, state);
                    state.RebuildRuntimeCaches();
                }

                stats.RestoreState(
                    data._maxHealth, data._currentHealth,
                    data._maxStamina, data._currentStamina,
                    data._maxMana, data._currentMana,
                    data._maxUltimate, data._currentUltimate
                );

                loadedLocation = data._location;
                loadedSceneIndex = data._buildSceneIndex;

                if (PlotManager.Instance != null && PlotManager.Instance.BranchRegistry != null)
                    PlotManager.Instance.BranchRegistry.TryFireAll(state);

                Debug.LogWarning("[SaveSystem] State successfully restored from backup.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Critical Failure! Backup is also corrupted: {e.Message}");
                return false;
            }
        }

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

        public static void DeleteSaveData()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
            if (File.Exists(BackupPath))
            {
                File.Delete(BackupPath);
            }
            Debug.Log("[SaveSystem] Old save data wiped for a New Game.");
        }

        public static bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }

        #endregion

        #region Data Structures

        [Serializable]
        private class SaveData
        {
            public int _saveFormatVersion;
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

            public string _plotStateJson;

            public SaveData(PlotState plot, PlayerStatsManager stats, Vector3 saveLocation, int sceneIndex)
            {
                _saveFormatVersion = plot != null ? plot.SaveFormatVersion : 1;
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

                _plotStateJson = JsonUtility.ToJson(plot);
            }
        }

        #endregion
    }
}