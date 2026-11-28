"""Builds "House 1" from ConceptArt/CanalTown/House_1.png and exports it as one FBX plus a .blend for hand tweaking.

Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_house_1.py -- [--preview <folder>]

The concept: a narrow three-storey brick townhouse, two bays per facade, stone quoins, a mossy stone plinth,
segmental stone window heads with keystones, olive louvred shutters, flower boxes, an arched front door with a
lantern and a "24" plaque, a hipped terracotta roof with a skylight, a brick chimney and zinc gutters.

Conventions (Blender space, metres): origin at the bottom centre of the footprint, Z up, the front facade faces -Y,
the "right side" of the concept faces +X. Material names are meant to be remapped to HDRP materials in Unity.
"""
import math
import random
import sys
from pathlib import Path

import bmesh
import bpy
from mathutils import Matrix, Vector

OUTPUT = Path("Assets/_Project/Art/Models/House1/House1.fbx")
BLEND_OUTPUT = Path("Tools/Art/Blend/House1.blend")

WIDTH = 6.8
DEPTH = 7.2
GROUND_STOREY = 3.4
UPPER_STOREY = 3.1
EAVES = GROUND_STOREY + 2 * UPPER_STOREY
CORNICE = 0.3
PLINTH = 0.7
PLINTH_PROUD = 0.06
CORE_INSET = 0.26
BAY_FRACTIONS = (0.29, 0.71)

WINDOW_W = 0.95
GROUND_WINDOW = (0.85, 1.95, 0.1)  # bottom, height to the arch spring, arch rise
UPPER_WINDOW = (0.85, 1.55, 0.09)
DOOR = (0.22, 2.15, 0.6)
DOOR_W = 1.2
REVEAL = 0.22
ARCH_THICK = 0.3
SURROUND_PROUD = 0.05

BEVEL_WIDTH = 0.012
BEVEL_ANGLE = math.radians(50)  # bevels boxes, leaves the rounder leaves, flowers and pipes alone
STONE_JITTER = 0.012

ROOF_PITCH = math.radians(34)
OVERHANG = 0.4
ROOF_COVER = 0.06  # the tiled surface sits this far above the roof deck
EAVE_EDGE = 0.07  # visible thickness of the tile course at the eaves

CHIMNEY_XY = (1.5, -1.9)
SKYLIGHT = (2, 5.0, 5.85, 1.4, 2.3)  # roof face, u range along the eave, p range up the slope (plan metres)

MATERIALS = {
    "Mortar": (0.6, 0.55, 0.47),
    "Brick": (1.0, 0.97, 0.94),
    "Stone": (0.95, 0.93, 0.88),
    "StoneDark": (0.84, 0.8, 0.72),
    "Moss": (0.3, 0.32, 0.17),
    "RoofTiles": (1.0, 0.95, 0.9),
    "RoofTileC": (0.5, 0.2, 0.11),
    "RoofUnder": (0.25, 0.1, 0.06),
    "Zinc": (0.55, 0.57, 0.58),
    "Glass": (0.04, 0.05, 0.06),
    "Frame": (0.78, 0.75, 0.68),
    "Shutter": (1.0, 0.92, 0.55),
    "Curtain": (0.75, 0.72, 0.64),
    "Wood": (0.75, 0.7, 0.65),
    "Iron": (0.05, 0.05, 0.05),
    "Terracotta": (0.55, 0.32, 0.18),
    "Plaque": (0.8, 0.79, 0.75),
    "LampGlow": (1.0, 0.7, 0.35),
    "StoneGrime": (0.7, 0.65, 0.56),
    "ShutterFaded": (1.0, 1.0, 0.72),
    "BoxWood": (0.9, 0.82, 0.72),
}
# Scanned CC0 textures (Poly Haven) under Assets/_Project/Art/Textures, with the metres one tile covers.
# Unity applies the same sets in House1Assets; here they only make the .blend and previews honest.
TEXTURES = {
    "Brick": ("brick_4", 1.05),
    "Stone": ("beige_wall_001", 2.0),
    "StoneDark": ("beige_wall_001", 2.0),
    "StoneGrime": ("beige_wall_001", 2.0),
    "Wood": ("weathered_planks", 2.0),
    "BoxWood": ("worn_planks", 1.4),
    "Shutter": ("green_rough_planks", 1.5),
    "ShutterFaded": ("green_rough_planks", 1.5),
}
# Sets mapped through the mesh UVs (in metres) instead of projected, so their pattern follows the surface:
# roof tiles must run down each slope whichever way the face points.
UV_TEXTURES = {
    "RoofTiles": ("clay_roof_tiles_02", 2.5),
}
TEXTURE_ROOT = Path("Assets/_Project/Art/Textures")
# Large-scale dirt and run-off over every scanned set above (Tools/Art/make_wear_detail_map.py); Unity lays the same
# map over House 1's worn materials as an HDRP detail map at this size.
WEAR_DETAIL = TEXTURE_ROOT / "wear_detail" / "wear_detail.png"
WEAR_METRES_PER_TILE = 5.0

# Scanned Poly Haven periwinkle (CC0): six photo-textured plants. Tools/Art/make_flower_textures.py makes the HDRP sets,
# including a copy with geranium-red petals. Material names must match House1Assets.
PLANT_SOURCE = Path("Tools/Art/Sources/periwinkle_plant/periwinkle_plant_2k.gltf")
PLANT_MATERIALS = {"PlantPink": "periwinkle_plant", "PlantRed": "periwinkle_plant_red"}
UPRIGHT_PLANTS = 24
HANGING_PLANTS = 14
PLANT_SCALE = (1.1, 1.6)
PLANT_SINK = 0.12  # plants start below the soil so their bare lower stems are hidden
PLANT_DECIMATE = 0.22  # the scans are dense; at window-box distance a fifth of the triangles reads the same

BOX_CENTRE_PROUD = 0.22
BOX_DEPTH = 0.24
BOX_HEIGHT = 0.22
BOX_EXTRA_LENGTH = 0.45  # beyond the window width
SOIL_TOP = 0.21  # above the box bottom

window_boxes = []  # (side, s, base) of every flower box, planted once the house mesh is built

