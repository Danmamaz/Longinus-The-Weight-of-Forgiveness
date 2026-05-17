using UnityEngine;
using UnityEditor;
using Longinus.PlotSystem;

/// <summary>
/// Editor utility: Longinus → Plot → Generate 12 Branch Assets
/// Creates one PlotBranch ScriptableObject per branch in Assets/Data/PlotState/Branches/.
/// Skips any branch whose .asset file already exists to preserve Inspector edits.
/// </summary>
public static class PlotBranchCreator
{
    private const string OUTPUT_FOLDER = "Assets/Data/PlotState/Branches";

    // ConditionCheckType enum indices (must match PlotStructures.cs order)
    private const int HAS_FLAG            = 0; // ConditionCheckType.HasFlag
    private const int DOES_NOT_HAVE_FLAG  = 1; // ConditionCheckType.DoesNotHaveFlag
    private const int INT_GREATER_OR_EQUAL = 2; // ConditionCheckType.IntGreaterOrEqual

    // BranchType enum indices (must match PlotBranch.cs order)
    private const int MAJOR  = 0;
    private const int MEDIUM = 1;
    private const int MINOR  = 2;

    [MenuItem("Longinus/Plot/Generate 12 Branch Assets")]
    public static void GenerateBranchAssets()
    {
        EnsureFolderExists();

        // ── BR-01  Boss Defeated ─────────────────────────────────────────────
        // Trigger: BossDeathHandler.DeathSequence calls TryFireBranch("BR-01") after the
        //          death animation. The Phase 2 kill check is done in code, not here.
        // Condition: DoesNotHaveFlag "Flag_BossDefeated" — prevents re-fire on save reload.
        Create("BR-01", "Boss Defeated",
            "Fired when the boss dies in Phase 2. Triggers the arena env swap consequence.", MAJOR,
            conditions: new[]
            {
                Cond(DOES_NOT_HAVE_FLAG, "Flag_BossDefeated", 0)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_BossDefeated", modifyInt: false, "", 0)
            });

        // ── BR-02  Sanctuary Burned ──────────────────────────────────────────
        // Trigger: PlotTrigger_Interact on the altar prop calls TryFireBranch("BR-02").
        // Requires: "altar_torch_active" flag set by a companion PlotTrigger_FlagOnEvent
        //           on the torch item before the player reaches the altar.
        Create("BR-02", "Sanctuary Burned",
            "Fired when the player uses the torch on the altar prop.", MAJOR,
            conditions: new[]
            {
                Cond(HAS_FLAG, "altar_torch_active", 0)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_SanctuaryBurned", modifyInt: false, "", 0)
            });

        // ── BR-03  Forbidden Path ────────────────────────────────────────────
        // Trigger: PlotTrigger_Interact on the sealed door.
        // Requires: player must have picked up the rusted key (Flag_RustedKey set).
        Create("BR-03", "Forbidden Path",
            "Fired when the rusted key is used on the sealed door.", MAJOR,
            conditions: new[]
            {
                Cond(HAS_FLAG, "Flag_RustedKey", 0)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_ForbiddenPath", modifyInt: false, "", 0)
            });

        // ── BR-04  Bloodstained Altar ────────────────────────────────────────
        // Auto-detected: fires when enemyKills reaches 10.
        // Counter incremented by EnemyStatsManager.Die() (Part 5).
        Create("BR-04", "Bloodstained Altar",
            "Fires once the player has killed 10 enemies before confronting the boss.", MEDIUM,
            conditions: new[]
            {
                Cond(INT_GREATER_OR_EQUAL, "enemyKills", 10)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_BloodstainedAltar", modifyInt: false, "", 0)
            });

        // ── BR-05  Path of Mercy ─────────────────────────────────────────────
        // Auto-detected: fires when the mercy zone is cleared without attacking the healer.
        // "healer_attacked" is set by a damage trigger on the healer NPC.
        // "mercy_zone_cleared" is set by PlotTrigger_FlagOnEvent at the zone exit.
        Create("BR-05", "Path of Mercy",
            "Fires if the player clears the mercy zone without attacking the healer NPC.", MEDIUM,
            conditions: new[]
            {
                Cond(DOES_NOT_HAVE_FLAG, "healer_attacked", 0),
                Cond(HAS_FLAG, "mercy_zone_cleared", 0)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_PathOfMercy", modifyInt: false, "", 0)
            });

        // ── BR-06  Echoes of the Slain ───────────────────────────────────────
        // Auto-detected: fires after the boss is summoned following a prior kill.
        // Depends on BR-01. "boss_summoned_after_kill" is set by the summon trigger.
        Create("BR-06", "Echoes of the Slain",
            "Fires when the boss is summoned again after the player's first kill.", MEDIUM,
            conditions: new[]
            {
                Cond(HAS_FLAG, "Flag_BossDefeated", 0),
                Cond(HAS_FLAG, "boss_summoned_after_kill", 0)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_EchoesOfSlain", modifyInt: false, "", 0)
            });

        // ── BR-07  First Bonfire Lit ─────────────────────────────────────────
        // Trigger: PlotTrigger_Interact on the first bonfire prop.
        // "bonfire_interact" is set by PlotTrigger_FlagOnEvent just before/during interaction.
        Create("BR-07", "First Bonfire Lit",
            "Fires the first time the player lights the bonfire.", MINOR,
            conditions: new[]
            {
                Cond(HAS_FLAG, "bonfire_interact", 0)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_FirstBonfire", modifyInt: false, "", 0)
            });

        // ── BR-08  Wandering Scribe ──────────────────────────────────────────
        // Auto-detected: fires once the player has spoken to the Scribe at least once.
        // PlotTrigger_Counter on the Scribe NPC increments "scribe_talks" on dialogue end.
        Create("BR-08", "Wandering Scribe",
            "Fires after the player speaks to the Scribe NPC at least once.", MINOR,
            conditions: new[]
            {
                Cond(INT_GREATER_OR_EQUAL, "scribe_talks", 1)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_WanderingScribe", modifyInt: false, "", 0)
            });

        // ── BR-09  Amulet Obtained ───────────────────────────────────────────
        // Trigger: PlotTrigger_FlagOnEvent on the destructible crate sets "crate_looted_amulet".
        // Auto-detected: TryFireAll fires BR-09 when that flag is set.
        Create("BR-09", "Amulet Obtained",
            "Fires when the player loots the amulet from the destructible crate.", MINOR,
            conditions: new[]
            {
                Cond(HAS_FLAG, "crate_looted_amulet", 0)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_Amulet", modifyInt: false, "", 0)
            });

        // ── BR-10  Vendor Discount ───────────────────────────────────────────
        // Auto-detected: fires when PlayerDeaths reaches 5.
        // Counter incremented by PlayerStatsManager.Die() (Part 6).
        Create("BR-10", "Vendor Discount",
            "Fires once the player has died 5 or more times.", MINOR,
            conditions: new[]
            {
                Cond(INT_GREATER_OR_EQUAL, "PlayerDeaths", 5)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_VendorDiscount", modifyInt: false, "", 0)
            });

        // ── BR-11  Silent Bell ───────────────────────────────────────────────
        // Trigger: PlotTrigger_Interact on the chapel bell prop.
        // "bell_activated" is set by PlotTrigger_FlagOnEvent wired to the bell animation.
        Create("BR-11", "Silent Bell",
            "Fires when the player activates the chapel bell.", MINOR,
            conditions: new[]
            {
                Cond(HAS_FLAG, "bell_activated", 0)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_SilentBell", modifyInt: false, "", 0)
            });

        // ── BR-12  Crow Companion ────────────────────────────────────────────
        // Auto-detected: fires when CrowFeedCount reaches 3.
        // PlotTrigger_Counter on the crow feed prop increments "CrowFeedCount".
        Create("BR-12", "Crow Companion",
            "Fires after the player feeds the crow prop three times.", MINOR,
            conditions: new[]
            {
                Cond(INT_GREATER_OR_EQUAL, "CrowFeedCount", 3)
            },
            consequences: new[]
            {
                Conseq(setFlag: true, "Flag_CrowBonded", modifyInt: false, "", 0)
            });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlotBranchCreator] 12 branch assets generated in " + OUTPUT_FOLDER);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void Create(
        string id, string displayName, string description, int typeIndex,
        (int checkType, string key, int amount)[] conditions,
        (bool setFlag, string flagToSet, bool modifyInt, string intKey, int intAmount)[] consequences)
    {
        string assetPath = $"{OUTPUT_FOLDER}/{id}.asset";

        if (AssetDatabase.LoadAssetAtPath<PlotBranch>(assetPath) != null)
        {
            Debug.Log($"[PlotBranchCreator] {id} already exists — skipped.");
            return;
        }

        var branch = ScriptableObject.CreateInstance<PlotBranch>();
        AssetDatabase.CreateAsset(branch, assetPath);

        var so = new SerializedObject(branch);

        so.FindProperty("_branchId").stringValue    = id;
        so.FindProperty("_displayName").stringValue = displayName;
        so.FindProperty("_description").stringValue = description;
        so.FindProperty("_type").enumValueIndex     = typeIndex;

        var condsProp = so.FindProperty("_conditions");
        condsProp.ClearArray();
        condsProp.arraySize = conditions.Length;
        for (int i = 0; i < conditions.Length; i++)
        {
            var e = condsProp.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("CheckType").enumValueIndex = conditions[i].checkType;
            e.FindPropertyRelative("Key").stringValue          = conditions[i].key;
            e.FindPropertyRelative("RequiredAmount").intValue  = conditions[i].amount;
        }

        var conseqProp = so.FindProperty("_consequences");
        conseqProp.ClearArray();
        conseqProp.arraySize = consequences.Length;
        for (int i = 0; i < consequences.Length; i++)
        {
            var e = conseqProp.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("SetFlag").boolValue    = consequences[i].setFlag;
            e.FindPropertyRelative("FlagToSet").stringValue = consequences[i].flagToSet;
            e.FindPropertyRelative("ModifyInt").boolValue  = consequences[i].modifyInt;
            e.FindPropertyRelative("IntKey").stringValue   = consequences[i].intKey;
            e.FindPropertyRelative("IntAmount").intValue   = consequences[i].intAmount;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static (int checkType, string key, int amount) Cond(int checkType, string key, int amount)
        => (checkType, key, amount);

    private static (bool setFlag, string flagToSet, bool modifyInt, string intKey, int intAmount)
        Conseq(bool setFlag, string flagToSet, bool modifyInt, string intKey, int intAmount)
        => (setFlag, flagToSet, modifyInt, intKey, intAmount);

    private static void EnsureFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/PlotState"))
            AssetDatabase.CreateFolder("Assets/Data", "PlotState");
        if (!AssetDatabase.IsValidFolder("Assets/Data/PlotState/Branches"))
            AssetDatabase.CreateFolder("Assets/Data/PlotState", "Branches");
    }
}
