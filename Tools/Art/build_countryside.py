"""Builds the countryside pieces (docs/CanalTownArtList.md #23 and #25–#28), concept: city-overview.png.

Fields: `WheatPatch` and `VegetableRows` (4 × 4 m, tileable). Meadow: `WildflowerClump`, `RockA`, `RockB`, `Sheep`.
Lanes: `FenceSegment` (3 m post-and-rail, tileable along X). Entry markers at the map edges: `Milestone`,
`WaysideShrine` and `TownSign`.
Origin at ground level in the middle of each piece.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_countryside.py -- [--preview <folder>] [--piece Sheep]
"""
import math
import sys
from pathlib import Path

import bmesh
from mathutils import Matrix, Vector

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

rng = kit.rng
PATCH = 4.0
ROW_SPACING = 0.25
STALK_WIDTH = 0.25
FENCE_LENGTH = 3.0
TOWN_NAME = "STILARGO"


def build_wheat(parts):
    """Rows of wheat as ribbons of uneven height. Fields tile this across hectares, so it stays a few hundred faces."""
    mesh = parts.detail
    y = -PATCH / 2 + ROW_SPACING / 2
    while y < PATCH / 2:
        x = -PATCH / 2
        while x < PATCH / 2 - 0.01:
            x1 = min(x + STALK_WIDTH, PATCH / 2)
            h0, h1 = rng.uniform(0.75, 1.0), rng.uniform(0.75, 1.0)
            lean = rng.uniform(-0.06, 0.06)
            mesh.two_sided([Vector((x, y, 0)), Vector((x1, y, 0)), Vector((x1 + lean, y, h1)), Vector((x + lean, y, h0))], "Wheat", (0, -1, 0))
            x = x1
        y += ROW_SPACING


def build_vegetable_rows(parts):
    """Ridged rows of cabbages, each a low faceted dome so whole fields of them stay light (no bevel either)."""
    mesh = parts.roof
    for k in range(5):
        y = -PATCH / 2 + 0.4 + k * 0.8
        mesh.box((0, y, 0.06), (PATCH, 0.45, 0.12), "Soil")
        x = -PATCH / 2 + 0.2
        while x < PATCH / 2 - 0.1:
            base = Vector((x + rng.uniform(-0.03, 0.03), y, 0.12))
            mesh.cylinder(base, base + Vector((0, 0, 0.16)), 0.13, rng.choice(("Greens", "FoliageLight")), segments=6, radius_end=0.06)
            x += 0.35


def build_wildflowers(parts):
    mesh = parts.detail
    for _ in range(60):
        base = Vector((rng.gauss(0, 0.4), rng.gauss(0, 0.4), 0))
        tip = base + Vector((rng.uniform(-0.1, 0.1), rng.uniform(-0.1, 0.1), rng.uniform(0.25, 0.55)))
        side = Vector((rng.uniform(-1, 1), rng.uniform(-1, 1), 0)).normalized() * 0.015
        mesh.two_sided([base - side, base + side, tip], "Grass", side.cross(tip - base))
    for _ in range(18):
        p = Vector((rng.gauss(0, 0.35), rng.gauss(0, 0.35), rng.uniform(0.3, 0.55)))
        mesh.cylinder(Vector((p.x, p.y, 0)), p, 0.006, "Grass", segments=4)
        mesh.sphere(p, rng.uniform(0.03, 0.045), rng.choice(("FlowerRed", "FlowerWhite", "FlowerPink", "Orange")))


def rock(size, seed_turn):
    def build(parts):
        mesh = parts.stone
        result = bmesh.ops.create_icosphere(mesh.bm, subdivisions=2, radius=1.0, matrix=Matrix.Identity(4))
        for vert in result["verts"]:
            noise = 0.75 + 0.35 * math.sin(vert.co.x * 3.1 + seed_turn) * math.cos(vert.co.y * 2.7 - seed_turn) + rng.uniform(-0.06, 0.06)
            vert.co = Vector((vert.co.x * size[0], vert.co.y * size[1], max(-0.1, vert.co.z) * size[2])) * noise
        mesh._paint(result["verts"], "StoneDark")
        for _ in range(4):
            mesh.sphere((rng.uniform(-size[0], size[0]) * 0.5, rng.uniform(-size[1], size[1]) * 0.5, size[2] * 0.75), 0.08, "Moss")
    return build


