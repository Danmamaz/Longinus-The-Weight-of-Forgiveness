![Logo](Assets/Visuals/UI/Logo.png)
![Unity Version](https://img.shields.io/badge/Unity-6000.0.x-blue.svg)
![Genre](https://img.shields.io/badge/Genre-Souls--like-red.svg)
![Status](https://img.shields.io/badge/Status-Technical_Vertical_Slice-orange.svg)

**Longinus** is a hardcore Souls-like Action-RPG project. The main focus is on the technical implementation of the combat system and a flexible, branching storyline mechanism based on the state of the game world and the player’s decisions.

---

## 🛠 Technical Specifications

- **Engine:** Unity 6000.2.10f1
- **Render:** Universal Render Pipeline (URP)
- **Architecture:**
  - **State Machine** pattern for the player and AI.
  - **ScriptableObject-driven architecture** for story and data systems.
  - **Singleton Pattern** for central managers (`PlotManager`).

---

## ⚔️ Core Systems (MVP)

1. **Combat Cycle:** Lock-on system, invincibility frames (i-frames), and stamina management.
2. **Plot & Karma System:** Centralized progression management via `PlotManager`:
   - Karma tracking.
   - Recording decisions via `DecisionNode`.
   - Dynamic path unlocking (`onPathOpened`).
3. **Branching Narrative:** Implementation of multiple endings (`EndingDefinition`) depending on the player’s actions and the state of the world.

---

## 📂 Project Structure
```text
Assets/
├── Scripts/
│   ├── Player/             # Controller, locomotion, combat system
│   ├── Enemy/              # AI, base classes, and state machines
│   ├── Systems/
│   │   └── Plot/           # Plot system core (PlotManager, PlotState)
│   └── Editor/             # Validation tools (PlotSystemValidator)
├── Data/                   # ScriptableObjects (Solutions, Consequences, Endings)
└── Visual Related/         # Assets, shaders, and animations
```

---

## 🚀 Getting Started

1. Make sure you have **Unity 6000.2.10f1** installed.
2. Clone the repository:
```bash
git clone https://github.com/danmamaz/longinus-the-weight-of-forgiveness.git
```
3. Open the project in Unity Hub.
4. Check the Package Manager for the following core components:
   - **Input System** (Action-based)
   - **Universal RP** (URP)
5. Main scene for initialization: `Assets/Scenes/Main Menu.unity`.

---

## 👤 Author

**Danylo (Danmamaz)**

The project was developed with a focus on systematic game design and the implementation of ScriptableObject-driven systems.

*Documentation of the development process is available on the author’s YouTube channel.*