MATERIAL_INDEX = {name: index for index, name in enumerate(MATERIALS)}

rng = random.Random(24)


# ---------------------------------------------------------------- mesh helpers

class Mesh:
    """One bmesh with a material index per face."""

    def __init__(self):
        self.bm = bmesh.new()
        self.uv = self.bm.loops.layers.uv.new("UVMap")

    def poly(self, corners, material, facing):
        """A single-sided face; it is flipped if needed so its visible side looks along `facing`.

        Unity culls back faces, so every open face must be wound towards where it is seen from.
        """
        face = self.bm.faces.new([self.bm.verts.new(c) for c in corners])
        face.material_index = MATERIAL_INDEX[material]
        face.normal_update()
        if face.normal.dot(Vector(facing)) < 0:
            face.normal_flip()

    def poly_uv(self, corners, uvs, material, facing):
        """Like poly, with a UV (in metres) per corner for UV-mapped materials."""
        verts = [self.bm.verts.new(c) for c in corners]
        uv_of = dict(zip(verts, uvs))
        face = self.bm.faces.new(verts)
        face.material_index = MATERIAL_INDEX[material]
        face.normal_update()
        if face.normal.dot(Vector(facing)) < 0:
            face.normal_flip()
        for loop in face.loops:
            loop[self.uv].uv = uv_of[loop.vert]

    def hexahedron(self, front, back, material):
        """Two matching quads (counter-clockwise seen from the front) joined into a closed block."""
        v = [self.bm.verts.new(c) for c in list(front) + list(back)]
        for ids in ((0, 1, 2, 3), (7, 6, 5, 4), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)):
            face = self.bm.faces.new([v[i] for i in ids])
            face.material_index = MATERIAL_INDEX[material]
            face.normal_update()
            centre = sum((v[i].co for i in ids), Vector()) / 4
            middle = sum((x.co for x in v), Vector()) / 8
            if face.normal.dot(centre - middle) < 0:
                face.normal_flip()

    def box(self, centre, size, material):
        c = Vector(centre)
        h = Vector(size) / 2
        corner = lambda sx, sy, sz: c + Vector((sx * h.x, sy * h.y, sz * h.z))
        front = [corner(-1, -1, -1), corner(1, -1, -1), corner(1, -1, 1), corner(-1, -1, 1)]
        back = [corner(-1, 1, -1), corner(1, 1, -1), corner(1, 1, 1), corner(-1, 1, 1)]
        self.hexahedron(front, back, material)

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
        for face in {f for v in verts for f in v.link_faces}:
            face.material_index = MATERIAL_INDEX[material]

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
            modifier.segments = 2
            modifier.limit_method = "ANGLE"
            modifier.angle_limit = BEVEL_ANGLE
            modifier.use_clamp_overlap = True
        return obj


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

    def quad(self, mesh, s0, s1, h0, h1, d, material):
        mesh.poly([self.point(s0, h0, d), self.point(s1, h0, d), self.point(s1, h1, d), self.point(s0, h1, d)], material, self.outward)

    def block(self, mesh, outline, d0, d1, material):
        """Extrude a four-point (s, h) outline from depth d0 to d1."""
        front = [self.point(s, h, d1) for s, h in outline]
        back = [self.point(s, h, d0) for s, h in outline]
        mesh.hexahedron(front, back, material)


FRONT, RIGHT, BACK, LEFT = range(4)


def sides():
    w, d = WIDTH / 2, DEPTH / 2
    return [
        Side(FRONT, (-w, -d, 0), (1, 0, 0), (0, -1, 0), WIDTH),
        Side(RIGHT, (w, -d, 0), (0, 1, 0), (1, 0, 0), DEPTH),
        Side(BACK, (w, d, 0), (-1, 0, 0), (0, 1, 0), WIDTH),
        Side(LEFT, (-w, d, 0), (0, -1, 0), (-1, 0, 0), DEPTH),
    ]


# ---------------------------------------------------------------- facade layout

def jamb_width(course):
    return 0.2 if course % 2 == 0 else 0.32


class Opening:
    """A window or door with a segmental (or round) arch head."""

    def __init__(self, s, spec, width, kind, flowers=False):
        self.s = s
        self.bottom, self.height, self.rise = spec
        self.width = width
        self.kind = kind
        self.flowers = flowers
        half = width / 2
        self.radius = (half * half + self.rise * self.rise) / (2 * self.rise)
        self.centre_h = self.spring + self.rise - self.radius

    left = property(lambda self: self.s - self.width / 2)
    right = property(lambda self: self.s + self.width / 2)
    spring = property(lambda self: self.bottom + self.height)
    top = property(lambda self: self.spring + self.rise)

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

    def jamb_courses(self):
        """(bottom, top, width) of the alternating stone blocks up each jamb."""
        count = max(2, round(self.height / 0.32))
        step = self.height / count
        return [(self.bottom + k * step, self.bottom + (k + 1) * step, jamb_width(k)) for k in range(count)]

    def covers(self, s, h):
        """True where the stone surround (or the hole) hides the brickwork."""
        if h < self.bottom - 0.14 or h > self.top + ARCH_THICK + 0.12:
            return False
        if h <= self.spring:
            for bottom, top, width in self.jamb_courses():
                if bottom - 0.02 <= h <= top + 0.02 and self.left - width <= s <= self.right + width:
                    return True
            sill = self.kind != "door" and h < self.bottom and abs(s - self.s) < self.width / 2 + 0.3
            return sill
        dist = math.hypot(s - self.s, h - self.centre_h)
        key = 0.1 if abs(s - self.s) < 0.12 else 0.0
        return dist < self.radius + ARCH_THICK + key and abs(s - self.s) < self.width / 2 + 0.36


def openings(side):
    result = []
    for k, fraction in enumerate(BAY_FRACTIONS):
        s = side.length * fraction
        if side.index == FRONT and k == 1:
            result.append(Opening(s, DOOR, DOOR_W, "door"))
        else:
            result.append(Opening(s, GROUND_WINDOW, WINDOW_W + 0.1, "window"))
        first_floor_flowers = side.index == FRONT or (side.index == RIGHT and k == 1)
        bottom, height, rise = UPPER_WINDOW
        result.append(Opening(s, (GROUND_STOREY + bottom, height, rise), WINDOW_W, "window", first_floor_flowers))
        result.append(Opening(s, (GROUND_STOREY + UPPER_STOREY + bottom, height, rise), WINDOW_W, "window"))
    return result


