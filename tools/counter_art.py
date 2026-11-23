#!/usr/bin/env python3
"""Pixel art for the More Drinks counter (visual only).

Produces one 32x32 sheet laid out as a 2x2 tile grid:
  row 0 (y 0-15)  : counter TOP   -> Front layer   at Saloon (16,18) and (17,18)
  row 1 (y 16-31) : counter FRONT -> Buildings layer at Saloon (16,19) and (17,19)
The Buildings tiles keep the shop Action and solidity; the Front tiles carry no
properties, so they are decoration only.
"""
from PIL import Image

T = (0, 0, 0, 0)
def c(r, g, b, a=255): return (r, g, b, a)

# Warm Saloon palette: walnut, golden brown, brass, cream, dark enamel.
OUT   = c(38, 22, 16)      # darkest outline
W0    = c(58, 33, 22)      # deep shadow walnut
W1    = c(82, 48, 30)      # dark walnut
W2    = c(110, 66, 40)     # mid walnut
W3    = c(140, 88, 50)     # golden brown
W4    = c(172, 118, 66)    # light golden brown
W5    = c(200, 148, 88)    # highlight
B0    = c(104, 72, 34)     # brass shadow
B1    = c(160, 114, 52)    # brass mid
B2    = c(210, 164, 82)    # brass light
B3    = c(244, 214, 130)   # brass highlight
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
GRN   = c(78, 112, 84)

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

MACH_O = c(36, 26, 26)
MACH = {
    'O': MACH_O, 'a': B3, 'b': B2, 'm': B1, 'd': B0,
    's': S3, 't': S2, 'e': S1, 'k': S0,
    'C': CR2, 'c': CR1, 'w': CR0, 'r': RED2, 'o': CF,
}
MACHINE = [
    "..OOOOO..",
    ".ObbbbmdO",
    "ObbbbbbmO",
    "OsttttteO",
    "OCceeeerO",
    "OeedmdeeO",
    "OekcockeO",
    "OekccckeO",
    "OOOOOOOOO",
]
KET_O = c(64, 28, 24)
KET = {
    'O': KET_O, 'R': RED, 'r': RED2, 'h': c(226, 138, 104), 'y': c(112, 46, 38),
    'a': B3, 'b': B2, 'm': B1, 'd': B0,
}
KETTLE = [
    "...ObO...",
    "..ObaaO..",
    ".OOOOOOO.",
    "OmOhrRRyO",
    ".OOrRRRyO",
    "..ObbbbmO",
    "..OrRRRyO",
    "..OyyyyyO",
    "...OOOOO.",
]
CUPS_O = c(54, 36, 30)
CUPS = {'O': CUPS_O, 'c': CR1, 'C': CR2, 'w': CR0, 'o': CF, 't': c(156, 110, 52)}
CUP = [
    ".OOOO..",
    "OCoocO.",
    "OcccwOO",
    ".OcwwO.",
    "OOwwwOO",
]

def draw_top(im):
    """Counter top, 32x16 (Front layer). Opaque so it covers Gus's legs like the vanilla bar."""
    hline(im, 0, 31, 0, OUT)
    hline(im, 1, 30, 1, W1)
    for y in range(2, 13):
        hline(im, 1, 30, y, W3 if y < 10 else W2)
    hline(im, 1, 30, 2, W2)
    for x in range(1, 31):
        if x % 2 == 0: px(im, x, 9, W3)
    hline(im, 1, 30, 3, W4)           # glossy back highlight
    hline(im, 3, 28, 4, W5)
    # long, sparse grain streaks (no speckle)
    for x0, x1, y, col in ((2, 9, 7, W2), (14, 22, 7, W2), (24, 29, 7, W2),
                           (5, 11, 10, W1)):
        hline(im, x0, x1, y, col)
    # front nosing with brass strip
    hline(im, 1, 30, 13, B3)
    hline(im, 1, 30, 14, B1)
    hline(im, 1, 30, 15, OUT)
    for x in (0, 31): vline(im, x, 0, 15, OUT)
    vline(im, 1, 2, 12, W4); vline(im, 30, 2, 12, W1)
    px(im, 1, 13, B3); px(im, 30, 13, B2)
    px(im, 1, 14, B2); px(im, 30, 14, B0)

def contact_shadow(im, x0, x1, y):
    for x in range(x0, x1 + 1):
        px(im, x, y, W1)

def draw_objects(im):
    contact_shadow(im, 3, 12, 12)
    blit(im, MACHINE, 3, 3, MACH)          # x3..11, y3..11
    contact_shadow(im, 15, 24, 12)
    blit(im, KETTLE, 15, 3, KET)           # x15..23, y3..11
    contact_shadow(im, 24, 30, 12)
    blit(im, CUP, 24, 7, CUPS)             # x24..30, y7..11

def draw_front(im):
    """Counter front face, 32x16 (Buildings layer tiles, carries the shop Action)."""
    rect(im, 0, 0, 31, 15, W2)
    hline(im, 0, 31, 0, OUT)
    hline(im, 1, 30, 1, W0)
    # vertical grain on the stiles / rail
    for x in range(1, 31):
        if x % 5 == 2: vline(im, x, 13, 14, W1)
    # posts
    vline(im, 1, 2, 14, W4)
    vline(im, 30, 2, 14, W1)
    # brass-framed plaque built into the panelling
    fx0, fx1, fy0, fy1 = 2, 29, 2, 12
    rect(im, fx0, fy0, fx1, fy1, B1)
    hline(im, fx0, fx1, fy0, B3)
    hline(im, fx0, fx1, fy1, B0)
    vline(im, fx0, fy0, fy1, B2)
    vline(im, fx1, fy0, fy1, B0)
    rect(im, fx0 + 1, fy0 + 1, fx1 - 1, fy1 - 1, W0)
    hline(im, fx0 + 1, fx1 - 1, fy0 + 1, OUT)
    # lettering
    word = "DRINKS"
    total = sum(len(GLYPHS[ch][0]) for ch in word) + len(word) - 1
    inner_w = (fx1 - 1) - (fx0 + 1) + 1
    x = fx0 + 1 + (inner_w - total) // 2
    y = fy0 + 3
    for ch in word:
        g = GLYPHS[ch]
        for r, bits in enumerate(g):
            for cidx, bit in enumerate(bits):
                if bit == "1":
                    px(im, x + cidx, y + r, B3 if r < 3 else B2)
        x += len(g[0]) + 1
    # toe kick
    hline(im, 2, 29, 13, W1)
    hline(im, 2, 29, 14, W0)
    hline(im, 1, 30, 15, OUT)
    for x in (0, 31): vline(im, x, 0, 15, OUT)

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
