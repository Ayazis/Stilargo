"""Builds the detailed "Corner House" from ConceptArt/CanalTown (stilargo-finished-gameplay.png) and exports it as one FBX.

Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_corner_house.py -- [--preview out.png]

Everything is real geometry so the look survives at the gameplay camera: individual canal roof tiles, hip and ridge
caps, stone quoins and window surrounds, brick where the plaster has fallen off, panelled shutters, curtains, flower
boxes, a cornice with a zinc gutter, drainpipes and a chimney with clay pots.

Conventions (Blender space, metres): origin at the bottom centre of the footprint, Z up, the front facade faces -Y.
Material names are remapped to HDRP materials by CornerHouseAssets in Unity.
"""
import math
import random
import sys
from pathlib import Path

import bmesh
import bpy
from mathutils import Matrix, Vector

OUTPUT = Path("Assets/_Project/Art/Models/CornerHouse/CornerHouse.fbx")

WIDTH = 12.4
DEPTH = 10.0
GROUND_STOREY = 3.8
UPPER_STOREY = 3.3
UPPER_STOREYS = 2
EAVES = GROUND_STOREY + UPPER_STOREY * UPPER_STOREYS

CORE_INSET = 0.3
BAY = 2.05
WINDOW_W = 1.0
WINDOW_H = 1.75
SILL_H = 0.85
GROUND_WINDOW_H = 1.5
DOOR_W = 1.3
DOOR_H = 2.7
SURROUND = 0.17
SURROUND_PROUD = 0.06
REVEAL = 0.22

PLASTER_CELL = 0.2
PLASTER_THICK = 0.025
BRICK_L = 0.24
BRICK_H = 0.065
MORTAR = 0.012
BRICK_PROUD = 0.012

ROOF_PITCH = math.radians(33)
OVERHANG = 0.5
TILE_W = 0.21
TILE_ROW = 0.3
TILE_R = 0.085

MATERIALS = {
    "Plaster": (0.56, 0.4, 0.2),
    "Mortar": (0.55, 0.5, 0.42),
    "BrickA": (0.45, 0.2, 0.13),
    "BrickB": (0.52, 0.26, 0.16),
    "BrickC": (0.38, 0.18, 0.13),
    "Stone": (0.68, 0.63, 0.53),
    "RoofTileA": (0.5, 0.2, 0.11),
    "RoofTileB": (0.58, 0.26, 0.14),
    "RoofTileC": (0.42, 0.18, 0.11),
    "RoofUnder": (0.12, 0.08, 0.06),
    "Zinc": (0.55, 0.57, 0.58),
    "Glass": (0.04, 0.05, 0.06),
    "Frame": (0.78, 0.75, 0.68),
    "Shutter": (0.09, 0.2, 0.12),
    "Curtain": (0.62, 0.34, 0.14),
    "Wood": (0.28, 0.17, 0.1),
    "Iron": (0.06, 0.06, 0.06),
    "Terracotta": (0.55, 0.27, 0.15),
    "Flower": (0.7, 0.08, 0.08),
    "Leaf": (0.12, 0.25, 0.08),
}
MATERIAL_INDEX = {name: index for index, name in enumerate(MATERIALS)}

rng = random.Random(7)


# ---------------------------------------------------------------- mesh helpers

