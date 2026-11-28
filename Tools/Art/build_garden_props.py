"""Builds the park, garden and yard pieces (docs/CanalTownArtList.md #20, #21 and #22), concept: park-and-courtyards.png.

Park: `Gazebo` (an octagonal iron bandstand), `Monument` (a stepped plinth with a bronze column and sphere).
Garden: `HedgeSegment` and `GardenWall` (2 m, tileable along X), `VegetableBed` (2 × 1 m), `WashingLine` (4 m).
Yard: `FarmCart` (a two-wheeled cart with hay), `HayStack`, `WoodPile`.
Origin at ground level in the middle of each piece.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_garden_props.py -- [--preview <folder>] [--piece Gazebo]
"""
import math
import sys
from pathlib import Path

from mathutils import Vector

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

rng = kit.rng
SEGMENT = 2.0
DRY = -5.0  # a waterline far below keeps the low garden wall from weathering like a quay
GAZEBO_RADIUS = 2.2
GAZEBO_SIDES = 8
GAZEBO_FLOOR = 0.45
GAZEBO_EAVES = 3.0


def build_gazebo(parts):
    stone, detail = parts.stone, parts.detail
    turn = math.pi / GAZEBO_SIDES
    kit.prism(stone, (0, 0, 0), GAZEBO_RADIUS + 0.2, 0.0, GAZEBO_FLOOR - 0.15, GAZEBO_SIDES, "Stone", rotation=turn)
    kit.prism(stone, (0, 0, 0), GAZEBO_RADIUS + 0.1, GAZEBO_FLOOR - 0.15, GAZEBO_FLOOR, GAZEBO_SIDES, "StoneDark", rotation=turn)
    corners = [Vector((math.cos(turn + 2 * math.pi * k / GAZEBO_SIDES), math.sin(turn + 2 * math.pi * k / GAZEBO_SIDES), 0)) * GAZEBO_RADIUS
               for k in range(GAZEBO_SIDES)]
    for k, corner in enumerate(corners):
        nxt = corners[(k + 1) % GAZEBO_SIDES]
        detail.cylinder(corner + Vector((0, 0, GAZEBO_FLOOR)), corner + Vector((0, 0, GAZEBO_EAVES)), 0.06, "Iron", segments=8)
        detail.beam(corner + Vector((0, 0, GAZEBO_EAVES - 0.1)), nxt + Vector((0, 0, GAZEBO_EAVES - 0.1)), 0.12, "Iron")
        if k != 0:
            for z in (GAZEBO_FLOOR + 0.1, GAZEBO_FLOOR + 0.9):
                detail.beam(corner + Vector((0, 0, z)), nxt + Vector((0, 0, z)), 0.04, "Iron")
            for t in (0.2, 0.4, 0.6, 0.8):
                p = corner + (nxt - corner) * t
                detail.cylinder(p + Vector((0, 0, GAZEBO_FLOOR + 0.1)), p + Vector((0, 0, GAZEBO_FLOOR + 0.9)), 0.012, "Iron", segments=4)
        for t in (0.25, 0.75):
            p = corner + (nxt - corner) * t
            detail.beam(corner + Vector((0, 0, GAZEBO_EAVES - 0.6)), p + Vector((0, 0, GAZEBO_EAVES - 0.15)), 0.03, "Iron")
    kit.prism(parts.roof, (0, 0, 0), GAZEBO_RADIUS + 0.35, GAZEBO_EAVES, GAZEBO_EAVES + 1.5, GAZEBO_SIDES, "Zinc", radius_top=0.08,
              rotation=turn)
    detail.sphere((0, 0, GAZEBO_EAVES + 1.62), 0.12, "Bronze", subdivisions=2)
    detail.cylinder((0, 0, GAZEBO_EAVES + 1.7), (0, 0, GAZEBO_EAVES + 2.1), 0.02, "Iron", segments=6)


