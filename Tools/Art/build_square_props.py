"""Builds the square and forecourt pieces (docs/CanalTownArtList.md #17 and #18), concept: stilargo-church-square.png.

The squares themselves are paved by the game's ground material; these pieces dress them:
- `SquareInlay`: a 6 m compass-rose inlay set into the paving (the centre of the main square).
- `ChurchForecourt`: a 14 × 6 m apron of laid paving with stone bands, in front of the church steps.
- `StoneBench`: a stone slab bench on two blocks.
- `Planter`: a big terracotta pot with an olive tree, as beside the church portal.
- `CafeTerrace`: three café tables with chairs under canvas parasols.
Origin at ground level in the middle of each piece; flat pieces stand 2 cm proud of the ground so they never z-fight it.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_square_props.py -- [--preview <folder>] [--piece Planter]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402
from mathutils import Vector  # noqa: E402

PROUD = 0.02
ROSE_RADIUS = 3.0
ROSE_POINTS = 8
ROSE_POINT_WIDTH = 0.6
FORECOURT_WIDTH, FORECOURT_DEPTH = 14.0, 6.0
BAND = 0.3
BAND_SPACING = 2.0
TERRACE_TABLES = ((-1.6, 0.0), (0.0, 1.4), (1.6, 0.0))


def build_inlay(parts):
    mesh = parts.detail
    kit.annulus_wall(mesh, (0, 0, 0), ROSE_RADIUS - 0.25, ROSE_RADIUS, 0.0, PROUD, 48, "StoneDark")
    kit.annulus_wall(mesh, (0, 0, 0), ROSE_RADIUS - 0.45, ROSE_RADIUS - 0.25, 0.0, PROUD, 48, "Stone")
    kit.prism(mesh, (0, 0, 0), ROSE_RADIUS - 0.44, 0.0, PROUD * 0.5, 48, "Cobble")
    up = Vector((0, 0, PROUD + 0.002))
    for k in range(ROSE_POINTS * 2):
        a = 2 * math.pi * k / (ROSE_POINTS * 2)
        length = ROSE_RADIUS - 0.55 if k % 2 == 0 else (ROSE_RADIUS - 0.55) * 0.55
        tip = Vector((math.cos(a), math.sin(a), 0)) * length + up
        for side, material in ((-1, "StoneDark"), (1, "Stone")):
            flank = Vector((math.cos(a + side * math.pi / (ROSE_POINTS * 2)), math.sin(a + side * math.pi / (ROSE_POINTS * 2)), 0)) * ROSE_POINT_WIDTH
            mesh.poly([up, flank + up, tip] if side > 0 else [up, tip, flank + up], material, (0, 0, 1))
    kit.prism(mesh, (0, 0, 0), 0.22, 0.0, PROUD + 0.004, 16, "Bronze")


def build_forecourt(parts):
    mesh = parts.detail
    w, d = FORECOURT_WIDTH / 2, FORECOURT_DEPTH / 2
    mesh.poly([(-w, -d, PROUD * 0.5), (w, -d, PROUD * 0.5), (w, d, PROUD * 0.5), (-w, d, PROUD * 0.5)], "Cobble", (0, 0, 1))
    for y in (-d + BAND / 2, d - BAND / 2):
        mesh.box((0, y, PROUD / 2 + 0.002), (FORECOURT_WIDTH, BAND, PROUD), "StoneDark")
    x = -w + BAND / 2
    while x < w:
        mesh.box((x, 0, PROUD / 2 + 0.002), (BAND, FORECOURT_DEPTH - 2 * BAND, PROUD), "Stone")
        x += BAND_SPACING
    mesh.box((w - BAND / 2, 0, PROUD / 2 + 0.002), (BAND, FORECOURT_DEPTH - 2 * BAND, PROUD), "Stone")
    for x in (-w + 1.2, w - 1.2):
        mesh.box((x, d - 0.6, 0.3), (0.35, 0.35, 0.6), "Stone")
        mesh.sphere((x, d - 0.6, 0.72), 0.16, "Stone", subdivisions=2)


def build_stone_bench(parts):
    mesh = parts.stone
    for x in (-0.6, 0.6):
        mesh.box((x, 0, 0.21), (0.32, 0.4, 0.42), "StoneDark")
    mesh.box((0, 0, 0.47), (1.8, 0.48, 0.1), "Stone")


def build_planter(parts):
    detail = parts.detail
    detail.cylinder((0, 0, 0), (0, 0, 0.62), 0.3, "Terracotta", segments=16, radius_end=0.38)
    detail.cylinder((0, 0, 0.58), (0, 0, 0.68), 0.42, "Terracotta", segments=16)
    kit.prism(detail, (0, 0, 0), 0.37, 0.6, 0.64, 16, "Mortar")
    build_olive_tree(detail, Vector((0, 0, 0.62)))


def build_olive_tree(mesh, base):
    """A gnarled, leaning trunk splitting into branches with grey-green leaf clusters."""
    points = [base]
    for k in range(4):
        points.append(points[-1] + Vector((kit.rng.uniform(-0.08, 0.08), kit.rng.uniform(-0.08, 0.08), 0.22)))
    for a, b in zip(points, points[1:]):
        mesh.cylinder(a, b, 0.07, "Timber", segments=7, radius_end=0.06)
    fork = points[-1]
    for k in range(4):
        angle = k * math.pi / 2 + kit.rng.uniform(-0.3, 0.3)
        tip = fork + Vector((math.cos(angle) * 0.4, math.sin(angle) * 0.4, kit.rng.uniform(0.35, 0.55)))
        mesh.cylinder(fork, tip, 0.035, "Timber", segments=6, radius_end=0.02)
        for _ in range(7):
            leaf = tip + Vector((kit.rng.gauss(0, 0.2), kit.rng.gauss(0, 0.2), kit.rng.gauss(0.05, 0.12)))
            mesh.sphere(leaf, kit.rng.uniform(0.14, 0.22), "Olive")


def build_cafe_terrace(parts):
    for x, y in TERRACE_TABLES:
        kit.build_table_set(parts.detail, (x, y, 0))
        build_parasol(parts.detail, Vector((x, y, 0)))


def build_parasol(mesh, base):
    """A canvas parasol through the table: pole, eight striped panels and a finial."""
    top = base + Vector((0, 0, 2.35))
    mesh.cylinder(base + Vector((0, 0, 0.76)), top, 0.022, "Timber", segments=6)
    radius, drop = 1.15, 0.38
    rim = [base + Vector((math.cos(2 * math.pi * k / 8) * radius, math.sin(2 * math.pi * k / 8) * radius, 2.35 - drop)) for k in range(9)]
    for k in range(8):
        mesh.two_sided([top, rim[k], rim[k + 1]], "CanvasA" if k % 2 == 0 else "CanvasB", (0, 0, 1))
        mesh.beam(top - Vector((0, 0, 0.03)), rim[k] - Vector((0, 0, 0.03)), 0.015, "Timber")
    mesh.sphere(top + Vector((0, 0, 0.04)), 0.04, "Timber")


PIECES = {
    "SquareInlay": kit.Piece(build_inlay),
    "ChurchForecourt": kit.Piece(build_forecourt),
    "StoneBench": kit.Piece(build_stone_bench),
    "Planter": kit.Piece(build_planter),
    "CafeTerrace": kit.Piece(build_cafe_terrace),
}
kit.run_pieces(PIECES, seed=17)