def quoin_course_width(course):
    return 0.6 if course % 2 == 0 else 0.34


QUOIN_COURSE = 0.34


def owns_corners(side):
    """Front and back run past the corners; the sides stop flush against them.

    If both facades ran past a corner, each block's end face would lie in the other's front face and z-fight.
    """
    return side.index in (FRONT, BACK)


def corner_overrun(side, proud):
    return proud if owns_corners(side) else 0.0


def quoin_width_at(side, h):
    if h < PLINTH:
        return 0.0
    return quoin_course_width(int((h - PLINTH) / QUOIN_COURSE) + side.index)


# ---------------------------------------------------------------- weathering

def lowness(h):
    """1 at the ground fading to 0 at 3 m: splash-back, damp and moss gather low on the wall."""
    return max(0.0, 1.0 - h / 3.0)


def stone_material(h):
    roll = rng.random()
    if roll < 0.4 * lowness(h):
        return "Moss"
    if roll < 0.25 + 0.35 * lowness(h):
        return "StoneGrime"
    return "Stone" if rng.random() > 0.15 else "StoneDark"


def in_opening_column(holes, s, h0, h1):
    return any(o.left - 0.4 < s < o.right + 0.4 and h0 < o.top + ARCH_THICK and h1 > o.bottom - 0.2 for o in holes)


def build_weathering(mesh, side, holes):
    """Moss along the top of the plinth; dirt and run-off come from the weathered_grime texture in Unity."""
    for _ in range(rng.randint(10, 16)):
        s = rng.uniform(0.1, side.length - 0.1)
        if in_opening_column(holes, s, 0.0, PLINTH + 0.1):
            continue
        mesh.sphere(side.point(s, PLINTH + 0.01, rng.uniform(0.02, 0.06)),
                    rng.uniform(0.03, 0.07), "Moss")


# ---------------------------------------------------------------- walls

def build_core(mesh):
    inset = 2 * CORE_INSET
    mesh.box((0, 0, EAVES / 2), (WIDTH - inset, DEPTH - inset, EAVES), "Mortar")


def build_wall(mesh, side, holes):
    build_backing(mesh, side, holes)
    build_plinth(mesh, side, holes)
    build_weathering(mesh, side, holes)
    build_quoins(mesh, side)
    build_cornice(mesh, side)


def build_backing(mesh, side, holes):
    """The brick face of the wall (a scanned brick texture), cut around every opening so windows stay recessed."""
    xs = sorted({0.0, side.length} | {o.left for o in holes} | {o.right for o in holes})
    zs = sorted({0.0, EAVES} | {o.bottom for o in holes} | {o.top for o in holes})
    for s0, s1 in zip(xs, xs[1:]):
        for h0, h1 in zip(zs, zs[1:]):
            sc, hc = (s0 + s1) / 2, (h0 + h1) / 2
            if not any(o.left < sc < o.right and o.bottom < hc < o.top for o in holes):
                side.quad(mesh, s0, s1, h0, h1, 0.0, "Brick")


def build_plinth(mesh, side, holes):
    doors = [o for o in holes if o.kind == "door"]
    courses = (0.0, 0.38, PLINTH)
    for row, (h0, h1) in enumerate(zip(courses, courses[1:])):
        overrun = corner_overrun(side, PLINTH_PROUD)
        s = -overrun - (0.25 if row % 2 else 0.0)
        while s < side.length + overrun:
            length = rng.uniform(0.45, 0.85)
            s0, s1 = max(s, -overrun), min(s + length, side.length + overrun)
            s += length
            if any(d.left - 0.02 < (s0 + s1) / 2 < d.right + 0.02 for d in doors):
                continue
            material = stone_material((h0 + h1) / 2)
            proud = PLINTH_PROUD + rng.uniform(-STONE_JITTER, STONE_JITTER) * 0.5
            side.box(mesh, (s0 + s1) / 2, (h0 + h1) / 2, proud / 2, (s1 - s0 - 0.015, h1 - h0 - 0.015, proud), material)


def build_quoins(mesh, side):
    course, h = 0, PLINTH
    while h + QUOIN_COURSE <= EAVES - CORNICE + 0.01:
        # Offsetting the course by side alternates long and short stones around each corner.
        width = quoin_course_width(course + side.index)
        height = QUOIN_COURSE - 0.015
        overrun = corner_overrun(side, SURROUND_PROUD)
        for s in (width / 2 - overrun, side.length - width / 2 + overrun):
            material = stone_material(h)
            proud = SURROUND_PROUD + rng.uniform(-STONE_JITTER, STONE_JITTER)
            side.box(mesh, s, h + height / 2, proud / 2, (width + rng.uniform(-0.03, 0.0), height, proud), material)
        h += QUOIN_COURSE
        course += 1


def build_cornice(mesh, side):
    bottom = EAVES - CORNICE
    for height, proud in ((0.12, 0.06), (0.08, 0.12), (0.1, 0.2)):
        length = side.length + 2 * corner_overrun(side, proud)
        side.box(mesh, side.length / 2, bottom + height / 2, proud / 2, (length, height, proud), "Stone")
        bottom += height


# ---------------------------------------------------------------- windows and doors

def build_opening(mesh, side, opening):
    build_reveal(mesh, side, opening)
    build_surround(mesh, side, opening)
    if opening.kind == "door":
        build_door(mesh, side, opening)
        return
    build_glazing(mesh, side, opening)
    build_shutters(mesh, side, opening)
    if opening.flowers:
        build_flower_box(mesh, side, opening)