def build_monument(parts):
    stone, detail = parts.stone, parts.detail
    for k, (size, height) in enumerate(((2.6, 0.25), (2.0, 0.25), (1.2, 1.4))):
        z0 = sum(h for _, h in ((2.6, 0.25), (2.0, 0.25), (1.2, 1.4))[:k])
        stone.box((0, 0, z0 + height / 2), (size, size, height), "Stone" if k < 2 else "StoneDark")
    stone.box((0, 0, 2.0), (1.4, 1.4, 0.2), "Stone")
    detail.cylinder((0, 0, 2.1), (0, 0, 4.4), 0.2, "Bronze", segments=16, radius_end=0.15)
    detail.sphere((0, 0, 4.7), 0.35, "Bronze", subdivisions=2)
    for k in range(4):
        a = k * math.pi / 2
        detail.box((math.cos(a) * 0.61, math.sin(a) * 0.61, 1.1), (0.5 if k % 2 == 0 else 0.04, 0.04 if k % 2 == 0 else 0.5, 0.35), "Bronze")


def build_hedge(parts):
    mesh = parts.detail
    mesh.box((0, 0, 0.55), (SEGMENT, 0.7, 1.1), "Hedge")
    for _ in range(26):
        p = Vector((rng.uniform(-SEGMENT / 2 + 0.2, SEGMENT / 2 - 0.2), rng.uniform(-0.32, 0.32), rng.uniform(0.3, 1.1)))
        mesh.sphere(p, rng.uniform(0.18, 0.28), rng.choice(("Hedge", "Hedge", "Foliage")))


def build_garden_wall(parts):
    kit.build_coursed_wall(parts.stone, -SEGMENT / 2, SEGMENT / 2, 0.0, 0.9, -0.2, -1, waterline=DRY, course=0.3)
    kit.build_coursed_wall(parts.stone, -SEGMENT / 2, SEGMENT / 2, 0.0, 0.9, 0.2, 1, waterline=DRY, course=0.3)
    kit.build_coping(parts.stone, -SEGMENT / 2, SEGMENT / 2, 0.25, 0.5, 1.02, 1, overhang=0.0, height=0.12)
    for x, facing in ((-SEGMENT / 2, -1), (SEGMENT / 2, 1)):
        parts.detail.poly([(x, -0.2, 0.0), (x, 0.2, 0.0), (x, 0.2, 0.9), (x, -0.2, 0.9)], "Mortar", (facing, 0, 0))
    for _ in range(5):
        parts.detail.sphere((rng.uniform(-0.9, 0.9), rng.choice((-0.26, 0.26)), rng.uniform(0.05, 0.4)), rng.uniform(0.05, 0.1), "Moss")


def build_vegetable_bed(parts):
    mesh = parts.detail
    for y in (-0.5, 0.5):
        mesh.box((0, y, 0.1), (2.0, 0.06, 0.2), "Timber")
    for x in (-1.0, 1.0):
        mesh.box((x, 0, 0.1), (0.06, 1.0, 0.2), "Timber")
    mesh.box((0, 0, 0.08), (1.94, 0.94, 0.16), "Soil")
    for y in (-0.25, 0.25):
        for k in range(6):
            x = -0.8 + k * 0.32
            if y < 0:
                build_cabbage(mesh, Vector((x, y, 0.16)))
            else:
                mesh.cylinder((x, y, 0.16), (x, y, 1.0), 0.012, "Timber", segments=4)
                for _ in range(5):
                    p = Vector((x + rng.uniform(-0.1, 0.1), y + rng.uniform(-0.1, 0.1), rng.uniform(0.3, 0.95)))
                    mesh.sphere(p, rng.uniform(0.06, 0.09), "Greens")
                mesh.sphere((x + 0.06, y, rng.uniform(0.4, 0.8)), 0.035, "Apple")


def build_cabbage(mesh, base):
    for k in range(6):
        a = k * math.pi / 3
        mesh.sphere(base + Vector((math.cos(a) * 0.07, math.sin(a) * 0.07, 0.06)), 0.07, "Greens")
    mesh.sphere(base + Vector((0, 0, 0.1)), 0.07, "FoliageLight")


