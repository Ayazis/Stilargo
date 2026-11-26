# City variety: where we stopped

Branch `feature/game-feel-city`. The goal: the city felt samey everywhere. The user asked for all four of
irregular streets, varied districts, varied buildings and street life.

## Done (first pass, built and tested)

- Town plan: market square in the middle block, house blocks around it, outskirts, park blocks
  (`TownPlan`, `HouseBlockLayout`, `MarketDressing`, `Config/Town.json`, `PlotPlacement.FacingSquare`).
- City is 5×5 blocks (`World/CityConfig.asset`). Logged in `CollaborationNotes.md`.

## Written but NOT compiled, wired or tested yet (WIP commit)

The code may not compile yet. Run the EditMode tests first.

Core (`Scripts/Core/City`):
- `StreetShaper`: merges pairs of outskirts blocks (clears the road between them) and cuts alleys. Only
  outskirts blocks change, so home (58, 58), the hill (86, 72) and the rival start stay on roads.
- `NeighbourhoodPicker`: outskirts become `Plaza`, `OldTown`, `Villas` or `Workshops` (`BlockRole`).
- `IBlockLayout` with `OpenBlockLayout`, `MixedBlockLayout` (+ `OpenGroundShare`), `PlotSplitter`, and
  `HouseBlockLayout` (now adds a courtyard `Garden` plot and `CornerExtraStoreys` on corner houses).
- `CityGenerator` rewritten to use the above. `CityGrid.ClearRoad`, `TownBlock`, `IntRange`, `Share`.
- New `PlotUse`: `Plaza`, `Garden`, `Yard`. `Plot.HasBuilding` is now true only for House/Church/School/Tavern.
- Settings: `StreetSettings`, `NeighbourhoodSettings`, `StreetLifeSettings` (settable props + `Validate()`),
  attached via `TownSettings.WithStreets/WithNeighbourhoods/WithStreetLife`; `RowHouseSettings.WithCornerExtraStoreys`.
- `StreetFurniturePlanner` → `List<StreetProp>` (`StreetPropKind`: Lamp, Tree, Crates, Barrels, Planter, Cart).

Gameplay (`Scripts/Gameplay/City`):
- `GardenDressing`, `YardDressing`, `PlazaDressing`, `StreetPropBuilder`, `TreeProps`, `PropFactory.Decorative`.

## Still to do

1. `CitySpawner.CreateDressings`: register `Plaza`, `Garden`, `Yard` dressings.
2. `CitySpawner.Construct(layout, IReadOnlyList<StreetProp>)`: build each prop with `StreetPropBuilder`
   before the NavMesh bake. In `WorldInstaller.InstallCity`, plan them with
   `new StreetFurniturePlanner(town.StreetLife).Plan(CityLayout, _seed)`.
3. Config: extend `Infrastructure/Config/TownConfig` + `Town.json` with `streets`, `neighbourhoods`,
   `streetLife` sections and `houseBlock.cornerExtraStoreys` (entry classes like `RowHouseEntry`).
4. Tests: `StreetShaper` (roads stay connected, ring around square unchanged, home/hill cells still road),
   `NeighbourhoodPicker`, `MixedBlockLayout` open share, `StreetFurniturePlanner` (no props in 1-wide alleys,
   deterministic), corner storeys, courtyard garden. Update `TownLayoutTests` (courtyard is now a Garden plot,
   so "covered" counts change) and the `RowHouseSettings` usages.
5. Build the exe, ask the user to judge the look. Add a `CollaborationNotes.md` entry (new PlotUses,
   `HasBuilding` change, `Town.json` sections, `CitySpawner.Construct` signature).

Ideas not started: bent/offset streets, setbacks, more roof types.