def build_sheep(parts):
    mesh = parts.detail
    for x in (-0.3, -0.1, 0.1, 0.3):
        for y in (-0.12, 0.12):
            mesh.sphere((x, y, 0.62 + rng.uniform(-0.03, 0.03)), 0.24, "Wool")
    mesh.sphere((0, 0, 0.78), 0.22, "Wool")
    for x in (-0.3, 0.3):
        for y in (-0.12, 0.12):
            mesh.cylinder((x, y, 0), (x, y, 0.45), 0.035, "BarkDark", segments=6)
    head = Vector((0.6, 0, 0.78))
    mesh.cylinder(Vector((0.42, 0, 0.72)), head, 0.11, "BarkDark", segments=8)
    mesh.cylinder(head, head + Vector((0.18, 0, -0.08)), 0.1, "BarkDark", segments=8, radius_end=0.06)
    for y in (-1, 1):
        mesh.beam(head + Vector((0, y * 0.08, 0.04)), head + Vector((-0.05, y * 0.2, 0.0)), 0.04, "BarkDark")
    mesh.sphere(Vector((0.55, 0, 0.88)), 0.1, "Wool")


def build_fence(parts):
    mesh = parts.detail
    for x in (-FENCE_LENGTH / 2, 0.0, FENCE_LENGTH / 2):
        mesh.beam((x, 0, 0), (x + rng.uniform(-0.02, 0.02), 0, 1.15), 0.12, "Timber")
    for z in (0.45, 0.9):
        mesh.beam((-FENCE_LENGTH / 2, 0.08, z + rng.uniform(-0.03, 0.03)), (FENCE_LENGTH / 2, 0.08, z + rng.uniform(-0.03, 0.03)), 0.08,
                  rng.choice(("Timber", "Wood")))


def build_milestone(parts):
    stone = parts.stone
    stone.box((0, 0, 0.45), (0.5, 0.3, 0.9), "Stone")
    stone.cylinder((-0.25, 0, 0.9), (0.25, 0, 0.9), 0.15, "Stone", segments=12)
    stone.box((0, 0, 0.05), (0.7, 0.5, 0.1), "StoneDark")
    parts.extra.append(kit.text_object(TOWN_NAME, 0.075, Vector((0, -0.155, 0.7)), 0.0, "Iron", extrude=0.004))
    parts.extra.append(kit.text_object("2", 0.2, Vector((0, -0.155, 0.45)), 0.0, "Iron", extrude=0.004))


def build_shrine(parts):
    stone, detail = parts.stone, parts.detail
    stone.box((0, 0, 0.15), (1.0, 1.0, 0.3), "StoneDark")
    stone.box((0, 0, 1.15), (0.7, 0.6, 1.7), "Stone")
    detail.box((0, -0.29, 1.4), (0.42, 0.04, 0.6), "Shade")
    detail.sphere((0, -0.2, 1.3), 0.12, "FlowerWhite", subdivisions=2)
    detail.cylinder((0, -0.2, 1.1), (0, -0.2, 1.22), 0.08, "Terracotta", segments=10)
    roof = kit.Roof(-0.5, -0.45, 0.5, 0.45, 2.0, math.radians(40), "gable", "x")
    kit.build_roof(parts.roof, roof, gutters=False)
    detail.box((0, 0, roof.ridge + 0.4), (0.06, 0.06, 0.6), "Iron")
    detail.box((0, 0, roof.ridge + 0.5), (0.3, 0.05, 0.05), "Iron")
    for x in (-0.25, 0.25):
        detail.sphere((x, -0.6, 0.35), 0.12, "FlowerRed")


def build_town_sign(parts):
    detail = parts.detail
    for x in (-1.0, 1.0):
        detail.beam((x, 0, 0), (x, 0, 2.2), 0.12, "Timber")
    detail.box((0, 0, 1.8), (2.3, 0.06, 0.55), "Wood")
    detail.box((0, -0.035, 1.8), (2.2, 0.02, 0.45), "LinenBlue")
    parts.extra.append(kit.text_object(TOWN_NAME, 0.3, Vector((0, -0.05, 1.8)), 0.0, "Linen", extrude=0.006))
    roof = kit.Roof(-1.3, -0.25, 1.3, 0.25, 2.2, math.radians(35), "gable", "x")
    kit.build_roof(parts.roof, roof, gutters=False)


PIECES = {
    "WheatPatch": kit.Piece(build_wheat),
    "VegetableRows": kit.Piece(build_vegetable_rows),
    "WildflowerClump": kit.Piece(build_wildflowers),
    "RockA": kit.Piece(rock((0.9, 0.7, 0.6), 0.3)),
    "RockB": kit.Piece(rock((0.5, 0.45, 0.35), 1.7)),
    "Sheep": kit.Piece(build_sheep),
    "FenceSegment": kit.Piece(build_fence),
    "Milestone": kit.Piece(build_milestone),
    "WaysideShrine": kit.Piece(build_shrine),
    "TownSign": kit.Piece(build_town_sign),
}
kit.run_pieces(PIECES, seed=23)
