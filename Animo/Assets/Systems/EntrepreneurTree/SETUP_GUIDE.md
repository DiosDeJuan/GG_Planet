# Árbol del Emprendedor — Setup Guide

## Overview

The Entrepreneur Tree is a **progression system** that lets the player unlock products,
employees, security upgrades and improvements by spending **progress points** earned from
in-game achievements.

All new scripts live inside the `FLOBUK.StoreSimulator` namespace and are designed to
**extend** the existing Store Simulator asset without modifying any of its original files.

---

## File Structure

```
Assets/
├── Systems/
│   └── EntrepreneurTree/
│       ├── NodeData.cs                    ← ScriptableObject: one tree node
│       ├── TreeData.cs                    ← ScriptableObject: full tree definition
│       ├── EntrepreneurTreeManager.cs     ← Singleton: points + unlock logic
│       ├── AchievementSystem.cs           ← Achievement tracking + point awards
│       └── EntrepreneurTreeSaveIntegration.cs ← Save/load bridge (no asset modification)
├── UI/
│   └── Computer/
│       └── Upgrades/
│           ├── NodeUI.cs                  ← Single node button controller
│           ├── ConnectionLineUI.cs        ← Line between two nodes
│           └── UpgradesUIController.cs    ← Upgrades-tab controller
└── Data/
    └── (place your .asset files here)
```

---

## Step 1 — Create the Data Assets

### 1a. Create NodeData assets

For each node in the tree (products, employees, security levels, improvements):

1. Right-click in the `Assets/Data/` folder → **Create → EntrepreneurTree → NodeData**.
2. Fill in the fields:
   - **Id** — unique string (e.g. `"product_basics_1"`, `"employee_01"`, `"security_1"`)
   - **Node Type** — Product / Employee / Security / Improvement
   - **Title** — display name
   - **Description** — what it unlocks or does
   - **Icon** — optional Sprite
   - **Cost** — progress points required (default: 1)
   - **Required Node Ids** — IDs of prerequisite nodes (leave empty for root nodes)
   - **Ui Position** — pixel position inside the scroll canvas (arrange visually)

### Suggested node layout (uiPosition values, adjust to taste)

```
[Productos Básicos 1]  (0, 0)  ← root
       |
 ┌─────┼─────────────────────────────────────────┐
 │     │                                         │
[PB2]  [PB3]                             [Empleado 01]
(-200, -150) (200, -150)                 (600, 0)
 │
[Lácteos 1]  (-300, -300)
 │
[Lácteos 2]  (-300, -450)
...
```

### 1b. Create a TreeData asset

1. Right-click → **Create → EntrepreneurTree → TreeData**.
2. Save it as `EntrepreneurTreeData` inside `Assets/Data/`.
3. In the Inspector, expand **Nodes** and drag all your `NodeData` assets into the list.

---

## Step 2 — Scene Setup

Open the main **Game scene** (the one with the Store Simulator gameplay).

### 2a. Add manager components

Select the `Systems` GameObject (or any persistent game object in the scene).
Add the following components via **Add Component**:

| Component | Required field |
|---|---|
| `EntrepreneurTreeManager` | **Tree Data** → your `EntrepreneurTreeData` asset |
| `AchievementSystem` | *(no Inspector config needed)* |
| `EntrepreneurTreeSaveIntegration` | *(no Inspector config needed)* |

### 2b. Create the NodePrefab

1. In the Hierarchy, create **UI → Image** (name: `NodePrefab`), size 100 × 100 px.
2. Add a child **Image** for the icon (name: `Icon`).
3. Add a child **TMP_Text** for the label (name: `Label`).
4. Attach the **NodeUI** script.
5. Assign: `Background` → the root Image, `Icon Image` → the child Icon Image, `Title Label` → the TMP_Text.
6. Set the RectTransform **Anchor** to `(0.5, 0.5)` (Middle-Center).
7. Save as a Prefab under `Assets/UI/Computer/Upgrades/`.

### 2c. Create the LinePrefab

1. Create **UI → Image** (name: `LinePrefab`), set width to 100, height to 3.
2. Set Image color to white.
3. Attach **ConnectionLineUI** script.
4. Set the RectTransform **Anchor** to `(0.5, 0.5)`.
5. Save as a Prefab.

### 2d. Wire up the Upgrades tab

1. Open the `UIShopDesktop` prefab (or the canvas in the scene).
2. Find the panel rendered for the **UPGRADES** tab.
3. Add a child **Panel** named `EntrepreneurTreePanel`.
4. Inside it, build this hierarchy:

```
EntrepreneurTreePanel
├── PointsLabel         (TMP_Text, e.g. "Points: 0")
├── ScrollView          (UI → Scroll View)
│    └── Viewport
│         └── Content  (the auto-generated content RectTransform)
└── InfoPanel           (Panel, set inactive by default)
     ├── InfoTitle      (TMP_Text)
     ├── InfoDescription(TMP_Text)
     ├── InfoCost       (TMP_Text)
     ├── InfoRequirements (TMP_Text)
     └── UnlockButton   (Button → TMP_Text "Unlock")
```

5. Attach **UpgradesUIController** to `EntrepreneurTreePanel`.
6. Wire all Inspector fields on the controller:
   - **Tree Scroll Content** → the `Content` RectTransform inside the ScrollView
   - **Node Prefab** → your NodePrefab
   - **Line Prefab** → your LinePrefab
   - **Info Panel** / **Info Title** / **Info Description** / **Info Cost** / **Info Requirements** / **Info Unlock Button** → respective UI elements
   - **Points Label** → the PointsLabel TMP_Text

---

## Step 3 — Verify

Enter Play Mode and open the computer in-game.

- Navigate to the **UPGRADES** tab → you should see nodes arranged as specified by `uiPosition`.
- Nodes with no prerequisites are yellow (available), others are grey (locked).
- Clicking an available node attempts to unlock it (requires points).
- Hover a node to see the info panel.
- Earn points by triggering achievements (complete a sale, expand the store, etc.).

---

## Adding New Achievements

Open `AchievementSystem.cs` and:

1. Add a new value to the `AchievementId` enum (never rename existing values).
2. In an appropriate event handler (or a new one), call `Complete(AchievementId.YourNew)`.

---

## Adding New Nodes

1. Create a new `NodeData` asset (Step 1a).
2. Set `requiredNodeIds` to reference any prerequisite node IDs.
3. Add the asset to the `nodes` list in your `EntrepreneurTreeData`.
4. Adjust `uiPosition` so it sits correctly in the tree layout.

No code changes are needed for new nodes.

---

## Save / Load

The system saves to a **separate file** (`entrepreneurTree.dat`) alongside the existing
`save.dat` produced by `SaveGameSystem`. This means:

- The original `SaveGameSystem.cs` is **never modified**.
- Both files are written at the same time (when `SaveGameSystem.Save()` is called).
- Deleting `save.dat` without deleting `entrepreneurTree.dat` will cause a mismatch — delete both to fully reset.
