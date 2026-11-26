"""Builds the street props the street planner places (docs/CanalTownArtList.md #29, #31 and #32).

`StreetLamp` (a 3.8 m cast-iron lantern post), `ParkBench` (timber slats on cast-iron ends), `CrateStack`,
`BarrelGroup`, `StreetPlanter` (a stone trough with a shrub) and `Handcart`.
The street trees (#30) are `PlaneTree` and `UmbrellaPine` in build_trees.py.
Origin at ground level in the middle of each piece.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_street_props.py -- [--preview <folder>] [--piece StreetLamp]
"""
import math
import sys
from pathlib import Path

from mathutils import Vector

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

rng = kit.rng
LAMP_HEIGHT = 3.8
BENCH_LENGTH = 1.8


def build_lamp(parts):
    mesh = parts.detail
    kit.prism(mesh, (0, 0, 0), 0.22, 0.0, 0.5, 8, "Iron", radius_top=0.14)
    kit.prism(mesh, (0, 0, 0), 0.16, 0.5, 0.6, 8, "Iron")
    mesh.cylinder((0, 0, 0.6), (0, 0, LAMP_HEIGHT - 0.5), 0.07, "Iron", segments=10, radius_end=0.05)
    for z in (1.2, LAMP_HEIGHT - 0.8):
        mesh.cylinder((0, 0, z - 0.04), (0, 0, z + 0.04), 0.09, "Iron", segments=10)
    for k in range(4):
        a = k * math.pi / 2
        mesh.beam((0, 0, LAMP_HEIGHT - 0.9), (math.cos(a) * 0.25, math.sin(a) * 0.25, LAMP_HEIGHT - 0.5), 0.025, "Iron")
    mesh.box((0, 0, LAMP_HEIGHT - 0.48), (0.4, 0.4, 0.04), "Iron")
    lamp = Vector((0, 0, LAMP_HEIGHT - 0.22))
    mesh.cylinder(lamp - Vector((0, 0, 0.24)), lamp + Vector((0, 0, 0.22)), 0.16, "LampGlow", segments=6, radius_end=0.22)
    for k in range(6):
        a = 2 * math.pi * k / 6
        mesh.beam(lamp + Vector((math.cos(a) * 0.16, math.sin(a) * 0.16, -0.24)), lamp + Vector((math.cos(a) * 0.22, math.sin(a) * 0.22, 0.22)),
                  0.02, "Iron")
    kit.prism(mesh, (0, 0, 0), 0.3, LAMP_HEIGHT, LAMP_HEIGHT + 0.25, 6, "Iron", radius_top=0.04)
    mesh.sphere((0, 0, LAMP_HEIGHT + 0.3), 0.05, "Iron")


def build_bench(parts):
    mesh = parts.detail
    for x in (-BENCH_LENGTH / 2 + 0.15, BENCH_LENGTH / 2 - 0.15):
        mesh.beam((x, -0.22, 0), (x, -0.18, 0.43), 0.05, "Iron")
        mesh.beam((x, 0.2, 0), (x, 0.22, 0.85), 0.05, "Iron")
        mesh.beam((x, -0.25, 0.43), (x, 0.2, 0.43), 0.05, "Iron")
        mesh.beam((x, -0.25, 0.43), (x, -0.25, 0.65), 0.04, "Iron")
        mesh.beam((x, -0.25, 0.65), (x, 0.0, 0.65), 0.04, "Iron")
    for k in range(4):
        y = -0.22 + k * 0.13
        mesh.box((0, y, 0.45), (BENCH_LENGTH, 0.1, 0.035), "Timber")
    for k in range(3):
        z = 0.55 + k * 0.11
        mesh.box((0, 0.2 + k * 0.01, z), (BENCH_LENGTH, 0.035, 0.09), "Timber")


def build_crate_stack(parts):
    mesh = parts.detail
    for x, y, z, size in ((0, 0, 0, 0.6), (0.65, 0.05, 0, 0.6), (0.3, 0.0, 0.6, 0.55), (0.1, 0.7, 0, 0.5)):
        kit.build_crate(mesh, (x - 0.3, y - 0.3, z), size=size)


def build_barrel_group(parts):
    mesh = parts.detail
    for x, y in ((0, 0), (0.7, 0.1), (0.35, 0.6)):
        kit.build_barrel(mesh, (x - 0.35, y - 0.25, 0))
    mesh.cylinder((0.6, -0.6, 0.32), (1.4, -0.7, 0.32), 0.3, "Timber", segments=14)
    for t in (0.1, 0.9):
        mesh.cylinder((0.6 + 0.8 * t - 0.02, -0.6 - 0.1 * t, 0.32), (0.6 + 0.8 * t + 0.02, -0.6 - 0.1 * t, 0.32), 0.31, "Iron", segments=14)


def build_planter(parts):
    stone, detail = parts.stone, parts.detail
    stone.box((0, 0, 0.3), (1.2, 0.6, 0.6), "Stone")
    stone.box((0, 0, 0.62), (1.3, 0.7, 0.06), "StoneDark")
    detail.box((0, 0, 0.6), (1.1, 0.5, 0.02), "Soil")
    for x in (-0.3, 0.3):
        for _ in range(6):
            p = Vector((x + rng.gauss(0, 0.12), rng.gauss(0, 0.08), 0.75 + rng.uniform(0, 0.3)))
            detail.sphere(p, rng.uniform(0.15, 0.22), rng.choice(("Hedge", "Foliage")))
    for _ in range(8):
        detail.sphere((rng.uniform(-0.5, 0.5), rng.uniform(-0.25, 0.25), rng.uniform(0.85, 1.05)), 0.04, rng.choice(("FlowerRed", "FlowerPink")))


def build_handcart(parts):
    kit.build_cart_body(parts.detail, 1.2, 0.8, 0.4, 0.45, 0.7)
    for x in (-0.3, 0.2):
        kit.build_crate(parts.detail, (x, 0, 0.49), size=0.4)


PIECES = {
    "StreetLamp": kit.Piece(build_lamp),
    "ParkBench": kit.Piece(build_bench),
    "CrateStack": kit.Piece(build_crate_stack),
    "BarrelGroup": kit.Piece(build_barrel_group),
    "StreetPlanter": kit.Piece(build_planter),
    "Handcart": kit.Piece(build_handcart),
}
kit.run_pieces(PIECES, seed=29)
