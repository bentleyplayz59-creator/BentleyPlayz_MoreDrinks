#!/usr/bin/env python3
"""Pixel art for the More Drinks counter (visual only).

Produces one 32x32 sheet laid out as a 2x2 tile grid:
  row 0 (y 0-15)  : counter TOP   -> Front layer   at Saloon (16,18) and (17,18)
  row 1 (y 16-31) : counter FRONT -> Buildings layer at Saloon (16,19) and (17,19)
The Buildings tiles keep the shop Action and solidity; the Front tiles carry no
properties, so they are decoration only.

ALIGNMENT RULES (measured from the vanilla Saloon bar, tiles Front 919 / Buildings 951):
  TOP tile    y0-5   transparent (floor / Gus show through; only items may rise into it)
              y6     dark back edge line
              y7-15  flat orange counter surface
  FRONT tile  y0-2   surface continues (top lip: surface, highlight, shadow line)
              y3-14  maroon panelling
              y15    soft floor shadow (semi-transparent)
The old art started at y0 with a black outline and ended with a black outline at
y31, which made the kiosk stick up ~6px above the bar and sit 1px taller than it.
Everything here uses the vanilla bar's own colours so the seams disappear.
"""
from PIL import Image

T = (0, 0, 0, 0)
def c(r, g, b, a=255): return (r, g, b, a)

# --- Vanilla Saloon bar palette (sampled from the real bar) -----------------
EDGE   = c(105, 43, 27)    # back edge / dark frame line
MAR    = c(89, 23, 38)     # panel maroon
SURF   = c(165, 85, 6)     # counter surface orange
SURF_L = c(182, 103, 5)    # surface highlight / front lip
SURF_D = c(135, 65, 5)     # surface shadow / joint
SHADOW = c(40, 0, 70, 56)  # soft floor shadow under the bar

# --- Sign / accessory palette ------------------------------------------------
B0    = c(104, 72, 34)     # brass shadow
B1    = c(160, 114, 52)    # brass mid
B2    = c(210, 164, 82)    # brass light
B3    = c(244, 214, 130)   # brass highlight
SIGN  = c(52, 16, 28)      # dark sign board (a shade below the panel maroon)
CR0   = c(188, 160, 118)   # cream shade
CR1   = c(228, 206, 160)   # cream
CR2   = c(250, 236, 196)   # cream highlight
S0    = c(44, 40, 40)      # enamel dark
S1    = c(76, 70, 68)      # enamel mid
S2    = c(116, 108, 100)   # enamel light
S3    = c(168, 158, 142)   # enamel highlight
CF    = c(60, 34, 22)      # coffee
RED   = c(150, 62, 48)     # kettle glaze accent
RED2  = c(190, 92, 66)

GLYPHS = {  # 5 rows tall
    "D": ("110", "101", "101", "101", "110"),
    "R": ("110", "101", "110", "101", "101"),
    "I": ("111", "010", "010", "010", "111"),
    "N": ("1001", "1101", "1011", "1001", "1001"),
    "K": ("101", "101", "110", "101", "101"),
    "S": ("111", "100", "111", "001", "111"),
}

def px(img, x, y, col):
    if 0 <= x < img.width and 0 <= y < img.height:
        img.putpixel((x, y), col)

def rect(img, x0, y0, x1, y1, col):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            px(img, x, y, col)

def hline(img, x0, x1, y, col):
    for x in range(x0, x1 + 1): px(img, x, y, col)

def vline(img, x, y0, y1, col):
    for y in range(y0, y1 + 1): px(img, x, y, col)


# ---- ASCII sprites ---------------------------------------------------------
def blit(im, rows, ox, oy, legend):
    w = len(rows[0])
    for r in rows:
        assert len(r) == w, (r, len(r), w)
    for j, r in enumerate(rows):
        for i, ch in enumerate(r):
            if ch != '.':
                px(im, ox + i, oy + j, legend[ch])

