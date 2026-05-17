using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LonginusEditor
{
    public static class RCPlaytestChecklist
    {
        [MenuItem("Longinus/Build/Generate RC Playtest Checklist")]
        public static void Generate()
        {
            string path = "Assets/Documentation/RC_Playtest_Checklist.md";
            var lines = new List<string>
            {
                "# Release Candidate Playtest Checklist",
                $"Generated: {System.DateTime.Now}",
                "",
                "## Run-time: 25 minutes",
                "",
                "### Boot Sequence",
                "- [ ] Game launches without errors (check Player.log)",
                "- [ ] Main Menu music plays",
                "- [ ] New Game button works",
                "- [ ] Continue button shows correctly (depending on save)",
                "",
                "### Tutorial Chapter (~5 min)",
                "- [ ] Player spawns at correct position",
                "- [ ] Tutorial WASD prompts appear and dismiss correctly",
                "- [ ] Shift roll prompt → roll executes",
                "- [ ] LMB prompt → attack swings",
                "- [ ] First enemy encounter — combat feels responsive",
                "- [ ] Hit impacts spawn particles",
                "- [ ] Enemy dissolves on death",
                "- [ ] First bonfire interactable triggers BR-07",
                "- [ ] Checkpoint can be rested at",
                "- [ ] Save persists location and stats",
                "",
                "### Lock-On (3 min)",
                "- [ ] MMB toggles lock-on, marker appears on enemy",
                "- [ ] Camera orbits around player → target midpoint",
                "- [ ] Q/E switches between visible enemies",
                "- [ ] Lock-on releases when target dies",
                "- [ ] Lock-on releases when target leaves range",
                "",
                "### Beach Scene Transition",
                "- [ ] SceneTeleporter triggers correctly (only on Player tag)",
                "- [ ] Fade transition smooth",
                "- [ ] Beach scene loads without errors",
                "",
                "### Boss Encounter (~10 min)",
                "- [ ] Arena trigger seals walls",
                "- [ ] Boss music starts (Phase 1 track)",
                "- [ ] PostFX transitions to BossArenaPhase1 (cool blue)",
                "- [ ] Boss uses Sweep attack — colliders match animation",
                "- [ ] Boss uses Thrust attack",
                "- [ ] Boss uses AoESlam — damage in radius",
                "- [ ] Boss reaches 50% HP → Phase 2 transition",
                "- [ ] Phase 2 VFX plays (aura, roar, light shift)",
                "- [ ] Phase 2 music swap",
                "- [ ] Boss Phase 2 attacks fire (PhaseLeap, SpinFury)",
                "- [ ] PhaseLeap arcs through air toward player",
                "- [ ] Boss tint changes (red HDR shift)",
                "- [ ] Boss death triggers BR-01",
                "- [ ] Arena env swap: red lights, dense fog, hidden path opens",
                "- [ ] Boss corpse persists",
                "- [ ] Victory music plays",
                "- [ ] PostFX transitions to PostBossKill",
                "",
                "### Save/Load Verification",
                "- [ ] Save after boss kill",
                "- [ ] Quit to main menu, exit game completely",
                "- [ ] Relaunch → Continue → load resumes after boss",
                "- [ ] Arena STILL in post-boss state on load",
                "- [ ] Flag_BossDefeated persists",
                "",
                "### End Demo (~2 min)",
                "- [ ] Hidden path leads to End Demo scene",
                "- [ ] Scene loads cleanly",
                "- [ ] Credits or end message displays",
                "",
                "### Performance & Stability",
                "- [ ] No console errors during entire playthrough",
                "- [ ] FPS stays above 30 in arena",
                "- [ ] No memory leaks (check Task Manager every 5 min)",
                "- [ ] No infinite loops, no soft locks",
                "- [ ] Boss never gets stuck in invalid state",
                "",
                "## Sign-off",
                "- [ ] All critical paths pass",
                "- [ ] Build is ready for diploma defense",
                "",
                "Tester signature: ____________________  Date: __________",
            };

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
            Debug.Log($"[RC] Checklist generated: {path}");
        }
    }
}