def build_reveal(mesh, side, opening):
    """The opening goes back into the wall; its sides and arched head are lined with stone."""
    mid = opening.bottom + opening.height / 2
    for s in (opening.left - 0.02, opening.right + 0.02):
        side.box(mesh, s, mid, -REVEAL / 2, (0.04, opening.height, REVEAL), "StoneDark")
    arc = opening.arc(opening.radius, 10)
    for (s0, h0, _), (s1, h1, _) in zip(arc, arc[1:]):
        mesh.poly([side.point(s0, h0, 0.0), side.point(s1, h1, 0.0), side.point(s1, h1, -REVEAL), side.point(s0, h0, -REVEAL)],
                  "StoneDark", side.point(opening.s, opening.centre_h, 0.0) - side.point(s0, h0, 0.0))
    if opening.kind != "door":
        side.box(mesh, opening.s, opening.bottom - 0.02, -REVEAL / 2, (opening.width + 0.04, 0.04, REVEAL), "Stone")


def build_surround(mesh, side, opening):
    for bottom, top, width in opening.jamb_courses():
        height = top - bottom - 0.012
        material = stone_material(bottom)
        for edge, sign in ((opening.left, -1), (opening.right, 1)):
            side.box(mesh, edge + sign * width / 2, bottom + 0.006 + height / 2, SURROUND_PROUD / 2,
                     (width, height, SURROUND_PROUD), material)
    build_arch(mesh, side, opening)
    if opening.kind != "door":
        side.box(mesh, opening.s, opening.bottom - 0.07, 0.07, (opening.width + 0.5, 0.12, 0.14), "Stone")
        side.box(mesh, opening.s, opening.bottom - 0.16, 0.035, (opening.width + 0.38, 0.06, 0.07), "StoneDark")


def build_arch(mesh, side, opening):
    """Voussoirs fanned around the arch with a taller, prouder keystone in the middle."""
    count = 9 if opening.kind == "door" else 7
    inner = opening.arc(opening.radius, count)
    outer = opening.arc(opening.radius + ARCH_THICK, count)
    gap = 0.006
    for k in range(count):
        is_key = k == count // 2
        lift = 0.1 if is_key else 0.0
        proud = SURROUND_PROUD * (1.8 if is_key else 1.0)
        (s0, h0, a0), (s1, h1, a1) = inner[k], inner[k + 1]
        (t0, g0, _), (t1, g1, _) = outer[k], outer[k + 1]
        ext = lambda s, h, a: (s + math.cos(a) * lift, h + math.sin(a) * lift)
        t0, g0 = ext(t0, g0, a0)
        t1, g1 = ext(t1, g1, a1)
        shrink = lambda p, q: (p[0] + (q[0] - p[0]) * gap * 10, p[1] + (q[1] - p[1]) * gap * 10)
        a, b = shrink((s0, h0), (s1, h1)), shrink((s1, h1), (s0, h0))
        c, d = shrink((t1, g1), (t0, g0)), shrink((t0, g0), (t1, g1))
        material = "Stone" if is_key else stone_material(opening.spring)
        side.block(mesh, [a, b, c, d], 0.0, proud, material)


def build_glazing(mesh, side, opening):
    back = -REVEAL + 0.07
    arc = opening.arc(opening.radius, 10)
    outline = [(opening.left, opening.bottom), (opening.right, opening.bottom)] + [(s, h) for s, h, _ in reversed(arc)]
    # Arc runs left to right; reversed walks back over the top so the polygon stays counter-clockwise.
    mesh.poly([side.point(s, h, back) for s, h in outline], "Glass", side.outward)
    curtain = back - 0.05
    drape = opening.width * 0.3
    side.quad(mesh, opening.left, opening.left + drape, opening.bottom + 0.1, opening.spring, curtain, "Curtain")
    side.quad(mesh, opening.right - drape, opening.right, opening.bottom + 0.1, opening.spring, curtain, "Curtain")
    build_frame(mesh, side, opening, back)


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
    arc = opening.arc(opening.radius - bar / 2, 8)
    for (s0, h0, _), (s1, h1, _) in zip(arc, arc[1:]):
        side.block(mesh, [(s0, h0 - bar / 2), (s1, h1 - bar / 2), (s1, h1 + bar / 2), (s0, h0 + bar / 2)],
                   back, back + depth, "Frame")


def build_shutters(mesh, side, opening):
    width = opening.width / 2 + 0.06
    height = opening.top - opening.bottom + 0.05
    mid = opening.bottom + height / 2 - 0.03
    rail = 0.06
    paint = "ShutterFaded" if rng.random() < 0.15 else "Shutter"
    for sign in (-1, 1):
        centre = opening.s + sign * (opening.width / 2 + 0.34 + width / 2)
        jitter = rng.uniform(-0.008, 0.008)
        side.box(mesh, centre, mid, 0.04 + jitter, (width, height, 0.025), paint)
        for h in (mid - height / 2 + rail / 2, mid, mid + height / 2 - rail / 2):
            side.box(mesh, centre, h, 0.065 + jitter, (width, rail, 0.025), paint)
        for s in (centre - width / 2 + rail / 2, centre + width / 2 - rail / 2):
            side.box(mesh, s, mid, 0.065 + jitter, (rail, height, 0.025), paint)
        build_louvres(mesh, side, centre, mid, (width - 2 * rail, height - 3 * rail), paint)
        for h in (mid - height * 0.32, mid + height * 0.32):
            side.box(mesh, centre - sign * width / 2, h, 0.06, (0.04, 0.025, 0.03), "Iron")


def build_louvres(mesh, side, centre, mid, size, paint):
    """Slats tilted about 45 degrees, front edge low, so each casts a dark shadow line like real louvred shutters."""
    width, span = size
    pitch = 0.075
    drop, depth, thickness = 0.045, 0.04, 0.01
    front, back = 0.075, 0.075 - depth
    for half in (-1, 1):
        base = mid + half * (span / 4 + 0.015)
        count = int((span / 2) / pitch)
        for k in range(count):
            h = base - span / 4 + (k + 0.5) * (span / 2) / count
            s0, s1 = centre - width / 2, centre + width / 2
            top = [side.point(s0, h - drop / 2, front), side.point(s1, h - drop / 2, front),
                   side.point(s1, h + drop / 2, back), side.point(s0, h + drop / 2, back)]
            under = [p - Vector((0, 0, thickness)) for p in top]
            mesh.hexahedron(top, under, paint)


