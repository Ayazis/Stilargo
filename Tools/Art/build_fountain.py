"""Builds the square fountain and the courtyard well (docs/CanalTownArtList.md #15 and #16).

- `Fountain`: 4 × 4 m, concept stilargo-church-square.png. An octagonal stone basin with water, a square pedestal with
  four bronze spouts pouring into it, a small upper bowl and a finial.
- `Well`: 2 × 2 m. A round rubble wellhead with a timber frame, a winch with a crank, a rope and bucket, under a small
  tiled gable roof.
Origin at ground level in the middle of each piece.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_fountain.py -- [--preview <folder>] [--piece Well]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402
from mathutils import Vector  # noqa: E402

OCTAGON = 8
OCTAGON_TURN = math.pi / 8  # puts a flat side, not a corner, towards each axis
BASIN_OUTER = 2.0
BASIN_INNER = 1.7
BASIN_TOP = 0.65
WATER_LEVEL = 0.5
PEDESTAL_TOP = 1.6
SPOUT_HEIGHT = 1.15
JET_SEGMENTS = 8

WELL_INNER = 0.55
WELL_OUTER = 0.8
WELL_TOP = 0.85
WELL_SIDES = 16
POST_X = 0.78
BEAM_HEIGHT = 2.2
WINCH_HEIGHT = 1.55
ROOF_PITCH = math.radians(38)


def build_fountain(parts):
    stone, detail = parts.stone, parts.detail
    kit.prism(stone, (0, 0, 0), BASIN_OUTER + 0.18, 0.0, 0.12, OCTAGON, "StoneDark", rotation=OCTAGON_TURN)
    kit.annulus_wall(stone, (0, 0, 0), BASIN_INNER, BASIN_OUTER, 0.12, BASIN_TOP - 0.1, OCTAGON, "Stone", rotation=OCTAGON_TURN)
    kit.annulus_wall(stone, (0, 0, 0), BASIN_INNER - 0.05, BASIN_OUTER + 0.08, BASIN_TOP - 0.1, BASIN_TOP, OCTAGON, "Stone",
                     rotation=OCTAGON_TURN)
    kit.prism(detail, (0, 0, 0), BASIN_INNER + 0.01, 0.12, 0.16, OCTAGON, "StoneGrime", rotation=OCTAGON_TURN)
    kit.prism(detail, (0, 0, 0), BASIN_INNER + 0.01, WATER_LEVEL - 0.01, WATER_LEVEL, OCTAGON, "Water", rotation=OCTAGON_TURN)
    build_waterline_moss(detail)
    build_pedestal(stone, detail)


def build_waterline_moss(mesh):
    for _ in range(40):
        a = kit.rng.uniform(0, 2 * math.pi)
        r = BASIN_INNER - 0.02
        mesh.sphere((math.cos(a) * r, math.sin(a) * r, WATER_LEVEL + 0.02), kit.rng.uniform(0.03, 0.06), "Moss")


def build_pedestal(stone, detail):
    """A square shaft with moulded base and cap, four spouts, an upper bowl and a pine-cone finial."""
    stone.box((0, 0, 0.3), (0.9, 0.9, 0.3), "StoneDark")
    stone.box((0, 0, (0.45 + PEDESTAL_TOP) / 2), (0.66, 0.66, PEDESTAL_TOP - 0.45), "Stone")
    stone.box((0, 0, PEDESTAL_TOP + 0.05), (0.82, 0.82, 0.1), "Stone")
    kit.prism(stone, (0, 0, 0), 0.25, PEDESTAL_TOP + 0.1, PEDESTAL_TOP + 0.25, OCTAGON, "Stone", radius_top=0.6, rotation=OCTAGON_TURN)
    kit.prism(stone, (0, 0, 0), 0.6, PEDESTAL_TOP + 0.25, PEDESTAL_TOP + 0.35, OCTAGON, "Stone", rotation=OCTAGON_TURN)
    kit.prism(detail, (0, 0, 0), 0.52, PEDESTAL_TOP + 0.3, PEDESTAL_TOP + 0.36, OCTAGON, "Water", rotation=OCTAGON_TURN)
    stone.cylinder((0, 0, PEDESTAL_TOP + 0.35), (0, 0, PEDESTAL_TOP + 0.75), 0.1, "Stone", segments=10, radius_end=0.07)
    stone.sphere((0, 0, PEDESTAL_TOP + 0.85), 0.14, "Stone", subdivisions=2)
    stone.cylinder((0, 0, PEDESTAL_TOP + 0.95), (0, 0, PEDESTAL_TOP + 1.15), 0.08, "Stone", segments=10, radius_end=0.0)
    for k in range(4):
        direction = Vector((math.cos(k * math.pi / 2), math.sin(k * math.pi / 2), 0))
        build_spout(detail, direction)
        stone.box(direction * 0.34 + Vector((0, 0, SPOUT_HEIGHT)), (0.3 if direction.y else 0.04, 0.04 if direction.y else 0.3, 0.36),
                  "StoneDark")


def build_spout(mesh, direction):
    """A bronze pipe out of the pedestal with an arc of water falling into the basin."""
    root = direction * 0.33 + Vector((0, 0, SPOUT_HEIGHT))
    mouth = direction * 0.55 + Vector((0, 0, SPOUT_HEIGHT + 0.02))
    mesh.cylinder(root, mouth, 0.035, "Bronze", segments=8)
    reach, drop = 0.55, SPOUT_HEIGHT - WATER_LEVEL
    points = [mouth + direction * (reach * t) - Vector((0, 0, drop * t * t)) for t in (k / JET_SEGMENTS for k in range(JET_SEGMENTS + 1))]
    for a, b in zip(points, points[1:]):
        mesh.cylinder(a, b, 0.025, "Water", segments=6)
    splash = points[-1]
    mesh.cylinder(splash - Vector((0, 0, 0.01)), splash + Vector((0, 0, 0.015)), 0.12, "FlowerWhite", segments=10)


def build_well(parts):
    stone, detail = parts.stone, parts.detail
    for course, (z0, z1) in enumerate(((0.0, 0.3), (0.3, 0.6), (0.6, WELL_TOP - 0.1))):
        turn = course * math.pi / WELL_SIDES
        for k in range(WELL_SIDES):
            a0 = turn + 2 * math.pi * k / WELL_SIDES
            a1 = turn + 2 * math.pi * (k + 1) / WELL_SIDES - 0.02
            corner = lambda a, r, z: Vector((math.cos(a) * r, math.sin(a) * r, z))
            front = [corner(a0, WELL_OUTER, z0), corner(a1, WELL_OUTER, z0), corner(a1, WELL_OUTER, z1 - 0.01), corner(a0, WELL_OUTER, z1 - 0.01)]
            back = [corner(a0, WELL_INNER, z0), corner(a1, WELL_INNER, z0), corner(a1, WELL_INNER, z1 - 0.01), corner(a0, WELL_INNER, z1 - 0.01)]
            stone.hexahedron(front, back, kit.stone_material((z0 + z1) / 2))
    kit.annulus_wall(stone, (0, 0, 0), WELL_INNER - 0.04, WELL_OUTER + 0.06, WELL_TOP - 0.1, WELL_TOP, WELL_SIDES, "Stone")
    kit.prism(detail, (0, 0, 0), WELL_INNER, 0.15, 0.2, WELL_SIDES, "Shade")
    build_winch(detail)
    build_board_roof(detail)


def build_board_roof(mesh):
    """A small pitched roof of overlapping timber boards on rafters; canal tiles are too big to read at this size."""
    half_x, half_y = POST_X + 0.35, 0.8
    base, ridge = BEAM_HEIGHT + 0.08, BEAM_HEIGHT + 0.08 + half_y * math.tan(ROOF_PITCH)
    for sign in (-1, 1):
        for x in (-half_x + 0.05, 0.0, half_x - 0.05):
            mesh.beam((x, sign * half_y, base), (x, 0.0, ridge), 0.07, "Timber")
        rows = 6
        for k in range(rows):
            t0, t1 = k / rows, (k + 1.25) / rows
            y0, y1 = sign * half_y * (1 - t0), sign * half_y * max(0.0, 1 - t1)
            z0, z1 = base + (ridge - base) * t0 + 0.05, base + (ridge - base) * min(1.0, t1) + 0.05
            front = [Vector((-half_x, y0, z0)), Vector((half_x, y0, z0)), Vector((half_x, y1, z1)), Vector((-half_x, y1, z1))]
            back = [p + Vector((0, 0, 0.025)) for p in front]
            mesh.hexahedron(front, back, "Wood" if k % 2 else "Timber")
    mesh.beam((-half_x, 0, ridge + 0.07), (half_x, 0, ridge + 0.07), 0.1, "Wood")


def build_winch(mesh):
    for sign in (-1, 1):
        x = sign * POST_X
        mesh.beam((x, 0, 0.0), (x, 0, BEAM_HEIGHT), 0.14, "Timber")
        mesh.beam((x, 0, 0.6), (x * 0.6 + sign * 0.35, 0, 0.05), 0.08, "Timber")
    mesh.beam((-POST_X - 0.35, 0, BEAM_HEIGHT), (POST_X + 0.35, 0, BEAM_HEIGHT), 0.14, "Timber")
    mesh.cylinder((-POST_X + 0.07, 0, WINCH_HEIGHT), (POST_X - 0.07, 0, WINCH_HEIGHT), 0.12, "Timber", segments=12)
    for x in (-0.25, -0.1, 0.05, 0.2):
        mesh.cylinder((x, 0, WINCH_HEIGHT), (x + 0.12, 0, WINCH_HEIGHT), 0.13, "Rope", segments=12)
    crank = Vector((POST_X + 0.1, 0, WINCH_HEIGHT))
    mesh.cylinder((POST_X - 0.07, 0, WINCH_HEIGHT), crank, 0.025, "Iron", segments=6)
    mesh.beam(crank, crank + Vector((0, -0.05, -0.3)), 0.035, "Iron")
    mesh.cylinder(crank + Vector((0, -0.05, -0.3)), crank + Vector((0.15, -0.05, -0.3)), 0.025, "Timber", segments=6)
    bucket_top = Vector((0.0, -0.12, 1.05))
    mesh.cylinder((0.0, -0.12, WINCH_HEIGHT - 0.12), bucket_top + Vector((0, 0, 0.12)), 0.012, "Rope", segments=5)
    mesh.cylinder(bucket_top - Vector((0, 0, 0.3)), bucket_top, 0.13, "Timber", segments=12, radius_end=0.16)
    for z in (-0.25, -0.05):
        mesh.cylinder(bucket_top + Vector((0, 0, z - 0.015)), bucket_top + Vector((0, 0, z + 0.015)), 0.16 + z * 0.1, "Iron", segments=12)
    kit.build_ring(mesh, bucket_top + Vector((0, 0, 0.02)), (0, 1, 0), 0.14, 0.008, "Iron", segments=10)


PIECES = {"Fountain": kit.Piece(build_fountain), "Well": kit.Piece(build_well)}
kit.run_pieces(PIECES, seed=15)
