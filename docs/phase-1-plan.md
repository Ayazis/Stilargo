# Stilargo — Phase 1 Vertical Slice (greybox)

## Context
Stilargo is a top-down RTS (C&C camera) in a huge procedural city covered by fog of war, mixed with tower defence and King of the Hill. Phase 1 is one guy and one house. The core loop is mostly **scavenging** the fogged city for resources and bringing them home to **fortify** a building you claimed. A smaller part is **holding a hill** for income while an AI rival contests it. For units, the user wants stances and targets, cover and garrisoning, and waypoints and formations. The repo is empty apart from CLAUDE.md. The goal is a playable greybox slice that proves the loop, built on the layered architecture CLAUDE.md prescribes.

## Prerequisites (user, in editor)
- Create the Unity project in this repo (Unity 6 LTS, URP) and add a Unity `.gitignore`.
- Approve the packages: **Input System**, **AI Navigation** (runtime NavMesh baking for the procedural city) and **Test Framework**.

## Architecture
The asmdefs follow CLAUDE.md: `Stilargo.Core` (no UnityEngine) ← `Gameplay` / `UI` / `Infrastructure`, plus `Stilargo.Tests.EditMode`. A `GameBootstrapper` acts as the composition root and calls `Construct(...)` on the MonoBehaviours.

## Features (in build order, each one small change + EditMode tests for Core)
1. **Skeleton**: folders, asmdefs, the bootstrapper and an empty `Main` scene.
2. **Input + RTS camera**: the `IPlayerInput` abstraction and a pan/zoom/edge-scroll camera tuned by `CameraSettings` (a ScriptableObject).
3. **Procedural city (small)**
   - Core: `CityGenerator(seed, CityConfig)` produces a grid of blocks, roads and building plots.
   - Gameplay: spawns greybox buildings and bakes the NavMesh at runtime.
   - Tests: determinism per seed and road connectivity.
4. **Selection & orders**
   - Box/click selection.
   - Move orders with queued waypoints (shift).
   - Simple line and box formations via `IFormation` strategies.
5. **Combat & stances**
   - Core: `Health`/`IDamageable`, `WeaponDefinition` (a ScriptableObject), and the stances Aggressive/Defensive/Hold as `IStance` strategies.
   - Core: `ITargetPriority` decides which enemy a unit shoots first.
6. **Cover & garrison**
   - Buildings expose `GarrisonSlot`s for windows and roofs. Garrisoned units get a range and defence bonus.
   - Cover points along streets reduce incoming hit chance.
7. **Fog of war**
   - Core: `VisibilityGrid` keeps three states per cell: unseen, explored and visible. Units and buildings are vision sources.
   - Gameplay: a texture overlay shows the fog, and enemies are hidden outside visibility.
8. **Scavenging & economy**
   - Loot caches spawn in buildings. A unit picks them up and carries them home.
   - Core: a `Wallet` holds the money.
9. **Claim & fortify house**
   - Claim one building as the base.
   - Spend money on `FortificationDefinition`s (barricade, reinforced window, turret), which work as tower-defence elements.
10. **Hill + rival AI**
    - One hill zone gives income while you hold it.
    - One AI rival scavenges and contests the hill, using a simple state machine of strategies.
    - Win/lose: hold the hill for N minutes, or lose your house.
11. **Minimal UI**: money, selected unit stance buttons, fortify menu and hill timer.

## Multi-subagent execution
Each subagent owns its own feature folders (`Core/<Feature>`, `Gameplay/<Feature>`, `Tests/EditMode/<Feature>`). No two agents touch the same file. The bootstrapper, scene and asmdefs belong to me, the lead, and only I edit them.

**Wave 0 (lead, sequential)**
- The skeleton (step 1).
- The shared interfaces in Core: `IDamageable`, `IVisionSource`, `IPlayerInput`, `IUnitOrders`, `Wallet`, and a `CityLayout` data model.

These are the contracts every agent codes against.

**Wave 1 (5 agents in parallel, each in its own worktree)**