def build_flower_box(mesh, side, opening):
    """A wooden window box; the scanned plants are added after the house mesh is built (see build_plants)."""
    base = opening.bottom - 0.12
    length = opening.width + BOX_EXTRA_LENGTH
    side.box(mesh, opening.s, base + BOX_HEIGHT / 2, BOX_CENTRE_PROUD, (length, BOX_HEIGHT, BOX_DEPTH), "BoxWood")
    side.box(mesh, opening.s, base + SOIL_TOP - 0.005, BOX_CENTRE_PROUD, (length - 0.08, 0.01, BOX_DEPTH - 0.06), "Wood")  # soil
    window_boxes.append((side, opening.s, base, length / 2))


def import_plant_templates():
    bpy.ops.import_scene.gltf(filepath=str(PLANT_SOURCE.resolve()))
    # The two smallest scans are single shoots; only the bushy ones build a full window box.
    templates = sorted((o for o in bpy.context.scene.objects if o.type == "MESH" and o.name.startswith("periwinkle_plant")),
                       key=lambda o: len(o.data.polygons))[2:]
    if not templates:
        raise RuntimeError(f"No periwinkle meshes in {PLANT_SOURCE}")
    return templates, {name: plant_material(name, texture_set) for name, texture_set in PLANT_MATERIALS.items()}


def plant_material(name, texture_set):
    """Opaque scanned-leaf material through the mesh UVs.

    The glTF import's own material fades alpha with vertex colours, which joining into the house drops (the plants
    would vanish), so the preview gets a plain one; Unity remaps the slot anyway.
    """
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    nodes, links = material.node_tree.nodes, material.node_tree.links
    bsdf = nodes.get("Principled BSDF")
    folder = (TEXTURE_ROOT / texture_set).resolve()
    albedo = nodes.new("ShaderNodeTexImage")
    albedo.image = bpy.data.images.load(str(folder / f"{texture_set}_albedo.jpg"))
    links.new(albedo.outputs["Color"], bsdf.inputs["Base Color"])
    normal_image = nodes.new("ShaderNodeTexImage")
    normal_image.image = bpy.data.images.load(str(folder / f"{texture_set}_normal.jpg"))
    normal_image.image.colorspace_settings.name = "Non-Color"
    normal_map = nodes.new("ShaderNodeNormalMap")
    links.new(normal_image.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])
    bsdf.inputs["Roughness"].default_value = 0.55
    return material


def plant(template, material, matrix):
    data = template.data.copy()
    data.materials.clear()
    data.materials.append(material)
    obj = bpy.data.objects.new("Plant", data)
    bpy.context.collection.objects.link(obj)
    obj.matrix_world = matrix
    return obj


def clip_outside_box(obj, side, s, base, half_length):
    """Deletes the parts of a plant that would show through the box or the wall: below the soil inside the box, or
    behind the facade. The scans are whole plants, so whatever is sunk into the soil would poke out of the box sides."""
    front = BOX_CENTRE_PROUD + BOX_DEPTH / 2
    soil = base + SOIL_TOP
    to_world = obj.matrix_world

    def hidden(co):
        offset = to_world @ co - side.origin
        d, h = offset.dot(side.outward), offset.z
        inside_box = abs(offset.dot(side.along) - s) < half_length + 0.02 and d < front + 0.01 and h < soil
        return d < 0.0 or inside_box

    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if hidden(v.co)], context="VERTS")
    bm.to_mesh(obj.data)
    bm.free()


def plant_matrix(position, up, along, scale):
    """A plant standing at `position` with its own +Z along `up`, spun freely about it."""
    up = up.normalized()
    x = (along - up * along.dot(up)).normalized()
    y = up.cross(x)
    spin = Matrix.Rotation(rng.uniform(0, math.tau), 4, "Z")
    basis = Matrix((x, y, up)).transposed().to_4x4()
    return Matrix.Translation(position) @ basis @ spin @ Matrix.Scale(scale, 4)


def build_plants():
    """Scanned plants in every window box: a row standing in the soil and a row hanging over the front edge."""
    templates, materials = import_plant_templates()
    up = Vector((0, 0, 1))
    planted = []
    for box in window_boxes:
        side, s, base, half_length = box
        span = half_length - 0.04
        for k in range(UPRIGHT_PLANTS):
            u = -span + (k + rng.uniform(0.2, 0.8)) * 2 * span / UPRIGHT_PLANTS
            where = side.point(s + u, base + SOIL_TOP - PLANT_SINK, BOX_CENTRE_PROUD + rng.uniform(-0.07, 0.07))
            planted.append((box, where, up))
        for k in range(HANGING_PLANTS):
            u = -span + (k + rng.uniform(0.2, 0.8)) * 2 * span / HANGING_PLANTS
            # Rooted on the box's top front edge, so they spill over it instead of growing out through its front.
            where = side.point(s + u, base + SOIL_TOP, BOX_CENTRE_PROUD + BOX_DEPTH / 2)
            droop = side.outward * rng.uniform(0.7, 1.0) - up * rng.uniform(0.4, 0.75)
            planted.append((box, where, droop))
    objects = []
    for box, where, direction in planted:
        side, s, base, half_length = box
        material = materials["PlantRed" if rng.random() < 0.55 else "PlantPink"]
        obj = plant(rng.choice(templates), material, plant_matrix(where, direction, side.along, rng.uniform(*PLANT_SCALE)))
        clip_outside_box(obj, side, s, base, half_length)
        objects.append(obj)
    for template in templates:
        bpy.data.objects.remove(template)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.object.join()
    plants = bpy.context.view_layer.objects.active
    plants.name = "Plants"
    plants.data.uv_layers[0].name = "UVMap"
    decimate = plants.modifiers.new("Decimate", "DECIMATE")
    decimate.ratio = PLANT_DECIMATE
    return plants


