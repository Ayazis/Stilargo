# Collaboration Notes

Scratchpad for Ci and FL: heads-ups, shared-file changes, questions and handovers.

- Newest entries go at the top. Start each with the date and author: `### 2026-11-19 · FL`.
- Tag who must act: **@Ci**, **@FL** or **@both**.
- When something is handled, mark it ~~struck through~~ or delete it. Lasting decisions belong in `docs/` or `CLAUDE.md`, not here.

---

### 2026-11-25 · FL (claim & fortify, step 9 part 2: wiring, branch `ccr-5b226a2b-iaq9mi`)

**@Ci: I changed four of your files (all small changes). Please regenerate `Main`; don't merge YAML.**
- `GameContext` has a new member, `Config` (`GameConfig`), with one property per JSON config file. `GameBootstrapper` loads it through `Infrastructure/Config/GameConfigLoader` before running the installers. Add your own config files to `GameConfig`/`GameConfigLoader` too.
- `CoverGarrisonInstaller` now exposes `Garrisons` (the `GarrisonDirectory`), so claiming can find the occupied building.
- `MainSceneCreator` calls `FortificationSceneSetup.Create(...)` (`Editor/Fortification`). The installer list is now `[WorldInstaller, CombatInstaller, CoverGarrisonInstaller, UnitWorldInstaller, FortificationInstaller]`. Run `Stilargo → Recreate Main Scene`.

**@both: how it plays.**
- Garrison units in a building. The panel below the money label then shows a claim button. Clicking it claims the building that holds the most garrisoned units (Core `ClaimCandidatePicker`).
- After claiming, the panel shows the house's HP and a buy button for each fortification.
- Built fortifications appear as greybox blocks. Turrets shoot with a tracer and reveal fog within their range (Core `StaticVisionSource`).
- New materials, all made through `HdrpMaterials`: `Greybox_Barricade`, `Greybox_ReinforcedWindow`, `Greybox_Turret` and `Greybox_TurretTracer`.

**@Ci, for step 10:**
- `FortificationInstaller.Claim.House` is an `ICombatTarget`. Add it to the enemies' targets once it's claimed, and use its `Died` event as the lose condition.
- Clicks on the placeholder panel also reach your `SelectionController`. A click on a button deselects the selected units. That's harmless for now; step 11's UI should block pointer input over panels.

### 2026-11-25 · FL (claim & fortify, step 9 part 1: Core, branch `ccr-5b226a2b-iaq9mi`)

**@both: new Core module `Core/Fortification`, with EditMode tests.** It's plain C# only; nothing is in the scene yet.
- `HouseClaim.TryClaim(GarrisonBuilding)` claims one building per game, for free. The claimed building becomes a `ClaimedHouse`, and the drop-off moves to its entrance.
- `ClaimedHouse` implements `ICombatTarget`. Its `Position` is the entrance and its `Protection` is `None`. It has its own HP, starting at 400. **@Ci:** enemies can target it in step 10, and its `Died` event is the lose condition.
- Fortification slots come from the garrison layout: one at the entrance, one per garrison window and one per roof spot. `FortificationShop.TryBuy(definition)` fills the first free slot of the right kind and charges the `Wallet`. It returns a `PurchaseResult` (`Built`, `NoClaimedHouse`, `HouseDestroyed`, `NoFreeSlot`, `CannotAfford`).
- Barricades (entrance) and reinforced windows add HP. A `Turret` (roof) shoots the nearest visible enemy in range using Core's `Weapon`.

**@both: first JSON config file, following the new rule.** `Assets/StreamingAssets/Config/Fortification.json` holds the house HP, costs, HP bonuses and turret stats. `Infrastructure/Config/ConfigFile` reads any file from `StreamingAssets/Config`, and `FortificationConfig` maps this one to `FortificationSettings`, which validates every value. Keys missing from the file keep their default values. A missing file or an invalid value fails at startup and the error names the file. We can reuse `ConfigFile` for later config files.

**@Ci: one small change in my scavenging code.** `DropOff.Position` can now move, via `Relocate`. `ScavengingArea` already reads it live.

### 2026-11-23 · Ci (CLAUDE.md rules, branch `docs/config-files-rule`)

**@both: two new rules in `CLAUDE.md`.**
- **Tunable values go in JSON config files**, one per feature, under `Assets/StreamingAssets/Config/` (e.g. `Combat.json`). They ship next to the exe, so values can be changed without touching code or rebuilding. An `Infrastructure` loader maps them to plain C# settings classes that get injected; `Core` never reads files. Each value has a default and is validated on load. Asset references can stay in ScriptableObjects.
- **Refactor when necessary** as part of the work, keeping behavior the same and tests passing. Mention larger refactors before starting them.

Existing hard-coded values (e.g. `WeaponDefinition`, `CameraSettings`) aren't moved yet; we'll do that when we next touch them.

### 2026-11-22 · Ci (step status)

**@FL: `docs/phase-1-plan.md` now has status columns** in the Ci/FL order table. Please set your step to Done in the PR that finishes it, like I'll do for mine.

### 2026-11-22 · Ci (cover & garrison, branch `ci/cover-garrison`)

**@FL: `ICombatTarget` has a new member, `CombatProtection Protection`** (hit-chance and damage multipliers, `CombatProtection.None` = unprotected). `IDamageable` is unchanged. If your step-9 fortifications implement `ICombatTarget`, return `CombatProtection.None` or your own armour values.

**@FL: combat now needs visibility.** Player units only target enemies that are currently visible in your `VisibilityGrid` (Core `IVisibilityQuery`, adapted by `Gameplay/Combat/VisibilityGridQuery`). The AI side still sees everything. `CombatInstaller` now has a `_world` reference to read `WorldInstaller.VisibilityGrid`; I didn't change any of your files.

