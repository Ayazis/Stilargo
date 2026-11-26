"""Builds the canal-town church with bell tower (docs/CanalTownArtList.md #7, concept: stilargo-church-square.png).

A stone nave with a rose window and a columned portal up a flight of steps facing the square, buttressed sides with tall
round-headed windows, and an 18 m bell tower with an open belfry, a bronze bell and a cross.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_church.py -- [--preview <folder>]
"""
import math
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402
from mathutils import Vector  # noqa: E402

NAME = "Church"
NAVE_X, NAVE_Y, NAVE_WIDTH, NAVE_DEPTH = -1.7, 0.25, 9.0, 17.0
NAVE_STOREYS = (4.5, 4.5)
TOWER_X, TOWER_Y, TOWER_SIZE = 4.55, -6.5, 3.5
TOWER_STOREYS = (4.5, 4.5, 4.5, 4.5)
BELFRY = 3
STEPS = 4
STEP_RISE = 0.17
SIDE_WINDOWS = (0.2, 0.4, 0.6, 0.8)
BUTTRESSES = (0.1, 0.3, 0.5, 0.7, 0.9)
TOWER_END = 0.25  # the first quarter of the nave's right side stands against the tower


def build(parts):
    nave = build_nave(parts)
    tower = build_tower(parts)
    nave_roof = kit.roof_over(nave, math.radians(32), kind="gable", ridge_axis="y")
    kit.build_roof(parts.roof, nave_roof, nave)
    build_cross(parts.detail, (NAVE_X, nave_roof.y0 + 0.25, nave_roof.ridge + 0.15), 1.6)
    tower_roof = kit.roof_over(tower, math.radians(40), overhang=0.35)
    kit.build_roof(parts.roof, tower_roof)
    build_cross(parts.detail, (TOWER_X, TOWER_Y, tower_roof.ridge + 0.1), 1.4)
    build_bell(parts.detail, tower)


def build_nave(parts):
    nave = kit.Block((NAVE_X, NAVE_Y), NAVE_WIDTH, NAVE_DEPTH, NAVE_STOREYS, "Ashlar", surround="stone", plinth=0.9)
    portal = nave.add(kit.FRONT, kit.Opening(NAVE_WIDTH / 2, STEPS * STEP_RISE, 2.2, 2.8, "gate", rise=1.1, paint="Wood"))
    nave.add(kit.FRONT, kit.RoundWindow(NAVE_WIDTH / 2, 6.7, 1.3))
    for index in (kit.RIGHT, kit.LEFT):
        for fraction in clear_of_tower(index, SIDE_WINDOWS):
            nave.add(index, kit.Opening(NAVE_DEPTH * fraction, 3.0, 0.9, 3.2, "slit", rise=0.45, depth=0.4))
    nave.add(kit.BACK, kit.RoundWindow(NAVE_WIDTH / 2, 6.0, 0.8, spokes=8))
    kit.build_block(parts.stone, parts.detail, nave)
    front = nave.side(kit.FRONT)
    build_portal_frame(parts.stone, front, portal)
    build_steps(parts.stone, front, portal)
    for index in (kit.RIGHT, kit.LEFT):
        build_buttresses(parts.stone, nave.side(index), clear_of_tower(index, BUTTRESSES))
    return nave


def build_tower(parts):
    tower = kit.Block((TOWER_X, TOWER_Y), TOWER_SIZE, TOWER_SIZE, TOWER_STOREYS, "Ashlar", surround="stone", plinth=0.9,
                      hollow_from=BELFRY)
    for index in (kit.FRONT, kit.RIGHT, kit.BACK):
        for storey in (1, 2):
            tower.add(index, kit.Opening(TOWER_SIZE / 2, tower.floor(storey) + 1.0, 0.5, 1.4, "slit", rise=0.25, depth=0.4))
    for index in (kit.FRONT, kit.RIGHT, kit.BACK, kit.LEFT):
        tower.add(index, kit.Opening(TOWER_SIZE / 2, tower.floor(BELFRY) + 0.9, 1.4, 2.3, "belfry", rise=0.7))
    kit.build_block(parts.stone, parts.detail, tower)
    return tower


def build_portal_frame(mesh, side, portal):
    """Columns either side of the portal under an entablature and a small stone gable."""
    for sign in (-1, 1):
        s = portal.s + sign * (portal.width / 2 + 0.55)
        base = side.point(s, portal.bottom, 0.3)
        mesh.box(base + Vector((0, 0, 0.2)), (0.55, 0.55, 0.4), "Stone")
        mesh.cylinder(base + Vector((0, 0, 0.4)), base + Vector((0, 0, portal.top - portal.bottom + 0.2)), 0.18, "Stone", segments=16)
        mesh.box(base + Vector((0, 0, portal.top - portal.bottom + 0.3)), (0.55, 0.55, 0.2), "Stone")
    top = portal.top + 0.5
    half = portal.width / 2 + 0.95
    side.box(mesh, portal.s, top, 0.25, (2 * half, 0.25, 0.5), "Stone")
    side.block(mesh, [(portal.s - half, top + 0.12), (portal.s + half, top + 0.12), (portal.s + 0.01, top + 1.1),
                      (portal.s - 0.01, top + 1.1)], 0.0, 0.4, "Stone")


def build_steps(mesh, side, portal):
    """A wide flight of steps up to the portal, as in the church-square concept."""
    width = portal.width + 6.0
    for k in range(STEPS):
        depth = 0.4 * (STEPS - k)
        side.box(mesh, portal.s, STEP_RISE * (k + 0.5), depth / 2, (width - 0.4 * k, STEP_RISE, depth), "Stone")


def clear_of_tower(index, fractions):
    return [f for f in fractions if index != kit.RIGHT or f > TOWER_END]


def build_buttresses(mesh, side, fractions):
    for fraction in fractions:
        s = NAVE_DEPTH * fraction
        side.box(mesh, s, 2.6, 0.35, (0.6, 5.2, 0.7), "StoneDark")
        side.block(mesh, [(s - 0.3, 5.2), (s + 0.3, 5.2), (s + 0.3, 6.4), (s - 0.3, 6.4)], 0.0, 0.25, "Stone")


def build_cross(mesh, base, height):
    x, y, z = base
    mesh.box((x, y, z + height / 2), (0.12, 0.12, height), "Iron")
    mesh.box((x, y, z + height * 0.68), (height * 0.55, 0.1, 0.1), "Iron")


def build_bell(mesh, tower):
    """A bronze bell hung from a timber yoke in the open belfry."""
    floor = tower.base + tower.floor(BELFRY)
    top = tower.eaves - 0.35
    centre = Vector((TOWER_X, TOWER_Y, 0))
    inner = TOWER_SIZE / 2 - tower.wall_thickness
    mesh.beam(centre + Vector((-inner, 0, top)), centre + Vector((inner, 0, top)), 0.2, "Timber")
    mouth = floor + 1.6
    crown = top - 0.35
    mesh.cylinder(centre + Vector((0, 0, mouth)), centre + Vector((0, 0, crown)), 0.62, "Bronze", segments=20, radius_end=0.3)
    mesh.sphere(centre + Vector((0, 0, crown)), 0.31, "Bronze", subdivisions=2)
    mesh.cylinder(centre + Vector((0, 0, mouth - 0.08)), centre + Vector((0, 0, mouth + 0.06)), 0.68, "Bronze", segments=20)


kit.run(NAME, build, seed=11)
