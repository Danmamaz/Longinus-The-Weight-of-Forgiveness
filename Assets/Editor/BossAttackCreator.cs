using UnityEngine;
using UnityEditor;
using Longinus.EnemySystem;

/// <summary>
/// Editor utility: Longinus → Boss → attack asset generation.
/// Creates BossAttackDefinition assets under Assets/Data/Combat/BossAttacks/.
/// Skips any asset that already exists to preserve Inspector edits.
/// </summary>
public static class BossAttackCreator
{
    private const string OUTPUT_FOLDER = "Assets/Data/Combat/BossAttacks";

    [MenuItem("Longinus/Boss/Create All Boss Attacks")]
    public static void CreateAllAttacks()
    {
        CreatePhase1Attacks();
        CreatePhase2Attacks();
    }

    [MenuItem("Longinus/Boss/Create Phase1 Attacks")]
    public static void CreatePhase1Attacks()
    {
        EnsureFolderExists();

        Create("BossAttack_Sweep",
            attackId: "Sweep",
            animatorTrigger: "AttackSweep",
            boneGroupName: "SweepArm",
            minRange: 0f,
            maxRange: 3.5f,
            cooldown: 3f,
            weight: 1.0f,
            phase1: true,
            phase2: true);

        Create("BossAttack_Thrust",
            attackId: "Thrust",
            animatorTrigger: "AttackThrust",
            boneGroupName: "ThrustWeapon",
            minRange: 2f,
            maxRange: 5f,
            cooldown: 2.5f,
            weight: 1.0f,
            phase1: true,
            phase2: true);

        Create("BossAttack_AoESlam",
            attackId: "AoESlam",
            animatorTrigger: "AttackAoESlam",
            boneGroupName: "",
            minRange: 0f,
            maxRange: 4f,
            cooldown: 6f,
            weight: 0.6f,
            phase1: true,
            phase2: true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BossAttackCreator] Phase 1 attack assets generated in " + OUTPUT_FOLDER);
    }

    [MenuItem("Longinus/Boss/Create Phase2 Attacks")]
    public static void CreatePhase2Attacks()
    {
        EnsureFolderExists();

        Create("BossAttack_PhaseLeap",
            attackId: "PhaseLeap",
            animatorTrigger: "AttackPhase2_A",
            boneGroupName: "LeapImpact",
            minRange: 5f,
            maxRange: 12f,
            cooldown: 8f,
            weight: 0.8f,
            phase1: false,
            phase2: true);

        Create("BossAttack_SpinFury",
            attackId: "SpinFury",
            animatorTrigger: "AttackPhase2_B",
            boneGroupName: "SpinBlades",
            minRange: 0f,
            maxRange: 3f,
            cooldown: 10f,
            weight: 0.5f,
            phase1: false,
            phase2: true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BossAttackCreator] Phase 2 attack assets generated in " + OUTPUT_FOLDER);
    }

    private static void Create(
        string assetName, string attackId, string animatorTrigger, string boneGroupName,
        float minRange, float maxRange, float cooldown, float weight,
        bool phase1, bool phase2)
    {
        string assetPath = $"{OUTPUT_FOLDER}/{assetName}.asset";

        if (AssetDatabase.LoadAssetAtPath<BossAttackDefinition>(assetPath) != null)
        {
            Debug.Log($"[BossAttackCreator] {assetName} already exists — skipped.");
            return;
        }

        var def = ScriptableObject.CreateInstance<BossAttackDefinition>();
        AssetDatabase.CreateAsset(def, assetPath);

        var so = new SerializedObject(def);
        so.FindProperty("_attackId").stringValue       = attackId;
        so.FindProperty("_animatorTrigger").stringValue = animatorTrigger;
        so.FindProperty("_boneGroupName").stringValue  = boneGroupName;
        so.FindProperty("_minRange").floatValue        = minRange;
        so.FindProperty("_maxRange").floatValue        = maxRange;
        so.FindProperty("_cooldown").floatValue        = cooldown;
        so.FindProperty("_weight").floatValue          = weight;
        so.FindProperty("_allowedInPhase1").boolValue  = phase1;
        so.FindProperty("_allowedInPhase2").boolValue  = phase2;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Combat"))
            AssetDatabase.CreateFolder("Assets/Data", "Combat");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Combat/BossAttacks"))
            AssetDatabase.CreateFolder("Assets/Data/Combat", "BossAttacks");
    }
}