**@FL: `Main` scene changed (regenerate, don't merge YAML)**
- The installer list is now `[WorldInstaller, CombatInstaller, CoverGarrisonInstaller, UnitWorldInstaller]`. The new `CoverGarrisonInstaller` (on "Cover And Garrisons") derives cover points and garrison slots from `CityLayout` at startup.
- Player units have a new `GarrisonOccupant`. While garrisoned, a unit's `NavMeshAgent` is off and it stands on its slot (roof at Y = storeys × 3 m). On a roof it registers an extra `IVisionSource` with your grid (radius = its `VisionSourceComponent.VisionRadius` + 8 m), so rooftops see further.
- New materials: `Greybox_Cover` (lit, sRGB 138/125/99) and `Greybox_GarrisonSlot` (unlit teal), both made through `HdrpMaterials`.
- `WeaponDefinition` has a hit chance (default 0.75). The existing rifle assets keep their values and pick up the default.

**@both: new Core modules, with EditMode tests:** `Core/Garrison` (slots per storey on road-facing facades plus roof, occupancy, bonuses) and `Core/Cover` (cover points on road cells next to plots; standing within 1.2 m halves incoming hit chance). Right-clicking a building with units selected garrisons them; any move order makes them leave. `MoveOrderController` has `AddHandler(IPointerCommandHandler)` if you need right-click on something else, e.g. the claimed house in step 9.

### 2026-11-21 · FL (city in Main, branch `feature/city-in-main`)

**@Ci: Main now plays in the procedural city. I changed your `MainSceneCreator` (regenerate, don't merge YAML)**
- `Main` contains a `World.prefab` instance, which brings the city, fog and loot. The "Walkable World" ground and the landmark boxes are gone. So is `GroundNavMeshInstaller` (`Gameplay/Navigation`), along with `Greybox_Ground.mat` and `Greybox_Landmark.mat`.
- The bootstrapper's installer list is now `[WorldInstaller, CombatInstaller, UnitWorldInstaller]`. The new `UnitWorldInstaller` (`Gameplay/World`) gives each player unit a `VisionSourceComponent` and a `ScavengerComponent`, hides the enemies with `FogHideable`, and drives a money label (`Gameplay/Economy/WalletLabelView`, below your stance label).
- Spawns are placed from the city: the squad starts on the street through home (the drop-off at 58, 58) and the enemies on the next street north, 28 m away. The camera starts looking at home. Editor-only `CitySite` regenerates the layout from the prefab's seed and config, and the scene build fails if a spawn point isn't on a road.
- The scene creator writes `CameraSettings` bounds to the city's extent, (0, 0)–(116, 116). It only touches the bounds; your other tuning stays.
- `AddToBuildSettings` now drops entries for deleted scenes.
- Enemies are now hidden in fog. With weapon range 12 m and vision radius 10 m, units may shoot at an enemy they can't see yet. Your call whether combat should require visibility.
- The open branch `ccr-9e2b2367-06myp0` will conflict with this in `MainSceneCreator`. Merge it by hand and rerun `Stilargo → Recreate Main Scene`.

**@both: `CitySpawner` registers its NavMesh right after baking** (`AddData`, per Ci's NavMesh gotcha), so units placed by later installers find it.

**@both: `WorldTest` is deleted**, along with `WorldTestBootstrap`, `WorldTestHud`, `WorldTestAutoScavenger` and `WorldTestSceneBuilder`, and it's out of Build Settings. Main is the only scene.

**@both: new Core method `CityLayout.IsRoadAt(worldX, worldZ)`**, with tests in `Tests/EditMode/City/CityLayoutTests.cs`.

### 2026-11-20 · Ci (combat & stances, branch `ci/combat-stances`)

**@FL: `IDamageable` is unchanged.** Combat targets use a new Core contract, `Core/Combat/ICombatTarget` (`IDamageable` plus `Position` and `HealthFraction`). For fortifications in step 9, either implement `ICombatTarget` (then units can shoot them) or plain `IDamageable`. `Core.Combat.Health` is a ready-made `IDamageable` you can wrap.

**@FL: `IPlayerInput` has three new members**: `WasAggressiveStancePressedThisFrame`, `WasDefensiveStancePressedThisFrame` and `WasHoldStancePressedThisFrame` (bound to Z, X and C). Add them to any fake `IPlayerInput` you have; `FakePlayerInput` is updated.

**@FL: `Main` scene changed (regenerate, don't merge YAML)**
- `MainSceneCreator` now adds four greybox enemies (red, with a red ground ring), a tracer `LineRenderer` + `HitEffectView` on every unit, and `StanceController`/`StanceLabelView` on "Player Controls".
- New assets: `Settings/Weapons/PlayerRifle.asset`, `EnemyRifle.asset` (`WeaponDefinition`), and `Greybox_Enemy`, `Greybox_EnemyMarker`, `Greybox_PlayerTracer` and `Greybox_EnemyTracer` materials, all made through `HdrpMaterials`.
- The open branch `ccr-9e2b2367-06myp0` also rewrites `MainSceneCreator` (shared `PlaceholderMaterials`). Whichever lands second must merge that file by hand and rerun `Stilargo → Recreate Main Scene`. My lit colours are inside its 30–240/255 albedo range.

**@both: combat rules live in Core.** `UnitCombat` (the per-unit brain), `Weapon`, the target priorities in `Core/Combat/Targeting` and the stances in `Core/Combat/Stances`, all with EditMode tests. The Gameplay side is `Gameplay/Combat` (`CombatUnit`, `HitEffectView`, `WeaponDefinition`) and `Gameplay/Stances`.

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
