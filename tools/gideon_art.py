#!/usr/bin/env python3
"""Pixel art for Gideon, Gus's cousin (visual only).

Gideon is derived from the vanilla Gus sprite sheet and portraits in WHITE_COKE/
(the decoded vanilla files). Nothing is hand-drawn from scratch, so his frame layout
matches Gus exactly (4 columns x 7 rows of 16x32 walking/idle frames; portraits are
a 2x2 grid of 64x64 expressions).

Changes from Gus:
  * jacket recolored orange -> teal, collar yellow -> cream, blue tie/sweater -> red
  * curly handlebar mustache (sprite: front + side frames; portraits: all 4 faces)
"""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
SPRITE_SRC = ROOT / "WHITE_COKE" / "Characters_Gus.png"    # 64x224
PORTRAIT_SRC = ROOT / "WHITE_COKE" / "Portraits_Gus.png"   # 128x128

# ---------------------------------------------------------------- sprite sheet
SPRITE_SHIRT = {
    (247, 143, 4, 255):  (46, 160, 150, 255),    # jacket mid   -> teal
    (183, 93, 0, 255):   (30, 115, 112, 255),    # jacket shade
    (119, 57, 2, 255):   (18, 76, 84, 255),      # jacket dark
    (70, 38, 0, 255):    (10, 44, 54, 255),      # jacket darkest
    (255, 187, 0, 255):  (238, 228, 196, 255),   # collar highlight -> cream
    (30, 58, 150, 255):  (176, 34, 52, 255),     # tie blue -> red
    (16, 18, 82, 255):   (110, 16, 34, 255),     # tie shade
}
SPRITE_MUSTACHE = (41, 21, 6, 255)

# (frame_x, frame_y) -> (kind, mouth_row).  F front, R right-facing, L left-facing,
# B back, X = recolor only (odd poses where the mustache anchor is unclear).
def _frames():
    f = {}
    for fx in range(4):
        f[(fx, 0)] = ("F", 15 if fx % 2 == 0 else 16)
        f[(fx, 1)] = ("R", 14 if fx % 2 == 0 else 15)
        f[(fx, 2)] = ("B", None)
        f[(fx, 3)] = ("L", 14 if fx % 2 == 0 else 15)
        f[(fx, 4)] = ("F", 16)
    f[(0, 5)] = ("F", 16); f[(1, 5)] = ("F", 13); f[(2, 5)] = ("X", 16)
    f[(0, 6)] = ("F", 15); f[(1, 6)] = ("F", 15); f[(2, 6)] = ("F", 15); f[(3, 6)] = ("X", 16)
    return f

def build_sprite(src=None):
    src = Image.open(src or SPRITE_SRC).convert("RGBA")
    out = src.copy()
    px = out.load()
    w, h = out.size

    def put(fx, fy, x, y):
        if 0 <= x < 16 and 0 <= y < 32 and fx * 16 + x < w and fy * 32 + y < h:
            px[fx * 16 + x, fy * 32 + y] = SPRITE_MUSTACHE

    for (fx, fy), (kind, my) in _frames().items():
        if (fy + 1) * 32 > h:
            continue
        ox, oy = fx * 16, fy * 32
        y0 = 15 if kind == "B" else my + 1
        for y in range(y0, 28):
            for x in range(16):
                p = px[ox + x, oy + y]
                if p in SPRITE_SHIRT:
                    px[ox + x, oy + y] = SPRITE_SHIRT[p]
        if kind == "F":
            mr = my - 1
            for x in range(4, 12):
                put(fx, fy, x, mr)
            for (x, y) in [(3, mr), (2, mr), (2, mr - 1), (3, mr - 2)]:   # curled tips
                put(fx, fy, x, y); put(fx, fy, 15 - x, y)
        elif kind in ("R", "L"):
            pts = [(9, my - 2), (10, my - 2), (11, my - 2), (12, my - 2),
                   (9, my - 1), (10, my - 1), (11, my - 1), (12, my - 1),
                   (13, my - 1), (14, my - 1), (14, my - 2), (14, my - 3)]
            for (x, y) in pts:
                put(fx, fy, x if kind == "R" else 15 - x, y)
    return out

# ------------------------------------------------------------------- portraits
PORTRAIT_SHIRT = {
    (219, 130, 0, 255): (46, 160, 150, 255),
    (173, 98, 0, 255):  (30, 115, 112, 255),
    (144, 72, 0, 255):  (18, 76, 84, 255),
    (255, 222, 0, 255): (238, 228, 196, 255),
    (37, 33, 161, 255): (176, 34, 52, 255),
    (77, 71, 224, 255): (214, 70, 84, 255),
    (20, 20, 80, 255):  (110, 16, 34, 255),
    (40, 102, 11, 255): (232, 214, 166, 255),
    (32, 67, 22, 255):  (178, 160, 118, 255),
}
_F = (111, 49, 0, 255)    # mustache brown
_G = (83, 24, 34, 255)    # mustache shadow
_S = (250, 160, 92, 255)  # skin (inside the curl)
# curl ring for the LEFT end, mirrored about x = 26.5 for the right end
_CURL = [
    (14, 28, _S), (15, 28, _S), (16, 28, _S), (14, 29, _S), (16, 29, _S),
    (14, 30, _S), (15, 30, _S), (16, 30, _S),
    (14, 27, _F), (15, 27, _F), (16, 27, _F), (13, 28, _F), (17, 28, _F),
    (13, 29, _F), (15, 29, _G), (17, 29, _F), (13, 30, _F), (17, 30, _F),
    (13, 31, _F), (14, 31, _G), (15, 31, _F), (16, 31, _F), (17, 31, _F),
    (13, 32, _G),
]

def build_portrait(src=None):
    out = Image.open(src or PORTRAIT_SRC).convert("RGBA")
    px = out.load()
    for ty in range(2):
        for tx in range(2):
            ox, oy = tx * 64, ty * 64
            for y in range(37, 64):
                for x in range(64):
                    p = px[ox + x, oy + y]
                    if p in PORTRAIT_SHIRT:
                        px[ox + x, oy + y] = PORTRAIT_SHIRT[p]
            for (x, y, c) in _CURL:
                px[ox + x, oy + y] = c
                px[ox + 53 - x, oy + y] = c
    return out

def build():
    """Return (sprite_sheet 64x224, portrait_sheet 128x128)."""
    sprite, portrait = build_sprite(), build_portrait()
    assert sprite.size == (64, 224) and portrait.size == (128, 128)
    return sprite, portrait

if __name__ == "__main__":
    s, p = build()
    s.save("Gideon.png"); p.save("Gideon_Portrait.png")
    print("wrote Gideon.png, Gideon_Portrait.png")