def build_door(mesh, side, opening):
    back = -REVEAL + 0.06
    arc = opening.arc(opening.radius, 12)
    outline = [(opening.left, opening.bottom), (opening.right, opening.bottom)] + [(s, h) for s, h, _ in reversed(arc)]
    mesh.poly([side.point(s, h, back) for s, h in outline], "Wood", side.outward)
    leaf = 0.05
    for k, (h0, h1) in enumerate(((0.15, 1.0), (1.15, 2.1))):
        for s0, s1 in ((opening.left + 0.12, opening.s - 0.06), (opening.s + 0.06, opening.right - 0.12)):
            side.box(mesh, (s0 + s1) / 2, opening.bottom + (h0 + h1) / 2, back + leaf / 2,
                     (s1 - s0, h1 - h0, leaf), "Wood")
    side.box(mesh, opening.s, opening.bottom + opening.height / 2 + opening.rise / 2, back + 0.03,
             (0.05, opening.height + opening.rise, 0.06), "Wood")
    knocker = side.point(opening.s + 0.22, opening.bottom + 1.15, back + 0.09)
    mesh.cylinder(knocker - side.outward * 0.01, knocker + side.outward * 0.01, 0.08, "Iron", segments=12)
    side.box(mesh, opening.s, 0.11, 0.18, (opening.width + 0.7, 0.22, 0.36), "Stone")
    side.box(mesh, opening.s, 0.11, -REVEAL / 2, (opening.width + 0.04, 0.22, REVEAL), "StoneDark")


# ---------------------------------------------------------------- roof

def roof_faces():
    """Each roof face as (eave start, along, inward plan direction, eave length, run to the ridge)."""
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
    rise = math.tan(ROOF_PITCH)
    slope_len = 1 / math.cos(ROOF_PITCH)
    up = Vector((0, 0, 1))
    for face, (start, along, inward, length, run) in enumerate(roof_faces()):
        slope = (inward + up * rise).normalized()
        normal = along.cross(slope).normalized()
        if normal.z < 0:
            normal = -normal

        def at(u, p, lift=0.0):
            return start + along * u + inward * p + up * (base + p * rise) + normal * lift

        mesh.poly([at(0, 0), at(length, 0), at(length - run, run), at(run, run)], "RoofUnder", normal)
        build_tiled_surface(mesh, at, normal, inward, length, run, slope_len)
        build_hip(mesh, at(0, 0), at(run, run))
        build_hip(mesh, at(length, 0), at(length - run, run))
        build_gutter(mesh, at(0, 0) - inward * 0.08, at(length, 0) - inward * 0.08)
        if face == SKYLIGHT[0]:
            build_skylight(mesh, at, normal)
    run = min(WIDTH, DEPTH) / 2 + OVERHANG
    half = (max(WIDTH, DEPTH) - min(WIDTH, DEPTH)) / 2
    ridge_h = base + run * rise
    build_hip(mesh, Vector((0, -half, ridge_h)), Vector((0, half, ridge_h)))


def build_tiled_surface(mesh, at, normal, inward, length, run, slope_len):
    """The tiles as one UV-mapped face (scanned clay tiles, parallax in HDRP) plus their visible edge at the eaves.

    UVs are metres: u along the eave, v up the slope, so the tile courses run level and the barrels run downhill.
    """
    corners = [(0, 0), (length, 0), (length - run, run), (run, run)]
    mesh.poly_uv([at(u, p, ROOF_COVER) for u, p in corners], [(u, p * slope_len) for u, p in corners], "RoofTiles", normal)
    edge = [at(0, 0, ROOF_COVER - EAVE_EDGE), at(length, 0, ROOF_COVER - EAVE_EDGE), at(length, 0, ROOF_COVER), at(0, 0, ROOF_COVER)]
    mesh.poly_uv(edge, [(0, 0), (length, 0), (length, EAVE_EDGE), (0, EAVE_EDGE)], "RoofTiles", -inward)


def build_hip(mesh, a, b):
    pieces = max(1, int((b - a).length / 0.36))
    for k in range(pieces):
        s0 = a + (b - a) * (k / pieces)
        s1 = a + (b - a) * ((k + 0.97) / pieces)
        mesh.cylinder(s0 + Vector((0, 0, 0.1)), s1 + Vector((0, 0, 0.1)), 0.1, "RoofTileC", segments=8)


def build_gutter(mesh, a, b):
    drop = Vector((0, 0, -0.06))
    mesh.cylinder(a + drop, b + drop, 0.07, "Zinc", segments=8)


def build_skylight(mesh, at, normal):
    _, u0, u1, p0, p1 = SKYLIGHT
    lift = 0.12
    mesh.poly([at(u0, p0, lift), at(u1, p0, lift), at(u1, p1, lift), at(u0, p1, lift)], "Glass", normal)
    frame = 0.07
    for (ua, pa), (ub, pb) in (((u0, p0), (u1, p0)), ((u1, p0), (u1, p1)), ((u1, p1), (u0, p1)), ((u0, p1), (u0, p0))):
        mesh.cylinder(at(ua, pa, lift), at(ub, pb, lift), frame, "Zinc", segments=4)


def roof_height_at(x, y):
    """Height of the tiled roof surface above a plan point."""
    w, d = WIDTH / 2 + OVERHANG, DEPTH / 2 + OVERHANG
    p = min(w - abs(x), d - abs(y))
    return EAVES + 0.02 + p * math.tan(ROOF_PITCH)


def build_chimney(mesh):
    x, y = CHIMNEY_XY
    size_x, size_y = 0.9, 0.75
    bottom = roof_height_at(x + size_x / 2, y - size_y / 2) - 0.3
    ridge = EAVES + (min(WIDTH, DEPTH) / 2 + OVERHANG) * math.tan(ROOF_PITCH)
    top = ridge + 0.45
    mesh.box((x, y, (bottom + top) / 2), (size_x, size_y, top - bottom), "Brick")
    mesh.box((x, y, top + 0.06), (size_x + 0.14, size_y + 0.14, 0.12), "Stone")
    mesh.box((x, y, top + 0.16), (size_x - 0.1, size_y - 0.1, 0.08), "Stone")
    mesh.cylinder((x, y, top + 0.2), (x, y, top + 0.55), 0.13, "Terracotta", segments=12)
    mesh.cylinder((x, y, top + 0.5), (x, y, top + 0.58), 0.16, "Terracotta", segments=12)


