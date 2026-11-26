"""Builds the canal-town farmhouse (docs/CanalTownArtList.md #5).

Rustic rubble stone, two low storeys, a gable roof, a big chimney, a bench and a woodpile.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_farmhouse.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

NAME = "Farmhouse"
SIZE = 8.0
STOREYS = (3.0, 2.6)
SMALL = {"width": 0.75, "height": 1.0, "rise": 0.0, "paint": "ShutterBlue"}


def build(parts):
    house = kit.Block((0, 0), SIZE, SIZE, STOREYS, "Rubble", surround="stone", cornice=False, plinth=0.5)
    house.add(kit.FRONT, kit.Opening(SIZE * 0.35, 0.0, 1.1, 2.1, "door", rise=0.0))
    house.windows(kit.FRONT, (0.75,), 0, sill=1.0, **SMALL)
    house.windows(kit.FRONT, (0.25, 0.75), 1, sill=0.6, **SMALL)
    for index in (kit.RIGHT, kit.LEFT):
        house.windows(index, (0.5,), 0, sill=1.1, **SMALL)
    house.add(kit.BACK, kit.Opening(SIZE * 0.6, 0.0, 1.0, 2.0, "door", rise=0.0))
    house.windows(kit.BACK, (0.25,), 1, sill=0.6, width=0.7, height=0.9, rise=0.0, shutters=False)
    kit.build_block(parts.stone, parts.detail, house)
    kit.build_bench(parts.detail, (SIZE * 0.1, -SIZE / 2 - 0.5, 0), 0.0, length=1.4)
    kit.build_woodpile(parts.detail, (SIZE / 2 + 0.6, -1.0, 0))
    kit.build_barrel(parts.detail, (-SIZE / 2 - 0.5, -SIZE / 2 + 0.8, 0))
    roof = kit.roof_over(house, math.radians(24), overhang=0.5, kind="gable", ridge_axis="x")
    kit.build_roof(parts.roof, roof, house, gutters=False)
    kit.build_chimney(parts.detail, roof, -SIZE / 2 + 0.9, 0.0, material="Rubble", size=(1.2, 1.0))


kit.run(NAME, build, seed=5)
