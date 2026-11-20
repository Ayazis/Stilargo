# Collaboration Notes

Scratchpad for Ci and FL: heads-ups, shared-file changes, questions and handovers.

- Newest entries go at the top. Start each with the date and author: `### 2026-11-19 · FL`.
- Tag who must act: **@Ci**, **@FL** or **@both**.
- When something is handled, mark it ~~struck through~~ or delete it. Lasting decisions belong in `docs/` or `CLAUDE.md`, not here.

---

### 2026-11-20 · FL (photoreal follow-ups)

**@Ci: `MainSceneCreator` now uses shared helpers (no behaviour change)**
- Its private `LoadOrCreate…Material` and `EnsureFolder` methods moved to `Editor/Rendering/PlaceholderMaterials` and `Editor/AssetFolders`, which `WorldPrefabBuilder` uses too. Asset paths and GUIDs are unchanged, so `Main.unity` doesn't need regenerating.
- `HdrpMaterials.CreateLit` now throws if an albedo is outside the PBR range: the brightest sRGB channel must be 30/255–240/255. `UnitColor` was `(0.25, 0.55, 0.95)` and is now `(0.25, 0.55, 0.9)`. `Greybox_Unit.mat` already exists, so set its Base Color to `408CE6` by hand.

**@both: test asmdef change.** `Stilargo.Tests.EditMode` now references `Stilargo.Editor`, for the new `Rendering` tests.

**@both: URP leftovers, all harmless.** `GraphicsSettings.asset` still maps URP to the deleted global-settings asset, and `ProjectSettings/URPProjectSettings.asset` still exists. Don't merge the old branches `fl/parallel-wave-0` and `ci/build-exe-rule` again: they still carry the URP global settings.

### 2026-11-19 · FL (photoreal rendering, branch `feature/photoreal-rendering`)

**@both: the project is now on HDRP (URP removed)**
- `com.unity.render-pipelines.universal` is gone and `high-definition` 17.6.0 is in. `UrpPipelineSetup`, the URP assets and `Assets/DefaultVolumeProfile.asset` were deleted. `Stilargo.Editor` now references `Unity.RenderPipelines.HighDefinition.Runtime`.
- **Color space is Linear.** Lights use physical units: the sun is 6000 K at 100,000 lux, with automatic exposure.
- `Stilargo → Apply Photoreal Rendering Settings` creates `Settings/HdrpPipeline.asset` and `Settings/PhotorealVolumeProfile.asset`, and is safe to rerun. The volume holds a physically based sky, fog, SSAO, SSR, contact shadows, ACES, bloom and vignette. Quality levels force anisotropic filtering and full-resolution textures.
- Make new materials only with `Editor/Rendering/HdrpMaterials` (`CreateLit`, `CreateUnlit`, `CreateTransparentUnlit`). Lit color is `_BaseColor`; unlit is `_UnlitColor` / `_UnlitColorMap`.

**@Ci: I changed `MainSceneCreator`**
It now uses `HdrpMaterials` for its materials and `PhotorealSceneDefaults` for the camera (TAA), the sun and a global volume. The Greybox materials and `Main.unity` were regenerated, so the material GUIDs changed. If you get conflicts, run `Stilargo → Recreate Main Scene` instead of merging YAML.

**@both: `FogOverlay._textureProperty` now defaults to `_UnlitColorMap`** (HDRP Unlit). The World prefab and its materials were regenerated.

**@both: harmless warning**
HDRP's global settings still reference one URP type (`MipGenRenderPipelineRuntimeResources`). The log warns about it, but the build is fine.

### 2026-11-19 · Ci (selection & orders, branch `ci/selection-orders`)

**@FL: `IUnitOrders` is unchanged.** Units implement it through Core's `UnitOrders` (a waypoint queue over an `INavigationAgent`). `SelectableUnit` (`Stilargo.Gameplay.Units`) has a `NavMeshAgent`, so you can still add `ScavengerComponent` to it.

**@FL: contract changes in my files**
- `IPlayerInput` has a new member, `WasCycleFormationPressedThisFrame` (bound to F). If you have a fake `IPlayerInput` anywhere, add it.
- New Core contracts: `Core/Selection/ISelectable` (Position, Orders, IsSelected, Select, Deselect) and `Core/Formations/IFormation` (`SlotsFor(count, center, facing)`).

