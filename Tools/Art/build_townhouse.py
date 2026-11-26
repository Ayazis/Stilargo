"""Builds the canal-town square-facing townhouse (docs/CanalTownArtList.md #3).

Four storeys of rose plaster over an arcade of shops, with balconies on the first floor.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_townhouse.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

NAME = "Townhouse"
WIDTH, DEPTH = 8.0, 8.0
STOREYS = (4.2, 3.3, 3.3, 3.0)
BAYS = (0.2, 0.5, 0.8)
ARCADE_DEPTH = 1.6
CORE_CLEARANCE = 0.02  # keeps the core's front face just behind the loggia's back wall so the two never z-fight


def build(parts):
    house = kit.Block((0, 0), WIDTH, DEPTH, STOREYS, "PlasterRose", surround="stone",
                      front_recess=ARCADE_DEPTH - kit.CORE_INSET + CORE_CLEARANCE)
    for fraction in BAYS:
        house.add(kit.FRONT, kit.Opening(WIDTH * fraction, 0.0, 1.8, 2.8, "arcade", rise=0.9, depth=ARCADE_DEPTH))
    house.windows(kit.FRONT, BAYS, 1, sill=0.05, height=2.3, balcony=True)
    house.windows(kit.FRONT, BAYS, 2, flowers=True)
    house.windows(kit.FRONT, BAYS, 3, height=1.3)
    for index in (kit.RIGHT, kit.LEFT):
        house.windows(index, (0.3, 0.7), 1)
        house.windows(index, (0.3, 0.7), 2)
        house.windows(index, (0.5,), 3, height=1.3)
    house.add(kit.BACK, kit.Opening(WIDTH * 0.5, 0.0, 1.2, 2.4, "door", rise=0.3))
    for storey in (1, 2, 3):
        house.windows(kit.BACK, BAYS, storey, height=1.4)
    kit.build_block(parts.stone, parts.detail, house)
    front = house.side(kit.FRONT)
    for storey in (1, 2):
        kit.build_string_course(parts.detail, front, house.floor(storey) - 0.05)
    for fraction in (0.35, 0.65):
        kit.build_wall_lantern(parts.detail, front, WIDTH * fraction, 3.7)
    roof = kit.roof_over(house, math.radians(28), overhang=0.5)
    kit.build_roof(parts.roof, roof)
    kit.build_chimney(parts.detail, roof, -1.8, 1.4)
    kit.build_chimney(parts.detail, roof, 2.0, 1.8, size=(0.6, 0.6))


kit.run(NAME, build, seed=3)
