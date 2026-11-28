"""Builds the canal bridge (docs/CanalTownArtList.md #12), concept: park-and-courtyards.png.

A stone arch bridge spanning the 10 m canal with a 4 m cobbled deck between parapets, landing on the 4 m quays.
Units walk over it, so the deck is a smooth, gentle hump.

Conventions: the canal runs along X, the bridge spans along Y, the origin is quay paving level (z = 0) at the middle
of the span. The water is 1.2 m below the paving (as in build_quay.py).
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_bridge.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402
from mathutils import Vector  # noqa: E402

NAME = "Bridge"
SPAN_HALF = 5.0  # half of the canal width
END_HALF = 7.0  # the deck lands 2 m onto each quay
DECK_HALF = 2.0  # half of the 4 m walkway
PARAPET = 0.35
PARAPET_HEIGHT = 0.9
CROWN = 0.8  # deck rise at mid-span
WATER_LEVEL = -1.2
FOOT = -2.0
SPRING = -1.0  # arch springing height at the quay faces
RING = 0.5  # voussoir depth
DECK_BUILD_UP = 0.25  # fill and paving between the arch crown and the deck
SLICES = 56
NEWEL_INSET = 0.45  # keeps the newel clear of the deck end
VOUSSOIRS = 23

OUTER = DECK_HALF + PARAPET
INTRADOS_CROWN = CROWN - DECK_BUILD_UP - RING
ARCH_RADIUS = (SPAN_HALF ** 2 + (INTRADOS_CROWN - SPRING) ** 2) / (2 * (INTRADOS_CROWN - SPRING))
ARCH_CENTRE = INTRADOS_CROWN - ARCH_RADIUS


def deck(y):
    return CROWN * (1 - (y / END_HALF) ** 2)


def intrados(y):
    return ARCH_CENTRE + math.sqrt(max(0.0, ARCH_RADIUS ** 2 - y * y))


def slices():
    ys = [-END_HALF + 2 * END_HALF * k / SLICES for k in range(SLICES + 1)]
    return sorted(set(ys) | {-SPAN_HALF, SPAN_HALF})


def build(parts):
    ys = slices()
    for y0, y1 in zip(ys, ys[1:]):
        in_span = abs((y0 + y1) / 2) < SPAN_HALF
        bottom0, bottom1 = (intrados(y0), intrados(y1)) if in_span else (FOOT, FOOT)
        for sign in (-1, 1):
            x = sign * OUTER
            parts.detail.poly([(x, y0, bottom0), (x, y1, bottom1), (x, y1, deck(y1)), (x, y0, deck(y0))], "Ashlar", (sign, 0, 0))
            build_parapet_slice(parts.stone, sign, y0, y1)
        if in_span:
            parts.detail.poly([(-OUTER, y0, intrados(y0)), (OUTER, y0, intrados(y0)), (OUTER, y1, intrados(y1)),
                               (-OUTER, y1, intrados(y1))], "StoneGrime", (0, 0, -1))
        parts.detail.poly([(-DECK_HALF, y0, deck(y0)), (DECK_HALF, y0, deck(y0)), (DECK_HALF, y1, deck(y1)),
                           (-DECK_HALF, y1, deck(y1))], "Cobble", (0, 0, 1))
    for sign in (-1, 1):
        build_voussoirs(parts.stone, sign)
        y = sign * SPAN_HALF
        parts.detail.poly([(-OUTER, y, FOOT), (OUTER, y, FOOT), (OUTER, y, SPRING), (-OUTER, y, SPRING)], "StoneGrime", (0, -sign, 0))
        for x in (-OUTER - 0.05, OUTER + 0.05):
            parts.stone.box((x, y, (FOOT + SPRING) / 2), (0.5, 0.7, SPRING - FOOT), kit.waterline_material(WATER_LEVEL, WATER_LEVEL))
            build_newel(parts, (math.copysign(DECK_HALF + PARAPET / 2, x), sign * (END_HALF - NEWEL_INSET)))
    for y in (-END_HALF, END_HALF):
        facing = (0, 1 if y > 0 else -1, 0)
        parts.detail.poly([(-OUTER, y, FOOT), (OUTER, y, FOOT), (OUTER, y, 0.0), (-OUTER, y, 0.0)], "Ashlar", facing)
        parts.stone.box((0, y, -0.06), (2 * OUTER, 0.3, 0.12), "StoneDark")


def build_parapet_slice(mesh, sign, y0, y1):
    inner, outer = sign * DECK_HALF, sign * OUTER
    lip = 0.05
    for (xa, xb), z_low, z_high, material in (((inner, outer), 0.0, PARAPET_HEIGHT - 0.1, "Ashlar"),
                                             ((inner - sign * lip, outer + sign * lip), PARAPET_HEIGHT - 0.1, PARAPET_HEIGHT, "Stone")):
        front = [Vector((xb, y0, deck(y0) + z_low)), Vector((xb, y1, deck(y1) + z_low)), Vector((xb, y1, deck(y1) + z_high)),
                 Vector((xb, y0, deck(y0) + z_high))]
        back = [Vector((xa, p.y, p.z)) for p in front]
        mesh.hexahedron(front, back, material)


def build_voussoirs(mesh, sign):
    """The arch ring on each face, a little proud of the spandrel."""
    x = sign * (OUTER + 0.03)
    start = math.asin(SPAN_HALF / ARCH_RADIUS)
    for k in range(VOUSSOIRS):
        a0 = -start + 2 * start * k / VOUSSOIRS + 0.004
        a1 = -start + 2 * start * (k + 1) / VOUSSOIRS - 0.004
        depth = RING + (0.12 if k == VOUSSOIRS // 2 else 0.0)
        points = []
        for a, r in ((a0, ARCH_RADIUS), (a1, ARCH_RADIUS), (a1, ARCH_RADIUS + depth), (a0, ARCH_RADIUS + depth)):
            points.append(Vector((0, math.sin(a) * r, ARCH_CENTRE + math.cos(a) * r)))
        front = [p + Vector((x + sign * 0.03, 0, 0)) for p in points]
        back = [p + Vector((x - sign * 0.06, 0, 0)) for p in points]
        mesh.hexahedron(front, back, kit.stone_material(points[0].z + 1.0))


def build_newel(parts, at):
    """A square stone pillar at each end of a parapet, carrying a lantern."""
    x, y = at
    base = deck(y)
    parts.stone.box((x, y, base + 0.6), (0.5, 0.5, 1.2), "Stone")
    parts.stone.box((x, y, base + 1.25), (0.6, 0.6, 0.1), "StoneDark")
    mesh = parts.detail
    mesh.cylinder((x, y, base + 1.3), (x, y, base + 2.4), 0.05, "Iron", segments=8)
    lamp = Vector((x, y, base + 2.6))
    mesh.box(lamp, (0.24, 0.24, 0.34), "LampGlow")
    for dx in (-0.12, 0.12):
        for dy in (-0.12, 0.12):
            mesh.cylinder(lamp + Vector((dx, dy, -0.18)), lamp + Vector((dx, dy, 0.18)), 0.012, "Iron", segments=4)
    mesh.box(lamp + Vector((0, 0, -0.2)), (0.28, 0.28, 0.05), "Iron")
    mesh.cylinder(lamp + Vector((0, 0, 0.18)), lamp + Vector((0, 0, 0.34)), 0.19, "Iron", segments=8, radius_end=0.03)


kit.run(NAME, build, seed=12, preview_ground=FOOT, preview_water=(WATER_LEVEL, -SPAN_HALF, SPAN_HALF))