# Items are drawn one-per-tile so nothing straddles the tile seam at x=16.
#   left tile  (x0-15)  : espresso machine with a cup under the spout
#   right tile (x16-31) : teapot + a stack of two cups
O  = c(40, 24, 20)         # item outline
MACH = {
    'O': O, 'H': S3, 't': S2, 'e': S1, 'k': S0,
    'R': c(200, 60, 48), 'G': CR2, 'g': CR1, 'b': B2, 'd': B0, 'o': CF,
}
MACHINE = [        # 12 wide x 12 tall
    "..OOOOOOOO..",
    ".OHHHHHHHHO.",
    "OHttttttttkO",
    "OtGGGtttRRkO",
    "OtGgGtttRRkO",
    "OtGGGttttekO",
    "OttttttttekO",
    "OeeeeeeeeekO",
    "OekOekkOekkO",
    "OOOOOOOOOOOO",
    ".OgGGOOOdbO.",
    ".OOOOOOOOOO.",
]
TEA = {
    'O': O, 'C': CR2, 'c': CR1, 'w': CR0, 'r': RED2, 'R': RED, 'b': B3, 'd': B2,
}
TEAPOT = [         # 11 wide x 9 tall
    "....Obb....",
    "...OOOOO...",
    "..OCCCccO..",
    "OOOCcccwOO.",
    "O.OcccccwOO",
    "O.OcrrrwwOO",
    "OOOcrrrwwwO",
    "..OOcwwwwO.",
    "...OOOOOO..",
]
CUPSTACK = [       # 4 wide x 6 tall
    "OOOO",
    "OCcO",
    "OOOO",
    "OCwO",
    "OcwO",
    ".OO.",
]

# Joint positions inside each 16px tile (the vanilla bar has a board joint at x=2).
JOINT_X = (2, 18)

def draw_top(im):
    """Counter top, 32x16 (Front layer). y0-5 stay transparent like the vanilla bar top."""
    hline(im, 0, 31, 6, EDGE)                       # back edge
    rect(im, 0, 7, 31, 15, SURF)                    # flat orange surface
    for jx in JOINT_X:
        vline(im, jx, 7, 15, SURF_D)
        vline(im, jx - 1, 10, 15, SURF_L)
        vline(im, jx + 1, 12, 15, SURF_L)

def contact_shadow(im, x0, x1, y):
    for x in range(x0, x1 + 1):
        px(im, x, y, SURF_D)

def draw_objects(im):
    """One group per tile, base resting on the surface at y13, soft contact shadow at y14."""
    contact_shadow(im, 2, 13, 14)
    blit(im, MACHINE, 2, 2, MACH)          # x2..13, y2..13 (centred in tile x0-15)
    contact_shadow(im, 18, 30, 14)
    blit(im, TEAPOT, 17, 5, TEA)           # x17..27, y5..13
    blit(im, CUPSTACK, 27, 8, TEA)         # x27..30, y8..13 (overlaps teapot handle column)

def draw_front(im):
    """Counter front face, 32x16 (Buildings layer tiles, carries the shop Action)."""
    # y0-2: surface lip that continues straight from the top tile (matches vanilla 951)
    hline(im, 0, 31, 0, SURF)
    hline(im, 0, 31, 1, SURF_L)
    hline(im, 0, 31, 2, SURF_D)
    for jx in JOINT_X:
        px(im, jx - 1, 0, SURF_L); px(im, jx, 0, SURF_D); px(im, jx + 1, 0, SURF_L)
        px(im, jx, 1, SURF_D)
        px(im, jx, 2, EDGE)
    # y3-14: panelling
    rect(im, 0, 3, 31, 14, MAR)
    # brass-framed DRINKS plaque, x1..30, y4..13
    fx0, fx1, fy0, fy1 = 1, 30, 4, 13
    rect(im, fx0, fy0, fx1, fy1, B1)
    hline(im, fx0, fx1, fy0, B3)
    hline(im, fx0, fx1, fy1, B0)
    vline(im, fx0, fy0, fy1, B2)
    vline(im, fx1, fy0, fy1, B0)
    rect(im, fx0 + 1, fy0 + 1, fx1 - 1, fy1 - 1, SIGN)
    # lettering, centred, y6..10
    word = "DRINKS"
    total = sum(len(GLYPHS[ch][0]) for ch in word) + len(word) - 1
    inner_w = (fx1 - 1) - (fx0 + 1) + 1
    x = fx0 + 1 + (inner_w - total) // 2
    y = fy0 + 2
    for ch in word:
        g = GLYPHS[ch]
        for r, bits in enumerate(g):
            for cidx, bit in enumerate(bits):
                if bit == "1":
                    px(im, x + cidx, y + r, B3 if r < 3 else B2)
        x += len(g[0]) + 1
    # y15: soft floor shadow, same as the vanilla bar's bottom row
    hline(im, 0, 31, 15, SHADOW)

def build():
    sheet = Image.new("RGBA", (32, 32), T)
    top = Image.new("RGBA", (32, 16), T)
    front = Image.new("RGBA", (32, 16), T)
    draw_top(top)
    draw_objects(top)
    draw_front(front)
    sheet.paste(top, (0, 0))
    sheet.paste(front, (0, 16))
    return sheet

if __name__ == "__main__":
    build().save("counter_art_test.png")
