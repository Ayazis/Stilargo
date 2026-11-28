# Canal town: art to model in Blender

Everything the new canal town places, with the sizes the generator uses. One grid cell = 2 m, one storey = 3 m.
Each item is a placeholder today (boxes and primitives in `Gameplay/City/*Dressing.cs`). Style reference: `ConceptArt/CanalTown/`.

Sizes vary per plot, so buildings work best as a **modular kit** (walls per storey and bay, corners, roofs that
scale), not as fixed-size models.

## Buildings (can be garrisoned, so they need doors, windows and a road-facing side)

| # | Item | Where | Footprint | Height | Notes |
|---|------|-------|-----------|--------|-------|
| 1 | **Row house** | Courtyard blocks, along streets | 6–10 m wide × 8 m deep | 2–3 storeys | The most common building. Shares side walls with its neighbours. |
| 2 | **Corner house** | Block corners | 6–10 m × 8 m | 3–4 storeys (one more than its row) | Two street façades. `Art/Models/CornerHouse` already exists. |
| 3 | **Square-facing townhouse** | North side of the main square, market sides | 6–10 m × 8 m | 3–4 storeys | Grander: balconies, arcades or shopfronts on the ground floor. |
| 4 | **Detached home** | Edge of town, in its own garden | about 6–10 m × 6–10 m | 1–3 storeys | Stands alone: four finished façades, hip roof. |
| 5 | **Farmhouse** | Countryside, corner of a farmyard | 8 × 8 m | 1–2 storeys | Rustic stone, chimney. A barn next to it would fit the yard. |
| 6 | **Workshop / boatyard shed** | Boatyard block on the canal | 6–10 m × 8 m | 1–2 storeys | Brick, big doors, chimney (concept art: canal-boatyard). |
| 7 | **Church with bell tower** | Back of the church square | 12 × 18 m | 2 storeys, tower 6 storeys (18 m) | Rose window and portal facing the square. |
| 8 | **School** | Largest house plot (chosen by `Buildings.json`) | varies | 2 storeys | Ochre walls today. |
| 9 | **Tavern** (×2) | House plots near the market | varies | 2–4 storeys | Terracotta walls today. A sign, outdoor tables. |

Roofs: terracotta hip roofs with chimneys for all of the above; overhang and pitch are set in `CitySpawner`.

## Water and bridges

| # | Item | Size | Notes |
|---|------|------|-------|
| 10 | **Canal** | 10 m wide, runs right across the map | Water surface plus stone quay walls with steps and bollards. |
| 11 | **Quay** | 4 m wide paved strip on both banks | Railing or chains, mooring rings, lamps. |
| 12 | **Bridge** (×3) | 4 m wide, spans 10 m | Stone arch bridge (concept art: park-and-courtyards). Units must be able to walk over it. |
| 13 | **Pond** | Middle of the canal-side park, about 50% of the park | Reeds, lily pads, a stone edge. |
| 14 | **Boats** (optional) | — | Rowing boats along the quay and in the boatyard. |

## Squares and fountains

| # | Item | Size | Notes |
|---|------|------|-------|
| 15 | **Fountain** (large) | 4 × 4 m | Centre of the main square and of the inner parks. Round basin, column or statue. |
| 16 | **Well** | 2 × 2 m | Centre of house courtyards. Small stone well with a roof or winch. |
| 17 | **Main square paving** | a whole block (18–30 m) | Cobbles; benches, lamps, café tables. |
| 18 | **Church forecourt** | front of the church block | Paving, steps up to the portal. |
| 19 | **Market** | open part of the market block | Stalls with awnings, or a timber market hall (concept art: market-and-chapel). |

## Green spaces

| # | Item | Notes |
|---|------|-------|
| 20 | **Park** | Lawn, gravel paths, benches, statue, plane trees, willows by the pond, maybe a gazebo. |
| 21 | **Courtyard / private garden** | Lawn inside a low hedge or stone wall, fruit trees, vegetable beds, washing lines. |
| 22 | **Farmyard / workshop yard** | Packed earth or gravel: carts, crates, barrels, hay, woodpiles. |

## Countryside

| # | Item | Notes |
|---|------|-------|
| 23 | **Field** | Crop rows (wheat or vegetables) plus a crop material; parcels 16–32 m × 40 m. |
| 24 | **Orchard** | Fruit tree (one model, a few variations), planted 6 m apart in a grid. |
| 25 | **Meadow** | Grass with wildflowers, rocks, maybe sheep (concept art: city-overview). |
| 26 | **Country lane** | 4 m dirt track with grass edges; wooden fences along the fields. |
| 27 | **High road** | 6 m wide road from the map edge into town; this is where the soldiers enter. |
| 28 | **Entry markers** | Something at both map edges where the squads arrive: a milestone, wayside shrine or town sign. |

## Street props (already placed by the street planner)

| # | Item | Notes |
|---|------|-------|
| 29 | Street lamp | Cast-iron lantern on a post. |
| 30 | Street tree | Plane tree or umbrella pine; also lines the country lanes. |
| 31 | Crates, barrels, planters, handcart | Clutter along streets and in yards. |
| 32 | Bench | Parks, squares, gardens. |

## Materials worth making

Plaster in 6 earth tints (already exist), weathered stone (church, quays, fountains), brick (boatyard), terracotta
tiles, cobbles, gravel paths, packed earth, crop field, grass with flowers, water (canal and pond).
