"""Builds the rowing boat (docs/CanalTownArtList.md #14), concept: canal-boatyard.png.

A 4.2 m clinker-built wooden boat with a painted top strake, three thwarts, oarlocks and a pair of oars.
Conventions: the boat points along X, the origin is at the waterline in the middle of the hull.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_rowing_boat.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402
from mathutils import Vector  # noqa: E402

NAME = "RowingBoat"
LENGTH = 4.2
BEAM = 1.35
DEPTH = 0.55
DRAUGHT = 0.18  # how deep the keel sits below the waterline
SHEER = 0.18  # how much the gunwale rises towards bow and stern
STATIONS = 24
STRAKES = 6
CLINKER_STEP = 0.012  # each plank overlaps the one below and stands a little proud of it
THWARTS = (-1.0, 0.1, 1.15)


def half_beam(x):
    """Half the hull's width at a point along it: full amidships, pinched to a point at bow and stern."""
    t = min(1.0, abs(x) / (LENGTH / 2))
    return BEAM / 2 * (1 - t ** 2.2) ** 0.7


def section_point(x, t, side):
    """A point on the hull at station x, t = 0 at the keel to 1 at the gunwale, side = +1 or -1."""
    sheer = SHEER * (2 * x / LENGTH) ** 2
    bottom = -DRAUGHT + 0.1 * (2 * x / LENGTH) ** 2
    top = -DRAUGHT + DEPTH + sheer
    angle = t * math.pi / 2
    y = side * half_beam(x) * math.sin(angle)
    z = bottom + (top - bottom) * (1 - math.cos(angle)) ** 0.85
    return Vector((x, y, z))


def build(parts):
    build_hull(parts.detail)
    build_gunwales(parts.detail)
    for x in THWARTS:
        build_thwart(parts.detail, x)
    build_oars(parts.detail)
    parts.detail.beam(section_point(-LENGTH / 2, 0, 1), section_point(LENGTH / 2, 0, 1), 0.07, "Wood")


def build_hull(mesh):
    xs = [-LENGTH / 2 + LENGTH * k / STATIONS for k in range(STATIONS + 1)]
    for strake in range(STRAKES):
        t0, t1 = strake / STRAKES, (strake + 1) / STRAKES
        material = "BoatPaint" if strake == STRAKES - 1 else ("Timber" if strake % 2 else "Wood")
        proud = CLINKER_STEP * strake
        for side in (-1, 1):
            for x0, x1 in zip(xs, xs[1:]):
                quad = [section_point(x0, t0, side), section_point(x1, t0, side), section_point(x1, t1, side), section_point(x0, t1, side)]
                outward = Vector((0, side, -0.4 if strake < 2 else 0.2))
                quad = [p + outward.normalized() * proud * min(1.0, abs(p.y) * 6) for p in quad]
                mesh.two_sided(quad, material, outward)


def build_gunwales(mesh):
    xs = [-LENGTH / 2 + LENGTH * k / STATIONS for k in range(STATIONS + 1)]
    for side in (-1, 1):
        for x0, x1 in zip(xs, xs[1:]):
            mesh.beam(section_point(x0, 1, side) + Vector((0, 0, 0.02)), section_point(x1, 1, side) + Vector((0, 0, 0.02)), 0.05, "Wood")
    stem_top = section_point(LENGTH / 2, 1, 1)
    mesh.beam(section_point(LENGTH / 2, 0, 1), stem_top + Vector((0.03, 0, 0.06)), 0.07, "Wood")
    mesh.beam(section_point(-LENGTH / 2, 0, 1), section_point(-LENGTH / 2, 1, 1) + Vector((-0.03, 0, 0.06)), 0.07, "Wood")


def build_thwart(mesh, x):
    """A seat plank across the boat at about two-thirds of its depth, resting on a riser."""
    height = section_point(x, 0.72, 1).z
    width = section_point(x, 0.72, 1).y * 2 - 0.04
    mesh.box((x, 0, height), (0.24, width, 0.04), "Timber")
    mesh.box((x, 0, (height + section_point(x, 0, 1).z) / 2), (0.06, 0.06, height - section_point(x, 0, 1).z), "Wood")


def build_oars(mesh):
    """Two oars shipped along the thwarts, blades towards the bow, in bronze oarlocks."""
    lock_x = THWARTS[1] + 0.35
    for side in (-1, 1):
        lock = section_point(lock_x, 1, side) + Vector((0, 0, 0.05))
        mesh.cylinder(lock, lock + Vector((0, 0, 0.09)), 0.02, "Bronze", segments=6)
        handle = Vector((-1.3, side * 0.25, lock.z + 0.06))
        blade_root = Vector((1.3, side * 0.42, lock.z + 0.06))
        mesh.cylinder(handle, blade_root, 0.025, "Timber", segments=8)
        tip = blade_root + Vector((0.6, side * 0.05, 0))
        mesh.box((blade_root + tip) / 2, (0.6, 0.13, 0.02), "Timber")


kit.run(NAME, build, seed=14, preview_ground=-1.0, preview_water=(0.0, -30.0, 30.0))
