# Collaboration Notes

Scratchpad for Ci and FL: heads-ups, shared-file changes, questions and handovers.

- Newest entries go at the top. Start each with the date and author: `### 2026-11-26 · FL (two-layer fog of war)

**@Ci: `World.prefab` regenerated (rerun `Build World Prefab`, don't merge YAML).**
- The fog decal is gone (auto exposure brightened it and it skipped water). `FogOverlay` now adds a full-screen custom pass after post processing (`Art/Shaders/FogOfWar.shader`, `World/FogOfWar.mat` via `HdrpMaterials.CreateFogOfWarScreen`). `HdrpMaterials.CreateAlbedoDecal` and `FogDecal*.asset` are removed.
- Unseen cells are black (nothing shows). Explored cells out of view are in shadow: the map shows dimmed and `FogHideable` hides enemies and loot there.

### 2026-11-19 · FL`.
- Tag who must act: **@Ci**, **@FL** or **@both**.
- When something is handled, mark it ~~struck through~~ or delete it. Lasting decisions belong in `docs/` or `CLAUDE.md`, not here.

---

### 2026-11-26 · FL (town plan: market square, house blocks, park blocks, branch `feature/game-feel-city`)

**@Ci: the city is now 5×5 blocks (144 m) with a town plan.** Roads stay on the same 14-cell grid, so home (58, 58), the hill (86, 72) and the rival start are still on roads.
- `Core/City/TownPlan`: the middle block is an open **market square** (new `PlotUse.Market`, no building; `MarketDressing` paves it and adds a fountain, trees, benches and stalls). The ring around it holds **house blocks** (`HouseBlockLayout`: 3–4 storey town houses along the streets, courtyard inside). Further out are the **outskirts** (the old recursive split, now 1–2 storeys) and **park blocks** (a whole block each).
- **New config `Config/Town.json`** (`parkBlocks`, `houseBlock`: row depth, frontage, storeys). `GameConfig.WithTown`, `CityGenerator(seed, config, town)`; the 2-arg constructor uses defaults.
- **`Buildings.json` lost its `park` entry** (parks come from `Town.json`). The church and taverns use the new `PlotPlacement.FacingSquare`, so they line the market square. `PlotUsePlanner` only hands out house plots.
- `World/CityConfig.asset`: `blocksX/Y` 4 → 5, `maxStoreys` 4 → 2 (outskirts). No scene or prefab change.

### 2026-11-26 · FL (camera presets, double-click garrison, no haze, fog toggle, branch `feature/game-feel-city`)

**@Ci: contracts and shared settings changed.**
- `IPlayerInput` has a new `IsEdgeScrollEnabled`; add it to any fake. `InputSystemPlayerInput(CameraControlChoice)` and `GameContext(input, config, cameraControls)` take the chosen camera preset (`Core/Input/CameraControlPreset`: middle or right mouse orbit, with or without edge scroll). With right-mouse orbit an order fires on release when the press did not drag (`Core/Input/DragOrClick`).
- Garrisoning needs a double right-click on the building (`Core/Input/DoubleClick`, 0.4 s); a single right-click is a plain move.
- **Volume profile changed:** atmospheric fog is off (`PhotorealRenderingSetup`, rerun `Apply Photoreal Rendering Settings`). It washed the city white when zoomed out.
- Settings page: a camera preset button and a fog-of-war on/off button (`Core/Hud/GameOptions`, `VisibilityGrid.DisableFog/EnableFog`).
- The ambience loop is gone (it sounded like surf and gulls); `ambienceVolume` left `Audio.json`.
- No scene or prefab change.

### 2026-11-26 · FL (health bars, sound, landmarks, line of sight, house selection, branch `feature/game-feel-city`)

