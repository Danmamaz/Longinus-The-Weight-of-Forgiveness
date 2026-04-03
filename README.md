![Logo](https://github.com/Danmamaz/Longinus-The-Weight-of-Forgiveness/blob/main/Assets/Visual%20Related/UI/Logo.png)
![Unity Version](https://img.shields.io/badge/Unity-6000.0.x-blue.svg)
![Genre](https://img.shields.io/badge/Genre-Souls--like-red.svg)
![Status](https://img.shields.io/badge/Status-Technical_Vertical_Slice-orange.svg)

**Longinus** — це хардкорний Action-RPG-проєкт у жанрі Souls-like. Основна увага приділена технічній реалізації бойової системи та гнучкому механізму розгалуженого сюжету, що базується на стані ігрового світу та рішеннях гравця.

---

## 🛠 Технічні характеристики

- **Двигун:** Unity 6000.2.10f1
- **Рендер:** Universal Render Pipeline (URP)
- **Архітектура:**
  - Патерн **State Machine** для гравця та AI.
  - **ScriptableObject-driven architecture** для систем сюжету та даних.
  - **Singleton Pattern** для центральних менеджерів (`PlotManager`).

---

## ⚔️ Основні системи (MVP)

1. **Бойовий цикл:** Система Lock-on, кадрів невразливості (i-frames) та менеджмент витривалості.
2. **Plot & Karma System:** Централізоване керування прогресією через `PlotManager`:
   - Відстеження карми.
   - Реєстрація рішень через `DecisionNode`.
   - Динамічне розблокування шляхів (`onPathOpened`).
3. **Branching Narrative:** Реалізація декількох фіналів (`EndingDefinition`) залежно від дій гравця та стану світу.

---

## 📂 Структура проекту
```text
Assets/
├── Scripts/
│   ├── Player/             # Контролер, локомоція, бойова система
│   ├── Enemy/              # ШІ, базові класи та стейт-машини
│   ├── Systems/
│   │   └── Plot/           # Ядро системи сюжету (PlotManager, PlotState)
│   └── Editor/             # Інструменти валідації (PlotSystemValidator)
├── Data/                   # ScriptableObjects (Рішення, Наслідки, Фінали)
└── Visual Related/         # Асети, шейдери та анімації
```

---

## 🚀 Початок роботи

1. Переконайтеся, що встановлена версія **Unity 6000.2.10f1**.
2. Клонуйте репозиторій:
```bash
git clone https://github.com/danmamaz/longinus-the-weight-of-forgiveness.git
```
3. Відкрийте проєкт у Unity Hub.
4. Перевірте Package Manager на наявність основних компонентів:
   - **Input System** (Action-based)
   - **Universal RP** (URP)
5. Основна сцена для ініціалізації: `Assets/Scenes/Main Menu.unity`.

---

## 👤 Автор

**Данило (Danmamaz)**

Проект розроблено з акцентом на системний геймдизайн та реалізацію ScriptableObject-driven систем.

*Документація процесу розробки ведеться на YouTube-каналі автора.*
