"""Builds the market pieces (docs/CanalTownArtList.md #19), concept: market-and-chapel.png.

- `MarketStall`: a 3 × 2 m timber stall with a striped awning and crates of produce on the counter; it faces -Y.
- `MarketHall`: a 10 × 6 m open timber market hall on stone piers under a tiled hip roof, with two trestle tables.
Origin at ground level in the middle of each piece.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_market.py -- [--preview <folder>] [--piece MarketHall]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402
from mathutils import Vector  # noqa: E402

STALL_WIDTH, STALL_DEPTH = 3.0, 2.0
COUNTER_HEIGHT = 0.9
POST = 0.1
FRONT_POST_HEIGHT = 2.5
BACK_POST_HEIGHT = 2.1
PRODUCE = ("Apple", "Orange", "Greens", "Apple", "Orange")

HALL_WIDTH, HALL_DEPTH = 10.0, 6.0
HALL_BAYS_X, HALL_BAYS_Y = 4, 2
PIER_HEIGHT = 0.6
EAVES = 3.6


def build_stall(parts):
    mesh = parts.detail
    w, d = STALL_WIDTH / 2, STALL_DEPTH / 2
    for x in (-w, w):
        mesh.beam((x, -d, 0), (x, -d, FRONT_POST_HEIGHT), POST, "Timber")
        mesh.beam((x, d, 0), (x, d, BACK_POST_HEIGHT), POST, "Timber")
        mesh.beam((x, -d, FRONT_POST_HEIGHT - 0.05), (x, d, BACK_POST_HEIGHT - 0.05), POST * 0.8, "Timber")
    mesh.box((0, -d + 0.35, COUNTER_HEIGHT - 0.03), (STALL_WIDTH - 0.1, 0.7, 0.06), "Timber")
    for x in (-w + 0.3, 0.0, w - 0.3):
        mesh.beam((x, -d + 0.1, 0), (x, -d + 0.1, COUNTER_HEIGHT - 0.06), 0.07, "Wood")
    mesh.two_sided([Vector((-w, -d + 0.02, 0.05)), Vector((w, -d + 0.02, 0.05)), Vector((w, -d + 0.02, COUNTER_HEIGHT - 0.06)),
                    Vector((-w, -d + 0.02, COUNTER_HEIGHT - 0.06))], "CanvasA", (0, -1, 0))
    mesh.two_sided([Vector((-w, d, 0.3)), Vector((w, d, 0.3)), Vector((w, d, BACK_POST_HEIGHT)), Vector((-w, d, BACK_POST_HEIGHT))],
                   "CanvasB", (0, -1, 0))
    build_stall_awning(mesh, w, d)
    for k, x in enumerate((-0.95, -0.3, 0.35, 0.95)):
        build_produce_crate(mesh, Vector((x, -d + 0.35, COUNTER_HEIGHT)), PRODUCE[k])
    for x in (-0.9, 0.6):
        kit.build_crate(mesh, (x, d - 0.45, 0), size=0.5)
        build_produce_crate(mesh, Vector((x, d - 0.45, 0.5)), PRODUCE[(int(x * 10)) % len(PRODUCE)])


def build_stall_awning(mesh, w, d):
    """Striped canvas over the stall, sloping from the back posts to past the front."""
    stripes = 10
    front_y, front_z = -d - 0.5, FRONT_POST_HEIGHT - 0.2
    for k in range(stripes):
        x0 = -w - 0.1 + (STALL_WIDTH + 0.2) * k / stripes
        x1 = x0 + (STALL_WIDTH + 0.2) / stripes
        material = "CanvasA" if k % 2 == 0 else "CanvasB"
        corners = [Vector((x0, d, BACK_POST_HEIGHT + 0.02)), Vector((x1, d, BACK_POST_HEIGHT + 0.02)), Vector((x1, front_y, front_z)),
                   Vector((x0, front_y, front_z))]
        mesh.two_sided(corners, material, (0, -0.3, 1))
        mesh.two_sided([Vector((x0, front_y, front_z)), Vector((x1, front_y, front_z)), Vector((x1, front_y, front_z - 0.25)),
                        Vector((x0, front_y, front_z - 0.25))], material, (0, -1, 0))


def build_produce_crate(mesh, base, produce):
    """A shallow crate heaped with fruit or greens."""
    size = Vector((0.5, 0.36, 0.14))
    mesh.box(base + Vector((0, 0, size.z / 2)), (size.x, size.y, size.z), "Wood")
    for _ in range(18):
        p = base + Vector((kit.rng.uniform(-0.2, 0.2), kit.rng.uniform(-0.13, 0.13), size.z + kit.rng.uniform(-0.02, 0.06)))
        mesh.sphere(p, kit.rng.uniform(0.04, 0.055), produce)


def build_hall(parts):
    stone, detail = parts.stone, parts.detail
    w, d = HALL_WIDTH / 2, HALL_DEPTH / 2
    xs = [-w + HALL_WIDTH * k / HALL_BAYS_X for k in range(HALL_BAYS_X + 1)]
    ys = [-d + HALL_DEPTH * k / HALL_BAYS_Y for k in range(HALL_BAYS_Y + 1)]
    detail.poly([(-w - 0.4, -d - 0.4, 0.02), (w + 0.4, -d - 0.4, 0.02), (w + 0.4, d + 0.4, 0.02), (-w - 0.4, d + 0.4, 0.02)],
                "Cobble", (0, 0, 1))
    for x in xs:
        for y in ys:
            is_edge = x in (xs[0], xs[-1]) or y in (ys[0], ys[-1])
            if not is_edge:
                continue
            stone.box((x, y, PIER_HEIGHT / 2), (0.5, 0.5, PIER_HEIGHT), "Stone")
            detail.beam((x, y, PIER_HEIGHT), (x, y, EAVES), 0.24, "Timber")
    for y in (ys[0], ys[-1]):
        detail.beam((xs[0], y, EAVES - 0.12), (xs[-1], y, EAVES - 0.12), 0.26, "Timber")
        for a, b in zip(xs, xs[1:]):
            build_braces(detail, Vector((a, y, 0)), Vector((b, y, 0)))
    for x in (xs[0], xs[-1]):
        detail.beam((x, ys[0], EAVES - 0.12), (x, ys[-1], EAVES - 0.12), 0.26, "Timber")
        for a, b in zip(ys, ys[1:]):
            build_braces(detail, Vector((x, a, 0)), Vector((x, b, 0)))
    for x in xs[1:-1]:
        detail.beam((x, ys[0], EAVES), (x, ys[-1], EAVES), 0.22, "Timber")
    roof = kit.Roof(-w - 0.6, -d - 0.6, w + 0.6, d + 0.6, EAVES + 0.12, math.radians(30), "hip")
    kit.build_roof(parts.roof, roof)
    for x in (-2.2, 2.2):
        build_trestle_table(detail, Vector((x, 0, 0)))


def build_braces(mesh, a, b):
    """Diagonal knee braces from each post up to the beam."""
    along = (b - a).normalized()
    for start, direction in ((a, along), (b, -along)):
        mesh.beam(start + Vector((0, 0, EAVES - 1.0)), start + direction * 0.8 + Vector((0, 0, EAVES - 0.25)), 0.12, "Timber")


def build_trestle_table(mesh, centre):
    mesh.box(centre + Vector((0, 0, 0.85)), (2.4, 0.9, 0.06), "Timber")
    for x in (-0.9, 0.9):
        for y in (-0.35, 0.35):
            mesh.beam(centre + Vector((x, y, 0)), centre + Vector((x * 0.9, 0, 0.82)), 0.06, "Wood")
    for k, x in enumerate((-0.75, 0.0, 0.75)):
        build_produce_crate(mesh, centre + Vector((x, 0, 0.88)), PRODUCE[(k + int(centre.x)) % len(PRODUCE)])


PIECES = {"MarketStall": kit.Piece(build_stall), "MarketHall": kit.Piece(build_hall)}
kit.run_pieces(PIECES, seed=19)