class Mesh:
    """One bmesh with a material index per face."""

    def __init__(self):
        self.bm = bmesh.new()

    def quad(self, corners, material):
        verts = [self.bm.verts.new(c) for c in corners]
        face = self.bm.faces.new(verts)
        face.material_index = MATERIAL_INDEX[material]

    def box(self, centre, size, material, matrix=None):
        cx, cy, cz = centre
        hx, hy, hz = size[0] / 2, size[1] / 2, size[2] / 2
        corners = [Vector((cx + sx * hx, cy + sy * hy, cz + sz * hz)) for sz in (-1, 1) for sy in (-1, 1) for sx in (-1, 1)]
        if matrix is not None:
            corners = [matrix @ c for c in corners]
        v = [self.bm.verts.new(c) for c in corners]
        for ids in ((0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)):
            self.bm.faces.new([v[i] for i in ids]).material_index = MATERIAL_INDEX[material]

    def cylinder(self, start, end, radius, material, segments=8):
        start, end = Vector(start), Vector(end)
        axis = end - start
        rotation = axis.normalized().to_track_quat("Z", "Y").to_matrix().to_4x4()
        matrix = Matrix.Translation((start + end) / 2) @ rotation
        result = bmesh.ops.create_cone(self.bm, cap_ends=True, segments=segments, radius1=radius, radius2=radius,
                                       depth=axis.length, matrix=matrix)
        self._paint(result["verts"], material)

    def sphere(self, centre, radius, material):
        result = bmesh.ops.create_icosphere(self.bm, subdivisions=1, radius=radius, matrix=Matrix.Translation(centre))
        self._paint(result["verts"], material)

    def _paint(self, verts, material):
        faces = {f for v in verts for f in v.link_faces}
        for f in faces:
            f.material_index = MATERIAL_INDEX[material]

    def to_object(self, name, bevel=0.0):
        mesh = bpy.data.meshes.new(name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.collection.objects.link(obj)
        for material in MATERIALS:
            obj.data.materials.append(bpy.data.materials[material])
        if bevel > 0.0:
            modifier = obj.modifiers.new("Bevel", "BEVEL")
            modifier.width = bevel
            modifier.segments = 1
            modifier.limit_method = "ANGLE"
        return obj


class Side:
    """A facade in its own frame: s runs along the wall from its left corner, h is height, d sticks outwards."""

    def __init__(self, origin, along, outward, length, has_door):
        self.origin = Vector(origin)
        self.along = Vector(along)
        self.outward = Vector(outward)
        self.length = length
        self.has_door = has_door

    def point(self, s, h, d):
        return self.origin + self.along * s + self.outward * d + Vector((0, 0, h))

    def box(self, mesh, s, h, d, size, material):
        """size = (along, up, out) around the local centre (s, h, d)."""
        centre = self.point(s, h, d)
        world = self.along * size[0] + self.outward * size[2]
        mesh.box(centre, (abs(world.x), abs(world.y), size[1]), material)

    def quad(self, mesh, s0, s1, h0, h1, d, material):
        mesh.quad([self.point(s0, h0, d), self.point(s1, h0, d), self.point(s1, h1, d), self.point(s0, h1, d)], material)


def sides():
    w, dp = WIDTH / 2, DEPTH / 2
    return [
        Side((-w, -dp, 0), (1, 0, 0), (0, -1, 0), WIDTH, True),
        Side((w, -dp, 0), (0, 1, 0), (1, 0, 0), DEPTH, True),
        Side((w, dp, 0), (-1, 0, 0), (0, 1, 0), WIDTH, False),
        Side((-w, dp, 0), (0, -1, 0), (-1, 0, 0), DEPTH, False),
    ]


# ---------------------------------------------------------------- facade layout

class Opening:
    def __init__(self, s, bottom, width, height, kind):
        self.s, self.bottom, self.width, self.height, self.kind = s, bottom, width, height, kind

    @property
    def left(self):
        return self.s - self.width / 2

    @property
    def right(self):
        return self.s + self.width / 2

    @property
    def top(self):
        return self.bottom + self.height

    def blocks(self, s, h, margin):
        return self.left - margin <= s <= self.right + margin and self.bottom - margin - 0.12 <= h <= self.top + margin + 0.25


def openings(side):
    count = max(1, int((side.length - 1.4) // BAY))
    first = (side.length - (count - 1) * BAY) / 2
    result = []
    for bay in range(count):
        s = first + bay * BAY
        if side.has_door and bay == count // 2:
            result.append(Opening(s, 0.15, DOOR_W, DOOR_H, "door"))
        else:
            result.append(Opening(s, SILL_H, WINDOW_W, GROUND_WINDOW_H, "ground"))
        for storey in range(UPPER_STOREYS):
            result.append(Opening(s, GROUND_STOREY + storey * UPPER_STOREY + SILL_H, WINDOW_W, WINDOW_H, "upper"))
    return result


def quoin_width(h):
    course = int(h / 0.36)
    return 0.62 if course % 2 == 0 else 0.36


def is_quoin(side, s, h):
    width = quoin_width(h)
    return s < width or s > side.length - width


def patches(side):
    """Where the plaster has come away: a few ragged blobs, more of them low on the wall."""
    result = []
    for _ in range(rng.randint(3, 5)):
        result.append((rng.uniform(0.8, side.length - 0.8), rng.uniform(GROUND_STOREY + 0.3, EAVES - 1.0),
                       rng.uniform(0.35, 0.9), rng.uniform(0.25, 0.6)))
    return result


def in_patch(blobs, s, h):
    for cs, ch, rs, rh in blobs:
        ragged = 1.0 + 0.35 * math.sin(s * 9.1 + h * 5.3) * math.cos(h * 7.7 - s * 3.1)
        if ((s - cs) / rs) ** 2 + ((h - ch) / rh) ** 2 < ragged:
            return True
    return False


# ---------------------------------------------------------------- walls

def build_core(mesh):
    inner_w, inner_d = WIDTH - 2 * CORE_INSET, DEPTH - 2 * CORE_INSET
    mesh.box((0, 0, EAVES / 2), (inner_w, inner_d, EAVES), "Mortar")


def build_wall(mesh, side, holes):
    blobs = patches(side)
    plinth = 0.55
    band_top = GROUND_STOREY + 0.14

    def blocked(s, h):
        return any(o.blocks(s, h, SURROUND) for o in holes) or is_quoin(side, s, h)

    # Mortar backing where bricks show, and the brick courses themselves.
    course = 0
    h = plinth
    while h < EAVES - 0.35:
        offset = (BRICK_L / 2) if course % 2 else 0.0
        s = -offset
        while s < side.length:
            cs, ch = s + BRICK_L / 2, h + BRICK_H / 2
            ground = h < GROUND_STOREY - 0.15
            if 0 < cs < side.length and not blocked(cs, ch) and (ground or in_patch(blobs, cs, ch)) and not (band_top - 0.3 < ch < band_top):
                material = rng.choice(("BrickA", "BrickA", "BrickB", "BrickC"))
                proud = BRICK_PROUD + rng.uniform(-0.004, 0.004)
                side.box(mesh, cs, ch, proud / 2, (BRICK_L - MORTAR, BRICK_H - MORTAR, proud), material)
            s += BRICK_L
        h += BRICK_H
        course += 1

    # Plaster in horizontal runs of cells, upper storeys only, with ragged holes over the brick patches.
    rows = int((EAVES - 0.35 - band_top) / PLASTER_CELL)
    for row in range(rows):
        h0 = band_top + row * PLASTER_CELL
        hc = h0 + PLASTER_CELL / 2
        run_start = None
        cells = int(side.length / PLASTER_CELL) + 1
        for col in range(cells + 1):
            s0 = col * PLASTER_CELL
            sc = s0 + PLASTER_CELL / 2
            solid = col < cells and sc < side.length and not blocked(sc, hc) and not in_patch(blobs, sc, hc)
            if solid and run_start is None:
                run_start = s0
            if not solid and run_start is not None:
                s1 = min(s0, side.length)
                thick = PLASTER_THICK + 0.006 * math.sin(row * 1.7)
                side.box(mesh, (run_start + s1) / 2, hc, thick / 2, (s1 - run_start, PLASTER_CELL, thick), "Plaster")
                run_start = None

    build_backing(mesh, side, holes)
    build_stonework(mesh, side, plinth, band_top)


def build_backing(mesh, side, holes):
    """The mortar face behind bricks and plaster, cut around every opening so windows stay recessed."""
    xs = sorted({0.0, side.length} | {o.left for o in holes} | {o.right for o in holes})
    zs = sorted({0.0, EAVES} | {o.bottom for o in holes} | {o.top for o in holes})
    for s0, s1 in zip(xs, xs[1:]):
        for h0, h1 in zip(zs, zs[1:]):
            sc, hc = (s0 + s1) / 2, (h0 + h1) / 2
            if not any(o.left < sc < o.right and o.bottom < hc < o.top for o in holes):
                side.quad(mesh, s0, s1, h0, h1, 0.0, "Mortar")


def build_stonework(mesh, side, plinth, band_top):
    side.box(mesh, side.length / 2, plinth / 2, 0.05, (side.length + 0.1, plinth, 0.1), "Stone")
    side.box(mesh, side.length / 2, band_top - 0.14, 0.06, (side.length + 0.12, 0.28, 0.12), "Stone")
    h = plinth
    while h < EAVES - 0.4:
        width = quoin_width(h + 0.01)
        height = 0.34
        side.box(mesh, width / 2, h + height / 2, 0.035, (width, height, 0.07), "Stone")
        side.box(mesh, side.length - width / 2, h + height / 2, 0.035, (width, height, 0.07), "Stone")
        h += 0.36
    # Cornice: a stepped stone moulding under the eaves.
    for step, (height, proud) in enumerate(((0.16, 0.1), (0.1, 0.2), (0.12, 0.32))):
        bottom = EAVES - 0.38 + sum((0.16, 0.1, 0.12)[:step])
        side.box(mesh, side.length / 2, bottom + height / 2, proud / 2, (side.length + 2 * proud, height, proud), "Stone")


# ---------------------------------------------------------------- windows and doors

def build_opening(mesh, side, opening, flowers):
    # Reveal: the opening goes back into the wall, lined with plaster.
    depth = REVEAL
    side.box(mesh, opening.left - 0.02, opening.bottom + opening.height / 2, -depth / 2, (0.04, opening.height, depth), "Plaster")
    side.box(mesh, opening.right + 0.02, opening.bottom + opening.height / 2, -depth / 2, (0.04, opening.height, depth), "Plaster")
    side.box(mesh, opening.s, opening.top + 0.02, -depth / 2, (opening.width + 0.08, 0.04, depth), "Plaster")
    side.box(mesh, opening.s, opening.bottom - 0.02, -depth / 2, (opening.width + 0.08, 0.04, depth), "Stone")
    build_surround(mesh, side, opening)
    if opening.kind == "door":
        build_door(mesh, side, opening)
        return
    build_glazing(mesh, side, opening)
    if opening.kind == "upper":
        build_shutters(mesh, side, opening)
    if flowers:
        build_flower_box(mesh, side, opening)


def build_surround(mesh, side, opening):
    sw, proud = SURROUND, SURROUND_PROUD
    mid_h = opening.bottom + opening.height / 2
    side.box(mesh, opening.left - sw / 2, mid_h, proud / 2, (sw, opening.height, proud), "Stone")
    side.box(mesh, opening.right + sw / 2, mid_h, proud / 2, (sw, opening.height, proud), "Stone")
    lintel = 0.24
    side.box(mesh, opening.s, opening.top + lintel / 2, proud / 2, (opening.width + 2 * sw + 0.06, lintel, proud), "Stone")
    side.box(mesh, opening.s, opening.top + lintel / 2 + 0.02, proud, (0.2, lintel + 0.06, proud * 1.6), "Stone")
    if opening.kind != "door":
        side.box(mesh, opening.s, opening.bottom - 0.05, 0.07, (opening.width + 2 * sw + 0.12, 0.08, 0.14), "Stone")


def build_glazing(mesh, side, opening):
    back = -REVEAL + 0.06
    side.quad(mesh, opening.left, opening.right, opening.bottom, opening.top, back, "Glass")
    curtain = back - 0.05
    drape = opening.width * 0.28
    side.quad(mesh, opening.left, opening.left + drape, opening.bottom + 0.3, opening.top, curtain, "Curtain")
    side.quad(mesh, opening.right - drape, opening.right, opening.bottom + 0.3, opening.top, curtain, "Curtain")
    side.quad(mesh, opening.left, opening.right, opening.top - 0.25, opening.top, curtain + 0.001, "Curtain")
    bar, depth = 0.05, 0.05
    f = back + depth / 2
    w, h = opening.width, opening.height
    mid_h = opening.bottom + h / 2
    for s in (opening.left + bar / 2, opening.right - bar / 2, opening.s):
        side.box(mesh, s, mid_h, f, (bar, h, depth), "Frame")
    for z in (opening.bottom + bar / 2, opening.top - bar / 2, opening.top - h * 0.28):
        side.box(mesh, opening.s, z, f, (w, bar, depth), "Frame")
    for z in (opening.bottom + h * 0.36,):
        side.box(mesh, opening.s, z, f, (w, bar * 0.6, depth * 0.8), "Frame")


def build_shutters(mesh, side, opening):
    width = opening.width / 2 + 0.03
    height = opening.height + 0.05
    mid_h = opening.bottom + height / 2 - 0.025
    for sign in (-1, 1):
        edge = opening.s + sign * (opening.width / 2 + SURROUND + 0.02)
        centre = edge + sign * width / 2
        jitter = rng.uniform(-0.01, 0.01)
        side.box(mesh, centre, mid_h, 0.03 + jitter, (width, height, 0.03), "Shutter")
        rail = 0.06
        for z in (mid_h - height / 2 + rail / 2, mid_h, mid_h + height / 2 - rail / 2):
            side.box(mesh, centre, z, 0.05 + jitter, (width, rail, 0.02), "Shutter")
        for s in (centre - width / 2 + rail / 2, centre + width / 2 - rail / 2):
            side.box(mesh, s, mid_h, 0.05 + jitter, (rail, height, 0.02), "Shutter")
        slats = 9
        for half in (-1, 1):
            base = mid_h + half * height / 4
            for k in range(slats):
                z = base - height / 4 + (k + 0.5) * (height / 2 - rail) / slats
                side.box(mesh, centre, z, 0.048 + jitter, (width - 2 * rail, 0.02, 0.012), "Shutter")


def build_flower_box(mesh, side, opening):
    base = opening.bottom - 0.02
    side.box(mesh, opening.s, base + 0.1, 0.2, (opening.width + 0.1, 0.2, 0.2), "Terracotta")
    for _ in range(14):
        p = side.point(opening.s + rng.uniform(-opening.width / 2, opening.width / 2), base + 0.22 + rng.uniform(0, 0.12),
                       0.2 + rng.uniform(-0.08, 0.1))
        mesh.sphere(p, rng.uniform(0.05, 0.08), "Leaf")
    for _ in range(9):
        p = side.point(opening.s + rng.uniform(-opening.width / 2, opening.width / 2), base + 0.28 + rng.uniform(0, 0.12),
                       0.22 + rng.uniform(-0.05, 0.12))
        mesh.sphere(p, rng.uniform(0.035, 0.05), "Flower")


def build_door(mesh, side, opening):
    back = -REVEAL + 0.05
    w, h = opening.width, opening.height
    side.box(mesh, opening.s, opening.bottom + h / 2, back, (w, h, 0.06), "Wood")
    plank = w / 6
    for k in range(1, 6):
        side.box(mesh, opening.left + k * plank, opening.bottom + h / 2, back + 0.035, (0.02, h * 0.95, 0.01), "Iron")
    side.box(mesh, opening.s, opening.bottom + h * 0.6, back + 0.04, (w * 0.9, 0.06, 0.02), "Wood")
    side.box(mesh, opening.s + w * 0.3, opening.bottom + h * 0.47, back + 0.06, (0.05, 0.05, 0.05), "Iron")
    side.box(mesh, opening.s, 0.08, 0.15, (w + 0.6, 0.16, 0.3), "Stone")
    # Arched fanlight in stone above the door: a fan of voussoirs.
    radius = w / 2 + SURROUND
    top = opening.top + 0.24
    for k in range(7):
        angle = math.pi * (k + 0.5) / 7
        s = opening.s + math.cos(angle) * radius
        z = top + math.sin(angle) * radius * 0.55
        side.box(mesh, s, z, SURROUND_PROUD * 0.6, (0.22, 0.2, SURROUND_PROUD * 1.2), "Stone")


# ---------------------------------------------------------------- roof

def roof_faces():
    """Each roof face as (eave start, eave end, inward plan direction, half length at eave, run at ridge)."""
    w, d = WIDTH / 2 + OVERHANG, DEPTH / 2 + OVERHANG
    run = min(w, d)
    return [
        (Vector((-w, -d, 0)), Vector((1, 0, 0)), Vector((0, 1, 0)), 2 * w, run),
        (Vector((w, d, 0)), Vector((-1, 0, 0)), Vector((0, -1, 0)), 2 * w, run),
        (Vector((w, -d, 0)), Vector((0, 1, 0)), Vector((-1, 0, 0)), 2 * d, run),
        (Vector((-w, d, 0)), Vector((0, -1, 0)), Vector((1, 0, 0)), 2 * d, run),
    ]


def build_roof(mesh):
    base = EAVES + 0.02
    rise_per_run = math.tan(ROOF_PITCH)
    slope_per_run = 1 / math.cos(ROOF_PITCH)
    for start, along, inward, length, run in roof_faces():
        up = Vector((0, 0, 1))

        def at(u, p, lift=0.0):
            return start + along * u + inward * p + up * (base + p * rise_per_run) + normal * lift

        slope = (inward + up * rise_per_run).normalized()
        normal = along.cross(slope).normalized()
        if normal.z < 0:
            normal = -normal
        # The roof deck under the tiles.
        mesh.quad([at(0, 0), at(length, 0), at(length - run, run), at(run, run)], "RoofUnder")
        rows = int(run * slope_per_run / TILE_ROW)
        for row in range(rows):
            p = (row * TILE_ROW + TILE_ROW / 2) / slope_per_run
            u0, u1 = p + 0.05, length - p - 0.05
            count = int((u1 - u0) / TILE_W)
            if count <= 0:
                continue
            margin = (u1 - u0 - count * TILE_W) / 2
            for k in range(count):
                u = u0 + margin + (k + 0.5) * TILE_W
                build_tile(mesh, at, slope, normal, along, u, p, (row + k) % 2 == 0)
        build_hip(mesh, at(0, 0), at(run, run))
        build_hip(mesh, at(length, 0), at(length - run, run))
        gutter_start = at(0, 0) + Vector((0, 0, -0.05)) - inward * 0.05
        gutter_end = at(length, 0) + Vector((0, 0, -0.05)) - inward * 0.05
        mesh.cylinder(gutter_start, gutter_end, 0.075, "Zinc", segments=8)
    ridge_w = WIDTH / 2 + OVERHANG - min(WIDTH, DEPTH) / 2 - OVERHANG
    ridge_h = base + (min(WIDTH, DEPTH) / 2 + OVERHANG) * rise_per_run
    build_hip(mesh, Vector((-ridge_w, 0, ridge_h)), Vector((ridge_w, 0, ridge_h)))


def build_tile(mesh, at, slope, normal, along, u, p, is_pan):
    """A canal tile: a half pipe along the slope, pans open upwards, caps over the joints."""
    length = TILE_ROW * 1.35
    segments = 6
    jitter = rng.uniform(-0.012, 0.012)
    material = rng.choice(("RoofTileA", "RoofTileA", "RoofTileB", "RoofTileC"))
    radius = TILE_R if is_pan else TILE_R * 0.85
    centre = at(u, p, 0.06 + jitter if not is_pan else 0.02 + jitter)
    ring = []
    for k in range(segments + 1):
        a = math.pi * k / segments
        offset = along * (math.cos(a) * radius) + normal * (math.sin(a) * radius * (1 if not is_pan else -0.55))
        ring.append(offset)
    tail = slope * (-length / 2)
    head = slope * (length / 2)
    for k in range(segments):
        mesh.quad([centre + tail + ring[k], centre + tail + ring[k + 1], centre + head + ring[k + 1], centre + head + ring[k]], material)


def build_hip(mesh, a, b):
    length = (b - a).length
    pieces = max(1, int(length / 0.38))
    for k in range(pieces):
        s0 = a + (b - a) * (k / pieces)
        s1 = a + (b - a) * ((k + 0.95) / pieces)
        mesh.cylinder(s0 + Vector((0, 0, 0.1)), s1 + Vector((0, 0, 0.1)), 0.1, "Stone", segments=8)


def build_chimney(mesh):
    rise = math.tan(ROOF_PITCH)
    x, y = WIDTH * 0.18, DEPTH * 0.1
    run = min(WIDTH, DEPTH) / 2 + OVERHANG
    ridge_h = EAVES + run * rise
    bottom = EAVES + 1.0
    top = ridge_h + 0.7
    mesh.box((x, y, (bottom + top) / 2), (1.3, 0.85, top - bottom), "Stone")
    mesh.box((x, y, top + 0.06), (1.45, 1.0, 0.12), "Stone")
    for dx in (-0.36, 0.0, 0.36):
        mesh.cylinder((x + dx, y, top + 0.1), (x + dx, y, top + 0.5), 0.1, "Terracotta", segments=10)
        mesh.cylinder((x + dx, y, top + 0.46), (x + dx, y, top + 0.52), 0.12, "Terracotta", segments=10)


def build_drainpipes(mesh):
    w, d = WIDTH / 2 + OVERHANG * 0.6, DEPTH / 2 + 0.12
    for x in (-w + 0.9, w - 0.9):
        mesh.cylinder((x, -d, 0.1), (x, -d, EAVES - 0.05), 0.055, "Zinc", segments=8)
        for z in (2.0, 4.6, 7.9):
            mesh.box((x, -d + 0.06, z), (0.16, 0.12, 0.05), "Iron")


def build_lantern(mesh):
    x, y, z = WIDTH / 2 + 0.05, -DEPTH / 2 + 1.6, 3.0
    mesh.box((x + 0.25, y, z + 0.35), (0.5, 0.04, 0.04), "Iron")
    mesh.box((x + 0.45, y, z), (0.22, 0.22, 0.32), "Glass")
    mesh.box((x + 0.45, y, z + 0.2), (0.28, 0.28, 0.06), "Iron")


# ---------------------------------------------------------------- scene

def make_materials():
    for name, colour in MATERIALS.items():
        material = bpy.data.materials.new(name)
        material.diffuse_color = (*colour, 1.0)
        material.use_nodes = True
        bsdf = material.node_tree.nodes.get("Principled BSDF")
        bsdf.inputs["Base Color"].default_value = (*colour, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.15 if name == "Glass" else (0.45 if name in ("Zinc", "Iron") else 0.85)
        if name in ("Zinc",):
            bsdf.inputs["Metallic"].default_value = 0.8


def build_house():
    stone, detail, roof = Mesh(), Mesh(), Mesh()
    build_core(detail)
    for index, side in enumerate(sides()):
        holes = openings(side)
        build_wall(detail, side, holes)
        for k, opening in enumerate(holes):
            flowers = opening.kind == "upper" and (k + index) % 3 != 0
            build_opening(detail, side, opening, flowers)
    build_roof(roof)
    build_chimney(stone)
    build_drainpipes(detail)
    build_lantern(detail)
    parts = [stone.to_object("Stone", bevel=0.02), detail.to_object("Detail"), roof.to_object("Roof")]
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        bpy.context.view_layer.objects.active = part
        part.select_set(True)
        for modifier in list(part.modifiers):
            bpy.ops.object.modifier_apply(modifier=modifier.name)
        part.select_set(False)
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[1]
    bpy.ops.object.join()
    house = bpy.context.view_layer.objects.active
    house.name = "CornerHouse"
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.remove_doubles(threshold=0.0005)
    bpy.ops.object.mode_set(mode="OBJECT")
    return house


def export(house):
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    house.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=str(OUTPUT),
        use_selection=True,
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        mesh_smooth_type="FACE",
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        path_mode="STRIP",
    )
    print(f"Exported {OUTPUT} with {len(house.data.polygons)} faces")


def render_preview(path):
    scene = bpy.context.scene
    ground = bpy.data.materials.new("Cobble")
    ground.use_nodes = True
    ground.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (0.32, 0.29, 0.25, 1)
    bpy.ops.mesh.primitive_plane_add(size=60)
    bpy.context.active_object.data.materials.append(ground)
    bpy.ops.object.camera_add(location=(21, -30, 26))
    camera = bpy.context.active_object
    direction = Vector((0, 0, 5)) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    camera.data.lens = 40
    scene.camera = camera
    bpy.ops.object.light_add(type="SUN")
    sun = bpy.context.active_object
    sun.data.energy = 4.5
    sun.data.color = (1.0, 0.78, 0.55)
    sun.rotation_euler = (math.radians(62), 0, math.radians(-38))
    world = bpy.data.worlds.new("Sky")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.35, 0.33, 0.32, 1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.6
    scene.world = world
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 48
    scene.cycles.use_denoising = True
    scene.render.resolution_x = 1600
    scene.render.resolution_y = 900
    scene.view_settings.view_transform = "AgX"
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    print(f"Preview written to {path}")


def main():
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    bpy.ops.wm.read_factory_settings(use_empty=True)
    make_materials()
    house = build_house()
    export(house)
    if "--preview" in args:
        render_preview(args[args.index("--preview") + 1])


main()