def build_washing_line(parts):
    mesh = parts.detail
    for x in (-2.0, 2.0):
        mesh.beam((x, 0, 0), (x, 0, 1.9), 0.08, "Timber")
        mesh.beam((x, -0.35, 1.85), (x, 0.35, 1.85), 0.06, "Timber")
    line = [Vector((-2.0 + 4.0 * k / 12, 0, 1.82 - 0.12 * 4 * (k / 12) * (1 - k / 12))) for k in range(13)]
    for a, b in zip(line, line[1:]):
        mesh.cylinder(a, b, 0.006, "Rope", segments=4)
    for k, (x, width, height) in enumerate(((-1.4, 0.6, 0.8), (-0.6, 0.7, 0.5), (0.2, 0.5, 0.9), (1.0, 0.8, 0.6))):
        top = 1.82 - 0.12 * 4 * ((x + 2) / 4) * (1 - (x + 2) / 4)
        sway = rng.uniform(-0.05, 0.05)
        cloth = [Vector((x, 0, top)), Vector((x + width, 0, top)), Vector((x + width, sway, top - height)), Vector((x, sway, top - height))]
        mesh.two_sided(cloth, "Linen" if k % 2 == 0 else "LinenBlue", (0, -1, 0))
        for peg in (x + 0.05, x + width - 0.05):
            mesh.box((peg, 0, top), (0.02, 0.03, 0.08), "Timber")


def build_farm_cart(parts):
    kit.build_cart_body(parts.detail, 2.4, 1.3, 0.7, 0.8, 1.6)
    build_hay_load(parts.detail, Vector((0, 0, 0.84)), (1.1, 0.6, 0.5))


def build_hay_load(mesh, base, size):
    for _ in range(18):
        p = base + Vector((rng.uniform(-size[0], size[0]), rng.uniform(-size[1], size[1]), rng.uniform(0.1, size[2])))
        mesh.sphere(p, rng.uniform(0.25, 0.38), "Hay")


def build_hay_stack(parts):
    mesh = parts.detail
    mesh.cylinder((0, 0, 0), (0, 0, 1.6), 1.4, "Hay", segments=16, radius_end=1.5)
    mesh.cylinder((0, 0, 1.6), (0, 0, 3.0), 1.5, "Hay", segments=16, radius_end=0.15)
    mesh.cylinder((0, 0, 2.9), (0, 0, 3.4), 0.05, "Timber", segments=5)
    for _ in range(20):
        a, z = rng.uniform(0, 2 * math.pi), rng.uniform(0.2, 2.4)
        r = 1.45 if z < 1.6 else 1.5 * (1 - (z - 1.6) / 1.4) + 0.1
        mesh.sphere((math.cos(a) * r, math.sin(a) * r, z), rng.uniform(0.12, 0.25), "Hay")


def build_wood_pile(parts):
    kit.build_woodpile(parts.detail, (0, 0, 0), rows=5)
    parts.detail.box((0.9, 0.6, 0.2), (0.5, 0.5, 0.4), "Timber")
    parts.detail.beam((0.9, 0.6, 0.4), (1.1, 0.55, 0.9), 0.04, "Timber")
    parts.detail.box((1.03, 0.58, 0.68), (0.03, 0.18, 0.12), "Iron")


PIECES = {
    "Gazebo": kit.Piece(build_gazebo),
    "Monument": kit.Piece(build_monument),
    "HedgeSegment": kit.Piece(build_hedge),
    "GardenWall": kit.Piece(build_garden_wall),
    "VegetableBed": kit.Piece(build_vegetable_bed),
    "WashingLine": kit.Piece(build_washing_line),
    "FarmCart": kit.Piece(build_farm_cart),
    "HayStack": kit.Piece(build_hay_stack),
    "WoodPile": kit.Piece(build_wood_pile),
}
kit.run_pieces(PIECES, seed=21)
