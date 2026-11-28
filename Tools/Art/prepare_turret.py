"""Turns the Sketchfab "large-caliber turret" (stasbelyk13, CC-BY) into a Unity-ready FBX and HDRP texture set.

Run headless from the repo root, with the unzipped download in <download folder>:
    blender --background --disable-autoexec <download folder>/source/Sketchfab_2022_05_31_23_58_20.blend
        --python Tools/Art/prepare_turret.py -- <download folder>

What it changes:
- Bones get clear names: Base (fixed), Yaw (turns left/right), Pitch (barrels up/down). The aim-target bone,
  its constraints and the demo animation are dropped; Unity drives Yaw and Pitch from code.
- A "Muzzle" empty, parented to Pitch, marks the middle of the barrel tips for tracers and muzzle flashes.
- The model is scaled to half size (4.8 m of gun is too long for a town-house roof).
- Textures go to 2048 px: base colour as-is, the DirectX normal map flipped to OpenGL, and metallic + roughness
  packed into an HDRP mask map (R metallic, G occlusion 1, B detail 1, A smoothness).
"""
import sys
from pathlib import Path

import bpy
import numpy as np
from mathutils import Vector

MODEL_OUTPUT = Path("Assets/_Project/Art/Models/Turret/Turret.fbx")
TEXTURE_FOLDER = Path("Assets/_Project/Art/Models/Turret/Textures")
TEXTURE_NAME = "Turret"
TEXTURE_SIZE = 2048
EXPORT_SCALE = 0.5

ARMATURE_NAME = "111"
MESH_NAME = "222"
BONE_NAMES = {"Bone": "Base", "Bone.002": "Yaw", "Bone.001": "Pitch"}
AIM_TARGET_BONE = "Bone.003"

# Vertices this close to the furthest barrel point count as the barrel tips.
TIP_TOLERANCE_METRES = 0.03


def main():
    download = Path(sys.argv[sys.argv.index("--") + 1])
    armature = bpy.data.objects[ARMATURE_NAME]
    mesh = bpy.data.objects[MESH_NAME]
    strip_animation(armature, mesh)
    apply_smoothing(mesh)
    rename_and_trim_bones(armature)
    add_muzzle(armature, mesh)
    armature.name = "TurretRig"
    mesh.name = "TurretMesh"
    export_fbx(armature, mesh)
    export_textures(download / "textures")


def strip_animation(armature, mesh):
    for pose_bone in armature.pose.bones:
        for constraint in list(pose_bone.constraints):
            pose_bone.constraints.remove(constraint)
        pose_bone.location = (0, 0, 0)
        pose_bone.rotation_quaternion = (1, 0, 0, 0)
        pose_bone.scale = (1, 1, 1)
    for obj in (armature, mesh):
        obj.animation_data_clear()
    for action in list(bpy.data.actions):
        bpy.data.actions.remove(action)


def apply_smoothing(mesh):
    # The Auto Smooth geometry-nodes modifier would not survive FBX export, so bake it into the mesh.
    bpy.context.view_layer.objects.active = mesh
    for modifier in [m for m in mesh.modifiers if m.type == "NODES"]:
        with bpy.context.temp_override(object=mesh, active_object=mesh):
            bpy.ops.object.modifier_apply(modifier=modifier.name)


def rename_and_trim_bones(armature):
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.mode_set(mode="EDIT")
    edit_bones = armature.data.edit_bones
    edit_bones.remove(edit_bones[AIM_TARGET_BONE])
    bpy.ops.object.mode_set(mode="OBJECT")
    for old, new in BONE_NAMES.items():
        armature.data.bones[old].name = new


def add_muzzle(armature, mesh):
    group = mesh.vertex_groups["Pitch"].index
    points = [mesh.matrix_world @ v.co for v in mesh.data.vertices
              if any(g.group == group and g.weight > 0.5 for g in v.groups)]
    # The barrels point along -Y in the source file.
    tip_y = min(p.y for p in points)
    tips = [p for p in points if p.y <= tip_y + TIP_TOLERANCE_METRES]
    centre = sum(tips, Vector()) / len(tips)

    muzzle = bpy.data.objects.new("Muzzle", None)
    bpy.context.scene.collection.objects.link(muzzle)
    muzzle.parent = armature
    muzzle.parent_type = "BONE"
    muzzle.parent_bone = "Pitch"
    bpy.context.view_layer.update()
    muzzle.matrix_world.translation = centre
    print(f"Muzzle at {tuple(round(c, 3) for c in centre)}")


def export_fbx(armature, mesh):
    MODEL_OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.context.scene.objects:
        obj.select_set(obj.type in {"ARMATURE", "MESH", "EMPTY"})
    bpy.ops.export_scene.fbx(
        filepath=str(MODEL_OUTPUT),
        use_selection=True,
        object_types={"ARMATURE", "MESH", "EMPTY"},
        global_scale=EXPORT_SCALE,
        apply_scale_options="FBX_SCALE_ALL",
        add_leaf_bones=False,
        bake_anim=False,
        use_mesh_modifiers=False,
        path_mode="STRIP",
        embed_textures=False,
    )


def export_textures(source):
    TEXTURE_FOLDER.mkdir(parents=True, exist_ok=True)
    albedo = read(source / "333_Base_Color.png")
    write(albedo, f"{TEXTURE_NAME}_albedo.png", "sRGB")

    normal = read(source / "333_Normal_DirectX.png")
    normal[..., 1] = 1.0 - normal[..., 1]
    write(normal, f"{TEXTURE_NAME}_normal.png", "Non-Color")

    metallic = read(source / "333_Metallic-333_Roughness@channels=B.png")[..., 2]
    roughness = read(source / "333_Metallic-333_Roughness@channels=G.png")[..., 1]
    mask = np.stack([metallic, np.ones_like(metallic), np.ones_like(metallic), 1.0 - roughness], axis=-1)
    write(mask, f"{TEXTURE_NAME}_mask.png", "Non-Color")


def read(path):
    image = bpy.data.images.load(str(path))
    image.colorspace_settings.name = "Non-Color"
    image.scale(TEXTURE_SIZE, TEXTURE_SIZE)
    pixels = np.empty(TEXTURE_SIZE * TEXTURE_SIZE * 4, dtype=np.float32)
    image.pixels.foreach_get(pixels)
    return pixels.reshape(TEXTURE_SIZE, TEXTURE_SIZE, 4)


def write(pixels, name, colorspace):
    image = bpy.data.images.new(name, TEXTURE_SIZE, TEXTURE_SIZE, alpha=True)
    image.colorspace_settings.name = colorspace
    image.pixels.foreach_set(np.ascontiguousarray(pixels, dtype=np.float32).ravel())
    image.filepath_raw = str((TEXTURE_FOLDER / name).resolve())
    image.file_format = "PNG"
    image.save()
    print(f"Wrote {TEXTURE_FOLDER / name}")


main()
