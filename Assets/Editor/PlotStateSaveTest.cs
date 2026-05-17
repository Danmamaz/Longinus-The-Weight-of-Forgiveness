using UnityEngine;
using UnityEditor;
using Longinus.PlotSystem;

/// <summary>
/// Editor utility: Longinus → Plot → Run Save Round-Trip Test
/// Programmatically exercises PlotState serialization without entering Play Mode.
/// Validates that all 24 flags (12 consequence + 12 _fired) and 3 int counters
/// survive a JsonUtility round-trip identical to the SaveSystem path.
/// </summary>
public static class PlotStateSaveTest
{
    // ── Expected consequence flags ──────────────────────────────────────────
    private static readonly string[] CONSEQUENCE_FLAGS =
    {
        "Flag_BossDefeated",
        "Flag_SanctuaryBurned",
        "Flag_ForbiddenPath",
        "Flag_BloodstainedAltar",
        "Flag_PathOfMercy",
        "Flag_EchoesOfSlain",
        "Flag_FirstBonfire",
        "Flag_WanderingScribe",
        "Flag_Amulet",
        "Flag_VendorDiscount",
        "Flag_SilentBell",
        "Flag_CrowBonded",
    };

    // ── Expected _fired sentinel flags (one per branch) ──────────────────────
    private static readonly string[] FIRED_FLAGS =
    {
        "BR-01_fired",
        "BR-02_fired",
        "BR-03_fired",
        "BR-04_fired",
        "BR-05_fired",
        "BR-06_fired",
        "BR-07_fired",
        "BR-08_fired",
        "BR-09_fired",
        "BR-10_fired",
        "BR-11_fired",
        "BR-12_fired",
    };

    // ── Int counters that drive auto-detected branches ───────────────────────
    private static readonly (string key, int value)[] INT_COUNTERS =
    {
        ("enemyKills",    10),
        ("scribe_talks",   1),
        ("CrowFeedCount",  3),
    };

    [MenuItem("Longinus/Plot/Run Save Round-Trip Test")]
    public static void RunTest()
    {
        string plotStatePath = "Assets/Data/PlotState/MainPlotState.asset";
        PlotState state = AssetDatabase.LoadAssetAtPath<PlotState>(plotStatePath);

        if (state == null)
        {
            Debug.LogError($"[PlotStateSaveTest] Could not load PlotState at '{plotStatePath}'. " +
                           "Ensure MainPlotState.asset exists before running this test.");
            return;
        }

        // ── 1. Seed all flags and counters ───────────────────────────────────
        state.ResetState();

        foreach (string flag in CONSEQUENCE_FLAGS)
            state.SetFlag(flag);

        foreach (string flag in FIRED_FLAGS)
            state.SetFlag(flag);

        foreach (var (key, value) in INT_COUNTERS)
            state.SetInt(key, value);

        // ── 2. Simulate the SaveSystem serialization path ────────────────────
        string json = JsonUtility.ToJson(state);

        // ── 3. Simulate the LoadState deserialization path ───────────────────
        state.ResetState(); // wipe runtime caches to prove the round-trip restores them
        JsonUtility.FromJsonOverwrite(json, state);
        state.RebuildRuntimeCaches();

        // ── 4. Validate ──────────────────────────────────────────────────────
        int passed = 0;
        int failed = 0;

        foreach (string flag in CONSEQUENCE_FLAGS)
            Check(state.HasFlag(flag), flag, ref passed, ref failed);

        foreach (string flag in FIRED_FLAGS)
            Check(state.HasFlag(flag), flag, ref passed, ref failed);

        foreach (var (key, value) in INT_COUNTERS)
            Check(state.GetInt(key) == value, $"{key}=={value} (got {state.GetInt(key)})", ref passed, ref failed);

        // ── 5. Report ────────────────────────────────────────────────────────
        if (failed == 0)
            Debug.Log($"[PlotStateSaveTest] ALL {passed} checks passed. Save round-trip is stable.");
        else
            Debug.LogError($"[PlotStateSaveTest] {failed} check(s) FAILED, {passed} passed. " +
                           "See above for details.");

        // Restore the asset to a clean state so the editor doesn't persist test flags.
        state.ResetState();
        EditorUtility.SetDirty(state);
        AssetDatabase.SaveAssets();
    }

    private static void Check(bool condition, string label, ref int passed, ref int failed)
    {
        if (condition)
        {
            passed++;
        }
        else
        {
            Debug.LogError($"[PlotStateSaveTest] FAIL: '{label}' was not present after round-trip.");
            failed++;
        }
    }
}
