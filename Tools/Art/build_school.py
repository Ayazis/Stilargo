"""Builds the canal-town school (docs/CanalTownArtList.md #8).

A long, symmetrical two-storey ochre building with a pedimented entrance up a few steps and a SCHOOL inscription.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_school.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

NAME = "School"
WIDTH, DEPTH = 14.0, 9.0
STOREYS = (3.6, 3.4)
BAYS = (0.1, 0.27, 0.5, 0.73, 0.9)
GROUND_BAYS = (0.1, 0.27, 0.73, 0.9)
PLAQUE_HEIGHT = 5.3  # above the pediment, in the first floor's centre bay
TALL = {"height": 1.7, "shutters": False}


def build(parts):
    school = kit.Block((0, 0), WIDTH, DEPTH, STOREYS, "PlasterOchre", surround="stone")
    door = school.add(kit.FRONT, kit.Opening(WIDTH * 0.5, 0.51, 1.7, 2.2, "door", rise=0.6))
    school.windows(kit.FRONT, GROUND_BAYS, 0, sill=1.0, **TALL)
    school.windows(kit.FRONT, GROUND_BAYS, 1, **TALL)
    for index in (kit.RIGHT, kit.LEFT):
        school.windows(index, (0.3, 0.7), 0, sill=1.0, **TALL)
        school.windows(index, (0.3, 0.7), 1, **TALL)
    school.add(kit.BACK, kit.Opening(WIDTH * 0.5, 0.0, 1.2, 2.3, "door", rise=0.0))
    school.windows(kit.BACK, GROUND_BAYS, 0, sill=1.0, **TALL)
    school.windows(kit.BACK, BAYS, 1, **TALL)
    kit.build_block(parts.stone, parts.detail, school)
    front = school.side(kit.FRONT)
    kit.build_string_course(parts.detail, front, school.floor(1) - 0.05)
    build_pediment(parts.detail, front, door)
    parts.extra.append(kit.facade_text(front, "SCHOOL", WIDTH * 0.5, PLAQUE_HEIGHT, 0.3, d=0.09))
    for sign in (-1, 1):
        kit.build_wall_lantern(parts.detail, front, WIDTH * 0.5 + sign * 1.75, 3.0)
    kit.front_drainpipes(parts.detail, school)
    roof = kit.roof_over(school, math.radians(26), overhang=0.5)
    kit.build_roof(parts.roof, roof)
    for x in (-4.0, 4.0):
        kit.build_chimney(parts.detail, roof, x, 1.5)


def build_pediment(mesh, side, door):
    """Pilasters either side of the entrance carrying an entablature, a triangular pediment and a plaque above."""
    left, right = door.left - 0.6, door.right + 0.6
    top = door.top + 0.45
    for s in (left + 0.15, right - 0.15):
        side.box(mesh, s, top / 2, 0.08, (0.3, top, 0.16), "Stone")
    side.box(mesh, door.s, top + 0.1, 0.12, (right - left + 0.3, 0.2, 0.24), "Stone")
    base, apex = top + 0.2, top + 0.2 + 0.7
    side.block(mesh, [(left - 0.15, base), (right + 0.15, base), (door.s + 0.01, apex), (door.s - 0.01, apex)], 0.0, 0.16, "Stone")
    side.box(mesh, door.s, PLAQUE_HEIGHT, 0.04, (2.6, 0.5, 0.08), "Plaque")


kit.run(NAME, build, seed=8)
