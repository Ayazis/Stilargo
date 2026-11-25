"""Builds the Mediterranean facade kit and exports one FBX per piece.

Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_facade_kit.py

Conventions (Blender space, metres): each piece fills one 2 m wide bay, its origin is the bottom centre of the bay
on the wall surface, Z is up and the piece sticks out of the wall towards -Y (Blender's front). Material slot names
(Trim, Glass, Shutter, Wood) are remapped to HDRP materials by WorldPrefabBuilder in Unity.
"""
import math
from pathlib import Path

import bpy

OUTPUT = Path("Assets/_Project/Art/Models/FacadeKit")

WINDOW_WIDTH = 0.9
WINDOW_HEIGHT = 1.4
WINDOW_SILL_HEIGHT = 0.9
TRIM_WIDTH = 0.12
TRIM_DEPTH = 0.05
SHUTTER_THICKNESS = 0.03
SHUTTER_SLATS = 10
SHUTTER_WIDTH = 0.38
DOOR_WIDTH = 1.1
DOOR_HEIGHT = 2.2
CORNICE_LENGTH = 2.0


def reset_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def material(name):
    existing = bpy.data.materials.get(name)
    return existing if existing else bpy.data.materials.new(name)


def box(name, size, centre, material_name):
    """Adds an axis-aligned box; size and centre are (x, y, z) in metres, y negative = out of the wall."""
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=centre)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    obj.data.materials.append(material(material_name))
    return obj


def join(name, parts):
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    piece = bpy.context.active_object
    piece.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return piece


def frame(width, height, bottom):
    """A stone surround around an opening of width x height whose bottom edge is at `bottom`."""
    half = width / 2
    outer = half + TRIM_WIDTH / 2
    depth_centre = -TRIM_DEPTH / 2
    top = bottom + height
    return [
        box("TrimLeft", (TRIM_WIDTH, TRIM_DEPTH, height + TRIM_WIDTH), (-outer, depth_centre, bottom + height / 2), "Trim"),
        box("TrimRight", (TRIM_WIDTH, TRIM_DEPTH, height + TRIM_WIDTH), (outer, depth_centre, bottom + height / 2), "Trim"),
        box("TrimTop", (width + 2 * TRIM_WIDTH, TRIM_DEPTH * 1.6, TRIM_WIDTH * 1.3), (0, -TRIM_DEPTH * 0.8, top + TRIM_WIDTH * 0.65), "Trim"),
    ]


def shutter(x, bottom):
    width = SHUTTER_WIDTH
    parts = [box("ShutterFrame", (width, SHUTTER_THICKNESS, WINDOW_HEIGHT), (x, -SHUTTER_THICKNESS / 2, bottom + WINDOW_HEIGHT / 2), "Shutter")]
    pitch = WINDOW_HEIGHT / (SHUTTER_SLATS + 1)
    for slat in range(SHUTTER_SLATS):
        z = bottom + pitch * (slat + 1)
        piece = box("Slat", (width * 0.86, 0.012, pitch * 0.55), (x, -SHUTTER_THICKNESS - 0.004, z), "Shutter")
        piece.rotation_euler = (math.radians(-30), 0, 0)
        parts.append(piece)
    return parts


def build_window():
    bottom = WINDOW_SILL_HEIGHT
    parts = [box("Glass", (WINDOW_WIDTH, 0.01, WINDOW_HEIGHT), (0, -0.005, bottom + WINDOW_HEIGHT / 2), "Glass")]
    mullion = 0.04
    parts.append(box("Mullion", (mullion, 0.02, WINDOW_HEIGHT), (0, -0.015, bottom + WINDOW_HEIGHT / 2), "Wood"))
    parts.append(box("Transom", (WINDOW_WIDTH, 0.02, mullion), (0, -0.015, bottom + WINDOW_HEIGHT * 0.7), "Wood"))
    parts += frame(WINDOW_WIDTH, WINDOW_HEIGHT, bottom)
    parts.append(box("Sill", (WINDOW_WIDTH + 0.3, 0.16, 0.07), (0, -0.08, bottom - 0.035), "Trim"))
    shutter_offset = WINDOW_WIDTH / 2 + TRIM_WIDTH / 2 + SHUTTER_WIDTH / 2 + 0.01
    parts += shutter(-shutter_offset, bottom)
    parts += shutter(shutter_offset, bottom)
    return join("Window", parts)


def build_door():
    parts = [box("DoorLeaf", (DOOR_WIDTH, 0.04, DOOR_HEIGHT), (0, -0.02, DOOR_HEIGHT / 2), "Wood")]
    plank = DOOR_WIDTH / 6
    for index in range(1, 6):
        parts.append(box("Groove", (0.015, 0.012, DOOR_HEIGHT * 0.96), (-DOOR_WIDTH / 2 + plank * index, -0.045, DOOR_HEIGHT / 2), "Trim"))
    parts += frame(DOOR_WIDTH, DOOR_HEIGHT, 0.0)
    parts.append(box("Step", (DOOR_WIDTH + 0.5, 0.35, 0.12), (0, -0.175, 0.06), "Trim"))
    return join("Door", parts)


def build_cornice():
    parts = [
        box("CorniceBand", (CORNICE_LENGTH, 0.1, 0.18), (0, -0.05, -0.3), "Trim"),
        box("CorniceMid", (CORNICE_LENGTH, 0.18, 0.08), (0, -0.09, -0.17), "Trim"),
        box("CorniceTop", (CORNICE_LENGTH, 0.26, 0.09), (0, -0.13, -0.045), "Trim"),
    ]
    return join("Cornice", parts)


def export(piece):
    bpy.ops.object.select_all(action="DESELECT")
    piece.select_set(True)
    path = OUTPUT / f"{piece.name}.fbx"
    bpy.ops.export_scene.fbx(
        filepath=str(path),
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
    print(f"Exported {path}")


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for build in (build_window, build_door, build_cornice):
        reset_scene()
        export(build())


main()
