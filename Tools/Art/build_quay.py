"""Builds the canal quay pieces (docs/CanalTownArtList.md #10 and #11), concept: canal-boatyard.png.

Two tileable 8 m segments: `QuayWall` (a stone quay wall with its 4 m paved strip, bollard, mooring rings and chain
posts) and `QuaySteps` (the same with a flight of steps down to the water). Both are exported by running this script.

Conventions: X runs along the canal, the water lies towards -Y, the origin is the top of the quay edge (paving level,
z = 0) at the middle of the segment. The water surface is WATER_LEVEL below the paving.
Run headless from the repo root:
    blender --background --factory-startup --python Tools/Art/build_quay.py -- [--preview <folder>] [--piece QuaySteps]
"""
import sys
from pathlib import Path

sys.path.append(str(Path(__file__).resolve().parent))
import canal_kit as kit  # noqa: E402

LENGTH = 8.0
QUAY_WIDTH = 4.0
WATER_LEVEL = -1.2
WALL_FOOT = -2.0  # below the water, where the canal bed meets the wall
COPING_DEPTH = 0.55
COPING_HEIGHT = 0.25
LANDING_ABOVE_WATER = 0.06
STEP_WIDTH = 2.2
STEP_RISE = 0.2
STEP_TREAD = 0.32
CHAIN_POST_SPACING = 2.0
HALF = LENGTH / 2


def build_wall(parts, gap=None):
    """The quay face and its coping; `gap` is an (x0, x1) span left open for steps."""
    spans = [(-HALF, HALF)] if gap is None else [(-HALF, gap[0]), (gap[1], HALF)]
    for x0, x1 in spans:
        kit.build_coursed_wall(parts.stone, x0, x1, WALL_FOOT, -COPING_HEIGHT, 0.0, -1, waterline=WATER_LEVEL)
        kit.build_coping(parts.stone, x0, x1, 0.0, COPING_DEPTH, 0.0, -1, height=COPING_HEIGHT)
    for x in (-HALF + 1.2, HALF - 1.2):
        if gap is None or not gap[0] - 0.5 < x < gap[1] + 0.5:
            kit.build_ring(parts.detail, (x, -0.1, WATER_LEVEL + 0.55), (0, 1, 0), 0.12, 0.018, "Iron")
            parts.detail.box((x, -0.06, WATER_LEVEL + 0.68), (0.12, 0.08, 0.08), "Iron")


def build_paving(parts, stairwell=None):
    """The quay strip: cobbles behind the coping, finished at the back with a kerb; open over a stairwell (x0, x1, back)."""
    y0, y1 = COPING_DEPTH - 0.08, QUAY_WIDTH
    pieces = [(-HALF, HALF, y0)]
    if stairwell is not None:
        x0, x1, back = stairwell
        pieces = [(-HALF, x0, y0), (x1, HALF, y0), (x0, x1, back)]
    for xa, xb, ya in pieces:
        parts.detail.poly([(xa, ya, -0.01), (xb, ya, -0.01), (xb, y1, -0.01), (xa, y1, -0.01)], "Cobble", (0, 0, 1))
    parts.detail.box((0, y1 - 0.12, -0.05), (LENGTH, 0.24, 0.1), "StoneDark")


def build_chain_railing(parts, gap=None):
    """Short stone posts along the edge with an iron chain sagging between them, left open at steps."""
    xs = [-HALF + CHAIN_POST_SPACING / 2 + k * CHAIN_POST_SPACING for k in range(int(LENGTH / CHAIN_POST_SPACING))]
    posts = [x for x in xs if gap is None or not gap[0] - 0.3 < x < gap[1] + 0.3]
    for x in posts:
        parts.stone.box((x, 0.3, 0.35), (0.22, 0.22, 0.7), "Stone")
        parts.stone.box((x, 0.3, 0.73), (0.28, 0.28, 0.06), "StoneDark")
    for a, b in zip(posts, posts[1:]):
        if b - a < CHAIN_POST_SPACING + 0.01:
            kit.build_chain(parts.detail, (a + 0.11, 0.3, 0.6), (b - 0.11, 0.3, 0.6))


def build_steps(parts, x0, x1):
    """A flight cut into the quay, from the paving down to a landing at the water, between two cheek walls."""
    count = round(-WATER_LEVEL / STEP_RISE)
    back = count * STEP_TREAD
    middle = (x0 + x1) / 2
    for k in range(count):
        top = -(k + 1) * STEP_RISE
        front = back - (k + 1) * STEP_TREAD
        parts.stone.box((middle, front + STEP_TREAD / 2, (top + WALL_FOOT) / 2), (x1 - x0, STEP_TREAD, top - WALL_FOOT),
                        kit.waterline_material(top, WATER_LEVEL))
    parts.stone.box((middle, back + 0.1, -STEP_RISE / 2), (x1 - x0, 0.2, STEP_RISE), "StoneDark")
    landing = WATER_LEVEL + LANDING_ABOVE_WATER
    parts.stone.box((middle, -0.5, (landing + WALL_FOOT) / 2), (x1 - x0, 1.0, landing - WALL_FOOT), "StoneGrime")
    for x, inward in ((x0, 1), (x1, -1)):
        centre_x = x - inward * 0.2
        parts.stone.box((centre_x, (back - 1.0) / 2, (WALL_FOOT - COPING_HEIGHT) / 2), (0.4, back + 1.0, -WALL_FOOT - COPING_HEIGHT),
                        "StoneDark")
        parts.stone.box((centre_x, (back - 1.0) / 2, -COPING_HEIGHT / 2), (0.5, back + 1.1, COPING_HEIGHT), "Stone")
    return back


def build_quay_wall(parts):
    build_wall(parts)
    build_paving(parts)
    build_chain_railing(parts)
    kit.build_bollard(parts.detail, 1.0, 0.75)


def build_quay_steps(parts):
    gap = (-STEP_WIDTH / 2 - 0.4, STEP_WIDTH / 2 + 0.4)
    build_wall(parts, gap)
    back = build_steps(parts, -STEP_WIDTH / 2, STEP_WIDTH / 2)
    build_paving(parts, (gap[0], gap[1], back))
    build_chain_railing(parts, gap)
    for x in (gap[0] - 0.8, gap[1] + 0.8):
        kit.build_bollard(parts.detail, x, 0.75)


PIECES = {
    "QuayWall": kit.Piece(build_quay_wall, WALL_FOOT, (WATER_LEVEL, -30.0, 0.0)),
    "QuaySteps": kit.Piece(build_quay_steps, WALL_FOOT, (WATER_LEVEL, -30.0, 0.0)),
}
kit.run_pieces(PIECES, seed=10)