def build_drainpipes(mesh):
    w, d = WIDTH / 2, DEPTH / 2
    gutter = EAVES - 0.04
    pipes = [((-w + 0.3, -d - 0.1), (0, 1)), ((w - 0.3, -d - 0.1), (0, 1)),
             ((w + 0.1, d - 0.3), (-1, 0)), ((-w - 0.1, d - 0.3), (1, 0))]
    for (x, y), (nx, ny) in pipes:
        out = Vector((-nx, -ny, 0)) * (OVERHANG - 0.1)
        mesh.cylinder((x, y, 0.05), (x, y, gutter - 0.35), 0.05, "Zinc", segments=8)
        mesh.cylinder((x, y, gutter - 0.35), Vector((x, y, gutter - 0.05)) + out * 0.5, 0.05, "Zinc", segments=8)
        mesh.cylinder((x, y, 0.05), (x, y, 0.2), 0.065, "Zinc", segments=8)
        for z in (1.6, 3.8, 6.2, 8.6):
            mesh.box((x + nx * 0.06, y + ny * 0.06, z), (0.12 if ny else 0.1, 0.1 if ny else 0.12, 0.04), "Iron")


def build_lantern_and_plaque(mesh, side, door):
    s = door.right + 0.55
    bracket_h = 2.95
    mesh.cylinder(side.point(s, bracket_h, 0.0), side.point(s, bracket_h, 0.35), 0.018, "Iron", segments=6)
    mesh.cylinder(side.point(s, bracket_h - 0.25, 0.0), side.point(s, bracket_h, 0.25), 0.014, "Iron", segments=6)
    side.box(mesh, s, bracket_h - 0.03, 0.02, (0.1, 0.3, 0.04), "Iron")
    lamp = side.point(s, bracket_h - 0.25, 0.38)
    mesh.box(lamp, (0.2, 0.2, 0.3), "LampGlow")
    for dx in (-0.1, 0.1):
        for dy in (-0.1, 0.1):
            mesh.cylinder(lamp + Vector((dx, dy, -0.16)), lamp + Vector((dx, dy, 0.16)), 0.012, "Iron", segments=4)
    mesh.box(lamp + Vector((0, 0, -0.18)), (0.24, 0.24, 0.05), "Iron")
    mesh.cylinder(lamp + Vector((0, 0, 0.16)), lamp + Vector((0, 0, 0.3)), 0.16, "Iron", segments=8)
    mesh.sphere(lamp + Vector((0, 0, 0.34)), 0.04, "Iron")
    side.box(mesh, s, 2.05, 0.03, (0.36, 0.28, 0.03), "Iron")
    side.box(mesh, s, 2.05, 0.035, (0.32, 0.24, 0.03), "Plaque")
    return side.point(s, 2.05, 0.055)


def build_house_number(position, facing):
    """The "24" on the plaque, as real extruded text."""
    curve = bpy.data.curves.new("HouseNumber", "FONT")
    curve.body = "24"
    curve.size = 0.17
    curve.extrude = 0.004
    curve.align_x = "CENTER"
    curve.align_y = "CENTER"
    text = bpy.data.objects.new("HouseNumber", curve)
    bpy.context.collection.objects.link(text)
    text.rotation_euler = (math.radians(90), 0, math.atan2(facing.y, facing.x) + math.radians(90))
    text.location = position
    bpy.context.view_layer.update()
    evaluated = text.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh = bpy.data.meshes.new_from_object(evaluated)
    mesh.transform(text.matrix_world)
    bpy.data.objects.remove(text)
    obj = bpy.data.objects.new("HouseNumberMesh", mesh)
    bpy.context.collection.objects.link(obj)
    for material in MATERIALS:
        obj.data.materials.append(bpy.data.materials[material])
    for polygon in mesh.polygons:
        polygon.material_index = MATERIAL_INDEX["Iron"]
    return obj


# ---------------------------------------------------------------- scene

