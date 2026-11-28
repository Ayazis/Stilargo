"""Builds the canal-town trees (docs/CanalTownArtList.md #20, #24 and #30), concept: park-and-courtyards.png.

- `PlaneTree`: a 10 m London plane with a mottled trunk and a broad crown, for parks and streets.
- `UmbrellaPine`: a stone pine with a tall bare trunk and a flat crown, for streets and lanes.
- `Willow`: a weeping willow for pond sides, its crown hanging in curtains of leaves.
- `FruitTreeA`, `FruitTreeB`, `FruitTreeC`: small orchard and garden trees (apple, orange, pear).
- `Shrub`: a round bush for gardens and planters.
Origin at the foot of the trunk, on the ground.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_trees.py -- [--preview <folder>] [--piece Willow]
"""
import math
import sys
from pathlib import Path

import bmesh
from mathutils import Matrix, Vector

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

rng = kit.rng


def build_limb(mesh, start, direction, length, radius, material="Bark", steps=4):
    """A tapering, slightly wandering limb; returns its tip."""
    point = Vector(start)
    direction = Vector(direction).normalized()
    for k in range(steps):
        wobble = Vector((rng.uniform(-0.25, 0.25), rng.uniform(-0.25, 0.25), rng.uniform(-0.05, 0.15)))
        direction = (direction + wobble).normalized()
        nxt = point + direction * (length / steps)
        r0 = radius * (1 - 0.6 * k / steps)
        r1 = radius * (1 - 0.6 * (k + 1) / steps)
        mesh.cylinder(point, nxt, r0, material, segments=8, radius_end=r1)
        point = nxt
    return point


def build_blob(mesh, centre, radius, material):
    """A lumpy foliage cluster: an icosphere with its vertices pushed in and out."""
    result = bmesh.ops.create_icosphere(mesh.bm, subdivisions=2, radius=radius, matrix=Matrix.Translation(centre))
    for vert in result["verts"]:
        offset = vert.co - Vector(centre)
        vert.co = Vector(centre) + offset * rng.uniform(0.82, 1.12)
    mesh._paint(result["verts"], material)


def build_crown(mesh, centre, size, count, materials, blob=(0.9, 1.5)):
    """Foliage clusters scattered through an ellipsoid of half-extents `size`."""
    centre, size = Vector(centre), Vector(size)
    for _ in range(count):
        while True:
            p = Vector((rng.uniform(-1, 1), rng.uniform(-1, 1), rng.uniform(-1, 1)))
            if p.length <= 1:
                break
        position = centre + Vector((p.x * size.x, p.y * size.y, p.z * size.z))
        build_blob(mesh, position, rng.uniform(*blob), rng.choice(materials))


def build_plane_tree(parts):
    mesh = parts.detail
    top = build_limb(mesh, (0, 0, 0), (0, 0, 1), 4.2, 0.32)
    for k in range(4):
        a = k * math.pi / 2 + rng.uniform(-0.4, 0.4)
        build_limb(mesh, top, (math.cos(a), math.sin(a), 1.4), 3.0, 0.16)
    build_crown(mesh, (0, 0, 7.4), (4.2, 4.2, 2.6), 48, ("Foliage", "Foliage", "FoliageLight"))


def build_umbrella_pine(parts):
    mesh = parts.detail
    top = build_limb(mesh, (0, 0, 0), (0.15, 0, 1), 7.0, 0.26, "BarkDark", steps=6)
    tips = []
    for k in range(5):
        a = k * 2 * math.pi / 5 + rng.uniform(-0.3, 0.3)
        tips.append(build_limb(mesh, top, (math.cos(a), math.sin(a), 0.6), 2.4, 0.11, "BarkDark", steps=3))
    for tip in tips:
        build_crown(mesh, tip + Vector((0, 0, 0.5)), (1.6, 1.6, 0.4), 7, ("PineNeedles", "Hedge"), blob=(0.7, 1.1))
    build_crown(mesh, top + Vector((0, 0, 1.6)), (2.6, 2.6, 0.5), 10, ("PineNeedles",), blob=(0.8, 1.2))


def build_willow(parts):
    mesh = parts.detail
    top = build_limb(mesh, (0, 0, 0), (0, 0, 1), 2.8, 0.34)
    for k in range(5):
        a = k * 2 * math.pi / 5
        build_limb(mesh, top, (math.cos(a), math.sin(a), 1.0), 2.6, 0.14)
    build_crown(mesh, (0, 0, 5.6), (2.6, 2.6, 1.2), 16, ("FoliageLight", "Foliage"), blob=(0.8, 1.2))
    for _ in range(110):
        a = rng.uniform(0, 2 * math.pi)
        r = rng.uniform(1.2, 3.3)
        root = Vector((math.cos(a) * r, math.sin(a) * r, rng.uniform(5.0, 6.4)))
        build_strand(mesh, root, rng.uniform(2.8, 4.8), Vector((math.cos(a), math.sin(a), 0)))


def build_strand(mesh, root, length, outward):
    """A hanging curtain of willow leaves: a narrow two-sided ribbon arching out and falling."""
    width = rng.uniform(0.12, 0.2)
    side = Vector((-outward.y, outward.x, 0)) * width / 2
    points = [root + outward * (0.4 * math.sin(math.pi * t * 0.5)) - Vector((0, 0, length * t)) for t in (k / 5 for k in range(6))]
    material = rng.choice(("FoliageLight", "Foliage"))
    for k, (a, b) in enumerate(zip(points, points[1:])):
        taper = 1 - k / 6
        mesh.two_sided([a - side * taper, a + side * taper, b + side * (taper - 0.16), b - side * (taper - 0.16)], material, outward)


def fruit_tree(fruit, foliage, seed_turn):
    def build(parts):
        mesh = parts.detail
        top = build_limb(mesh, (0, 0, 0), (rng.uniform(-0.2, 0.2), 0, 1), 1.3, 0.12)
        for k in range(4):
            a = seed_turn + k * math.pi / 2 + rng.uniform(-0.3, 0.3)
            build_limb(mesh, top, (math.cos(a), math.sin(a), 1.1), 1.4, 0.06, steps=3)
        build_crown(mesh, (0, 0, 2.6), (1.4, 1.4, 1.0), 14, foliage, blob=(0.5, 0.8))
        for _ in range(26):
            a = rng.uniform(0, 2 * math.pi)
            z = rng.uniform(1.8, 3.2)
            r = 1.5 * math.sqrt(max(0.0, 1 - ((z - 2.6) / 1.1) ** 2)) + 0.1
            mesh.sphere((math.cos(a) * r, math.sin(a) * r, z), 0.06, fruit)
    return build


def build_shrub(parts):
    build_crown(parts.detail, (0, 0, 0.55), (0.55, 0.55, 0.4), 8, ("Hedge", "Foliage"), blob=(0.3, 0.45))


PIECES = {
    "PlaneTree": kit.Piece(build_plane_tree),
    "UmbrellaPine": kit.Piece(build_umbrella_pine),
    "Willow": kit.Piece(build_willow),
    "FruitTreeA": kit.Piece(fruit_tree("Apple", ("Foliage", "FoliageLight"), 0.0)),
    "FruitTreeB": kit.Piece(fruit_tree("Orange", ("Hedge", "Foliage"), 0.7)),
    "FruitTreeC": kit.Piece(fruit_tree("Greens", ("FoliageLight", "FoliageAutumn"), 1.3)),
    "Shrub": kit.Piece(build_shrub),
}
kit.run_pieces(PIECES, seed=20)
