"""Packs a Poly Haven roughness map into an HDRP mask map (R metallic, G occlusion, B detail mask, A smoothness).

Usage: python -I Tools/Art/pack_mask_maps.py <texture folder> [<texture folder> ...]
Each folder must hold <name>_roughness.jpg; <name>_mask.png is written next to it.
"""
import sys
from pathlib import Path

from PIL import Image, ImageChops

NON_METAL = 0
NO_OCCLUSION = 255
FULL_DETAIL = 255


def pack(folder: Path) -> Path:
    roughness_path = next(folder.glob("*_roughness.jpg"))
    roughness = Image.open(roughness_path).convert("L")
    smoothness = ImageChops.invert(roughness)
    size = roughness.size
    mask = Image.merge("RGBA", (
        Image.new("L", size, NON_METAL),
        Image.new("L", size, NO_OCCLUSION),
        Image.new("L", size, FULL_DETAIL),
        smoothness,
    ))
    out = roughness_path.with_name(roughness_path.name.replace("_roughness.jpg", "_mask.png"))
    mask.save(out)
    roughness_path.unlink()
    return out


if __name__ == "__main__":
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    for arg in sys.argv[1:]:
        print(pack(Path(arg)))
