<div align="center">

![Logo](Assets/Visuals/UI/Logo.png)

# Longinus: The Weight of Forgiveness

**Diploma Project — Technical Vertical Slice of a Souls-like Action-RPG**

![Unity](https://img.shields.io/badge/Unity-6000.2.10f1-black?logo=unity)
![URP](https://img.shields.io/badge/Render-Universal%20RP-blue)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)
![Genre](https://img.shields.io/badge/Genre-Souls--like%20Action--RPG-red)
![Status](https://img.shields.io/badge/Status-Technical%20Vertical%20Slice-orange)

</div>

---

## 📋 Table of Contents

1. [About the Project](#about-the-project)
2. [Tech Stack](#tech-stack)
3. [Architecture Overview](#architecture-overview)
4. [Core Systems](#core-systems)
   - [Player System](#player-system)
   - [Enemy & Boss AI](#enemy--boss-ai)
   - [Plot & Karma System](#plot--karma-system)
   - [Save System](#save-system)
   - [Audio System](#audio-system)
   - [Visuals & Post-Processing](#visuals--post-processing)
   - [UI System](#ui-system)
   - [Editor & QA Toolchain](#editor--qa-toolchain)
5. [Project Structure](#project-structure)
6. [Getting Started](#getting-started)
7. [Author](#author)

---

## About the Project

**Longinus: The Weight of Forgiveness** is a technical vertical slice of a hardcore Souls-like Action-RPG, built as a diploma project. The game is set in a dark medieval world where the player embodies Longinus — a fallen knight seeking redemption. Every decision carries moral weight and permanently shapes the world and its ending.

The primary focus of this project is **not** art or content volume, but the **technical depth** of its systems:

- A fully data-driven **branching narrative** engine built on ScriptableObjects
- A **hierarchical State Machine** controlling every actor in the game
- A **persistent save system** with XOR encryption and hot-backup
- A boss encounter with **two distinct combat phases** and reactive arena logic
- A complete **QA toolchain** built directly into the Unity Editor

---

## Tech Stack

| Category | Technology / Version |
|---|---|
| Engine | Unity 6000.2.10f1 |
| Render Pipeline | Universal Render Pipeline (URP 17.2.0) |
| Language | C# (.NET) |
| Input | Unity Input System 1.14.2 (Action-based) |
| Camera | Cinemachine 3.1.5 |
| Navigation | Unity AI Navigation 2.0.9 |
| Animation | Animation Rigging 1.3.1 + Timeline 1.8.9 |
| IDE | JetBrains Rider / Visual Studio |
| Version Control | Git + Git LFS |

---

## Architecture Overview

The project applies a strict set of architectural patterns to ensure scalability and testability:

```
┌─────────────────────────────────────────────────┐
│                 PRESENTATION LAYER               │
│   UIManager · PlayerStatsUI · EnemyHealthUI      │
│   LockOnMarkerUI · AttackCooldownUI · MainMenu   │
└──────────────────────┬──────────────────────────┘
                       │ observes
┌──────────────────────▼──────────────────────────┐
│                  GAME SYSTEMS LAYER              │
│  PlotManager (Singleton) ← ScriptableObjects     │
│  AudioDirector · SceneController · SaveSystem    │
│  PostProcessingDirector · UIManager              │
└──────────────────────┬──────────────────────────┘
                       │ drives
┌──────────────────────▼──────────────────────────┐
│                   ACTOR LAYER                    │
│  PlayerController ↔ PlayerStateMachine           │
│  EnemyController  ↔ EnemyStateMachine            │
│  BossController   ↔ BossAttackSelector           │
└─────────────────────────────────────────────────┘
```

**Key design decisions:**

| Pattern | Where Applied |
|---|---|
| **State Machine** | Player locomotion/combat, every Enemy/Boss AI |
| **ScriptableObject Architecture** | `PlotBranch`, `PlotState`, `AttackDefinition`, `BossAttackDefinition` |
| **Singleton** | `PlotManager`, `AudioDirector`, `SceneController`, `UIManager` |
| **Observer / UnityEvent** | `PlotManager.OnFlagUpdated` broadcasts to doors, dialogue, UI |
| **Interface Segregation** | `IDamageable`, `IInteractable`, `INoiseSource` — actors never depend on concrete types |
| **Object Pooling** | `HitImpactPool` — reuses hit-effect VFX to avoid runtime allocations |

---

## Core Systems

### Player System

**Scripts:** `PlayerController`, `PlayerStateMachine`, `PlayerLocomotion`, `PlayerCombatManager`, `PlayerStatsManager`, `LockOnSystem`, `LockOnCamera`, `InteractionSystem`, `TutorialController`

The player is built around a **hierarchical State Machine** (`PlayerStateMachine`) that cleanly separates locomotion states (Idle, Walk, Run, Roll) from combat states (Attack, Stagger, Death). No monolithic `Update()` with boolean flags — each state is a self-contained class that owns its entry, tick, and exit logic.

**Combat pipeline:**
1. Input System raises an action event → `PlayerController` routes it.
2. `PlayerCombatManager` checks stamina availability and the current combo index.
3. If valid, it enables the `weaponCollider` via an Animation Event, consuming stamina.
4. The `DamageCollider` broadcasts `IDamageable.TakeDamage()` on overlap — no direct coupling to enemy classes.
5. After the animation window closes, the collider disables and the combo window either advances or resets.

**Lock-On System:** `LockOnSystem` casts a sphere to collect candidates, sorts them by screen-space distance from the crosshair, and feeds the nearest target to `LockOnCamera` — a dedicated Cinemachine Virtual Camera that keeps the target framed while the player moves freely.

**Key features:**
- Stamina-gated combo system with configurable `AttackDefinition[]` arrays (ScriptableObject-driven)
- I-frame (invincibility frame) window during dodge rolls, implemented as a state flag checked by `DamageCollider`
- Interaction system with proximity detection and `IInteractable` dispatch

---

### Enemy & Boss AI

**Scripts:** `EnemyController`, `EnemyStateMachine`, `EnemyStatsManager`, `EnemyMovementManager`, `EnemyAnimationEventRelay`, `RangedEnemyController`, `ProjectileLauncher`, `Projectile`, `BossController`, `BossAttackSelector`, `BossDeathHandler`, `BossArenaTrigger`, `AoESlamHitbox`, `BoneColliderGroup`, `BossAttackDefinition`

All enemies share a base `EnemyStateMachine` architecture identical to the player's, making it straightforward to add new enemy types. The AI uses **Unity AI Navigation** for pathfinding.

**Enemy types:**
- **Melee Enemy** — patrol → detect → chase → attack loop driven by the state machine
- **Ranged Enemy** (`RangedEnemyController`) — maintains distance, uses `ProjectileLauncher` to fire homing/ballistic projectiles
- **Boss (Longinus)** — two-phase encounter with a dedicated `BossController`

**Boss Fight Architecture:**

```
BossArenaTrigger (OnTriggerEnter)
        │ fires Engage trigger
        ▼
BossController ──── BossAttackSelector (weighted random, phase-aware)
        │                    │
        │         Phase 1:  AttackSweep · AttackThrust · AttackAoESlam
        │         Phase 2:  AttackPhase2_A (leap) · AttackPhase2_B (fury)
        │
        ├── HP ≤ 50% → PhaseTransition → IsPhase2 = true
        │              BossPhasePostFX (post-processing shift)
        │              ArenaEnvSwap (environment state change)
        │
        └── HP = 0   → BossDeathHandler → PlotManager.SetFlag("boss_defeated")
                        OnFlagUpdated → doors unlock, ending branch evaluates
```

- `BoneColliderGroup` — maps hitbox bone transforms; activates only during specific attack animations via Animation Events relayed through `EnemyAnimationEventRelay`
- `AoESlamHitbox` — ground-slam area-of-effect that expands radially over a configurable duration
- Arena walls activate on entry and deactivate on death, preventing the player from leaving mid-fight

---

### Plot & Karma System

**Scripts:** `PlotManager`, `PlotState`, `PlotBranch`, `PlotBranchRegistry`, `PlotStructures` (`PlotCondition`, `PlotConsequence`, `ConditionCheckType`), `PlotTrigger_FlagOnEvent`, `PlotTrigger_Interact`, `PlotTrigger_Counter`, `ArenaEnvSwap`, `DecisionInteractable`

This is the **central technical contribution** of the project. The entire narrative is data-driven — no story logic lives in code. All story state is encoded in a single **`PlotState` ScriptableObject** (a dictionary of string flags and int counters), and every consequence or condition is expressed as a serialized struct in the Inspector.

**Flow:**

```
Player interacts with DecisionInteractable
         │
         ▼
PlotTrigger_Interact.Execute()
         │
         ├─► PlotManager.SetFlag("decision_mercy")
         │
         ▼
PlotManager.OnFlagUpdated.Invoke("decision_mercy")
         │
         ├─► PlotBranchRegistry evaluates ALL PlotBranch assets
         │         PlotBranch BR-03 conditions met → fire consequences
         │         PlotConsequence: SetFlag("path_redemption_open")
         │
         └─► Subscribed listeners (Door, ArenaEnvSwap, UI) react immediately
```

**`PlotBranch` (ScriptableObject) anatomy:**

| Field | Type | Purpose |
|---|---|---|
| `_branchId` | `string` | Unique ID, e.g. `"BR-03"` |
| `_type` | `BranchType` | `Major / Medium / Minor` — affects ending weight |
| `_conditions` | `List<PlotCondition>` | AND-gated, evaluated by `PlotManager.AreConditionsMet()` |
| `_consequences` | `List<PlotConsequence>` | Applied atomically when all conditions pass |

**Fire-once guarantee:** the first time a branch fires, `PlotManager` sets flag `"{branchId}_fired"`. All subsequent evaluations see this flag and short-circuit — no branch ever fires twice.

**Trigger types:**
- `PlotTrigger_FlagOnEvent` — sets a flag in response to any `UnityEvent`
- `PlotTrigger_Interact` — sets a flag when the player interacts with an object
- `PlotTrigger_Counter` — increments an int counter; branches gate on `IntGreaterOrEqual`

---

### Save System

**Script:** `SaveSystem`, `SaveData`

The save system serializes both `PlotState` and `PlayerStatsManager` data into a single `SaveData` DTO, converts it to JSON, and applies **XOR stream encryption** before writing to disk. A hot backup (`save.backup`) is created before every overwrite to prevent data loss on crash.

```csharp
// High-level API
SaveSystem.SaveState(plotState, statsManager, transform.position, sceneIndex);
SaveSystem.LoadState(out plotState, out statsManager, out spawnPosition, out sceneIndex);
```

**Features:**
- XOR encryption with a project-specific key
- Atomic backup-before-write pattern
- Scene index persistence — player returns to the exact checkpoint location
- Checkpoints (`Checkpoint.cs`) trigger saves at bonfire-equivalent rest points

---

### Audio System

**Scripts:** `AudioDirector`, `AudioGameEventBridge`

`AudioDirector` is a singleton that manages three independent `AudioSource` channels: **Music**, **SFX**, and **Ambient**. Music transitions use coroutine-based crossfading with a configurable `MUSIC_FADE_DURATION`.

**Music tracks:**

| Track | Context |
|---|---|
| `MainMenu` | Main menu scene |
| `Exploration` | World traversal |
| `BossPhase1` | Boss encounter — first phase |
| `BossPhase2` | Boss encounter — second phase (triggered by `BossController`) |
| `Victory` | Boss defeated |
| `Death` | Player death sting |

`AudioGameEventBridge` decouples the rest of the codebase from `AudioDirector` by translating `UnityEvent` calls (assignable in the Inspector) into audio commands — zero code changes needed to add new audio triggers.

---

### Visuals & Post-Processing

**Scripts:** `PostProcessingDirector`, `BossPhasePostFX`, `BossPhase2Tint`, `DissolveOnDeath`, `HitImpactPool`, `ArenaFogVolume`, `GlobalFoliagePlayerSync`, `AmbientDust`, `VisualsBootstrap`

All visual effects are coordinated through `PostProcessingDirector` and initialized in the correct order by `VisualsBootstrap` on scene load.

| Script | Effect |
|---|---|
| `BossPhasePostFX` | Blends URP Volume weights on phase transition (desaturation, vignette) |
| `BossPhase2Tint` | Applies a full-screen color grade when the boss enters Phase 2 |
| `DissolveOnDeath` | Drives a dissolve shader via material property blocks on enemy death |
| `HitImpactPool` | Object pool for hit spark VFX — eliminates per-hit `Instantiate` / `Destroy` calls |
| `ArenaFogVolume` | Switches URP Fog Volume profiles when the arena seals |
| `GlobalFoliagePlayerSync` | Passes player world position to the foliage shader for wind/bend interaction |
| `AmbientDust` | Spawns and recycles ambient dust particles without allocating per frame |

---

### UI System

**Scripts:** `UIManager`, `PlayerStatsUI`, `EnemyHealthUI`, `LockOnMarkerUI`, `AttackCooldownUI`, `MainMenu`, `DeathScreenAnimationBridge`, `StatDividerUI`, `UIUnscaledTimeProvider`

The UI is fully decoupled from game logic through the `UIManager` singleton and the observer pattern. No UI component holds a direct reference to a player or enemy; it subscribes to events or polls exposed properties.

| Component | Displays |
|---|---|
| `PlayerStatsUI` | HP bar, Stamina bar with drain/regen animation |
| `EnemyHealthUI` | Boss HP bar with phase-transition flash |
| `LockOnMarkerUI` | World-space marker tracked above the locked target |
| `AttackCooldownUI` | Per-attack cooldown radial indicator |
| `DeathScreenAnimationBridge` | Drives the death screen fade sequence via Animator events |
| `UIUnscaledTimeProvider` | Supplies `Time.unscaledDeltaTime` to UI animations — menus animate correctly when `Time.timeScale = 0` |

---

### Editor & QA Toolchain

**Scripts:** `QAScanner`, `QAAutoFix`, `PlotBranchCreator`, `PlotStateSaveTest`, `RCPlaytestChecklist`, `BossAttackCreator`, `VerticalSliceBuilder`, `BuildPipelineHelper`, `PreBuildValidator`, `LightingBaker`, `VisualSetupEditor`

A significant engineering effort went into the Editor toolchain — accessible via the **Longinus** top-level menu in Unity.

| Tool | Menu Path | Purpose |
|---|---|---|
| `QAScanner` | `Longinus/QA/Run Full Scan` | Static analysis: finds `Debug.Log` in `Update`, missing null-checks, hardcoded scene indices, `FindObjectOfType` in hot paths, unreferenced public fields; outputs a Markdown report to `Assets/Documentation/QA_Report.md` |
| `QAAutoFix` | `Longinus/QA/Auto Fix` | Applies safe automatic fixes from the QA report |
| `PlotBranchCreator` | `Longinus/Plot System/Create Branch` | Wizard for creating pre-wired `PlotBranch` ScriptableObjects |
| `PlotStateSaveTest` | `Longinus/Plot System/Test Save` | Simulates a save/load cycle in the Editor without entering Play mode |
| `BossAttackCreator` | `Longinus/Boss/Create Attack` | Generates a `BossAttackDefinition` asset from a template |
| `RCPlaytestChecklist` | `Longinus/QA/Playtest Checklist` | Renders an interactive RC checklist in an EditorWindow |
| `PreBuildValidator` | (called by build pipeline) | Validates scenes, tags, and layer configuration before any build |
| `VerticalSliceBuilder` | `Longinus/Build/Build Vertical Slice` | One-click build pipeline for the vertical slice deliverable |
| `LightingBaker` | `Longinus/Build/Bake Lighting` | Batch-bakes all scenes with the correct light settings |

---

## Project Structure

```text
Assets/
├── Scripts/
│   ├── Player/                  # PlayerController, StateMachine, Combat, Locomotion, Stats
│   │   ├── Data/                # AttackDefinition (ScriptableObject)
│   │   └── Tutorial/            # TutorialController
│   ├── Enemy/
│   │   ├── Base/                # EnemyController, EnemyStateMachine, EnemyStatsManager,
│   │   │                        #   EnemyMovementManager, EnemyAnimationEventRelay
│   │   ├── Ranged/              # RangedEnemyController, ProjectileLauncher, Projectile
│   │   └── Boss/                # BossController, BossAttackSelector, BossDeathHandler,
│   │       │                    #   BossArenaTrigger, AoESlamHitbox, BoneColliderGroup
│   │       └── Data/            # BossAttackDefinition (ScriptableObject)
│   ├── Systems/
│   │   ├── Plot/                # PlotManager, PlotState, PlotBranch, PlotBranchRegistry,
│   │   │   │                    #   PlotStructures (conditions & consequences)
│   │   │   ├── Triggers/        # PlotTrigger_FlagOnEvent, PlotTrigger_Interact, PlotTrigger_Counter
│   │   │   ├── ScriptableObjects/ # PlotBranch, PlotState (asset definitions)
│   │   │   └── Environment/     # ArenaEnvSwap
│   │   ├── Levels/              # Checkpoint, SceneTeleporter
│   │   ├── UI/                  # UIManager, PlayerStatsUI, EnemyHealthUI, LockOnMarkerUI, …
│   │   ├── SceneController.cs
│   │   └── SaveSystem.cs
│   ├── Audio/                   # AudioDirector, AudioGameEventBridge
│   ├── Visuals/                 # PostProcessingDirector, BossPhasePostFX, DissolveOnDeath,
│   │                            #   HitImpactPool, ArenaFogVolume, AmbientDust, …
│   ├── InGameItems/             # DamageCollider, Door, DecisionInteractable, TrainingDummy
│   ├── Environment/             # RuinedChapelGenerator
│   └── Interfaces/              # IDamageable, IInteractable, INoiseSource
├── Editor/                      # QAScanner, QAAutoFix, PlotBranchCreator, BossAttackCreator,
│                                #   VerticalSliceBuilder, PreBuildValidator, LightingBaker, …
├── Data/                        # ScriptableObject assets (PlotBranches, PlotState instance,
│                                #   AttackDefinitions, BossAttackDefinitions)
├── Scenes/                      # Main Menu, Tutorial, Exploration, Boss Arena
├── Visuals/                     # Materials, Shaders, VFX, UI sprites, Logo
├── Settings/
│   └── Inputs/                  # InputSystem_Actions.cs (generated)
└── Packages/
    └── manifest.json            # All package dependencies
```

---

## Getting Started

### Prerequisites

| Tool | Version |
|---|---|
| Unity Hub | Latest |
| Unity Editor | **6000.2.10f1** (exact version — LTS) |
| Git LFS | Required for binary assets |

### Installation

```bash
# 1. Clone the repository (LFS-aware)
git clone https://github.com/danmamaz/longinus-the-weight-of-forgiveness.git
cd longinus-the-weight-of-forgiveness

# 2. Pull LFS objects
git lfs pull
```

3. Open **Unity Hub** → **Add project from disk** → select the cloned folder.
4. Unity will resolve packages automatically via the Package Manager.
5. Open the entry scene: `Assets/Scenes/Main Menu.unity`
6. Press **Play** — the game initializes via `VisualsBootstrap` and `PlotManager` automatically.

### Running the QA Scanner

```
Unity Menu → Longinus → QA → Run Full Scan
```
The report is written to `Assets/Documentation/QA_Report.md`.

### Building the Vertical Slice

```
Unity Menu → Longinus → Build → Build Vertical Slice
```
`PreBuildValidator` runs automatically before the build starts.

---

## Author

**Danylo Mamaza** (`danmamaz`)

Diploma project developed at [University Name], Faculty of [Faculty Name], specialization **[Specialization]**, academic year 2025–2026.

The project was built with a focus on **engineering discipline**: clean architecture, data-driven design, and a production-grade toolchain — not just a playable prototype, but a system demonstrating professional software design within a game engine context.

*Development process documented on the author's YouTube channel.*

---

<div align="center">

*Longinus: The Weight of Forgiveness — © 2026 Danylo Mamaza. All rights reserved.*

</div>
