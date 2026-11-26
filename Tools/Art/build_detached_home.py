"""Builds the canal-town detached home (docs/CanalTownArtList.md #4).

Stands alone in its garden: four finished facades, two storeys of cream plaster and a hip roof.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_detached_home.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

NAME = "DetachedHome"
SIZE = 9.0
STOREYS = (3.3, 3.0)
BAYS = (0.22, 0.5, 0.78)


def build(parts):
    house = kit.Block((0, 0), SIZE, SIZE, STOREYS, "PlasterCream")
    house.add(kit.FRONT, kit.Opening(SIZE * 0.5, 0.51, 1.2, 2.3, "door", rise=0.3))
    house.windows(kit.FRONT, (BAYS[0], BAYS[2]), 0, sill=1.0, height=1.6, flowers=True)
    house.windows(kit.FRONT, BAYS, 1, flowers=True)
    for index in (kit.RIGHT, kit.LEFT):
        house.windows(index, (0.3, 0.7), 0, sill=1.0, height=1.6, paint="ShutterBlue")
        house.windows(index, (0.3, 0.7), 1, paint="ShutterBlue")
    house.add(kit.BACK, kit.Opening(SIZE * 0.7, 0.0, 1.0, 2.2, "door", rise=0.0))
    house.windows(kit.BACK, (0.3,), 0, sill=1.0, height=1.6)
    house.windows(kit.BACK, (0.3, 0.7), 1)
    kit.build_block(parts.stone, parts.detail, house)
    front = house.side(kit.FRONT)
    kit.build_wall_lantern(parts.detail, front, SIZE * 0.5 + 1.1, 3.0)
    kit.build_bench(parts.detail, (-SIZE * 0.25, -SIZE / 2 - 0.6, 0), 0.0)
    roof = kit.roof_over(house, math.radians(26), overhang=0.55)
    kit.build_roof(parts.roof, roof)
    kit.build_chimney(parts.detail, roof, 2.2, 1.0)


kit.run(NAME, build, seed=4)
