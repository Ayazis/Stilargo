"""Builds the canal-town row house (docs/CanalTownArtList.md #1): 8 m wide, 8 m deep, three storeys, ochre plaster.

It shares its side walls with its neighbours, so those are plain, and the gable roof runs along the street.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_row_house.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

NAME = "RowHouse"
WIDTH, DEPTH = 8.0, 8.0
STOREYS = (3.4, 3.1, 3.1)
BAYS = (0.2, 0.5, 0.8)


def build(parts):
    house = kit.Block((0, 0), WIDTH, DEPTH, STOREYS, "PlasterOchre", quoins=False)
    ground = house.floor(0)
    house.add(kit.FRONT, kit.Opening(WIDTH * BAYS[0], ground, 1.2, 2.3, "door", rise=0.35))
    for fraction in BAYS[1:]:
        house.add(kit.FRONT, kit.Opening(WIDTH * fraction, 0.95, 1.0, 1.75))
    house.windows(kit.FRONT, BAYS, 1, flowers=True)
    house.windows(kit.FRONT, BAYS, 2, height=1.35)
    house.add(kit.BACK, kit.Opening(WIDTH * 0.75, ground, 1.0, 2.2, "door", rise=0.0))
    house.windows(kit.BACK, (0.3,), 0, sill=1.0)
    house.windows(kit.BACK, (0.3, 0.75), 1, paint="ShutterFaded")
    house.windows(kit.BACK, (0.3, 0.75), 2, height=1.35)
    kit.build_block(parts.stone, parts.detail, house)
    side = house.side(kit.FRONT)
    kit.build_string_course(parts.detail, side, house.floor(1) - 0.05)
    kit.build_wall_lantern(parts.detail, side, WIDTH * BAYS[0] + 1.0, 2.9)
    parts.extra.append(kit.facade_text(side, "7", WIDTH * BAYS[0], ground + 2.95, 0.22))
    kit.front_drainpipes(parts.detail, house)
    roof = kit.roof_over(house, math.radians(28), kind="gable", ridge_axis="x")
    kit.build_roof(parts.roof, roof, house)
    kit.build_chimney(parts.detail, roof, 2.2, 1.6)


kit.run(NAME, build, seed=7)
