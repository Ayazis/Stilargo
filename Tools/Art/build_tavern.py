"""Builds the canal-town tavern (docs/CanalTownArtList.md #9).

Terracotta plaster, a shopfront under a striped awning, a hanging sign and café tables outside.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_tavern.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

NAME = "Tavern"
WIDTH, DEPTH = 8.0, 8.0
STOREYS = (3.6, 3.1, 3.1)
BAYS = (0.2, 0.5, 0.8)
SIGN_TEXT_OFFSET = 0.04  # each side of the board


def build(parts):
    tavern = kit.Block((0, 0), WIDTH, DEPTH, STOREYS, "PlasterTerracotta", surround="stone")
    tavern.add(kit.FRONT, kit.Opening(WIDTH * BAYS[0], 0.0, 1.3, 2.4, "door", rise=0.4))
    tavern.add(kit.FRONT, kit.Opening(WIDTH * 0.65, 0.3, 3.4, 2.2, "shop", rise=0.0, awning=True))
    tavern.windows(kit.FRONT, BAYS, 1, flowers=True)
    tavern.windows(kit.FRONT, BAYS, 2, height=1.35)
    for index in (kit.RIGHT, kit.LEFT):
        tavern.windows(index, (0.5,), 0, sill=1.0, height=1.6)
        tavern.windows(index, (0.3, 0.7), 1)
        tavern.windows(index, (0.3, 0.7), 2, height=1.35)
    tavern.add(kit.BACK, kit.Opening(WIDTH * 0.7, 0.0, 1.0, 2.2, "door", rise=0.0))
    tavern.windows(kit.BACK, (0.3, 0.7), 1)
    kit.build_block(parts.stone, parts.detail, tavern)
    front = tavern.side(kit.FRONT)
    kit.build_string_course(parts.detail, front, tavern.floor(1) - 0.05)
    build_sign(parts, front, WIDTH * BAYS[0] - 1.0)
    kit.build_wall_lantern(parts.detail, front, WIDTH * BAYS[0] + 1.05, 3.0)
    for x in (0.6, 2.6):
        kit.build_table_set(parts.detail, (x, -DEPTH / 2 - 1.8, 0))
    kit.build_barrel(parts.detail, (-WIDTH / 2 + 0.3, -DEPTH / 2 - 0.5, 0))
    kit.front_drainpipes(parts.detail, tavern)
    roof = kit.roof_over(tavern, math.radians(27), overhang=0.45)
    kit.build_roof(parts.roof, roof)
    kit.build_chimney(parts.detail, roof, -1.6, 1.8)


def build_sign(parts, front, s):
    """The hanging board with TAVERN lettered on both faces."""
    board = kit.build_hanging_sign(parts.detail, front, s, 3.25)
    yaw = math.atan2(-front.along.y, -front.along.x) + math.radians(90)
    for sign in (-1, 1):
        position = board + front.along * (sign * SIGN_TEXT_OFFSET)
        facing = yaw if sign < 0 else yaw + math.pi
        parts.extra.append(kit.text_object("TAVERN", 0.13, position, facing, "Plaque"))


kit.run(NAME, build, seed=9)
