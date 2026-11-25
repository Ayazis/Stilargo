# Stilargo — Agent Guide

Unity game in C#. Follow SOLID and Clean Code.

## Ground rules

- **Build first, optimize later.** Make it work and clean; optimize only when the Profiler shows a problem.
- Small, focused changes. Ask before adding packages or making architecture decisions.
- **When something is unclear, ask.** If a request, requirement or design choice is ambiguous, ask the user before building on a guess.
- Don't hand-edit Unity-managed files (`Library/`, `*.csproj`, `.meta`) or scene/prefab YAML; list editor steps for the user instead.
- New logic gets EditMode tests. No commits unless asked.
- **Refactor when necessary.** If a change makes code unclear, duplicated or breaks the rules below, clean it up as part of the work (keep behavior the same, tests green). Mention larger refactors to the user before starting them.

## Tunable values in config files

Gameplay parameters that someone may want to tweak (speeds, damage, ranges, timers, costs, spawn counts, etc.) go in data files, not in code, so they can be changed without touching C#.

- Use JSON files under `Assets/StreamingAssets/Config/` (one file per feature, e.g. `Combat.json`). These ship next to the exe and can be edited there without rebuilding.
- Load them through a small loader in `Infrastructure` that maps JSON to plain C# settings classes; inject those settings into `Core`/`Gameplay` code. `Core` never reads files itself.
- Every value has a sensible default and is validated on load; fail fast with a clear message on missing or invalid values.
- Name keys clearly and include units in the name where useful (`moveSpeedMetersPerSecond`, `cooldownSeconds`).
- Values that are tied to authored assets (prefab references, materials) can stay in ScriptableObjects.
- If it's unclear whether a value belongs in config or code, ask the user.

## Collaboration

Multiple people work on this repo.

- Before starting work, `git fetch` and check existing branches (`git branch -a`) so you don't duplicate or clash with someone's work.
- Never work directly on `main`; use a feature branch (`feature/<short-name>`).
- Regularly check for merge conflicts with `main` and with related branches, especially before committing or opening a PR. Report conflicts to the user; don't resolve someone else's changes without asking.
- Never force-push or rewrite shared history.
- Be extra careful with scenes and prefabs: they conflict badly. Tell the user if a scene/prefab is touched on another branch.
- `CollaborationNotes.md` is the shared scratchpad between Ci and FL. Read it before starting work. Add an entry when you change shared files (ProjectSettings, Build Settings, contracts, asmdefs) or need something from the other person.

## Build after each step

The user plays the game as a Windows exe, not in the Unity editor. When a feature step is finished, build a Windows player and tell the user where the exe is.

- Close the Unity editor for this project first; batch mode can't open a project that is already open.
- Use the editor version in `ProjectSettings/ProjectVersion.txt` (installed under `C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe`). If that version isn't installed, tell the user; don't upgrade the project.
- Build from the repo root (output goes to the git-ignored `Builds/` folder):

  ```powershell
  & "C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe" -batchmode -quit -projectPath . -buildWindows64Player "Builds/Windows/Stilargo.exe" -logFile "Builds/build.log"
  ```

- The build includes only the scenes enabled in Build Settings. If a step adds a scene, list the editor step to add it there.
- A step is done only when the build succeeds. On failure, read `Builds/build.log`, fix the errors and rebuild. Report anything you can't fix to the user.

## Visual fidelity (photoreal target)

- Pipeline is HDRP (`Assets/_Project/Settings/HdrpPipeline.asset`). Create materials only through `HdrpMaterials` (`HDRP/Lit`, `HDRP/Unlit`); never URP, Built-in or legacy shaders.
- Linear color space, physical units: 1 unit = 1 m, PBR-valid albedo (no pure black/white), lights in lux/lumen with color temperature (Kelvin). The sun is ~100,000 lux and exposure is automatic.
- Project-wide rendering comes from `Stilargo → Apply Photoreal Rendering Settings` (`Editor/Rendering/PhotorealRenderingSetup.cs`). It writes the pipeline asset and `Settings/PhotorealVolumeProfile.asset` (physically based sky, exposure, fog, SSAO, SSR, contact shadows, ACES). Change settings there, not by hand.
- Scene builders use `PhotorealSceneDefaults` for the camera (TAA), the sun and the global volume.
- Code-generated placeholder materials must be easy to swap for authored PBR assets.
- Don't change Graphics/Quality/Player settings or volume profiles without logging it in `CollaborationNotes.md`.
- Claude can't judge looks from code: after a visual change, build and ask the user to check it in the exe.

## Structure

- Own code under `Assets/_Project/Scripts/`, split into `Core` (plain C# game logic), `Gameplay`, `UI`, `Infrastructure`, `Editor`, each with its own `.asmdef`. `Core` depends on nothing of ours.
- Folders by feature; namespaces mirror folders (`Stilargo.Gameplay.Combat`).

## SOLID

- **S:** one reason to change per class. MonoBehaviours are thin; rules live in plain C#.
- **O:** extend with new types or ScriptableObject data, not growing `switch`/`if` chains.
- **L:** subclasses honor the base contract. Prefer composition; keep inheritance shallow.
- **I:** small role interfaces (`IDamageable`, `IInteractable`).
- **D:** depend on interfaces; inject dependencies via constructor or `Initialize(...)` from a bootstrapper. No singletons or `Find...` in gameplay code.

## Clean Code

- Clear names, no `Manager`/`Helper`/`Utils`. `PascalCase` types/members, `camelCase` locals, `_camelCase` private fields, bools as questions (`IsGrounded`).
- Short functions doing one thing, ≤ 3 params, no bool flag params, guard clauses over nesting.
- No magic numbers/strings, no dead or commented-out code. Comments explain *why*.
- Fail fast; never swallow exceptions.

## Unity

- `[SerializeField] private` instead of public fields.
- Cache `GetComponent` in `Awake`; subscribe in `OnEnable`, unsubscribe in `OnDisable`.
- Decouple with C# events, not `SendMessage`. Input via the new Input System behind an interface.
- Unity null checks: `== null`, not `?.`/`??`.
- One type per file, Allman braces, always use braces.