def make_materials():
    for name, colour in MATERIALS.items():
        material = bpy.data.materials.new(name)
        material.diffuse_color = (*colour, 1.0)
        material.use_nodes = True
        bsdf = material.node_tree.nodes.get("Principled BSDF")
        bsdf.inputs["Base Color"].default_value = (*colour, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.1 if name == "Glass" else (0.45 if name in ("Zinc", "Iron") else 0.85)
        if name == "Zinc":
            bsdf.inputs["Metallic"].default_value = 0.8
        if name == "LampGlow":
            bsdf.inputs["Emission Color"].default_value = (*colour, 1.0)
            bsdf.inputs["Emission Strength"].default_value = 6.0
        if name in TEXTURES:
            add_box_projected_texture(material, *TEXTURES[name])
        if name in UV_TEXTURES:
            add_uv_texture(material, *UV_TEXTURES[name])


def add_uv_texture(material, texture_set, metres_per_tile):
    """Albedo, normal and height through UVs in metres, the Blender counterpart of HDRP's UV0 mapping with parallax."""
    nodes, links = material.node_tree.nodes, material.node_tree.links
    bsdf = nodes.get("Principled BSDF")
    coords = nodes.new("ShaderNodeTexCoord")
    mapping = nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (1 / metres_per_tile,) * 3
    links.new(coords.outputs["UV"], mapping.inputs["Vector"])
    folder = (TEXTURE_ROOT / texture_set).resolve()

    def image(suffix, colour=True):
        node = nodes.new("ShaderNodeTexImage")
        node.image = bpy.data.images.load(str(folder / f"{texture_set}_{suffix}.jpg"))
        if not colour:
            node.image.colorspace_settings.name = "Non-Color"
        links.new(mapping.outputs["Vector"], node.inputs["Vector"])
        return node

    tint = nodes.new("ShaderNodeMix")
    tint.data_type = "RGBA"
    tint.blend_type = "MULTIPLY"
    tint.inputs["Factor"].default_value = 1.0
    tint.inputs["A"].default_value = bsdf.inputs["Base Color"].default_value
    links.new(image("albedo").outputs["Color"], tint.inputs["B"])
    links.new(tint.outputs["Result"], bsdf.inputs["Base Color"])
    normal_map = nodes.new("ShaderNodeNormalMap")
    links.new(image("normal", colour=False).outputs["Color"], normal_map.inputs["Color"])
    bump = nodes.new("ShaderNodeBump")
    bump.inputs["Distance"].default_value = 0.04
    links.new(image("height", colour=False).outputs["Color"], bump.inputs["Height"])
    links.new(normal_map.outputs["Normal"], bump.inputs["Normal"])
    links.new(bump.outputs["Normal"], bsdf.inputs["Normal"])


def add_wear(nodes, links, coords, colour):
    """Darkens `colour` by the wear detail map the way HDRP applies a detail albedo (x 2 * R, 0.5 is neutral)."""
    mapping = nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (1 / WEAR_METRES_PER_TILE,) * 3
    links.new(coords.outputs["Object"], mapping.inputs["Vector"])
    wear = nodes.new("ShaderNodeTexImage")
    wear.image = bpy.data.images.load(str(WEAR_DETAIL.resolve()))
    wear.image.colorspace_settings.name = "Non-Color"
    wear.projection = "BOX"
    wear.projection_blend = 0.2
    links.new(mapping.outputs["Vector"], wear.inputs["Vector"])
    channels = nodes.new("ShaderNodeSeparateColor")
    links.new(wear.outputs["Color"], channels.inputs["Color"])
    double = nodes.new("ShaderNodeMath")
    double.operation = "MULTIPLY"
    double.inputs[1].default_value = 2.0
    links.new(channels.outputs["Red"], double.inputs[0])
    worn = nodes.new("ShaderNodeMix")
    worn.data_type = "RGBA"
    worn.blend_type = "MULTIPLY"
    worn.inputs["Factor"].default_value = 1.0
    links.new(colour, worn.inputs["A"])
    links.new(double.outputs["Value"], worn.inputs["B"])
    return worn.outputs["Result"]


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
    links.new(add_wear(nodes, links, coords, tint.outputs["Result"]), bsdf.inputs["Base Color"])
    normal_image = nodes.new("ShaderNodeTexImage")
    normal_image.image = bpy.data.images.load(str(folder / f"{texture_set}_normal.jpg"))
    normal_image.image.colorspace_settings.name = "Non-Color"
    normal_image.projection = "BOX"
    normal_image.projection_blend = 0.2
    links.new(mapping.outputs["Vector"], normal_image.inputs["Vector"])
    normal_map = nodes.new("ShaderNodeNormalMap")
    links.new(normal_image.outputs["Color"], normal_map.inputs["Color"])
    links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])


def build_house():
    stone, detail, roof = Mesh(), Mesh(), Mesh()
    build_core(detail)
    number = None
    for side in sides():
        holes = openings(side)
        build_wall(detail, side, holes)
        for opening in holes:
            build_opening(stone if opening.kind == "door" else detail, side, opening)
        if side.index == FRONT:
            door = next(o for o in holes if o.kind == "door")
            number = (build_lantern_and_plaque(detail, side, door), side.outward)
    build_roof(roof)
    build_chimney(detail)
    build_drainpipes(detail)
    # Real masonry and joinery have worn edges; perfectly sharp boxes are what make a model read as a toy.
    parts = [stone.to_object("Stone", bevel=BEVEL_WIDTH), detail.to_object("Detail", bevel=BEVEL_WIDTH),
             roof.to_object("Roof"), build_house_number(*number), build_plants()]
    apply_modifiers(parts)
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[1]
    bpy.ops.object.join()
    house = bpy.context.view_layer.objects.active
    house.name = "House1"
    return house


def apply_modifiers(parts):
    """Joining keeps only the active object's modifiers, so bake each part's bevel first."""
    for part in parts:
        bpy.ops.object.select_all(action="DESELECT")
        bpy.context.view_layer.objects.active = part
        part.select_set(True)
        for modifier in list(part.modifiers):
            bpy.ops.object.modifier_apply(modifier=modifier.name)


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
        add_leaf_bones=False,
        path_mode="STRIP",
    )
    print(f"Exported {OUTPUT} with {len(house.data.polygons)} faces")


def setup_render_scene():
    scene = bpy.context.scene
    bpy.ops.object.light_add(type="SUN")
    sun = bpy.context.active_object
    sun.data.energy = 3.5
    sun.data.color = (1.0, 0.93, 0.85)
    sun.rotation_euler = (math.radians(50), 0, math.radians(-30))
    world = bpy.data.worlds.new("Sky")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.42, 0.41, 0.4, 1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 1.0
    scene.world = world
    scene.render.engine = "CYCLES"
    scene.cycles.samples = 48
    scene.cycles.use_denoising = True
    scene.view_settings.view_transform = "AgX"
    bpy.ops.object.camera_add()
    scene.camera = bpy.context.active_object


def render_view(folder, name, location, target, ortho_scale=None, size=(900, 1100)):
    scene = bpy.context.scene
    camera = scene.camera
    camera.location = location
    camera.rotation_euler = (Vector(target) - Vector(location)).to_track_quat("-Z", "Y").to_euler()
    camera.data.type = "ORTHO" if ortho_scale else "PERSP"
    if ortho_scale:
        camera.data.ortho_scale = ortho_scale
    else:
        camera.data.lens = 35
    scene.render.resolution_x, scene.render.resolution_y = size
    scene.render.filepath = str(Path(folder) / f"{name}.png")
    bpy.ops.render.render(write_still=True)


def render_previews(folder):
    setup_render_scene()
    mid = (0, 0, 6.6)
    render_view(folder, "front", (0, -40, 6.6), mid, ortho_scale=14.5)
    render_view(folder, "right", (40, 0, 6.6), mid, ortho_scale=14.5)
    render_view(folder, "three_quarter", (17, -20, 9), (0, 0, 6.0), size=(1100, 1100))
    print(f"Previews written to {folder}")


def main():
    args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    bpy.ops.wm.read_factory_settings(use_empty=True)
    make_materials()
    house = build_house()
    export(house)
    BLEND_OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_OUTPUT.resolve()))
    if "--preview" in args:
        render_previews(args[args.index("--preview") + 1])


main()
