"""Builds the park pond pieces (docs/CanalTownArtList.md #13): pond size varies per park, so it is a small kit.

- `PondEdge`: a 2 m straight stone kerb, tileable; the water lies towards -Y, the origin is lawn level at the middle.
- `PondCorner`: a quarter-circle kerb (outer radius 1.2 m) that joins two edges at a corner; origin at the corner.
- `Reeds`: a clump of reeds and bulrushes about 1.2 m across, origin at the water surface.
- `LilyPads`: a loose cluster of lily pads and two flowers, origin at the water surface.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_pond.py -- [--preview <folder>] [--piece Reeds]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402
from mathutils import Vector  # noqa: E402

EDGE_LENGTH = 2.0
KERB_DEPTH = 0.4
KERB_TOP = 0.12
WATER_LEVEL = -0.2
POND_FLOOR = -0.6
CORNER_RADIUS = 1.2
CORNER_SEGMENTS = 6


def build_kerb_stone(mesh, a, b, outward):
    """One kerb block between two points on the water-side edge, `outward` pointing over the water."""
    a, b = Vector(a), Vector(b)
    inward = -Vector(outward).normalized() * KERB_DEPTH
    lip = Vector(outward).normalized() * 0.05
    top = Vector((0, 0, KERB_TOP + kit.rng.uniform(-0.01, 0.01)))
    bottom = Vector((0, 0, POND_FLOOR))
    front = [a + lip + bottom, b + lip + bottom, b + lip + top, a + lip + top]
    back = [p + inward - lip for p in front]
    mesh.hexahedron(front, back, kit.waterline_material(KERB_TOP, WATER_LEVEL + 0.25))


def build_moss_line(mesh, a, b, outward):
    a, b = Vector(a), Vector(b)
    for _ in range(6):
        p = a + (b - a) * kit.rng.random() + Vector(outward).normalized() * 0.07
        mesh.sphere(p + Vector((0, 0, WATER_LEVEL + 0.03)), kit.rng.uniform(0.04, 0.08), "Moss")


def build_edge(parts):
    x = -EDGE_LENGTH / 2
    while x < EDGE_LENGTH / 2 - 0.05:
        length = min(kit.rng.uniform(0.55, 0.8), EDGE_LENGTH / 2 - x)
        build_kerb_stone(parts.stone, (x + 0.006, 0, 0), (x + length - 0.006, 0, 0), (0, -1, 0))
        x += length
    build_moss_line(parts.detail, (-EDGE_LENGTH / 2, 0, 0), (EDGE_LENGTH / 2, 0, 0), (0, -1, 0))


def build_corner(parts):
    """Quarter circle from the -X edge round to the -Y edge, centred so it meets PondEdge pieces flush."""
    centre = Vector((CORNER_RADIUS, CORNER_RADIUS, 0))
    for k in range(CORNER_SEGMENTS):
        a0 = math.pi + (math.pi / 2) * k / CORNER_SEGMENTS
        a1 = math.pi + (math.pi / 2) * (k + 1) / CORNER_SEGMENTS
        p0 = centre + Vector((math.cos(a0), math.sin(a0), 0)) * CORNER_RADIUS
        p1 = centre + Vector((math.cos(a1), math.sin(a1), 0)) * CORNER_RADIUS
        mid = (a0 + a1) / 2
        outward = (math.cos(mid), math.sin(mid), 0)
        build_kerb_stone(parts.stone, p0, p1, outward)
        build_moss_line(parts.detail, p0, p1, outward)


def build_reeds(parts):
    mesh = parts.detail
    for _ in range(70):
        base = Vector((kit.rng.gauss(0, 0.35), kit.rng.gauss(0, 0.35), WATER_LEVEL * 0 - 0.05))
        height = kit.rng.uniform(0.8, 1.7)
        lean = Vector((kit.rng.uniform(-0.25, 0.25), kit.rng.uniform(-0.25, 0.25), 0))
        build_blade(mesh, base, base + lean + Vector((0, 0, height)), kit.rng.uniform(0.02, 0.035))
    for _ in range(9):
        base = Vector((kit.rng.gauss(0, 0.25), kit.rng.gauss(0, 0.25), -0.05))
        top = base + Vector((kit.rng.uniform(-0.1, 0.1), kit.rng.uniform(-0.1, 0.1), kit.rng.uniform(1.3, 1.8)))
        mesh.cylinder(base, top, 0.008, "Reed", segments=5)
        mesh.cylinder(top - Vector((0, 0, 0.32)), top - Vector((0, 0, 0.08)), 0.035, "ReedHead", segments=8)


def build_blade(mesh, base, tip, width):
    """A thin, slightly curved leaf blade, two-sided so it reads from every angle."""
    side = Vector((kit.rng.uniform(-1, 1), kit.rng.uniform(-1, 1), 0)).normalized() * width
    middle = (base + tip) / 2 + Vector((kit.rng.uniform(-0.08, 0.08), kit.rng.uniform(-0.08, 0.08), 0))
    facing = side.cross(tip - base)
    mesh.two_sided([base - side, base + side, middle + side * 0.8, middle - side * 0.8], "Reed", facing)
    mesh.two_sided([middle - side * 0.8, middle + side * 0.8, tip], "Reed", facing)


def build_lily_pads(parts):
    mesh = parts.detail
    for _ in range(14):
        centre = Vector((kit.rng.gauss(0, 0.6), kit.rng.gauss(0, 0.6), 0.005))
        radius = kit.rng.uniform(0.1, 0.2)
        notch = kit.rng.uniform(0, 2 * math.pi)
        rim = [centre + Vector((math.cos(notch + 0.35 + (2 * math.pi - 0.7) * k / 14), math.sin(notch + 0.35 + (2 * math.pi - 0.7) * k / 14), 0))
               * radius for k in range(15)]
        mesh.poly([centre] + rim, "LilyPad", (0, 0, 1))
    for _ in range(2):
        centre = Vector((kit.rng.gauss(0, 0.4), kit.rng.gauss(0, 0.4), 0.02))
        for k in range(8):
            a = 2 * math.pi * k / 8
            petal = centre + Vector((math.cos(a), math.sin(a), 0.6)) * 0.07
            mesh.two_sided([centre, centre + Vector((math.cos(a + 0.3), math.sin(a + 0.3), 0)) * 0.03, petal,
                            centre + Vector((math.cos(a - 0.3), math.sin(a - 0.3), 0)) * 0.03], "FlowerPink", (0, 0, 1))
        mesh.sphere(centre + Vector((0, 0, 0.03)), 0.02, "Straw")


PIECES = {
    "PondEdge": kit.Piece(build_edge, POND_FLOOR, (WATER_LEVEL, -10.0, 0.0)),
    "PondCorner": kit.Piece(build_corner, POND_FLOOR),
    "Reeds": kit.Piece(build_reeds, -0.01),
    "LilyPads": kit.Piece(build_lily_pads, -0.01),
}
kit.run_pieces(PIECES, seed=13)
