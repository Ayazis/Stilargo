"""Turns the "weathered_grime" texture (Tools/Art/make_weathering_texture.py) into an HDRP detail map, so brick, stone
and wood get large-scale dirt and run-off on top of their scanned texture without the scan's tiling showing.

HDRP detail map channels: R albedo (0.5 = unchanged, lower darkens), G normal Y, B smoothness (0.5 = unchanged),
A normal X. The normal is left flat; the scans carry the relief.

Usage: python -I Tools/Art/make_wear_detail_map.py
Writes Assets/_Project/Art/Textures/wear_detail/wear_detail.png.
"""
from pathlib import Path

import numpy as np
from PIL import Image

SOURCE = Path("Assets/_Project/Art/Textures/weathered_grime/weathered_grime_albedo.jpg")
OUTPUT = Path("Assets/_Project/Art/Textures/wear_detail/wear_detail.png")

NEUTRAL = 0.5
FLAT_NORMAL = 0.5
CLEAN_BRIGHTNESS = 0.97  # the grime texture's clean value (ALBEDO_BRIGHT in make_weathering_texture.py)
DARKENING = 0.55  # how much of the grime's darkness reaches the albedo; the concept is only lightly soiled
DULLING = 0.5  # dirty patches are rougher


def main():
    brightness = np.asarray(Image.open(SOURCE).convert("L")).astype(np.float32) / 255
    dirt = np.clip(CLEAN_BRIGHTNESS - brightness, 0, 1)
    albedo = np.clip(NEUTRAL - dirt * DARKENING, 0, 1)
    smoothness = np.clip(NEUTRAL - dirt * DULLING, 0, 1)
    flat = np.full_like(albedo, FLAT_NORMAL)
    detail = np.stack([albedo, flat, smoothness, flat], axis=-1)
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray((detail * 255).round().astype(np.uint8), "RGBA").save(OUTPUT)
    print(f"Wrote {OUTPUT}, darkest albedo {albedo.min():.2f}")


main()
