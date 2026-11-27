"""Generates a tileable "weathered_grime" PBR texture set for HDRP: soft dirt blotches, vertical run-off and fine grain.

The albedo is near white so it multiplies a material's own colour, darkening it unevenly instead of recolouring it.
Usage: python -I Tools/Art/make_weathering_texture.py
Writes Assets/_Project/Art/Textures/weathered_grime/weathered_grime_{albedo.jpg,normal.jpg,mask.png}.
"""
from pathlib import Path

import numpy as np
from PIL import Image

NAME = "weathered_grime"
OUTPUT = Path("Assets/_Project/Art/Textures") / NAME
SIZE = 1024
SEED = 24

ALBEDO_BRIGHT = 0.97
ALBEDO_DIRTY = 0.6
WARM_TINT = np.array([1.0, 0.97, 0.92])
SMOOTH_CLEAN = 0.3
SMOOTH_DIRTY = 0.08
NORMAL_STRENGTH = 2.5

rng = np.random.default_rng(SEED)


def tileable_noise(cells_x, cells_y):
    """Smooth value noise that wraps at the edges, sampled on a SIZE x SIZE grid."""
    grid = rng.random((cells_y, cells_x))
    ys = np.linspace(0, cells_y, SIZE, endpoint=False)
    xs = np.linspace(0, cells_x, SIZE, endpoint=False)
    y0, x0 = np.floor(ys).astype(int), np.floor(xs).astype(int)
    fy, fx = ys - y0, xs - x0
    fy, fx = fy * fy * (3 - 2 * fy), fx * fx * (3 - 2 * fx)
    y1, x1 = (y0 + 1) % cells_y, (x0 + 1) % cells_x
    a = grid[np.ix_(y0, x0)]
    b = grid[np.ix_(y0, x1)]
    c = grid[np.ix_(y1, x0)]
    d = grid[np.ix_(y1, x1)]
    top = a + (b - a) * fx[None, :]
    bottom = c + (d - c) * fx[None, :]
    return top + (bottom - top) * fy[:, None]


def fractal(cells, octaves, stretch_y=1.0):
    total, weight, amplitude = np.zeros((SIZE, SIZE)), 0.0, 1.0
    for octave in range(octaves):
        n = cells * 2 ** octave
        total += amplitude * tileable_noise(n, max(1, int(n / stretch_y)))
        weight += amplitude
        amplitude *= 0.5
    return total / weight


def smoothstep(edge0, edge1, x):
    t = np.clip((x - edge0) / (edge1 - edge0), 0, 1)
    return t * t * (3 - 2 * t)


def dirt_map():
    """0 = clean, 1 = dirty."""
    blotches = smoothstep(0.42, 0.75, fractal(4, 5))
    runs = smoothstep(0.5, 0.8, fractal(16, 4, stretch_y=8.0))
    grain = fractal(64, 3)
    return np.clip(0.65 * blotches + 0.45 * runs + 0.35 * (grain - 0.5), 0, 1)


def height_map():
    return fractal(48, 4) * 0.7 + fractal(160, 2) * 0.3


def normal_from_height(height):
    dx = (np.roll(height, -1, axis=1) - np.roll(height, 1, axis=1)) * NORMAL_STRENGTH
    dy = (np.roll(height, -1, axis=0) - np.roll(height, 1, axis=0)) * NORMAL_STRENGTH
    # OpenGL convention: green points up the image, so flip the row-gradient sign.
    n = np.dstack((-dx, dy, np.ones_like(height)))
    n /= np.linalg.norm(n, axis=2, keepdims=True)
    return n * 0.5 + 0.5


def to_image(values, mode):
    return Image.fromarray(np.clip(values * 255 + 0.5, 0, 255).astype(np.uint8), mode)


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    dirt = dirt_map()
    brightness = ALBEDO_BRIGHT + (ALBEDO_DIRTY - ALBEDO_BRIGHT) * dirt
    albedo = brightness[:, :, None] * WARM_TINT[None, None, :]
    to_image(albedo, "RGB").save(OUTPUT / f"{NAME}_albedo.jpg", quality=92)
    to_image(normal_from_height(height_map()), "RGB").save(OUTPUT / f"{NAME}_normal.jpg", quality=95)
    smoothness = SMOOTH_CLEAN + (SMOOTH_DIRTY - SMOOTH_CLEAN) * dirt
    ones = np.ones_like(dirt)
    mask = np.dstack((np.zeros_like(dirt), ones, ones, smoothness))
    to_image(mask, "RGBA").save(OUTPUT / f"{NAME}_mask.png")
    print(f"Wrote {OUTPUT}")


main()
