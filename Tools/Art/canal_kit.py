"""Shared building kit for the canal-town models (docs/CanalTownArtList.md), generalised from build_house_1.py.

Each build_<building>.py script describes its building as one or more `Block`s (a box of storeys with openings on
its four facades) plus a roof, and calls `run(name, build)`. Run a script headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_row_house.py -- [--preview <folder>]

Conventions (Blender space, metres): origin at the bottom centre of the footprint, Z up, the front facade faces -Y.
Every model uses the same material slot names (MATERIALS); Unity remaps them to HDRP materials in
CanalTownBuildingAssets, so keep the two lists in step.
"""
import math
import random
import sys
from pathlib import Path

import bmesh
import bpy
from mathutils import Matrix, Vector

MODEL_ROOT = Path("Assets/_Project/Art/Models")
BLEND_ROOT = Path("Tools/Art/Blend")
TEXTURE_ROOT = Path("Assets/_Project/Art/Textures")

MATERIALS = {
    "PlasterOchre": (0.72, 0.52, 0.3),
    "PlasterCream": (0.78, 0.7, 0.56),
    "PlasterRose": (0.74, 0.5, 0.42),
    "PlasterTerracotta": (0.66, 0.36, 0.22),
    "PlasterSand": (0.76, 0.64, 0.46),
    "Ashlar": (0.9, 0.86, 0.78),
    "Rubble": (0.7, 0.64, 0.55),
    "Brick": (0.9, 0.88, 0.86),
    "Mortar": (0.6, 0.55, 0.47),
    "Stone": (0.95, 0.93, 0.88),
    "StoneDark": (0.84, 0.8, 0.72),
    "StoneGrime": (0.7, 0.65, 0.56),
    "Moss": (0.3, 0.32, 0.17),
    "RoofTileA": (0.6, 0.24, 0.12),
    "RoofTileB": (0.68, 0.32, 0.17),
    "RoofTileC": (0.5, 0.2, 0.11),
    "TileDark": (0.36, 0.14, 0.08),
    "TileLichen": (0.44, 0.32, 0.21),
    "RoofUnder": (0.25, 0.1, 0.06),
    "Zinc": (0.55, 0.57, 0.58),
    "Glass": (0.04, 0.05, 0.06),
    "Frame": (0.78, 0.75, 0.68),
    "Shutter": (0.27, 0.3, 0.14),
    "ShutterFaded": (0.38, 0.4, 0.24),
    "ShutterBlue": (0.2, 0.3, 0.36),
    "Curtain": (0.75, 0.72, 0.64),
    "Wood": (0.16, 0.09, 0.05),
    "Timber": (0.32, 0.22, 0.13),
    "Iron": (0.05, 0.05, 0.05),
    "Bronze": (0.36, 0.26, 0.12),
    "Terracotta": (0.55, 0.32, 0.18),
    "CanvasA": (0.7, 0.66, 0.56),
    "CanvasB": (0.5, 0.16, 0.12),
    "Shade": (0.06, 0.05, 0.04),
    "FlowerRed": (0.7, 0.04, 0.04),
    "FlowerPink": (0.8, 0.35, 0.45),
    "FlowerWhite": (0.8, 0.8, 0.76),
    "Leaf": (0.12, 0.25, 0.07),
    "Plaque": (0.8, 0.79, 0.75),
    "LampGlow": (1.0, 0.7, 0.35),
    "Straw": (0.62, 0.52, 0.3),
    "Cobble": (0.85, 0.82, 0.76),
    "Rope": (0.55, 0.45, 0.3),
    "BoatPaint": (0.2, 0.32, 0.3),
    "Reed": (0.35, 0.4, 0.18),
    "ReedHead": (0.3, 0.18, 0.08),
    "LilyPad": (0.15, 0.3, 0.1),
    "Water": (0.08, 0.12, 0.1),
    "Olive": (0.3, 0.36, 0.22),
    "Apple": (0.6, 0.08, 0.05),
    "Orange": (0.8, 0.4, 0.05),
    "Greens": (0.2, 0.4, 0.1),
    "Bark": (0.4, 0.37, 0.3),
    "BarkDark": (0.22, 0.18, 0.14),
    "Foliage": (0.18, 0.3, 0.08),
    "FoliageLight": (0.3, 0.4, 0.12),
    "FoliageAutumn": (0.55, 0.42, 0.1),
    "PineNeedles": (0.12, 0.22, 0.08),
    "Hedge": (0.1, 0.22, 0.06),
    "Wheat": (0.72, 0.6, 0.3),
    "Hay": (0.66, 0.56, 0.3),
    "Wool": (0.8, 0.77, 0.7),
    "Linen": (0.82, 0.8, 0.74),
    "LinenBlue": (0.3, 0.4, 0.55),
    "Soil": (0.25, 0.18, 0.12),
    "Grass": (0.2, 0.32, 0.1),
    "Gravel": (0.62, 0.58, 0.5),
}
# Scanned CC0 textures (Poly Haven) under Assets/_Project/Art/Textures, with the metres one tile covers.
# Unity applies the same sets; here they only make the .blend and the previews honest.
TEXTURES = {
    "Brick": ("red_brick", 1.4),
    "Ashlar": ("beige_wall_001", 2.0),
    "Rubble": ("beige_wall_001", 1.2),
    "Stone": ("beige_wall_001", 2.0),
    "StoneDark": ("beige_wall_001", 2.0),
    "StoneGrime": ("beige_wall_001", 2.0),
    "Cobble": ("cobblestone_pavement", 2.0),
}
MATERIAL_INDEX = {name: index for index, name in enumerate(MATERIALS)}

BEVEL_WIDTH = 0.012
BEVEL_ANGLE = math.radians(50)  # bevels boxes, leaves the rounder leaves, flowers and pipes alone
STONE_JITTER = 0.012
CORE_INSET = 0.26
PLINTH_PROUD = 0.06
SURROUND_PROUD = 0.05
REVEAL = 0.22
ARCH_THICK = 0.3
QUOIN_COURSE = 0.34
CORNICE = 0.3
TILE_W = 0.2
TILE_ROW = 0.28
TILE_R = 0.115

FRONT, RIGHT, BACK, LEFT = range(4)

rng = random.Random(1)


# ---------------------------------------------------------------- mesh helpers

