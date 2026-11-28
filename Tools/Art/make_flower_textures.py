"""Turns the scanned Poly Haven periwinkle (CC0, Tools/Art/Sources/periwinkle_plant) into HDRP texture sets for the
House 1 window boxes: the photo atlas as-is (pink flowers) and a copy whose petals are shifted to geranium red.

Usage: python -I Tools/Art/make_flower_textures.py
Writes Assets/_Project/Art/Textures/periwinkle_plant{,_red}/<name>_{albedo.jpg,normal.jpg,mask.png}.
"""
import colorsys
import shutil
from pathlib import Path

import numpy as np
from PIL import Image

SOURCE = Path("Tools/Art/Sources/periwinkle_plant/textures")
OUTPUT = Path("Assets/_Project/Art/Textures")
NAME = "periwinkle_plant"
RED_NAME = "periwinkle_plant_red"

# The petals are the only magenta in the atlas; leaves, stems and the black background are far from these hues.
PETAL_HUE_MIN = 0.78
PETAL_HUE_MAX = 0.97
PETAL_MIN_SATURATION = 0.25
GERANIUM_HUE = 0.995
GERANIUM_SATURATION_BOOST = 1.25
GERANIUM_VALUE = 0.85
FULL_DETAIL = 255


def write_mask(folder, name):
    """HDRP mask map from Poly Haven's ARM: R metallic (arm B), G occlusion (arm R), B detail mask, A smoothness."""
    arm = np.asarray(Image.open(SOURCE / f"{NAME}_arm_2k.jpg").convert("RGB")).astype(np.uint8)
    mask = np.stack([arm[..., 2], arm[..., 0], np.full(arm.shape[:2], FULL_DETAIL, np.uint8), 255 - arm[..., 1]], axis=-1)
    Image.fromarray(mask, "RGBA").save(folder / f"{name}_mask.png")


def redden(albedo):
    rgb = albedo.astype(np.float32) / 255
    hsv = np.vectorize(colorsys.rgb_to_hsv, otypes=[np.float32] * 3)(rgb[..., 0], rgb[..., 1], rgb[..., 2])
    hue, saturation, value = hsv
    petal = (hue >= PETAL_HUE_MIN) & (hue <= PETAL_HUE_MAX) & (saturation >= PETAL_MIN_SATURATION)
    hue = np.where(petal, GERANIUM_HUE, hue)
    saturation = np.where(petal, np.minimum(1, saturation * GERANIUM_SATURATION_BOOST), saturation)
    value = np.where(petal, value * GERANIUM_VALUE, value)
    red = np.stack(np.vectorize(colorsys.hsv_to_rgb, otypes=[np.float32] * 3)(hue, saturation, value), axis=-1)
    return (red * 255).clip(0, 255).astype(np.uint8)


def write_set(name, albedo):
    folder = OUTPUT / name
    folder.mkdir(parents=True, exist_ok=True)
    Image.fromarray(albedo).save(folder / f"{name}_albedo.jpg", quality=92)
    shutil.copyfile(SOURCE / f"{NAME}_nor_gl_2k.jpg", folder / f"{name}_normal.jpg")
    write_mask(folder, name)
    print(f"Wrote {folder}")


def main():
    albedo = np.asarray(Image.open(SOURCE / f"{NAME}_diff_2k.jpg").convert("RGB"))
    write_set(NAME, albedo)
    write_set(RED_NAME, redden(albedo))


main()
