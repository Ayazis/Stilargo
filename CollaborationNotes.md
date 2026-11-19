# Collaboration Notes

Scratchpad for Ci and FL: heads-ups, shared-file changes, questions and handovers.

- Newest entries go at the top. Start each with the date and author: `### 2026-11-19 · FL`.
- Tag who must act: **@Ci**, **@FL** or **@both**.
- When something is handled, mark it ~~struck through~~ or delete it. Lasting decisions belong in `docs/` or `CLAUDE.md`, not here.

---

### 2026-11-19 · FL

**@Ci: URP is now actually on (shared settings changed)**
The project had no URP pipeline asset assigned, so it silently ran the Built-in pipeline and URP materials rendered magenta in builds. `Assets/_Project/Settings/UrpPipeline.asset` is now the default in `ProjectSettings/GraphicsSettings.asset`. The quality levels have no override of their own, so they all use it. It was created by `Stilargo → Set Up URP Pipeline` (`Scripts/Editor/Rendering/UrpPipelineSetup.cs`).

**@Ci: `EditorBuildSettings.asset` lists a temporary scene**
`Assets/_Project/WorldTest/WorldTest.unity` is in Build Settings so the exe can be built before `Main` exists. When you add `Main`, put it first (index 0) so it loads at startup. We'll both be editing this file, so expect a small merge.

**@Ci: how to wire the world into `GameBootstrapper`**
Add a `WorldInstaller` (`Stilargo.Gameplay.World`) to the scene, or as a prefab, and call `Install()` once. Afterwards:
- `worldInstaller.VisibilityGrid` → pass it to every unit's `VisionSourceComponent.Construct(grid)` and to every enemy's `FogHideable.Construct(grid)`.
- `worldInstaller.CityLayout` → road and plot data, e.g. for garrison/cover in step 6.
- The NavMesh is baked inside `Install()`, so spawn agents after calling it.

**@both: `WorldTest` is temporary**
`WorldTestBootstrap` and `Assets/_Project/WorldTest/` exist only until `Main` and `GameBootstrapper` land. Delete both at that point. To regenerate the scene, use `Stilargo → Build World Test Scene`.