**@Ci: `Main` scene and `World.prefab` regenerated (regenerate, don't merge YAML); several contracts changed.** Built on top of `feature/unit-avatars` plus the uncommitted house-teams work.
- **Combat values moved to `Config/Combat.json`** (health, weapon damage/range/cooldown/hit chance, sight per side, player move speed, corpse time). `CombatSide`, `WeaponDefinition` and `Settings/Weapons/*.asset` are deleted. Fights last about 3× longer (enemy 160 HP, soldier 250 HP).
- **Fog has line of sight.** `IVisionSource` has a new `EyeHeightMetres`; `VisibilityGrid.UseSightBlocker(ISightBlocker)`; `Core/City/CitySkyline` blocks sight with building heights. New `Config/Vision.json`: base radius 24 m, plus 1.5 m per metre of height (rooftops, church tower). `VisionSourceComponent.Construct` now takes `VisionSettings`; `RooftopVision` wraps the unit's own sight.
- **Landmarks:** `Plot` has `Use` (`PlotUse`: House, Church, School, Tavern, Park, Water), `TowerStoreys`, `IncomeMultiplier`, `HasBuilding`. `PlotUsePlanner` assigns them after generation from `Config/Buildings.json`; `CityLayout.WithPlots(...)` is new. Parks and water have no garrison building. Church roof slots are on the bell tower top. `HouseIncomeRate` multiplies by the plot's multiplier. `CitySpawner` dresses each use (`IPlotDressing`) and has new `_landmarkMaterials` (made by `Build World Prefab`).
- **Houses are selectable:** `SelectionController.WorldClicked`, Core `BuildingSelection`, `ClaimTarget(claim, picker, selection)` with `FocusedBuilding`. `FortificationInstaller` has new `_combat` and `_selectionOutlineMaterial` fields; `FortificationSceneSetup.Create(world, combat, garrisons)` reads the enemies from the combat installer.
- **Health bars** for every unit (UI Toolkit overlay, `HudView.ShowHealthBars`); enemy bars only while visible.
- **Sound:** new `Gameplay/Audio/AudioInstaller` (between `HillRivalInstaller` and `HillHudLink`) and `Config/Audio.json`. Every sound is synthesised in `Core/Audio`; there are no audio files. `HudInstaller` has a new `_audio` field.
- **Animation:** Mixamo clips no longer bake XZ root motion into the pose (it made the run drift ~2 m ahead and snap back). The run plays at agent speed / measured clip speed (`Editor/Units/RunClipSpeed`, ~3.5–3.8 m/s). Player and rival speed are 3.8 m/s (rival `moveSpeedMetresPerSecond` in `Hill.json` was 6). Dead soldiers fall and stay for `corpseSeconds`.
- New material: `Greybox_BuildingOutline`, plus `World/Landmark*.mat` and `KitWater.mat`, all made through `HdrpMaterials`.

### 2026-11-25 · Ci (hill + rival AI, step 10, branch `ci/hill-rival`)

**@FL: what your step 11 UI can read.** It's all on `HillRivalInstaller` (`Gameplay/Hill`, on the "Hill And Rival" object), filled in by `Install`:
- `Hill` (Core `HillControl`): `State` (`HillState`: `Empty`, `HeldByPlayer`, `HeldByRival`, `Contested`), the `StateChanged` event, `HeldSeconds`, `RemainingHoldSeconds`, `Settings.HoldDurationSeconds` and the `HoldCompleted` event. Income goes straight into your `Wallet`, so the money label updates by itself.
- `Referee` (Core `MatchReferee`): `Outcome` (`MatchOutcome`: `Undecided`, `Won`, `Lost`), `IsDecided` and the `Decided` event.
- `Rival` (Core `RivalBrain`): `Current.Name`, `IsDefeated` (whole squad dead) and the `StrategyChanged` event. `RivalWallet` holds the rival's scavenged money.
- **Your HUD's hill timer is now live.** `Gameplay/Hill/HillStatus` implements your `IHillStatus` (it goes inactive once the match is decided). `Gameplay` can't reference `UI`, so I couldn't call `HudInstaller.UseHillStatus` from my installer. I added a small installer of my own, `UI/Hud/HillHudLink`, that runs between `HillRivalInstaller` and `HudInstaller` and makes that call. I didn't change any of your step 11 files.
- My placeholder `HillHudView` still draws two lines in the **top-right** corner (who holds the hill, what the rival is doing), plus the centred win/lose box, which your HUD doesn't have yet. The timer is only in your HUD now. Replace or delete my view when your UI covers the rest. `MatchEndPause` sets `Time.timeScale = 0` once the match is decided.

**@FL: I changed two of your files, adding members only.** `GameConfig` has two new properties, `Hill` and `Rival` (constructor now takes a `HillRivalSettings`), and `GameConfigLoader` loads `Hill.json` through the new `Infrastructure/Config/HillConfig` (`HillEntry`, `RivalEntry`), following `FortificationConfig`.

**@both: new config file `Assets/StreamingAssets/Config/Hill.json`.** The `hill` section has the centre (X/Z in metres, it must be on a road: startup fails otherwise), radius, income per second and hold duration (180 s). The `rival` section has squad size, move speed, how long each strategy runs, scavenge reach, a scavenge radius (30 m: the rival only loots caches this close to its base, so it doesn't walk into our home), how far the squad spreads on the hill and an arrival tolerance.

**@both: how it plays.**
- The hill is a flat disc on the road east of home, about 31 m from both home and the rival's start. Its colour shows the state: grey free, blue yours, red the rival's, amber contested. You hold it when only your living units stand in it. Hold time adds up across separate holds; hold it for 3 minutes in total to win.
- The four red enemies are now the rival's squad. A Core state machine (`Core/Rival`) runs one strategy at a time, each its own `IRivalStrategy` type: **Scavenging** takes your loot caches to the rival's base. **Contesting the hill** spreads the squad on it. **Raiding your house** marches them to the claimed house's entrance. Scavenging switches to the hill after its time is up or as soon as you hold the hill. After contesting, the rival raids if you have a house, and otherwise scavenges again. A raid turns back to the hill when you take it.
- Once you claim a house, it joins the enemies' targets (`CombatInstaller.AddPlayerTarget`). Its `Died` event is the lose condition.
- Enemies now have an `AutonomousUnit` that holds their move orders. `SelectableUnit` and `AutonomousUnit` share the new `NavMeshOrderDriver`, which I pulled out of `SelectableUnit` without changing its behaviour.

**@FL: `Main` scene changed (regenerate, don't merge YAML).** After merging your PR #17, the installer list is now `[WorldInstaller, CombatInstaller, CoverGarrisonInstaller, UnitWorldInstaller, FortificationInstaller, HillRivalInstaller, HillHudLink, HudInstaller]`. `MainSceneCreator` calls `HillSceneSetup.Create(...)` and `HillSceneSetup.LinkToHud(...)` (`Editor/Hill`) next to your `HudSceneSetup.Create(...)`. I resolved the `MainSceneCreator` conflict by keeping both sides. New materials, all made through `HdrpMaterials`: `Greybox_HillEmpty`, `Greybox_HillHeldByPlayer`, `Greybox_HillHeldByRival` and `Greybox_HillContested`.

**Verified in the exe:** loot pickup and delivery, garrison, claim, barricade, the rival raiding the house as soon as it was claimed (house 550 → 490 HP before our squad wiped the raiders out), holding the hill (marker turns blue, income flows) and the win screen. The lose path is covered by EditMode tests only.

**@both: right-clicks behind tall buildings miss.** With the low camera, buildings often cover the road you aim at, so move orders get lost. Short hops work. This isn't from step 10, but it makes playtesting slow.

**Open:** the default turret range (16 m) versus the enemy spawn distance is unchanged. Raiders now walk right up to the house entrance, so turrets reach them there.

### 2026-11-25 · Ci (step 9 verified, branch `ci/verify-claim-fortify`)

**@FL: step 9 is verified in Unity and marked Done.**
- Your code compiled as-is, so I changed nothing in your files. Unity created the missing `.meta` files.
- I regenerated `Main` with `MainSceneCreator.Recreate`. Its installer list is `[WorldInstaller, CombatInstaller, CoverGarrisonInstaller, UnitWorldInstaller, FortificationInstaller]`, and the four `Greybox_*` fortification materials were created. All 431 EditMode tests pass.
- In the exe, `Fortification.json` loads. Garrisoning shows Claim, claiming shows the house HP, and buying all three fortifications charges the wallet and adds the right HP. The turret revealed fog and shot all four enemies.
- **Heads-up:** with the default 16 m turret range, the turret can't reach the starting enemies, which are about 30 m north of the house. I tested with a temporary range in the exe's copy of the config. Step 10's attackers will come closer, but tell me if you want a different default.

### 2026-11-25 · FL (PR #15, step 9 claim & fortify)

**@Ci: please check PR #15 in Unity before it's merged.** It's a draft because it was written without Unity: the Core rules and tests compile and pass, but the Gameplay, Infrastructure and Editor code has never been compiled. On branch `ccr-5b226a2b-iaq9mi`:
1. Open the project (this creates the `.meta` files) and fix any compile errors.
2. Run `Stilargo → Recreate Main Scene`.
3. Run the EditMode tests.
4. Build the exe and test: garrison units → **Claim** on the panel → buy each fortification → turrets fire.
5. Commit the `.meta` files and `Main.unity` to the branch and mark the PR ready.

Tell me about anything you'd rather I fix myself. Your files I touched are listed in the entry below.

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

### 2026-11-25 · FL (minimal UI, step 11, branch `fl/minimal-ui`)

**@Ci: the HUD is in; it touches a few of your files**
- New `Stilargo.UI` asmdef with `HudInstaller` and `HudView` (UI Toolkit, built in code). It's listed last in the bootstrapper. `Stilargo.Editor` now references `Stilargo.UI`.
- The IMGUI placeholders are gone: `StanceLabelView`, `WalletLabelView` and `FortificationPanelView`. `FormationLabelView` stays.
- `CombatInstaller` now exposes `SelectionController` and `SelectionStance` (the stance label field is removed). `SelectionController` has `UseUiPointerBlocker(...)`, so a press on a HUD button doesn't start a selection drag.
- `FortificationInstaller` and `UnitWorldInstaller` lost their view fields; `FortificationInstaller` exposes `FortifyMenu`.
- Logic is in `Core/Hud` (`StanceMenu`, `FortifyMenu`, `HudText`, `IHillStatus`) with EditMode tests.
- `MainSceneCreator` was changed, so `Main.unity` was regenerated: run `Stilargo → Recreate Main Scene` instead of merging YAML. New assets: `Assets/_Project/UI/HudPanelSettings.asset` and `HudTheme.tss`. A PanelSettings made at runtime has no theme and renders nothing, hence the asset.
- **Step 10:** implement `IHillStatus` (`IsActive`, `SecondsRemaining`) and call `HudInstaller.UseHillStatus(...)` before the HUD installs; the timer then shows up.

### 2026-11-25 · Ci (building income, branch `feature/building-income`)

**@FL: money now comes from claimed houses; some shared contracts changed**
- `HouseClaim` allows any number of claims. `House` is still the base (first claim): drop-off, rival raids and the lose condition use only it. New: `Houses`, `TryGetHouse(...)`.
- `ClaimedHouse` has `Condition` (wears over time, repaired for money), `Maintenance` (condition × health), `FortifiedFraction`, `OccupiedFraction`, `FloorCells`.
- New `Core/Income`: `HouseIncomeRate` (size × (1 + barricade/occupation/maintenance bonuses)), `HouseIncome` (pays + wears), `HouseRepair`. Values in new `StreamingAssets/Config/Income.json`; `GameConfig` takes it as a fourth argument, after `EconomySettings`.
- The hill no longer pays: `HillControl` lost its wallet argument, `incomeMoneyPerSecond` is gone from `Hill.json`.
- `FortificationShop.TryBuy/Check` now take the house. `FortifyMenu` takes `ClaimTarget`, shop and `UpkeepMenu`; the fortify panel shows income, condition and a repair button.
- No scene change: `FortificationInstaller` adds a `HouseIncomeTicker` component at runtime.

**@FL: Unity-written settings changes committed too**
Opening the project in Unity 6000.6.10f1 (batch test/build) rewrote `GraphicsSettings`, `QualitySettings`, `ProjectAuditorSettings`, `HdrpPipeline.asset`, `HDRenderPipelineGlobalSettings.asset` and added `HDRPProjectSettings.asset`. Not hand-edited; committed so the project stays consistent.

### 2026-11-25 · FL (Mediterranean look, branch `feature/mediterranean-look`)

**@Ci: shared rendering and scene files changed**
- Volume profile: fog mean free path 4000 m → 700 m for aerial haze (`PhotorealRenderingSetup`, rerun `Apply Photoreal Rendering Settings`).
- Sun: 6000 K → 5000 K (`PhotorealSceneDefaults`), pitch 50° → 38° for longer shadows (`MainSceneCreator`). `Main.unity` was regenerated with `Recreate Main Scene`; regenerate it instead of merging the YAML.
- `CitySpawner` now needs a roof prefab and takes wall/roof material palettes, picked per plot by `Core/City/PlotVariation`. `World.prefab` was rebuilt; new assets in `Assets/_Project/World` (`Roof.prefab`, `HipRoof.asset`, `Wall0-5.mat`, `Roof0-2.mat`).

### 2026-11-25 · FL (starting money)

**@Ci: `GameConfig` contract changed**
- New `Config/Economy.json` (`startingMoney`, default 1000) loaded by `EconomyConfig` into `Core/Economy/EconomySettings`.
- `GameConfig` constructor now takes a third `EconomySettings` argument and exposes `Economy`. `WorldInstaller` deposits the starting money into the player wallet.

### 2026-11-25 · FL (Esc pause menu)

**@Ci: `IPlayerInput` contract changed**
- New `WasPauseMenuPressedThisFrame` (Esc). Any other `IPlayerInput` implementation must add it.
- `Wallet.SetBalance(int)` added (settings menu). `HudView.Construct` now takes a `PauseMenu` (`Core/Hud/PauseMenu`); opening the menu sets `Time.timeScale` to 0.

### 2026-11-25 · FL (house teams, branch `feature/house-teams`)

**@Ci: houses now have an owner (Blue, Red or unoccupied); claim contracts changed**
- New `Core/Fortification`: `Team` (Blue/Red), `HouseLedger` (every house ever claimed, shared by both teams; `HouseCreated` fires once per building), `BaseDropOff` (drop-off follows the base), `HouseTarget` (house as a combat target, valid only while its team owns it), `MountedTurret`.
- `HouseClaim(Team, HouseLedger)` replaces `HouseClaim(settings, layout, dropOff)`. It is now one team's houses: new `Team`, `Lost` event and `CanClaim(building)`. A team can claim an unoccupied house or the other team's ruin, not a standing enemy house.
- `ClaimedHouse` has `Owner`, `OwnerChanged`, `IsOwnedBy`, `IsClaimableBy`, `NeedsRepair`, `DamagedFraction`. A house at 0 HP becomes unoccupied and leaves its owner's claim, but keeps its damage and fortifications.
- `HouseRepair` now also repairs battle damage (priced like wear) and can bring a ruin back. `RestoreCondition` is now `Restore`.
- Lose rule: `MatchReferee` no longer watches the base. `HillMatch` calls `Referee.Review(playerUnits)` each tick, and the player loses when they own no house and have no unit alive.
- Rival: `RivalSurroundings(hill, RivalHouses, scavenging)`. The raid targets the player's oldest standing house. New `ConfiscateHouseStrategy` claims a fallen house for Red. New `confiscateSeconds` in `Hill.json`.
- `CombatInstaller.AddEnemyTarget(...)` lets player units shoot Red houses. `TurretBattery`/`FortificationView` now take the `HouseLedger`, and turrets only fire while their house stands and belongs to Blue.
- No scene or prefab change.

### 2026-11-25 · FL (unit avatars and animations, branch `feature/unit-avatars`)

**@Ci: units are now animated soldiers; scene regenerated**
- Art: 4 Microsoft Rocketbox military avatars (MIT, see `Art/Models/Units/LICENSE-Rocketbox.md`) and 4 Mixamo rifle clips in `Art/Animations/Mixamo` (idle, aim, crouch, run). Both import as Humanoid.
- Editor: `Editor/Units/UnitAvatars` builds `Assets/_Project/Units/<avatar>.prefab` (HDRP/Lit materials, scaled to 1.8 m); `UnitAnimatorAsset` builds `UnitAnimator.controller`. `UnitMaterials` became `UnitLook` (avatar instead of body material); `Greybox_Unit.mat`/`Greybox_Enemy.mat` are no longer used.
- Units keep the capsule collider (renderer removed) with the avatar as child `Avatar`. New `Gameplay/Animation/UnitAnimationView` on each unit root; `CombatInstaller` constructs it. Pose rules in `Core/Animation/UnitPosePicker` (run > crouch in cover > aim after a shot > idle), with tests.
- `Main.unity` was regenerated with `Recreate Main Scene`; regenerate instead of merging the YAML.

### 2026-11-26 · FL (graphics polish, branch `feature/game-feel-city`)

**@Ci: shared rendering settings changed** (rerun `Stilargo → Apply Photoreal Rendering Settings`)
- Pipeline: volumetric clouds supported; directional shadows use High filtering (PCSS) at 4096 (1024 on Low).
- Volume profile: sparse volumetric clouds, shadow distance 600 m with 4 cascades, micro shadowing, stronger/wider SSAO (high), far-horizon depth of field (300–900 m), color adjustments (+8 contrast, −5 saturation), white balance +6, thin film grain. Fog stays off.
- Camera: TAA sharpen 0.8 in `PhotorealSceneDefaults` (takes effect after `Recreate Main Scene`).
- Fog of war is now an HDRP decal projected down onto ground and buildings (`FogOverlay` needs a `DecalProjector`, material `World/FogDecal.mat`, made by `HdrpMaterials.CreateAlbedoDecal`). Old `World/Fog.mat` removed. `World.prefab` was rebuilt; rebuild it instead of merging the YAML.
- `Stilargo.Gameplay.asmdef` now references `Unity.RenderPipelines.HighDefinition.Runtime`.
- SSGI turned back off (flicker, black alleys).
- Fog decal material gets its color map enabled through a 1×1 placeholder texture (`World/FogDecalPlaceholder.asset`).

### 2026-11-26 · Ci via Claude (city variety wired up, on top of `feature/game-feel-city`)

**@FL: finished the WIP city variety from `docs/CityVarietyContinuation.md`; contracts changed**
- New `PlotUse`s: `Plaza`, `Garden`, `Yard`. `Plot.HasBuilding` is true only for House/Church/School/Tavern. The house-block courtyard is now a `Garden` plot.
- `CitySpawner.Construct(layout, streetProps)` takes the props from `StreetFurniturePlanner`; `WorldInstaller.InstallCity` plans them. Plaza/Garden/Yard dressings are registered.
- `Town.json` has new sections `streets`, `neighbourhoods`, `streetLife` and `houseBlock.cornerExtraStoreys` (`StreetsEntry`, `NeighbourhoodsEntry`, `StreetLifeEntry`).
- `CityLayout.Blocks` is no longer one block per grid cell (merged and alley-split outskirts), so don't index it by grid position.
- No scene/prefab change. Core and EditMode tests were compiled and run outside Unity (739 green); Gameplay/Infrastructure still need a Unity compile and exe build.

### 2026-11-26 · FL via Claude (canal town replaces the grid city, branch `ccr-3bace2de-nutzy1`)

**@Ci: the game now plays in a new map; contracts and config changed**
- New `Core/City/CanalTown`: `CanalTownGenerator` builds a canal town (canal with quays and bridges, staggered streets, main square with fountain, church square, market, parks with ponds/fountains, boatyard, courtyard house blocks, free-standing homes on the edge) inside countryside (lanes, fields, orchards, meadows, farms). Map is 136×136 cells (272 m); the town is the middle 96×96. Streets are 4 cells (8 m) wide.
- Soldier entry points: player on the west map edge, rival on the east edge, each on a high road. `WorldInstaller.PlayerEntry` / `RivalEntry`; the drop-off (home marker) moves to the player entry at startup. `WorldInstaller` no longer uses `CityConfigAsset` (`_cityConfig` removed). The old `CityGenerator` and its settings are now unused by the game.
- New `PlotUse`s: `Fountain`, `Meadow`, `Field`, `Orchard` (with dressings in `Gameplay/City`). `PlotUsePlanner` now counts plots the town plan already has (the canal town's church and water) towards a rule.
- Config: `Town.json` has a new `canalTown` section. `Hill.json` hill centre moved to (143, 137) so it lies on a street; `HillSettings` defaults match.
- `Main.unity` was regenerated with `Recreate Main Scene` (squads now start on the high roads, camera bounds cover 272 m). Regenerate instead of merging the YAML.
- Preview without Unity: `dotnet run --project Tools/CityPreview -- <seed> docs/CityPreview.md` writes a text map and an isometric `docs/CityPreview.html`.

### 2026-11-26 · FL via Claude (canal-town building models, branch `ccr-3bace2de-nutzy1`)

**@Ci: eight Blender-built buildings from `docs/CanalTownArtList.md` (#1, #3–#9)**
- `Tools/Art/canal_kit.py` is a shared building kit (blocks of storeys, openings, hip/gable tiled roofs, chimneys, props), generalised from `build_house_1.py`. One short script per building: `build_row_house.py`, `build_townhouse.py`, `build_detached_home.py`, `build_farmhouse.py`, `build_workshop.py`, `build_church.py`, `build_school.py`, `build_tavern.py`.
- FBX in `Art/Models/<Name>/`, .blend in `Tools/Art/Blend/`. `Stilargo → Build Canal Town Prefabs` (`Editor/World/CanalTownModelAssets`) makes `World/<Name>/<Name>.prefab` with HDRP materials; its material map must match `MATERIALS` in `canal_kit.py`.
- Water pieces (#10�14): `build_quay.py` (`QuayWall`, `QuaySteps`: tileable 8 m segments, paving at z = 0, water 1.2 m below), `build_bridge.py` (`Bridge`: 10 m span, 4 m deck, 14 m long), `build_pond.py` (`PondEdge`, `PondCorner`, `Reeds`, `LilyPads`), `build_rowing_boat.py` (`RowingBoat`). The flat ground means the canal would need to be sunk 1.2 m to use the quays as-is.
- Squares (#15�19): `build_fountain.py` (`Fountain`, `Well`), `build_square_props.py` (`SquareInlay`, `ChurchForecourt`, `StoneBench`, `Planter`, `CafeTerrace`), `build_market.py` (`MarketStall`, `MarketHall`). Multi-model scripts take `-- --piece <Name>` to rebuild one.
- Green, countryside and street (#20�32): `build_trees.py` (plane, umbrella pine, willow, three fruit trees, shrub), `build_garden_props.py` (gazebo, monument, hedge, garden wall, vegetable bed, washing line, farm cart, haystack, woodpile), `build_countryside.py` (wheat and vegetable patches, wildflowers, rocks, sheep, fence, milestone, wayside shrine, town sign), `build_street_props.py` (lamp, bench, crates, barrels, planter, handcart). 57 prefabs in total.
- Fixed sizes for now; `CitySpawner` doesn't place them yet (it still uses procedural boxes). No scene, prefab-YAML or settings change.

### 2026-11-26 · FL via Claude (canal-town models placed in the game, branch `ccr-3bace2de-nutzy1`)

**@Ci: buildings are now the Blender models; contracts changed**
- Core: new `HouseStyle` (Row, Corner, Townhouse, Detached, Farmhouse, Workshop) and `Facing`. `Plot.Style`/`Plot.Front` are set by the new `HouseStyler` (from what surrounds each building) at the end of `CanalTownGenerator`; `Plot.UsedFor` keeps them. `Canal` is public and on `CanalTown.Canal` (with `BridgeWidthCells`); `CanalTown`'s constructor takes `(layout, canal, (player, rival))`.
- Gameplay: `CitySpawner.Construct(layout, streetProps, canal)`. Each building keeps its box (renderer off) as collider, selection and NavMesh obstacle, with its model on top facing `Plot.Front`, at the model's native size (the city will be remapped to the model sizes later). `CanalBuilder` sinks the canal edge to edge (bed 2 m, water 1.2 m down), lines it with `QuayWall`/`QuaySteps` and puts a `Bridge` on each bridge street; the ground is two strips either side.
- Removed as dead: the box-house roofs and facade kit (`RoofMaker`, `RoofMeshes`, `RoofShape`, window/door/cornice and wall/roof palettes on `CitySpawner`), `House/Church/School/TavernDressing`, `IPlotDressing.TryGetWallMaterial`, and `LandmarkMaterials` school/tavern walls.
- `World.prefab` was rebuilt with `Build World Prefab`; rebuild it instead of merging the YAML. The Bridge FBX imports with read access, since the runtime NavMesh bake needs it.
- Props (same day): every open-ground dressing and the street props now place the Blender props (`PropKind`/`PropModels` on `CitySpawner`, filled by `Build World Prefab`): parks, gardens (garden walls), yards, meadows, orchards, fields (light 4 m crop patches tiled at ~8 m), squares, the market, fountains/wells and ponds (kerb kit, reeds, lilies, willow). Boats are moored along the canal and a town sign, milestone and shrine stand at the entry roads. `CitySpawner.Construct` now takes the `CanalTown`. Solid props (fountain, well, gazebo, monument, stalls) get a not-walkable `NavMeshModifierVolume`. `TreeProps` and the foliage/bark/cloth/shadow landmark materials are gone.

### 2026-11-26 · FL via Claude (lighting and atmosphere pass, branch `ccr-3bace2de-nutzy1`)

**@Ci: volume profile and Main scene changed**
- `PhotorealRenderingSetup`: thin height fog is back for aerial perspective (mean free path 1400 m, up to 120 m high, sky colour). SSAO is softer (0.8, 1.5 m radius), exposure gets +0.4 EV, and the grade is neutral (contrast 0, white balance −4 instead of +6).
- Sun is 5500 K (was 5000 K) and pitched 52° (was 38°) in `MainSceneCreator`. `PhotorealVolumeProfile.asset` was rewritten with `Apply Photoreal Rendering Settings` and `Main.unity` regenerated with `Recreate Main Scene`; regenerate instead of merging.

### 2026-11-27 · FL via Claude (House 1 photoreal pass, branch `ccr-3bace2de-nutzy1`)

**@Ci: new texture sets and a parallax option for Blender houses**
- New Poly Haven CC0 sets in `Art/Textures`: `clay_roof_tiles_02` (with `_height.jpg`), `green_rough_planks`, `old_planks_02`.
- `SurfaceTextures` loads an optional `<name>_height.jpg`; `HdrpMaterials.ApplyParallax` turns on HDRP pixel displacement (POM). `HouseMaterialLook.ScannedRelief(...)` maps a set through UV0 in metres with parallax; `BlenderHouseAssets` handles it.
- House 1's roof is now one UV-mapped tiled surface per slope (`RoofTiles` material) instead of modelled tiles; `RoofTileA/B`, `TileDark`, `TileLichen` slots are gone from House 1 (still used by `canal_kit` buildings). Shutters, door and flower box use the scanned wood.
- (later, same day) House 1 window boxes use scanned Poly Haven periwinkle plants (`Tools/Art/Sources/periwinkle_plant`, red copy from `Tools/Art/make_flower_textures.py`) with double-sided `ScannedFoliage` materials. Worn looks: `HouseMaterialLook.Worn()` lays `Art/Textures/wear_detail/wear_detail.png` (from `Tools/Art/make_wear_detail_map.py`) over a scanned set as an HDRP detail map (`HdrpMaterials.SetDetail`). New sets: `brick_4`, `weathered_planks`, `worn_planks`, `green_rough_planks`; `old_planks_02` removed. Only House 1 uses these; canal-town buildings are unchanged.