**@FL: `Main` scene changed (regenerate, don't merge YAML)**
- `MainSceneCreator` now has `Stilargo → Recreate Main Scene` (batch: `-executeMethod Stilargo.Editor.Scenes.MainSceneCreator.Recreate`). It rebuilds `Main.unity` from code.
- Main now contains a "Walkable World" (ground plus landmarks under a `NavMeshSurface`), six greybox units and a "Player Controls" object.
- The bootstrapper's installer list is now `[GroundNavMeshInstaller, CombatInstaller]`. `GroundNavMeshInstaller` (`Gameplay/Navigation`) bakes that ground and is temporary. Once `WorldInstaller` derives from `Installer`, it replaces it in the list, ahead of `CombatInstaller`.

**@FL: NavMesh gotcha when `WorldInstaller` is called from the bootstrapper**
The bootstrapper runs in `Awake` at order -1000, before your components' own `Awake`/`OnEnable`. Two things break there:
- `NavMeshSurface.BuildNavMesh()` only registers the data if the surface is already enabled. Agents placed right after it find no NavMesh. Call `surface.AddData()` after the bake (it's idempotent); see `GroundNavMeshInstaller`.
- Components cached in `Awake` are still null when `Construct`/`Install` runs. Your `CacheComponents()` pattern already handles that; I copied it.

### 2026-11-19 · FL (world installer)

**@Ci: `WorldInstaller` is now an `Installer`, ready for Main (your proposal, accepted)**
- `WorldInstaller` derives from `Installer` and does all its work in `Install(GameContext)`. It reads nothing from the context yet.
- Drop in `Assets/_Project/World/World.prefab`. It's generated by `Stilargo → Build World Prefab` and contains the city, fog, loot and drop-off, all wired. Rerunning the menu item keeps asset GUIDs and config values, so your scene stays linked.
- In `Main`, add the prefab and put its `WorldInstaller` **first** in the bootstrapper's `_installers` list. Your installer can then read `VisibilityGrid`, `CityLayout`, `Wallet` and `ScavengingArea` from it.
- The city spans world X/Z 0–116 with its corner at the origin, and its ground's top face is at Y = 0. Main's placeholder ground plane and landmark boxes overlap it and will z-fight, so please remove them when you add the prefab.
- I didn't touch `Main.unity`, `GameContext` or your editor scripts. Sharing world services through `GameContext` is still open, so suggest a shape whenever you like.
- `WorldTest` now uses your `GameBootstrapper` with the prefab, the same wiring Main will have. It can go once Main has the world and a unit.

**@both: `runInBackground` is off**
The exe pauses whenever its window loses focus. That's normal for games, but it makes unattended test runs look frozen. Leave it unless one of us wants it changed.

### 2026-11-19 · FL (scavenging)

**@Ci: `Wallet` is in (`Stilargo.Core.Economy`)**
It has `Balance`, `Deposit(amount)`, `CanAfford(amount)`, `TrySpend(amount)` and a `BalanceChanged` event. Amounts must be positive. The single instance lives on `WorldInstaller.Wallet` once `Install()` has run.

**@Ci: scavenging doesn't need `IUnitOrders`**
Pickup and delivery happen when a unit gets close enough, so any unit you move can scavenge. To make a unit a scavenger, add `ScavengerComponent` and call `Construct(worldInstaller.ScavengingArea)`. It raises `Scavenged` with `PickedUp` or `Delivered`, and `Carrier.CarriedValue` shows what it's hauling. One unit carries one cache at a time. The drop-off is a scene transform for now; step 9 makes it the claimed house.

**@both: `WorldPoint` lives in `Stilargo.Core.Fog`, but scavenging uses it too**
It should probably move to a shared namespace (e.g. `Stilargo.Core`). That changes a contract, so I'll only do it with your OK.

### 2026-11-19 · Ci

**@FL: `Main` scene and `GameBootstrapper` are in (PR `ci/input-camera`)**
- `Assets/_Project/Scenes/Main.unity` is generated by `Stilargo → Create Main Scene` (`Scripts/Editor/Scenes/MainSceneCreator.cs`). It is first in Build Settings; `WorldTest` stays second.
- `GameBootstrapper` (`Stilargo.Infrastructure.Composition`) creates the input, builds a `GameContext` and calls `Install(GameContext)` on every `Installer` in its list. `Installer` is an abstract MonoBehaviour in `Stilargo.Gameplay.Composition`.
- New asmdef `Stilargo.Infrastructure` (refs Core, Gameplay, Unity.InputSystem). I added it as a reference to your `Stilargo.Editor` asmdef so the scene creator can add the bootstrapper.
- My contracts are in Core: `Input/IPlayerInput`, `Combat/IDamageable`, `Orders/IUnitOrders`. Positions use `System.Numerics` vectors because Core has no UnityEngine.

**@FL: proposal for wiring `WorldInstaller`**
Could `WorldInstaller` derive from `Installer` and do its current `Install()` work in `Install(GameContext context)`? Then it goes in the bootstrapper's list with no bootstrapper change. Units that need the `VisibilityGrid` or `CityLayout` could get them through `GameContext` once we agree on how; tell me if you'd prefer something else. After that, `WorldTest` can go.

### 2026-11-19 · FL

**@Ci: URP is now actually on (shared settings changed)**
The project had no URP pipeline asset assigned, so it silently ran the Built-in pipeline and URP materials rendered magenta in builds. `Assets/_Project/Settings/UrpPipeline.asset` is now the default in `ProjectSettings/GraphicsSettings.asset`. The quality levels have no override of their own, so they all use it. It was created by `Stilargo → Set Up URP Pipeline` (`Scripts/Editor/Rendering/UrpPipelineSetup.cs`).

~~**@Ci: `EditorBuildSettings.asset` lists a temporary scene**
`Assets/_Project/WorldTest/WorldTest.unity` is in Build Settings so the exe can be built before `Main` exists. When you add `Main`, put it first (index 0) so it loads at startup. We'll both be editing this file, so expect a small merge.~~ Done: `Main` is index 0.

**@Ci: how to wire the world into `GameBootstrapper`**
Add a `WorldInstaller` (`Stilargo.Gameplay.World`) to the scene, or as a prefab, and call `Install()` once. Afterwards:
- `worldInstaller.VisibilityGrid` → pass it to every unit's `VisionSourceComponent.Construct(grid)` and to every enemy's `FogHideable.Construct(grid)`.
- `worldInstaller.CityLayout` → road and plot data, e.g. for garrison/cover in step 6.
- The NavMesh is baked inside `Install()`, so spawn agents after calling it.

**@both: `WorldTest` is temporary**
`WorldTestBootstrap` and `Assets/_Project/WorldTest/` exist only until `Main` and `GameBootstrapper` land. Delete both at that point. To regenerate the scene, use `Stilargo → Build World Test Scene`.
