"""Builds the canal-town workshop / boatyard shed (docs/CanalTownArtList.md #6).

Brick, big arched double doors onto the yard, a loft hatch with a hoist beam above, a chimney, barrels and crates.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_workshop.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

NAME = "Workshop"
WIDTH, DEPTH = 9.0, 8.0
STOREYS = (3.8, 2.6)
GATE_FRACTION = 0.42


def build(parts):
    shed = kit.Block((0, 0), WIDTH, DEPTH, STOREYS, "Brick", surround="stone", plinth=0.5, cornice=False)
    gate_s = WIDTH * GATE_FRACTION
    shed.add(kit.FRONT, kit.Opening(gate_s, 0.0, 3.0, 2.6, "gate", rise=0.8))
    shed.windows(kit.FRONT, (0.84,), 0, sill=1.0, width=1.0, height=1.5)
    shed.add(kit.FRONT, kit.Opening(gate_s, shed.floor(1) + 0.3, 1.1, 1.3, "gate", rise=0.0, paint="ShutterFaded"))
    shed.windows(kit.FRONT, (0.84,), 1, sill=0.5, width=0.8, height=1.0, shutters=False)
    for index in (kit.RIGHT, kit.LEFT):
        shed.windows(index, (0.3, 0.7), 0, sill=1.2, width=1.0, height=1.4, shutters=False)
    shed.add(kit.BACK, kit.Opening(WIDTH * 0.25, 0.0, 1.0, 2.2, "door", rise=0.0))
    shed.windows(kit.BACK, (0.7,), 0, sill=1.2, width=1.0, height=1.4, shutters=False)
    kit.build_block(parts.stone, parts.detail, shed)
    front = shed.side(kit.FRONT)
    build_hoist(parts.detail, front, gate_s, shed)
    kit.build_wall_lantern(parts.detail, front, gate_s + 2.0, 3.2)
    for x in (-WIDTH / 2 + 0.6, -WIDTH / 2 + 1.3):
        kit.build_barrel(parts.detail, (x, -DEPTH / 2 - 0.6, 0))
    kit.build_crate(parts.detail, (WIDTH / 2 - 1.0, -DEPTH / 2 - 0.7, 0))
    kit.build_crate(parts.detail, (WIDTH / 2 - 1.0, -DEPTH / 2 - 0.7, 0.6), size=0.5)
    roof = kit.roof_over(shed, math.radians(26), overhang=0.5, kind="gable", ridge_axis="x")
    kit.build_roof(parts.roof, roof, shed)
    kit.build_chimney(parts.detail, roof, WIDTH / 2 - 1.2, 1.8)


def build_hoist(mesh, side, s, shed):
    """A timber beam over the loft hatch with a rope hanging from it."""
    h = shed.eaves - shed.base - 0.2
    mesh.beam(side.point(s, h, 0.0), side.point(s, h, 0.9), 0.18, "Timber")
    mesh.cylinder(side.point(s, h - 0.1, 0.85), side.point(s, shed.floor(1) - 0.6, 0.85), 0.015, "Iron", segments=4)


kit.run(NAME, build, seed=6)