class Mesh:
    """One bmesh with a material index per face."""

    def __init__(self):
        self.bm = bmesh.new()

    def poly(self, corners, material, facing):
        """A single-sided face, flipped if needed so its visible side looks along `facing` (Unity culls back faces)."""
        face = self.bm.faces.new([self.bm.verts.new(c) for c in corners])
        face.material_index = MATERIAL_INDEX[material]
        face.normal_update()
        if face.normal.dot(Vector(facing)) < 0:
            face.normal_flip()

    def two_sided(self, corners, material, facing):
        """Front and back faces 1 mm apart, so neither renderer nor Unity sees two faces in one plane."""
        offset = Vector(facing).normalized() * 0.0005
        self.poly([Vector(c) + offset for c in corners], material, facing)
        self.poly([Vector(c) - offset for c in reversed(corners)], material, -Vector(facing))

    def hexahedron(self, front, back, material):
        """Two matching quads (counter-clockwise seen from the front) joined into a closed block."""
        v = [self.bm.verts.new(c) for c in list(front) + list(back)]
        middle = sum((x.co for x in v), Vector()) / 8
        for ids in ((0, 1, 2, 3), (7, 6, 5, 4), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)):
            face = self.bm.faces.new([v[i] for i in ids])
            face.material_index = MATERIAL_INDEX[material]
            face.normal_update()
            centre = sum((v[i].co for i in ids), Vector()) / 4
            if face.normal.dot(centre - middle) < 0:
                face.normal_flip()

    def box(self, centre, size, material):
        c = Vector(centre)
        h = Vector(size) / 2
        corner = lambda sx, sy, sz: c + Vector((sx * h.x, sy * h.y, sz * h.z))
        front = [corner(-1, -1, -1), corner(1, -1, -1), corner(1, -1, 1), corner(-1, -1, 1)]
        back = [corner(-1, 1, -1), corner(1, 1, -1), corner(1, 1, 1), corner(-1, 1, 1)]
        self.hexahedron(front, back, material)

    def beam(self, start, end, thickness, material):
        """A square-section box from start to end, for timbers and rails at any angle."""
        start, end = Vector(start), Vector(end)
        axis = (end - start).normalized()
        side = axis.cross(Vector((0, 0, 1)))
        if side.length < 1e-4:
            side = Vector((1, 0, 0))
        side = side.normalized() * thickness / 2
        up = side.cross(axis).normalized() * thickness / 2
        ring = lambda p: [p - side - up, p + side - up, p + side + up, p - side + up]
        self.hexahedron(ring(start), ring(end), material)

    def cylinder(self, start, end, radius, material, segments=8, radius_end=None):
        start, end = Vector(start), Vector(end)
        axis = end - start
        rotation = axis.normalized().to_track_quat("Z", "Y").to_matrix().to_4x4()
        matrix = Matrix.Translation((start + end) / 2) @ rotation
        end_radius = radius if radius_end is None else radius_end
        result = bmesh.ops.create_cone(self.bm, cap_ends=True, segments=segments, radius1=radius, radius2=end_radius,
                                       depth=axis.length, matrix=matrix)
        self._paint(result["verts"], material)

    def sphere(self, centre, radius, material, subdivisions=1):
        result = bmesh.ops.create_icosphere(self.bm, subdivisions=subdivisions, radius=radius,
                                            matrix=Matrix.Translation(centre))
        self._paint(result["verts"], material)

    def _paint(self, verts, material):
        for face in {f for v in verts for f in v.link_faces}:
            face.material_index = MATERIAL_INDEX[material]

    def to_object(self, name, bevel=0.0):
        mesh = bpy.data.meshes.new(name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(name, mesh)
        bpy.context.collection.objects.link(obj)
        add_material_slots(obj)
        if bevel > 0.0:
            modifier = obj.modifiers.new("Bevel", "BEVEL")
            modifier.width = bevel
            modifier.segments = 2
            modifier.limit_method = "ANGLE"
            modifier.angle_limit = BEVEL_ANGLE
            modifier.use_clamp_overlap = True
        return obj


def add_material_slots(obj):
    for material in MATERIALS:
        obj.data.materials.append(bpy.data.materials[material])


class Side:
    """A facade in its own frame: s runs along the wall from its left corner, h is height, d sticks outwards."""

    def __init__(self, index, origin, along, outward, length):
        self.index = index
        self.origin = Vector(origin)
        self.along = Vector(along)
        self.outward = Vector(outward)
        self.length = length

    def point(self, s, h, d):
        return self.origin + self.along * s + self.outward * d + Vector((0, 0, h))

    def box(self, mesh, s, h, d, size, material):
        """size = (along, up, out) around the local centre (s, h, d)."""
        world = self.along * size[0] + self.outward * size[2]
        mesh.box(self.point(s, h, d), (abs(world.x), abs(world.y), size[1]), material)

    def quad(self, mesh, s0, s1, h0, h1, d, material, inward=False):
        facing = -self.outward if inward else self.outward
        mesh.poly([self.point(s0, h0, d), self.point(s1, h0, d), self.point(s1, h1, d), self.point(s0, h1, d)], material, facing)

    def block(self, mesh, outline, d0, d1, material):
        """Extrude a four-point (s, h) outline from depth d0 to d1."""
        front = [self.point(s, h, d1) for s, h in outline]
        back = [self.point(s, h, d0) for s, h in outline]
        mesh.hexahedron(front, back, material)

    def yaw(self):
        return math.atan2(self.outward.y, self.outward.x) + math.radians(90)


# ---------------------------------------------------------------- openings

class Opening:
    """A hole in a facade. kind: window, door, gate, shop, arcade, belfry or slit; rise 0 gives a flat stone lintel."""

    def __init__(self, s, bottom, width, height, kind="window", rise=0.22, **extras):
        self.s = s
        self.bottom = bottom
        self.width = width
        self.height = height
        self.kind = kind
        self.rise = min(rise, width / 2)
        self.flowers = extras.get("flowers", False)
        self.shutters = extras.get("shutters", kind == "window")
        self.balcony = extras.get("balcony", False)
        self.awning = extras.get("awning", False)
        self.paint = extras.get("paint", "Shutter")
        self.depth = extras.get("depth", REVEAL)
        if self.is_arched:
            half = width / 2
            self.radius = (half * half + self.rise * self.rise) / (2 * self.rise)
            self.centre_h = self.spring + self.rise - self.radius

    left = property(lambda self: self.s - self.width / 2)
    right = property(lambda self: self.s + self.width / 2)
    spring = property(lambda self: self.bottom + self.height)
    top = property(lambda self: self.spring + self.rise if self.is_arched else self.spring)
    is_arched = property(lambda self: self.rise > 0.01)

    def arc(self, radius, count):
        """Points from the left spring to the right spring on a circle around the arch centre."""
        half_angle = math.asin(min(1.0, (self.width / 2) / self.radius))
        if self.rise >= self.width / 2 - 1e-6:
            half_angle = math.pi / 2
        start, end = math.pi / 2 + half_angle, math.pi / 2 - half_angle
        result = []
        for k in range(count + 1):
            a = start + (end - start) * k / count
            result.append((self.s + math.cos(a) * radius, self.centre_h + math.sin(a) * radius, a))
        return result

    def outline(self, count=10):
        """Counter-clockwise (s, h) outline of the hole."""
        bottom = [(self.left, self.bottom), (self.right, self.bottom)]
        if not self.is_arched:
            return bottom + [(self.right, self.spring), (self.left, self.spring)]
        return bottom + [(s, h) for s, h, _ in reversed(self.arc(self.radius, count))]

    def jamb_courses(self):
        """(bottom, top, width) of the alternating stone blocks up each jamb."""
        count = max(2, round(self.height / 0.32))
        step = self.height / count
        return [(self.bottom + k * step, self.bottom + (k + 1) * step, 0.2 if k % 2 == 0 else 0.32) for k in range(count)]


class RoundWindow:
    """A rose or oculus window: a circle of glass with radial tracery inside a ring of voussoirs."""

    def __init__(self, s, centre_h, radius, spokes=12):
        self.s = s
        self.centre_h = centre_h
        self.radius = radius
        self.spokes = spokes
        self.kind = "rose"

    left = property(lambda self: self.s - self.radius)
    right = property(lambda self: self.s + self.radius)
    bottom = property(lambda self: self.centre_h - self.radius)
    top = property(lambda self: self.centre_h + self.radius)

    def ring(self, radius, count):
        return [(self.s + math.cos(2 * math.pi * k / count) * radius, self.centre_h + math.sin(2 * math.pi * k / count) * radius)
                for k in range(count + 1)]


# ---------------------------------------------------------------- blocks

class Block:
    """A box of storeys: walls with openings on its four facades, a stone plinth, quoins and a cornice.

    `surround` is "stone" for rusticated jambs and voussoirs (as on House 1) or "band" for a plain stone frame.
    Sides listed in `hidden` are left out (they stand against another block).
    """

    def __init__(self, centre, width, depth, storeys, wall, **style):
        self.cx, self.cy = centre
        self.width = width
        self.depth = depth
        self.storeys = list(storeys)
        self.wall = wall
        self.base = style.get("base", 0.0)
        self.plinth = style.get("plinth", 0.7)
        self.has_quoins = style.get("quoins", True)
        self.has_cornice = style.get("cornice", True)
        self.surround = style.get("surround", "band")
        self.hidden = set(style.get("hidden", ()))
        self.hollow_from = style.get("hollow_from")  # storey index from which the block is open inside (belfries)
        self.wall_thickness = style.get("wall_thickness", 0.5)
        self.front_recess = style.get("front_recess", 0.0)  # pulls the solid core back from the front for loggias
        self.openings = {index: [] for index in range(4)}

    @property
    def eaves(self):
        return self.base + sum(self.storeys)

    def floor(self, storey):
        """Height of a storey's floor above the block's base (facade-local, like opening heights)."""
        return sum(self.storeys[:storey])

    def sides(self):
        w, d = self.width / 2, self.depth / 2
        x, y, z = self.cx, self.cy, self.base
        return [
            Side(FRONT, (x - w, y - d, z), (1, 0, 0), (0, -1, 0), self.width),
            Side(RIGHT, (x + w, y - d, z), (0, 1, 0), (1, 0, 0), self.depth),
            Side(BACK, (x + w, y + d, z), (-1, 0, 0), (0, 1, 0), self.width),
            Side(LEFT, (x - w, y + d, z), (0, -1, 0), (-1, 0, 0), self.depth),
        ]

    def side(self, index):
        return self.sides()[index]

    def length(self, index):
        return self.width if index in (FRONT, BACK) else self.depth

    def add(self, index, opening):
        self.openings[index].append(opening)
        return opening

    def windows(self, index, fractions, storey, **options):
        """One window per bay fraction on a storey, sized for an upper floor unless overridden."""
        width = options.pop("width", 0.95)
        height = options.pop("height", 1.5)
        sill = options.pop("sill", 0.9)
        for fraction in fractions:
            self.add(index, Opening(self.length(index) * fraction, self.floor(storey) + sill, width, height, **options))


# ---------------------------------------------------------------- walls

def lowness(h):
    """1 at the ground fading to 0 at 3 m: splash-back, damp and moss gather low on the wall."""
    return max(0.0, 1.0 - h / 3.0)


def stone_material(h):
    roll = rng.random()
    if roll < 0.4 * lowness(h):
        return "Moss"
    if roll < 0.25 + 0.35 * lowness(h):
        return "StoneGrime"
    return "Stone" if rng.random() > 0.3 else "StoneDark"


def owns_corners(side):
    """Front and back run past the corners; the sides stop flush so the corner blocks don't z-fight."""
    return side.index in (FRONT, BACK)


def corner_overrun(side, proud):
    return proud if owns_corners(side) else 0.0


def is_plaster(material):
    return material.startswith("Plaster")


def build_block(stone, detail, block):
    if block.hollow_from is None:
        build_core(detail, block, block.eaves - block.base)
    else:
        build_belfry_floor(detail, block)
    for side in block.sides():
        if side.index in block.hidden:
            continue
        holes = block.openings[side.index]
        build_backing(detail, block, side, holes)
        if block.plinth > 0:
            build_plinth(detail, block, side, holes)
        if block.has_quoins:
            build_quoins(detail, block, side)
        if block.has_cornice:
            build_cornice(detail, side, block.eaves - block.base)
        if is_plaster(block.wall):
            build_fallen_plaster(detail, block, side, holes)
        for opening in holes:
            build_opening(stone if opening.kind in ("door", "gate") else detail, block, side, opening)


def build_core(mesh, block, height):
    inset = 2 * CORE_INSET
    recess = block.front_recess
    mesh.box((block.cx, block.cy + recess / 2, block.base + height / 2), (block.width - inset, block.depth - inset - recess, height),
             "Mortar")


def build_belfry_floor(mesh, block):
    """Solid below the hollow storey, an open room above with a floor and a ceiling under the roof."""
    floor = block.floor(block.hollow_from)
    build_core(mesh, block, floor)
    inner_w, inner_d = block.width - 2 * block.wall_thickness, block.depth - 2 * block.wall_thickness
    mesh.box((block.cx, block.cy, block.base + floor + 0.05), (inner_w, inner_d, 0.1), "StoneDark")
    mesh.box((block.cx, block.cy, block.eaves - 0.1), (inner_w, inner_d, 0.2), "Timber")


def cut_cells(side, holes, h_min, h_max, s_min=0.0, s_max=None):
    """Wall rectangles left over once every opening's bounding box is cut out."""
    s_max = side.length if s_max is None else s_max
    xs = sorted({s_min, s_max} | {o.left for o in holes} | {o.right for o in holes})
    zs = sorted({h_min, h_max} | {max(h_min, min(h_max, o.bottom)) for o in holes} | {max(h_min, min(h_max, o.top)) for o in holes})
    for s0, s1 in zip(xs, xs[1:]):
        for h0, h1 in zip(zs, zs[1:]):
            sc, hc = (s0 + s1) / 2, (h0 + h1) / 2
            if s1 - s0 > 1e-4 and h1 - h0 > 1e-4 and not any(o.left < sc < o.right and o.bottom < hc < o.top for o in holes):
                yield s0, s1, h0, h1


def build_backing(mesh, block, side, holes):
    """The wall face, cut around every opening so windows stay recessed; arched and round heads are filled around."""
    height = block.eaves - block.base
    for s0, s1, h0, h1 in cut_cells(side, holes, 0.0, height):
        side.quad(mesh, s0, s1, h0, h1, 0.0, block.wall)
    if block.hollow_from is not None:
        inner = -block.wall_thickness
        span = (block.wall_thickness, side.length - block.wall_thickness)
        for s0, s1, h0, h1 in cut_cells(side, holes, block.floor(block.hollow_from), height, *span):
            side.quad(mesh, s0, s1, h0, h1, inner, block.wall, inward=True)
    for opening in holes:
        build_head_fill(mesh, block, side, opening)


def build_head_fill(mesh, block, side, opening):
    """Wall triangles between an arched or round hole and its bounding box."""
    if isinstance(opening, RoundWindow):
        ring = opening.ring(opening.radius, 32)
        corners = [(opening.right, opening.top), (opening.left, opening.top), (opening.left, opening.bottom),
                   (opening.right, opening.bottom)]
        for k, (a, b) in enumerate(zip(ring, ring[1:])):
            corner = corners[k * 4 // 32]
            mesh.poly([side.point(*corner, 0.0), side.point(*a, 0.0), side.point(*b, 0.0)], block.wall, side.outward)
        return
    if not opening.is_arched:
        return
    arc = opening.arc(opening.radius, 10)
    for (s0, h0, _), (s1, h1, _) in zip(arc, arc[1:]):
        corner = (opening.left, opening.top) if (s0 + s1) / 2 < opening.s else (opening.right, opening.top)
        mesh.poly([side.point(*corner, 0.0), side.point(s0, h0, 0.0), side.point(s1, h1, 0.0)], block.wall, side.outward)


def build_plinth(mesh, block, side, holes):
    doors = [o for o in holes if o.kind in ("door", "gate", "shop", "arcade")]
    courses = (0.0, block.plinth * 0.55, block.plinth)
    overrun = corner_overrun(side, PLINTH_PROUD)
    for row, (h0, h1) in enumerate(zip(courses, courses[1:])):
        s = -overrun - (0.25 if row % 2 else 0.0)
        while s < side.length + overrun:
            length = rng.uniform(0.45, 0.85)
            s0, s1 = max(s, -overrun), min(s + length, side.length + overrun)
            s += length
            if any(d.left - 0.02 < (s0 + s1) / 2 < d.right + 0.02 for d in doors):
                continue
            proud = PLINTH_PROUD + rng.uniform(-STONE_JITTER, STONE_JITTER) * 0.5
            side.box(mesh, (s0 + s1) / 2, (h0 + h1) / 2, proud / 2, (s1 - s0 - 0.015, h1 - h0 - 0.015, proud),
                     stone_material((h0 + h1) / 2))


def build_quoins(mesh, block, side):
    course, h = 0, block.plinth
    top = block.eaves - block.base - (CORNICE if block.has_cornice else 0.0)
    overrun = corner_overrun(side, SURROUND_PROUD)
    while h + QUOIN_COURSE <= top + 0.01:
        # Offsetting the course by side alternates long and short stones around each corner.
        width = 0.6 if (course + side.index) % 2 == 0 else 0.34
        height = QUOIN_COURSE - 0.015
        for s in (width / 2 - overrun, side.length - width / 2 + overrun):
            proud = SURROUND_PROUD + rng.uniform(-STONE_JITTER, STONE_JITTER)
            side.box(mesh, s, h + height / 2, proud / 2, (width + rng.uniform(-0.03, 0.0), height, proud), stone_material(h))
        h += QUOIN_COURSE
        course += 1


def build_cornice(mesh, side, eaves):
    bottom = eaves - CORNICE
    for height, proud in ((0.12, 0.06), (0.08, 0.12), (0.1, 0.2)):
        length = side.length + 2 * corner_overrun(side, proud)
        side.box(mesh, side.length / 2, bottom + height / 2, proud / 2, (length, height, proud), "Stone")
        bottom += height


def build_string_course(mesh, side, h):
    """A thin stone band between storeys."""
    side.box(mesh, side.length / 2, h, 0.04, (side.length + 2 * corner_overrun(side, 0.08), 0.14, 0.08), "Stone")


def build_fallen_plaster(mesh, block, side, holes):
    """Patches where the plaster has fallen off and the brick shows, as in the concept art."""
    height = block.eaves - block.base
    for _ in range(int(side.length * height / 14)):
        w, h = rng.uniform(0.4, 1.3), rng.uniform(0.3, 0.9)
        s = rng.uniform(0.5, side.length - 0.5 - w)
        z = rng.uniform(block.plinth + 0.1, height - CORNICE - 0.2 - h)
        if any(o.left - 0.45 < s + w and s < o.right + 0.45 and o.bottom - 0.4 < z + h and z < o.top + 0.45 for o in holes):
            continue
        side.quad(mesh, s, s + w, z, z + h, 0.006, "Brick")


# ---------------------------------------------------------------- windows and doors

def build_opening(mesh, block, side, opening):
    if isinstance(opening, RoundWindow):
        build_rose(mesh, side, opening)
        return
    build_reveal(mesh, block, side, opening)
    if block.surround == "stone":
        build_stone_surround(mesh, side, opening)
    else:
        build_band_surround(mesh, side, opening)
    builders = {"window": build_window, "slit": build_slit, "door": build_door, "gate": build_gate, "shop": build_shopfront,
                "arcade": build_arcade_back, "belfry": lambda m, b, s, o: None}
    builders[opening.kind](mesh, block, side, opening)
    if opening.balcony:
        build_balcony(mesh, side, opening)
    if opening.awning:
        build_awning(mesh, side, opening.s, opening.width + 0.6, opening.top + 0.35)


def build_reveal(mesh, block, side, opening):
    """The opening goes back into the wall; its sides and head are lined with stone."""
    depth = block.wall_thickness if opening.kind == "belfry" else opening.depth
    mid = opening.bottom + opening.height / 2
    for s in (opening.left - 0.02, opening.right + 0.02):
        side.box(mesh, s, mid, -depth / 2, (0.04, opening.height, depth), "StoneDark")
    if opening.is_arched:
        arc = opening.arc(opening.radius, 10)
        for (s0, h0, _), (s1, h1, _) in zip(arc, arc[1:]):
            mesh.poly([side.point(s0, h0, 0.0), side.point(s1, h1, 0.0), side.point(s1, h1, -depth), side.point(s0, h0, -depth)],
                      "StoneDark", side.point(opening.s, opening.centre_h, 0.0) - side.point(s0, h0, 0.0))
    else:
        side.box(mesh, opening.s, opening.top + 0.02, -depth / 2, (opening.width + 0.04, 0.04, depth), "StoneDark")
    if opening.kind in ("window", "slit", "belfry"):
        side.box(mesh, opening.s, opening.bottom - 0.02, -depth / 2, (opening.width + 0.04, 0.04, depth), "Stone")


def build_stone_surround(mesh, side, opening):
    for bottom, top, width in opening.jamb_courses():
        height = top - bottom - 0.012
        material = stone_material(bottom)
        for edge, sign in ((opening.left, -1), (opening.right, 1)):
            side.box(mesh, edge + sign * width / 2, bottom + 0.006 + height / 2, SURROUND_PROUD / 2,
                     (width, height, SURROUND_PROUD), material)
    if opening.is_arched:
        build_arch(mesh, side, opening)
    else:
        build_lintel(mesh, side, opening, 0.32)
    build_sill(mesh, side, opening)


def build_band_surround(mesh, side, opening):
    band = 0.14
    for edge, sign in ((opening.left, -1), (opening.right, 1)):
        side.box(mesh, edge + sign * band / 2, opening.bottom + opening.height / 2, 0.02, (band, opening.height, 0.04), "Stone")
    if opening.is_arched:
        arc_in, arc_out = opening.arc(opening.radius, 10), opening.arc(opening.radius + band, 10)
        for k in range(10):
            outline = [arc_in[k][:2], arc_in[k + 1][:2], arc_out[k + 1][:2], arc_out[k][:2]]
            side.block(mesh, outline, 0.0, 0.04, "Stone")
    else:
        build_lintel(mesh, side, opening, 0.2)
    build_sill(mesh, side, opening)


def build_lintel(mesh, side, opening, height):
    side.box(mesh, opening.s, opening.top + height / 2, SURROUND_PROUD / 2, (opening.width + 0.5, height, SURROUND_PROUD), "Stone")
    side.box(mesh, opening.s, opening.top + height / 2 + 0.03, SURROUND_PROUD, (0.24, height + 0.06, SURROUND_PROUD * 2), "Stone")


def build_sill(mesh, side, opening):
    if opening.kind not in ("window", "slit"):
        return
    side.box(mesh, opening.s, opening.bottom - 0.07, 0.07, (opening.width + 0.4, 0.12, 0.14), "Stone")
    side.box(mesh, opening.s, opening.bottom - 0.16, 0.035, (opening.width + 0.28, 0.06, 0.07), "StoneDark")


def build_arch(mesh, side, opening):
    """Voussoirs fanned around the arch with a taller, prouder keystone in the middle."""
    count = 9 if opening.width > 1.4 else 7
    inner = opening.arc(opening.radius, count)
    outer = opening.arc(opening.radius + ARCH_THICK, count)
    for k in range(count):
        is_key = k == count // 2
        lift = 0.1 if is_key else 0.0
        proud = SURROUND_PROUD * (1.8 if is_key else 1.0)
        (s0, h0, a0), (s1, h1, a1) = inner[k], inner[k + 1]
        (t0, g0, _), (t1, g1, _) = outer[k], outer[k + 1]
        t0, g0 = t0 + math.cos(a0) * lift, g0 + math.sin(a0) * lift
        t1, g1 = t1 + math.cos(a1) * lift, g1 + math.sin(a1) * lift
        shrink = lambda p, q: (p[0] + (q[0] - p[0]) * 0.06, p[1] + (q[1] - p[1]) * 0.06)
        outline = [shrink((s0, h0), (s1, h1)), shrink((s1, h1), (s0, h0)), shrink((t1, g1), (t0, g0)), shrink((t0, g0), (t1, g1))]
        side.block(mesh, outline, 0.0, proud, "Stone" if is_key else stone_material(opening.spring))


def build_window(mesh, block, side, opening):
    back = -opening.depth + 0.07
    mesh.poly([side.point(s, h, back) for s, h in opening.outline()], "Glass", side.outward)
    drape = opening.width * 0.3
    for s0, s1 in ((opening.left, opening.left + drape), (opening.right - drape, opening.right)):
        side.quad(mesh, s0, s1, opening.bottom + 0.1, opening.spring, back - 0.05, "Curtain")
    build_frame(mesh, side, opening, back)
    if opening.shutters:
        build_shutters(mesh, side, opening)
    if opening.flowers:
        build_flower_box(mesh, side, opening)


def build_slit(mesh, block, side, opening):
    """A narrow church or barn window: glass deep in the wall and a single lead bar."""
    back = -opening.depth + 0.07
    mesh.poly([side.point(s, h, back) for s, h in opening.outline()], "Glass", side.outward)
    side.box(mesh, opening.s, opening.bottom + opening.height / 2, back + 0.02, (0.03, opening.height, 0.03), "Iron")


def build_frame(mesh, side, opening, back):
    bar, depth = 0.055, 0.05
    d = back + depth / 2
    mid = opening.bottom + opening.height / 2
    transom = opening.spring - 0.05
    for s in (opening.left + bar / 2, opening.right - bar / 2):
        side.box(mesh, s, mid, d, (bar, opening.height, depth), "Frame")
    side.box(mesh, opening.s, (opening.bottom + opening.top) / 2, d, (bar * 1.3, opening.top - opening.bottom, depth), "Frame")
    side.box(mesh, opening.s, opening.bottom + bar / 2, d, (opening.width, bar, depth), "Frame")
    side.box(mesh, opening.s, transom, d, (opening.width, bar, depth), "Frame")
    panes = 3
    for k in range(1, panes):
        h = opening.bottom + (transom - opening.bottom) * k / panes
        side.box(mesh, opening.s, h, d, (opening.width, bar * 0.5, depth * 0.7), "Frame")
    if opening.is_arched:
        arc = opening.arc(opening.radius - bar / 2, 8)
        for (s0, h0, _), (s1, h1, _) in zip(arc, arc[1:]):
            side.block(mesh, [(s0, h0 - bar / 2), (s1, h1 - bar / 2), (s1, h1 + bar / 2), (s0, h0 + bar / 2)], back, back + depth, "Frame")


def build_shutters(mesh, side, opening):
    width = opening.width / 2
    height = opening.top - opening.bottom + 0.05
    mid = opening.bottom + height / 2 - 0.03
    rail = 0.06
    paint = "ShutterFaded" if rng.random() < 0.35 else opening.paint
    for sign in (-1, 1):
        centre = opening.s + sign * (opening.width / 2 + 0.16 + width / 2)
        jitter = rng.uniform(-0.008, 0.008)
        side.box(mesh, centre, mid, 0.04 + jitter, (width, height, 0.025), paint)
        for h in (mid - height / 2 + rail / 2, mid, mid + height / 2 - rail / 2):
            side.box(mesh, centre, h, 0.065 + jitter, (width, rail, 0.025), paint)
        for s in (centre - width / 2 + rail / 2, centre + width / 2 - rail / 2):
            side.box(mesh, s, mid, 0.065 + jitter, (rail, height, 0.025), paint)
        slats = max(6, int(height / 0.14))
        for half in (-1, 1):
            base = mid + half * height / 4
            span = height / 2 - 1.5 * rail
            for k in range(slats // 2):
                h = base - span / 2 + (k + 0.5) * span / (slats // 2)
                side.box(mesh, centre, h, 0.062 + jitter, (width - 2 * rail, 0.022, 0.014), paint)
        for h in (mid - height * 0.32, mid + height * 0.32):
            side.box(mesh, centre - sign * width / 2, h, 0.06, (0.04, 0.025, 0.03), "Iron")


def build_flower_box(mesh, side, opening):
    base = opening.bottom - 0.12
    side.box(mesh, opening.s, base + 0.1, 0.22, (opening.width + 0.25, 0.2, 0.2), "Terracotta")
    span = opening.width / 2 + 0.12
    for _ in range(20):
        p = side.point(opening.s + rng.uniform(-span, span), base + 0.22 + rng.uniform(0, 0.16), 0.22 + rng.uniform(-0.08, 0.1))
        mesh.sphere(p, rng.uniform(0.05, 0.08), "Leaf")
    for _ in range(16):
        p = side.point(opening.s + rng.uniform(-span, span), base + 0.3 + rng.uniform(0, 0.16), 0.24 + rng.uniform(-0.05, 0.12))
        mesh.sphere(p, rng.uniform(0.035, 0.055), rng.choice(("FlowerRed", "FlowerRed", "FlowerPink", "FlowerWhite")))
    for _ in range(5):
        s = opening.s + rng.uniform(-span, span)
        for k in range(int(rng.uniform(0.25, 0.6) / 0.06)):
            sway = math.sin(k * 0.9 + s * 7) * 0.03
            mesh.sphere(side.point(s + sway, base + 0.18 - k * 0.06, 0.35), 0.035, "Leaf" if k % 3 else "FlowerWhite")


def door_leaf_outline(opening, back):
    return [(s, h) for s, h in opening.outline(12)], back


def build_door(mesh, block, side, opening):
    back = -opening.depth + 0.06
    mesh.poly([side.point(s, h, back) for s, h in opening.outline(12)], "Wood", side.outward)
    leaf = 0.05
    split = opening.width > 1.1
    columns = ((opening.left + 0.12, opening.s - 0.06), (opening.s + 0.06, opening.right - 0.12)) if split else \
        ((opening.left + 0.12, opening.right - 0.12),)
    panel_top = opening.height - 0.1
    for h0, h1 in ((0.15, panel_top * 0.45), (panel_top * 0.45 + 0.15, panel_top)):
        for s0, s1 in columns:
            side.box(mesh, (s0 + s1) / 2, opening.bottom + (h0 + h1) / 2, back + leaf / 2, (s1 - s0, h1 - h0, leaf), "Wood")
    if split:
        side.box(mesh, opening.s, opening.bottom + (opening.top - opening.bottom) / 2, back + 0.03,
                 (0.05, opening.top - opening.bottom, 0.06), "Wood")
    knocker = side.point(opening.s + 0.22, opening.bottom + 1.15, back + 0.09)
    mesh.cylinder(knocker - side.outward * 0.01, knocker + side.outward * 0.01, 0.07, "Iron", segments=12)
    build_doorstep(mesh, side, opening)


def build_doorstep(mesh, side, opening):
    if opening.bottom > 0.05:
        steps = max(1, round(opening.bottom / 0.17))
        rise = opening.bottom / steps
        for k in range(steps):
            depth = 0.36 * (steps - k)
            side.box(mesh, opening.s, rise * (k + 0.5), depth / 2, (opening.width + 0.7 - 0.1 * k, rise, depth), "Stone")
        return
    side.box(mesh, opening.s, 0.06, 0.18, (opening.width + 0.6, 0.12, 0.36), "Stone")


def build_gate(mesh, block, side, opening):
    """Big double plank doors (boatyard, barn, church portal) with iron strap hinges."""
    back = -opening.depth + 0.06
    mesh.poly([side.point(s, h, back) for s, h in opening.outline(12)], "Shade", side.outward)
    plank = 0.16
    top = opening.top - 0.02
    s = opening.left
    while s < opening.right - 0.01:
        s1 = min(s + plank, opening.right)
        mid = (s + s1) / 2
        h_top = top if not opening.is_arched else min(top, arc_height(opening, mid) - 0.02)
        side.box(mesh, mid, opening.bottom + (h_top - opening.bottom) / 2, back + 0.04,
                 (s1 - s - 0.012, h_top - opening.bottom, 0.05 + rng.uniform(-0.005, 0.005)), opening.paint)
        s = s1
    for fraction in (0.18, 0.62):
        h = opening.bottom + opening.height * fraction
        for s0, s1 in ((opening.left + 0.04, opening.s - 0.2), (opening.s + 0.2, opening.right - 0.04)):
            side.box(mesh, (s0 + s1) / 2, h, back + 0.075, (s1 - s0, 0.07, 0.015), "Iron")
    side.box(mesh, opening.s, opening.bottom + opening.height / 2, back + 0.07, (0.04, opening.height, 0.03), "Timber")
    side.box(mesh, opening.s, 0.05, 0.15, (opening.width + 0.4, 0.1, 0.3), "Stone")


def arc_height(opening, s):
    """Height of an opening's arched head above s."""
    dx = s - opening.s
    return opening.centre_h + math.sqrt(max(0.0, opening.radius * opening.radius - dx * dx))


def build_shopfront(mesh, block, side, opening):
    """A wide glazed shop window with a timber frame, a stall board and a fascia above."""
    back = -0.12
    paint = opening.paint
    mesh.poly([side.point(s, h, back) for s, h in opening.outline()], "Glass", side.outward)
    side.box(mesh, opening.s, opening.bottom + 0.3, back + 0.08, (opening.width, 0.6, 0.16), paint)
    mullions = max(2, round(opening.width / 0.8))
    for k in range(mullions + 1):
        s = opening.left + opening.width * k / mullions
        side.box(mesh, s, opening.bottom + opening.height / 2, back + 0.05, (0.08, opening.height, 0.1), paint)
    side.box(mesh, opening.s, opening.bottom + opening.height * 0.78, back + 0.04, (opening.width, 0.06, 0.08), paint)
    side.box(mesh, opening.s, opening.top + 0.2, 0.06, (opening.width + 0.3, 0.4, 0.12), paint)
    side.box(mesh, opening.s, opening.top + 0.42, 0.1, (opening.width + 0.4, 0.06, 0.2), paint)


def build_arcade_back(mesh, block, side, opening):
    """An open arch into a shaded loggia: back wall with a shop window, paved floor and a beamed ceiling."""
    depth = opening.depth
    side.quad(mesh, opening.left, opening.right, opening.bottom, opening.top, -depth, "PlasterCream")
    inner = Opening(opening.s, opening.bottom + 0.4, opening.width * 0.6, opening.height * 0.55, "window", rise=0.0)
    side.quad(mesh, inner.left, inner.right, inner.bottom, inner.spring, -depth + 0.01, "Glass")
    side.box(mesh, opening.s, inner.spring + 0.05, -depth + 0.04, (inner.width + 0.1, 0.1, 0.08), "Wood")
    mesh.poly([side.point(opening.left, 0.02, 0.0), side.point(opening.right, 0.02, 0.0), side.point(opening.right, 0.02, -depth),
               side.point(opening.left, 0.02, -depth)], "StoneDark", (0, 0, 1))
    ceiling = opening.spring
    mesh.poly([side.point(opening.left, ceiling, 0.0), side.point(opening.right, ceiling, 0.0),
               side.point(opening.right, ceiling, -depth), side.point(opening.left, ceiling, -depth)], "Timber", (0, 0, -1))
    build_wall_lantern(mesh, side, opening.s, ceiling - 0.05, -depth / 2, hanging=True)


def build_rose(mesh, side, rose):
    """Round window: glass, radial tracery, an inner ring and a moulded stone ring around it."""
    depth = 0.3
    ring = rose.ring(rose.radius, 32)
    mesh.poly([side.point(s, h, -depth) for s, h in ring[:-1]], "Glass", side.outward)
    for (s0, h0), (s1, h1) in zip(ring, ring[1:]):
        mesh.poly([side.point(s0, h0, 0.0), side.point(s1, h1, 0.0), side.point(s1, h1, -depth), side.point(s0, h0, -depth)],
                  "StoneDark", side.point(rose.s, rose.centre_h, 0.0) - side.point(s0, h0, 0.0))
    for ring_radius, thickness, proud in ((rose.radius + 0.12, 0.26, 0.08), (rose.radius + 0.3, 0.12, 0.04)):
        inner, outer = rose.ring(ring_radius - thickness / 2, 24), rose.ring(ring_radius + thickness / 2, 24)
        for k in range(24):
            side.block(mesh, [inner[k], inner[k + 1], outer[k + 1], outer[k]], 0.0, proud, "Stone")
    tracery = -depth + 0.08
    hub = rose.radius * 0.22
    for k in range(rose.spokes):
        a = 2 * math.pi * k / rose.spokes
        start = Vector((math.cos(a), math.sin(a))) * hub
        end = Vector((math.cos(a), math.sin(a))) * rose.radius
        p0 = side.point(rose.s + start.x, rose.centre_h + start.y, tracery)
        p1 = side.point(rose.s + end.x, rose.centre_h + end.y, tracery)
        mesh.beam(p0, p1, 0.07, "Stone")
        petal = Vector((math.cos(a + math.pi / rose.spokes), math.sin(a + math.pi / rose.spokes))) * rose.radius * 0.68
        mesh.cylinder(side.point(rose.s + petal.x, rose.centre_h + petal.y, tracery - 0.04),
                      side.point(rose.s + petal.x, rose.centre_h + petal.y, tracery + 0.04), rose.radius * 0.17, "Stone", segments=12)
    for radius in (hub, rose.radius * 0.5):
        circle = rose.ring(radius, 20)
        for (s0, h0), (s1, h1) in zip(circle, circle[1:]):
            mesh.beam(side.point(s0, h0, tracery), side.point(s1, h1, tracery), 0.06, "Stone")
    mesh.cylinder(side.point(rose.s, rose.centre_h, tracery - 0.05), side.point(rose.s, rose.centre_h, tracery + 0.06),
                  hub * 0.6, "Stone", segments=12)


# ---------------------------------------------------------------- facade extras

def build_balcony(mesh, side, opening):
    """A stone slab on brackets with a wrought-iron railing, under a French window."""
    width, depth = opening.width + 0.8, 0.7
    floor = opening.bottom
    side.box(mesh, opening.s, floor - 0.08, depth / 2, (width, 0.16, depth), "Stone")
    for s in (opening.s - width / 2 + 0.2, opening.s + width / 2 - 0.2):
        side.box(mesh, s, floor - 0.3, 0.2, (0.16, 0.3, 0.4), "Stone")
    rail_h = 0.95
    posts = int(width / 0.12)
    for k in range(posts + 1):
        s = opening.s - width / 2 + 0.04 + (width - 0.08) * k / posts
        mesh.cylinder(side.point(s, floor, depth - 0.04), side.point(s, floor + rail_h, depth - 0.04), 0.012, "Iron", segments=4)
    for d in (0.06, depth - 0.04):
        for s in (opening.s - width / 2 + 0.04, opening.s + width / 2 - 0.04):
            mesh.cylinder(side.point(s, floor, d), side.point(s, floor + rail_h, d), 0.018, "Iron", segments=4)
    for h in (floor + 0.1, floor + rail_h):
        a, b = side.point(opening.s - width / 2 + 0.04, h, depth - 0.04), side.point(opening.s + width / 2 - 0.04, h, depth - 0.04)
        mesh.cylinder(a, b, 0.02, "Iron", segments=6)
        for s in (opening.s - width / 2 + 0.04, opening.s + width / 2 - 0.04):
            mesh.cylinder(side.point(s, h, 0.06), side.point(s, h, depth - 0.04), 0.02, "Iron", segments=6)


def build_awning(mesh, side, s, width, top):
    """A striped canvas awning sloping out from the wall, with a scalloped-looking valance."""
    projection, drop = 1.2, 0.55
    stripes = max(4, int(width / 0.3))
    for k in range(stripes):
        s0 = s - width / 2 + width * k / stripes
        s1 = s0 + width / stripes
        material = "CanvasA" if k % 2 == 0 else "CanvasB"
        corners = [side.point(s0, top, 0.02), side.point(s1, top, 0.02), side.point(s1, top - drop, projection),
                   side.point(s0, top - drop, projection)]
        mesh.two_sided(corners, material, side.outward + Vector((0, 0, 1)))
        valance = [side.point(s0, top - drop, projection), side.point(s1, top - drop, projection),
                   side.point(s1, top - drop - 0.22, projection), side.point(s0, top - drop - 0.22, projection)]
        mesh.two_sided(valance, material, side.outward)
    for sign in (-1, 1):
        mesh.cylinder(side.point(s + sign * width / 2, top - drop, projection), side.point(s + sign * width / 2, top - 0.3, 0.0),
                      0.015, "Iron", segments=4)


def build_wall_lantern(mesh, side, s, h, d=0.0, hanging=False):
    if hanging:
        lamp = side.point(s, h - 0.45, d)
        mesh.cylinder(lamp + Vector((0, 0, 0.16)), lamp + Vector((0, 0, 0.45)), 0.012, "Iron", segments=4)
    else:
        mesh.cylinder(side.point(s, h, d), side.point(s, h, d + 0.35), 0.018, "Iron", segments=6)
        mesh.cylinder(side.point(s, h - 0.25, d), side.point(s, h, d + 0.25), 0.014, "Iron", segments=6)
        side.box(mesh, s, h - 0.03, d + 0.02, (0.1, 0.3, 0.04), "Iron")
        lamp = side.point(s, h - 0.25, d + 0.38)
    mesh.box(lamp, (0.2, 0.2, 0.3), "LampGlow")
    for dx in (-0.1, 0.1):
        for dy in (-0.1, 0.1):
            mesh.cylinder(lamp + Vector((dx, dy, -0.16)), lamp + Vector((dx, dy, 0.16)), 0.012, "Iron", segments=4)
    mesh.box(lamp + Vector((0, 0, -0.18)), (0.24, 0.24, 0.05), "Iron")
    mesh.cylinder(lamp + Vector((0, 0, 0.16)), lamp + Vector((0, 0, 0.28)), 0.16, "Iron", segments=8, radius_end=0.03)


def build_hanging_sign(mesh, side, s, h, width=0.9, height=0.6):
    """An iron bracket out from the wall with a timber board hanging from it; returns the board centre."""
    reach = width + 0.35
    mesh.cylinder(side.point(s, h, 0.0), side.point(s, h, reach), 0.025, "Iron", segments=6)
    mesh.cylinder(side.point(s, h - 0.5, 0.0), side.point(s, h, reach * 0.7), 0.018, "Iron", segments=6)
    side.box(mesh, s, h - 0.2, 0.02, (0.12, 0.6, 0.04), "Iron")
    centre = reach - width / 2 - 0.05
    for d in (centre - width / 2 + 0.05, centre + width / 2 - 0.05):
        mesh.cylinder(side.point(s, h, d), side.point(s, h - 0.18, d), 0.008, "Iron", segments=4)
    board = side.point(s, h - 0.18 - height / 2, centre)
    side.box(mesh, s, h - 0.18 - height / 2, centre, (0.06, height, width), "Wood")
    side.box(mesh, s, h - 0.18 - height / 2, centre, (0.07, height - 0.1, width - 0.1), "CanvasB")
    return board


def build_drainpipe(mesh, x, y, top, offset):
    """A zinc downpipe from the gutter at `top`; offset points from the wall out to where the pipe runs."""
    out = Vector(offset)
    mesh.cylinder((x, y, 0.05), (x, y, top - 0.35), 0.05, "Zinc", segments=8)
    mesh.cylinder((x, y, top - 0.35), Vector((x, y, top - 0.05)) + out * 2.5, 0.05, "Zinc", segments=8)
    mesh.cylinder((x, y, 0.05), (x, y, 0.2), 0.065, "Zinc", segments=8)
    z = 1.6
    while z < top - 0.6:
        mesh.box((x - out.x * 0.5, y - out.y * 0.5, z), (0.1, 0.1, 0.04), "Iron")
        z += 2.3


def front_drainpipes(mesh, block):
    w, d = block.width / 2, block.depth / 2
    for sign in (-1, 1):
        build_drainpipe(mesh, block.cx + sign * (w - 0.3), block.cy - d - 0.1, block.eaves - 0.04, (0, -0.1, 0))


def text_object(body, size, position, yaw, material, extrude=0.006):
    """Real extruded text (house names, numbers) as a mesh object."""
    curve = bpy.data.curves.new(f"Text{body}", "FONT")
    curve.body = body
    curve.size = size
    curve.extrude = extrude
    curve.align_x = "CENTER"
    curve.align_y = "CENTER"
    text = bpy.data.objects.new(f"Text{body}", curve)
    bpy.context.collection.objects.link(text)
    text.rotation_euler = (math.radians(90), 0, yaw)
    text.location = position
    bpy.context.view_layer.update()
    evaluated = text.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh = bpy.data.meshes.new_from_object(evaluated)
    mesh.transform(text.matrix_world)
    bpy.data.objects.remove(text)
    obj = bpy.data.objects.new(f"{body}Mesh", mesh)
    bpy.context.collection.objects.link(obj)
    add_material_slots(obj)
    for polygon in mesh.polygons:
        polygon.material_index = MATERIAL_INDEX[material]
    return obj


def facade_text(side, body, s, h, size, d=0.02, material="Iron"):
    return text_object(body, size, side.point(s, h, d), side.yaw(), material)


# ---------------------------------------------------------------- roofs

class Roof:
    """The tiled roof over a rectangle; `height_at(x, y)` gives the tiled surface height for chimneys."""

    def __init__(self, x0, y0, x1, y1, base, pitch, kind, ridge_axis="x"):
        self.x0, self.y0, self.x1, self.y1 = x0, y0, x1, y1
        self.base = base
        self.pitch = pitch
        self.kind = kind
        self.ridge_axis = ridge_axis

    @property
    def rise(self):
        return math.tan(self.pitch)

    def run(self):
        w, d = (self.x1 - self.x0) / 2, (self.y1 - self.y0) / 2
        if self.kind == "hip":
            return min(w, d)
        return d if self.ridge_axis == "x" else w

    @property
    def ridge(self):
        return self.base + self.run() * self.rise

    def height_at(self, x, y):
        cx, cy = (self.x0 + self.x1) / 2, (self.y0 + self.y1) / 2
        w, d = (self.x1 - self.x0) / 2, (self.y1 - self.y0) / 2
        if self.kind == "hip":
            p = min(w - abs(x - cx), d - abs(y - cy))
        elif self.ridge_axis == "x":
            p = d - abs(y - cy)
        else:
            p = w - abs(x - cx)
        return self.base + p * self.rise


def roof_over(block, pitch, overhang=0.4, kind="hip", ridge_axis="x", gable_overhang=0.3):
    """A roof over a block, its eaves `overhang` beyond the walls."""
    x_over = gable_overhang if kind == "gable" and ridge_axis == "x" else overhang
    y_over = gable_overhang if kind == "gable" and ridge_axis == "y" else overhang
    w, d = block.width / 2, block.depth / 2
    return Roof(block.cx - w - x_over, block.cy - d - y_over, block.cx + w + x_over, block.cy + d + y_over,
                block.eaves + 0.02, pitch, kind, ridge_axis)


def build_roof(mesh, roof, block=None, gutters=True):
    """Tiles every face; on a gable roof also closes the gable ends of `block` with its wall material."""
    up = Vector((0, 0, 1))
    for start, along, inward, length, run, trims in roof_faces(roof):
        slope = (inward + up * roof.rise).normalized()
        normal = along.cross(slope).normalized()
        if normal.z < 0:
            normal = -normal

        def at(u, p, lift=0.0):
            return start + along * u + inward * p + up * (roof.base + p * roof.rise) + normal * lift

        far_start, far_end = (run, length - run) if trims else (0.0, length)
        mesh.poly([at(0, 0), at(length, 0), at(far_end, run), at(far_start, run)], "RoofUnder", normal)
        build_tiles(mesh, at, slope, normal, along, length, run, trims, roof.pitch)
        if trims:
            build_cap_line(mesh, at(0, 0), at(run, run))
            build_cap_line(mesh, at(length, 0), at(length - run, run))
        else:
            build_cap_line(mesh, at(0, 0, 0.02), at(0, run, 0.02))
            build_cap_line(mesh, at(length, 0, 0.02), at(length, run, 0.02))
        if gutters:
            mesh.cylinder(at(0, 0) - inward * 0.08 + Vector((0, 0, -0.06)), at(length, 0) - inward * 0.08 + Vector((0, 0, -0.06)),
                          0.07, "Zinc", segments=8)
    build_ridge(mesh, roof)
    if roof.kind == "gable" and block is not None:
        build_gable_ends(mesh, roof, block)


def roof_faces(roof):
    """Each face as (eave start, along, inward plan direction, eave length, run to the ridge, trims at the hips)."""
    x0, y0, x1, y1 = roof.x0, roof.y0, roof.x1, roof.y1
    w, d = x1 - x0, y1 - y0
    run = roof.run()
    hip = roof.kind == "hip"
    faces = []
    if hip or roof.ridge_axis == "x":
        faces += [(Vector((x0, y0, 0)), Vector((1, 0, 0)), Vector((0, 1, 0)), w, run, hip),
                  (Vector((x1, y1, 0)), Vector((-1, 0, 0)), Vector((0, -1, 0)), w, run, hip)]
    if hip or roof.ridge_axis == "y":
        faces += [(Vector((x1, y0, 0)), Vector((0, 1, 0)), Vector((-1, 0, 0)), d, run, hip),
                  (Vector((x0, y1, 0)), Vector((0, -1, 0)), Vector((1, 0, 0)), d, run, hip)]
    return faces


def build_tiles(mesh, at, slope, normal, along, length, run, trims, pitch):
    slope_len = 1 / math.cos(pitch)
    rows = int(run * slope_len / TILE_ROW)
    for row in range(rows):
        p = (row * TILE_ROW + TILE_ROW / 2) / slope_len
        u0, u1 = (p + 0.06, length - p - 0.06) if trims else (0.04, length - 0.04)
        count = int((u1 - u0) / TILE_W)
        margin = (u1 - u0 - count * TILE_W) / 2
        for k in range(max(0, count)):
            u = u0 + margin + (k + 0.5) * TILE_W
            is_pan = (row + k) % 2 == 0
            lift = (0.05 if is_pan else 0.1) + rng.uniform(-0.01, 0.01)
            build_tile(mesh, at(u, p, lift), slope, normal, along, is_pan)


def build_tile(mesh, centre, slope, normal, along, is_pan):
    """A canal tile: a half pipe along the slope, pans open upwards, caps over the joints."""
    length = TILE_ROW * 1.35
    segments = 6
    material = rng.choice(("RoofTileA", "RoofTileA", "RoofTileB", "RoofTileB", "RoofTileC", "RoofTileC", "TileDark", "TileDark",
                           "TileLichen"))
    slope = (slope + along * rng.uniform(-0.015, 0.015)).normalized()
    radius = TILE_R if is_pan else TILE_R * 0.8
    bulge = -0.55 if is_pan else 1.0
    ring = [along * (math.cos(math.pi * k / segments) * radius) + normal * (math.sin(math.pi * k / segments) * radius * bulge)
            for k in range(segments + 1)]
    tail, head = slope * (-length / 2), slope * (length / 2)
    for k in range(segments):
        # Caps are seen from outside the pipe, pans from inside the trough.
        outward = (ring[k] + ring[k + 1]) * (-1 if is_pan else 1)
        mesh.poly([centre + tail + ring[k], centre + tail + ring[k + 1], centre + head + ring[k + 1], centre + head + ring[k]],
                  material, outward)


def build_cap_line(mesh, a, b):
    pieces = max(1, int((b - a).length / 0.36))
    for k in range(pieces):
        s0 = a + (b - a) * (k / pieces)
        s1 = a + (b - a) * ((k + 0.97) / pieces)
        mesh.cylinder(s0 + Vector((0, 0, 0.1)), s1 + Vector((0, 0, 0.1)), 0.1, "RoofTileC", segments=8)


def build_ridge(mesh, roof):
    cx, cy = (roof.x0 + roof.x1) / 2, (roof.y0 + roof.y1) / 2
    w, d = (roof.x1 - roof.x0) / 2, (roof.y1 - roof.y0) / 2
    run = roof.run()
    if roof.kind == "hip":
        half_x, half_y = w - run, d - run
    elif roof.ridge_axis == "x":
        half_x, half_y = w, 0.0
    else:
        half_x, half_y = 0.0, d
    if half_x + half_y > 0.01:
        build_cap_line(mesh, Vector((cx - half_x, cy - half_y, roof.ridge)), Vector((cx + half_x, cy + half_y, roof.ridge)))


def build_gable_ends(mesh, roof, block):
    """Triangles of wall closing each gable, with a stone coping along the rakes."""
    rise = roof.rise
    for side in block.sides():
        is_gable = side.index in ((RIGHT, LEFT) if roof.ridge_axis == "x" else (FRONT, BACK))
        if not is_gable or side.index in block.hidden:
            continue
        half = side.length / 2
        top = (block.eaves - block.base) + 0.02 + (half + (roof.run() - half)) * rise
        eaves = block.eaves - block.base
        corners = [side.point(0, eaves, 0.0), side.point(side.length, eaves, 0.0), side.point(half, top, 0.0)]
        mesh.poly(corners, block.wall, side.outward)


def build_chimney(mesh, roof, x, y, material="Brick", size=(0.9, 0.75)):
    size_x, size_y = size
    bottom = min(roof.height_at(x + sx * size_x / 2, y + sy * size_y / 2) for sx in (-1, 1) for sy in (-1, 1)) - 0.3
    top = roof.ridge + 0.6
    mesh.box((x, y, (bottom + top) / 2), (size_x, size_y, top - bottom), material)
    mesh.box((x, y, top + 0.06), (size_x + 0.14, size_y + 0.14, 0.12), "Stone")
    mesh.box((x, y, top + 0.16), (size_x - 0.1, size_y - 0.1, 0.08), "Stone")
    for dx in ((-0.18, 0.18) if size_x > 0.8 else (0.0,)):
        mesh.cylinder((x + dx, y, top + 0.2), (x + dx, y, top + 0.55), 0.12, "Terracotta", segments=12)
        mesh.cylinder((x + dx, y, top + 0.5), (x + dx, y, top + 0.58), 0.15, "Terracotta", segments=12)


# ---------------------------------------------------------------- masonry

def prism(mesh, centre, radius, z0, z1, sides, material, radius_top=None, rotation=0.0):
    """A regular polygonal prism (octagonal basins, plinths), optionally tapering."""
    top = radius if radius_top is None else radius_top
    c = Vector(centre)
    ring = lambda r, z: [c + Vector((math.cos(rotation + 2 * math.pi * k / sides) * r, math.sin(rotation + 2 * math.pi * k / sides) * r, z))
                         for k in range(sides)]
    bottom_ring, top_ring = ring(radius, z0), ring(top, z1)
    mesh.poly(top_ring, material, (0, 0, 1))
    mesh.poly(bottom_ring, material, (0, 0, -1))
    middle = c + Vector((0, 0, (z0 + z1) / 2))
    for k in range(sides):
        quad = [bottom_ring[k], bottom_ring[(k + 1) % sides], top_ring[(k + 1) % sides], top_ring[k]]
        mesh.poly(quad, material, sum(quad, Vector()) / 4 - middle)


def annulus_wall(mesh, centre, inner, outer, z0, z1, sides, material, rotation=0.0):
    """A polygonal ring wall (a basin rim), each side its own block so the masonry joints show."""
    c = Vector(centre)
    for k in range(sides):
        a0 = rotation + 2 * math.pi * k / sides
        a1 = rotation + 2 * math.pi * (k + 1) / sides
        corner = lambda a, r, z: c + Vector((math.cos(a) * r, math.sin(a) * r, z))
        front = [corner(a0, outer, z0), corner(a1, outer, z0), corner(a1, outer, z1), corner(a0, outer, z1)]
        back = [corner(a0, inner, z0), corner(a1, inner, z0), corner(a1, inner, z1), corner(a0, inner, z1)]
        mesh.hexahedron(front, back, material)


# ---------------------------------------------------------------- masonry walls

def waterline_material(z, waterline):
    """Stone that is green and dark around and below the waterline, cleaner above it."""
    if z < waterline + 0.15 and rng.random() < 0.7:
        return "Moss" if rng.random() < 0.5 else "StoneGrime"
    return stone_material(max(0.0, z - waterline))


def build_coursed_wall(mesh, x0, x1, z0, z1, y, outward, waterline=None, course=0.4):
    """Coursed ashlar blocks on a wall plane at y facing `outward` (+1 or -1 along Y), over a mortar backing."""
    face = Vector((0, outward, 0))
    mesh.poly([(x0, y, z0), (x1, y, z0), (x1, y, z1), (x0, y, z1)], "Mortar", face)
    rows = max(1, round((z1 - z0) / course))
    height = (z1 - z0) / rows
    for row in range(rows):
        za = z0 + row * height
        x = x0 - (rng.uniform(0.2, 0.5) if row % 2 else 0.0)
        while x < x1:
            length = rng.uniform(0.55, 1.1)
            xa, xb = max(x, x0), min(x + length, x1)
            x += length
            if xb - xa < 0.1:
                continue
            proud = 0.05 + rng.uniform(-STONE_JITTER, STONE_JITTER)
            zc = za + height / 2
            material = waterline_material(zc, waterline) if waterline is not None else stone_material(zc)
            mesh.box(((xa + xb) / 2, y + outward * proud / 2, zc), (xb - xa - 0.015, proud, height - 0.015), material)


def build_coping(mesh, x0, x1, y_face, depth, top, outward, overhang=0.08, height=0.25):
    """Big flat coping stones along a wall top, overhanging the face slightly."""
    x = x0
    while x < x1 - 0.05:
        length = min(rng.uniform(0.9, 1.4), x1 - x)
        y = y_face + outward * overhang - outward * depth / 2
        mesh.box((x + length / 2, y, top - height / 2 + rng.uniform(-0.005, 0.005)), (length - 0.012, depth, height),
                 "Stone" if rng.random() > 0.3 else "StoneDark")
        x += length


def build_ring(mesh, centre, axis, radius, thickness, material, segments=12):
    """A torus-like ring of short cylinders around `axis` (a unit vector)."""
    axis = Vector(axis).normalized()
    u = axis.orthogonal().normalized()
    v = axis.cross(u)
    points = [Vector(centre) + (u * math.cos(2 * math.pi * k / segments) + v * math.sin(2 * math.pi * k / segments)) * radius
              for k in range(segments + 1)]
    for a, b in zip(points, points[1:]):
        mesh.cylinder(a, b, thickness, material, segments=6)


def build_bollard(mesh, x, y, z=0.0):
    """A cast-iron mooring bollard with a domed cap."""
    mesh.cylinder((x, y, z), (x, y, z + 0.08), 0.2, "Iron", segments=14)
    mesh.cylinder((x, y, z + 0.08), (x, y, z + 0.45), 0.13, "Iron", segments=14, radius_end=0.15)
    mesh.cylinder((x, y, z + 0.45), (x, y, z + 0.52), 0.2, "Iron", segments=14)
    mesh.sphere((x, y, z + 0.55), 0.13, "Iron", subdivisions=2)


def build_chain(mesh, a, b, sag=0.18, links=14):
    """A sagging iron chain between two points."""
    a, b = Vector(a), Vector(b)
    points = [a + (b - a) * (k / links) - Vector((0, 0, sag * 4 * (k / links) * (1 - k / links))) for k in range(links + 1)]
    for p, q in zip(points, points[1:]):
        mesh.beam(p, q, 0.03, "Iron")


# ---------------------------------------------------------------- yard props

def build_bench(mesh, centre, yaw, length=1.6):
    along = Vector((math.cos(yaw), math.sin(yaw), 0))
    across = Vector((-along.y, along.x, 0))
    c = Vector(centre)
    for k in (-1, 1):
        leg = c + along * k * (length / 2 - 0.15)
        mesh.beam(leg, leg + Vector((0, 0, 0.42)), 0.08, "Timber")
    mesh.beam(c - along * length / 2 + Vector((0, 0, 0.45)), c + along * length / 2 + Vector((0, 0, 0.45)), 0.08, "Timber")
    mesh.beam(c - along * length / 2 + across * 0.12 + Vector((0, 0, 0.45)), c + along * length / 2 + across * 0.12 + Vector((0, 0, 0.45)),
              0.08, "Timber")


def build_table_set(mesh, centre):
    """A café table with two chairs."""
    c = Vector(centre)
    mesh.cylinder(c, c + Vector((0, 0, 0.72)), 0.04, "Iron", segments=8)
    mesh.cylinder(c + Vector((0, 0, 0.72)), c + Vector((0, 0, 0.76)), 0.38, "Timber", segments=16)
    mesh.cylinder(c, c + Vector((0, 0, 0.03)), 0.22, "Iron", segments=12)
    for sign in (-1, 1):
        seat = c + Vector((sign * 0.65, 0, 0))
        for dx in (-0.18, 0.18):
            for dy in (-0.18, 0.18):
                mesh.beam(seat + Vector((dx, dy, 0)), seat + Vector((dx, dy, 0.45)), 0.035, "Iron")
        mesh.box(seat + Vector((0, 0, 0.47)), (0.42, 0.42, 0.04), "Timber")
        back = seat + Vector((sign * 0.2, 0, 0))
        mesh.box(back + Vector((0, 0, 0.72)), (0.04, 0.42, 0.45), "Timber")


def build_barrel(mesh, centre, height=0.9, radius=0.32):
    c = Vector(centre)
    mesh.cylinder(c, c + Vector((0, 0, height * 0.5)), radius * 0.88, "Timber", segments=14, radius_end=radius)
    mesh.cylinder(c + Vector((0, 0, height * 0.5)), c + Vector((0, 0, height)), radius, "Timber", segments=14, radius_end=radius * 0.88)
    for z in (0.12, 0.38, 0.62, 0.88):
        r = radius * (0.9 + 0.1 * math.sin(math.pi * z)) + 0.01
        mesh.cylinder(c + Vector((0, 0, height * z - 0.02)), c + Vector((0, 0, height * z + 0.02)), r, "Iron", segments=14)


def build_crate(mesh, centre, size=0.6):
    c = Vector(centre) + Vector((0, 0, size / 2))
    mesh.box(c, (size - 0.04, size - 0.04, size - 0.04), "Timber")
    for z in (-1, 1):
        for axis in (Vector((1, 0, 0)), Vector((0, 1, 0))):
            other = Vector((axis.y, axis.x, 0))
            for k in (-1, 1):
                p = c + Vector((0, 0, z * (size / 2 - 0.04))) + other * k * (size / 2 - 0.02)
                mesh.beam(p - axis * size / 2, p + axis * size / 2, 0.06, "Wood")


def build_woodpile(mesh, centre, length=2.0, rows=4):
    c = Vector(centre)
    for row in range(rows):
        count = 9 - row
        for k in range(count):
            x = (k - (count - 1) / 2) * 0.2 + rng.uniform(-0.02, 0.02)
            z = 0.1 + row * 0.17
            mesh.cylinder(c + Vector((x, -0.4, z)), c + Vector((x, 0.4, z)), 0.085 + rng.uniform(-0.015, 0.01), "Timber", segments=7)


# ---------------------------------------------------------------- carts

def build_wheel(mesh, centre, radius, axis, spokes=10):
    build_ring(mesh, centre, axis, radius, 0.05, "Timber", segments=20)
    build_ring(mesh, centre, axis, radius + 0.03, 0.025, "Iron", segments=20)
    axis = Vector(axis).normalized()
    u = axis.orthogonal().normalized()
    v = axis.cross(u)
    for k in range(spokes):
        a = 2 * math.pi * k / spokes
        mesh.beam(Vector(centre), Vector(centre) + (u * math.cos(a) + v * math.sin(a)) * radius, 0.04, "Timber")
    mesh.cylinder(Vector(centre) - axis * 0.08, Vector(centre) + axis * 0.08, 0.08, "Wood", segments=10)


def build_cart_body(mesh, length, width, wheel_radius, bed_height, shafts):
    """A plank cart bed on two wheels with side rails and shafts or handles."""
    mesh.box((0, 0, bed_height), (length, width, 0.08), "Timber")
    for y in (-width / 2, width / 2):
        mesh.box((0, y, bed_height + 0.25), (length, 0.05, 0.4), "Wood")
        for x in (-length / 2 + 0.05, 0.0, length / 2 - 0.05):
            mesh.beam((x, y, bed_height), (x, y, bed_height + 0.45), 0.05, "Timber")
    mesh.cylinder((0, -width / 2 - 0.1, wheel_radius), (0, width / 2 + 0.1, wheel_radius), 0.04, "Iron", segments=6)
    for y in (-width / 2 - 0.12, width / 2 + 0.12):
        build_wheel(mesh, (0, y, wheel_radius), wheel_radius, (0, 1, 0))
    for y in (-width / 2 + 0.1, width / 2 - 0.1):
        mesh.beam((length / 2 - 0.1, y, bed_height), (length / 2 + shafts, y, 0.15), 0.07, "Timber")
    mesh.beam((-length / 2 + 0.1, 0, bed_height - 0.05), (-length / 2 - 0.2, 0, 0.0), 0.06, "Timber")



# ---------------------------------------------------------------- scene

def make_materials():
    for name, colour in MATERIALS.items():
        material = bpy.data.materials.new(name)
        material.diffuse_color = (*colour, 1.0)
        material.use_nodes = True
        bsdf = material.node_tree.nodes.get("Principled BSDF")
        bsdf.inputs["Base Color"].default_value = (*colour, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.1 if name in ("Glass", "Water") else (0.45 if name in ("Zinc", "Iron", "Bronze") else 0.85)
        if name in ("Zinc", "Bronze"):
            bsdf.inputs["Metallic"].default_value = 0.8
        if name == "LampGlow":
            bsdf.inputs["Emission Color"].default_value = (*colour, 1.0)
            bsdf.inputs["Emission Strength"].default_value = 6.0
        if name in TEXTURES:
            add_box_projected_texture(material, *TEXTURES[name])


def add_box_projected_texture(material, texture_set, metres_per_tile):
    """World-scale box projection, the Blender counterpart of Unity's triplanar mapping."""
    nodes, links = material.node_tree.nodes, material.node_tree.links
    bsdf = nodes.get("Principled BSDF")
    coords = nodes.new("ShaderNodeTexCoord")
    mapping = nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (1 / metres_per_tile,) * 3
    links.new(coords.outputs["Object"], mapping.inputs["Vector"])
    folder = (TEXTURE_ROOT / texture_set).resolve()
    albedo = nodes.new("ShaderNodeTexImage")
    albedo.image = bpy.data.images.load(str(folder / f"{texture_set}_albedo.jpg"))
    albedo.projection = "BOX"
    albedo.projection_blend = 0.2
    links.new(mapping.outputs["Vector"], albedo.inputs["Vector"])
    tint = nodes.new("ShaderNodeMix")
    tint.data_type = "RGBA"
    tint.blend_type = "MULTIPLY"
    tint.inputs["Factor"].default_value = 1.0
    tint.inputs["A"].default_value = bsdf.inputs["Base Color"].default_value
    links.new(albedo.outputs["Color"], tint.inputs["B"])
    links.new(tint.outputs["Result"], bsdf.inputs["Base Color"])
    normal_image = nodes.new("ShaderNodeTexImage")
    normal_image.image = bpy.data.images.load(str(folder / f"{texture_set}_normal.jpg"))
    normal_image.image.colorspace_settings.name = "Non-Color"
    normal_image.projection = "BOX"
    normal_image.projection_blend = 0.2
    links.new(mapping.outputs["Vector"], normal_image.inputs["Vector"])
    normal_map = nodes.new("ShaderNodeNormalMap")
    links.new(normal_image.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])


class Parts:
    """The meshes a building is made of: bevelled stone and detail, an unbevelled roof, and loose objects (text)."""

    def __init__(self):
        self.stone = Mesh()
        self.detail = Mesh()
        self.roof = Mesh()
        self.extra = []

    def finish(self, name):
        # Real masonry and joinery have worn edges; perfectly sharp boxes are what make a model read as a toy.
        objects = [self.stone.to_object("Stone", bevel=BEVEL_WIDTH), self.detail.to_object("Detail", bevel=BEVEL_WIDTH),
                   self.roof.to_object("Roof")] + self.extra
        apply_modifiers(objects)
        bpy.ops.object.select_all(action="DESELECT")
        for part in objects:
            part.select_set(True)
        bpy.context.view_layer.objects.active = objects[1]
        bpy.ops.object.join()
        building = bpy.context.view_layer.objects.active
        building.name = name
        return building


def apply_modifiers(parts):
    """Joining keeps only the active object's modifiers, so bake each part's bevel first."""
    for part in parts:
        bpy.ops.object.select_all(action="DESELECT")
        bpy.context.view_layer.objects.active = part
        part.select_set(True)
        for modifier in list(part.modifiers):
            bpy.ops.object.modifier_apply(modifier=modifier.name)


def export(building, name):
    output = MODEL_ROOT / name / f"{name}.fbx"
    output.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    building.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=str(output),
        use_selection=True,
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        path_mode="STRIP",
    )
    print(f"Exported {output} with {len(building.data.polygons)} faces")


def render_previews(building, folder, name, ground=0.0, water=None):
    scene = bpy.context.scene
    bpy.ops.object.light_add(type="SUN")
    sun = bpy.context.active_object
    sun.data.energy = 3.5
    sun.data.color = (1.0, 0.93, 0.85)
    sun.rotation_euler = (math.radians(50), 0, math.radians(-30))
    world = bpy.data.worlds.new("Sky")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.42, 0.41, 0.4, 1)
    scene.world = world
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 32
    scene.cycles.use_denoising = True
    scene.view_settings.view_transform = "AgX"
    bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, ground))
    if water is not None:
        level, y0, y1 = water
        bpy.ops.mesh.primitive_plane_add(size=1, location=(0, (y0 + y1) / 2, level))
        plane = bpy.context.active_object
        plane.scale = (60, y1 - y0, 1)
        plane.data.materials.append(bpy.data.materials["Water"])
    bpy.ops.object.camera_add()
    camera = bpy.context.active_object
    scene.camera = camera
    corners = [building.matrix_world @ Vector(c) for c in building.bound_box]
    centre = sum(corners, Vector()) / 8
    radius = max((c - centre).length for c in corners)
    views = {"three_quarter": Vector((0.8, -1.0, 0.55)), "back": Vector((-0.8, 1.0, 0.5)), "front": Vector((0.0, -1.0, 0.12))}
    for view, direction in views.items():
        location = centre + direction.normalized() * radius * 2.6
        camera.location = location
        camera.rotation_euler = (centre - location).to_track_quat("-Z", "Y").to_euler()
        camera.data.lens = 40
        scene.render.resolution_x, scene.render.resolution_y = 1000, 800
        scene.render.filepath = str(Path(folder) / f"{name}_{view}.png")
        bpy.ops.render.render(write_still=True)
    print(f"Previews written to {folder}")


def run(name, build, seed=1, preview_ground=0.0, preview_water=None):
    """Builds, exports the FBX and the .blend, and renders previews when called with -- --preview <folder>."""
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    rng.seed(seed)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    make_materials()
    parts = Parts()
    build(parts)
    building = parts.finish(name)
    export(building, name)
    BLEND_ROOT.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str((BLEND_ROOT / f"{name}.blend").resolve()))
    if "--preview" in args:
        render_previews(building, args[args.index("--preview") + 1], name, preview_ground, preview_water)


class Piece:
    """One model of a multi-model script, with how its preview is staged."""

    def __init__(self, build, preview_ground=0.0, preview_water=None):
        self.build = build
        self.preview_ground = preview_ground
        self.preview_water = preview_water


def run_pieces(pieces, seed):
    """Runs every piece of a script, or only the one named by -- --piece <name>."""
    names = [sys.argv[sys.argv.index("--piece") + 1]] if "--piece" in sys.argv else list(pieces)
    for name in names:
        piece = pieces[name]
        run(name, piece.build, seed, piece.preview_ground, piece.preview_water)
