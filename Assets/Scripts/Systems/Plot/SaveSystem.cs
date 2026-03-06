using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PlotBranching
{
    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "plot_save.dat");
        private static string BackupPath => Path.Combine(Application.persistentDataPath, "plot_save.backup");
        
        // Set to true to enable basic XOR encryption (prevents casual registry edits)
        private const bool USE_ENCRYPTION = true;
        private const string ENCRYPTION_KEY = "YourGameSpecificKey_ChangeThis"; // Change per project!

        public static void SaveState(PlotState state)
        {
            if (state == null)
            {
                Debug.LogError("SaveSystem: Cannot save null state!");
                return;
            }

            try
            {
                // Create backup of existing save before overwriting
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupPath, true);
                }

                string json = JsonUtility.ToJson(new PlotStateSaveData(state), false); // No pretty print for production
                
                if (USE_ENCRYPTION)
                {
                    json = Encrypt(json);
                }

                File.WriteAllText(SavePath, json);
                Debug.Log($"SaveSystem: State saved to {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveSystem: Failed to save - {e.Message}\n{e.StackTrace}");
            }
        }

        public static bool LoadState(PlotState state)
        {
            if (state == null)
            {
                Debug.LogError("SaveSystem: Cannot load into null state!");
                return false;
            }

            if (!File.Exists(SavePath))
            {
                Debug.Log("SaveSystem: No save file found.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                
                if (USE_ENCRYPTION)
                {
                    json = Decrypt(json);
                }

                PlotStateSaveData saveData = JsonUtility.FromJson<PlotStateSaveData>(json);
                
                if (saveData == null)
                {
                    throw new System.Exception("Deserialization returned null - file may be corrupt");
                }

                saveData.ApplyTo(state);
                Debug.Log("SaveSystem: State loaded successfully.");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveSystem: Failed to load - {e.Message}. Attempting backup restore...");
                return TryRestoreBackup(state);
            }
        }

        private static bool TryRestoreBackup(PlotState state)
        {
            if (!File.Exists(BackupPath))
            {
                Debug.LogError("SaveSystem: No backup available.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(BackupPath);
                
                if (USE_ENCRYPTION)
                {
                    json = Decrypt(json);
                }

                PlotStateSaveData saveData = JsonUtility.FromJson<PlotStateSaveData>(json);
                saveData.ApplyTo(state);
                
                // Restore backup to main save
                File.Copy(BackupPath, SavePath, true);
                
                Debug.LogWarning("SaveSystem: Restored from backup successfully.");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveSystem: Backup restore failed - {e.Message}");
                return false;
            }
        }

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
                Debug.Log("SaveSystem: Save files deleted.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SaveSystem: Failed to delete saves - {e.Message}");
            }
        }

        // Simple XOR encryption - not military grade, but stops casual tampering
        private static string Encrypt(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }
            
            return System.Convert.ToBase64String(data);
        }

        private static string Decrypt(string encryptedText)
        {
            byte[] data = System.Convert.FromBase64String(encryptedText);
            byte[] key = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }
            
            return Encoding.UTF8.GetString(data);
        }

        [System.Serializable]
        public class PlotStateSaveData
        {
            public int currentKarma;
            public string[] madeDecisionIDs;
            public string[] chosenOptions;
            public int currentWorldState; // Serialize as int for safety
            public string[] activeBuffIDs;
            public string[] unlockedBossIDs;
            public int saveVersion = 1; // For future migration support
            public string[] openedPathIDs;

            public PlotStateSaveData(PlotState state)
            {
                currentKarma = state.currentKarma;
                madeDecisionIDs = state.madeDecisionIDs.ToArray();
                chosenOptions = state.chosenOptions.ToArray();
                currentWorldState = (int)state.currentWorldState;
                activeBuffIDs = state.activeBuffIDs.ToArray();
                unlockedBossIDs = state.unlockedBossIDs.ToArray();
                openedPathIDs = state.openedPathIDs.ToArray();
            }

            public void ApplyTo(PlotState state)
            {
                state.currentKarma = currentKarma;
                state.madeDecisionIDs = new System.Collections.Generic.List<string>(madeDecisionIDs);
                state.chosenOptions = new System.Collections.Generic.List<string>(chosenOptions);
                state.currentWorldState = (WorldStateType)currentWorldState;
                state.activeBuffIDs = new System.Collections.Generic.List<string>(activeBuffIDs);
                state.unlockedBossIDs = new System.Collections.Generic.List<string>(unlockedBossIDs);
                state.openedPathIDs = new System.Collections.Generic.List<string>(openedPathIDs);
            }
        }
    }
}