| Agent | Steps | Area |
|---|---|---|
| A | 2 | Input + camera |
| B | 3 | Procedural city + NavMesh |
| C | 4 | Selection, waypoints, formations |
| D | 5 | Combat, stances, target priority |
| E | 7 | Fog of war (`VisibilityGrid`) |

**Wave 2 (4 agents in parallel, after Wave 1 is merged)**

| Agent | Steps | Needs |
|---|---|---|
| F | 6 | Cover & garrison (needs city + combat) |
| G | 8 | Scavenging & economy (needs city + orders) |
| H | 9 | Claim & fortify (needs city + combat + wallet) |
| I | 11 | Minimal UI (against the interfaces) |

**Wave 3 (1 agent)**
- J: step 10, hill zone + rival AI. It uses everything above.

**Lead between waves**
- Review each agent's diff against CLAUDE.md.
- Merge the branches.
- Wire everything in `GameBootstrapper`.
- Run EditMode tests and fix integration breaks.
- List the editor steps for you: assign references, create the SO assets.

Each agent's brief gives its folder ownership, the contracts it must use, and "done = compiles + EditMode tests pass".

## Split between two people (Ci and FL)
The split follows the code: **Ci** takes units and combat, and **FL** takes the world and the economy. Each person owns their own feature folders, so the two accounts never edit the same files. Each person works on their own branch (`ci/<feature>`, `fl/<feature>`) and opens a PR into `main`.

**Wave 0, split so both start in parallel**
- FL (small PR, merged first, Ci reviews): the asmdefs only (`Stilargo.Core`, `Stilargo.Gameplay`, `Stilargo.Tests.EditMode`). No other code.
- Each person then writes the contracts they own inside their own feature folders, as part of their first feature PR:
  - FL: `CityLayout`, `IVisionSource`, `Wallet`.
  - Ci: `IDamageable`, `IPlayerInput`, `IUnitOrders`, plus `GameBootstrapper` and the `Main` scene.
- Until the other person's contract is merged, code against a local fake in your test folder, never a copy of the contract. A contract changes only through a PR that the other person approves.

Update the status cell in the PR that finishes a step. "Done" means merged into `main`.

| Order | Ci: units & combat | Status | FL: world & economy | Status |
|---|---|---|---|---|
| 1 | 2. Input + RTS camera | Done (PR 6) | 3. Procedural city + NavMesh | Done (PR 4) |
| 2 | 4. Selection, waypoints, formations | Done (PR 8) | 7. Fog of war | Done (PR 4) |
| 3 | 5. Combat, stances, target priority | Done (PR 10) | 8. Scavenging & economy (uses `IUnitOrders`) | Done (PR 5) |
| 4 | 6. Cover & garrison (uses `CityLayout`) | Done (PR 12) | 9. Claim & fortify (uses `IDamageable`, `Wallet`) | Done (PR #15) |
| 5 | 10. Hill + rival AI | Not started | 11. Minimal UI | Done (this PR) |

Step 1 (skeleton) is done. The city has been in `Main` since PR 11.

**Ownership rules**
- `GameBootstrapper` and the `Main` scene each have a single owner, and scene YAML merges badly. Each person adds their own installer, `CombatInstaller` or `WorldInstaller`, and the bootstrapper calls both. Only Ci touches the scene, and FL gets things into it with prefabs.
- Before a dependency is merged, code against its contract and test with fakes. For example, FL tests scavenging with a fake `IUnitOrders`.
- Step 10 needs FL's steps 3, 8 and 9. Ci starts it last.

Both people's sub-steps can still run as parallel subagents inside each account, as described above.

## Out of scope (phase 2+)
More houses and troops, recruiting, ambient civilian NPCs, multiple AI players, a full-size city, art, audio and saving.

## Verification
- EditMode tests pass in the Test Runner: city generator, visibility grid, wallet, stances, target priority, formations and hill control.
- Play the `Main` scene and do the full loop:
  1. Explore through the fog.
  2. Loot a cache and carry it home.
  3. Fortify the house.
  4. Garrison the unit.
  5. Fight the rival at the hill.
  6. See win or lose.
- No new compiler warnings.
