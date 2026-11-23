#!/usr/bin/env python3
"""Render the More Drinks counter in the context of the Saloon map.

  python tools/preview_saloon.py OUT_PREFIX [--old]
      [--saloon-tmx WHITE_COKE/Maps_Saloon.tmx --assets-dir WHITE_COKE]

With --saloon-tmx and --assets-dir the real vanilla tilesheets are used
(files named like the tmx image sources, e.g.
.._.._.._.._.._.._.._.._Maps_townInterior.png). Without them a labelled
stand-in is drawn for the vanilla tiles, which is only good for checking the
counter's own proportions, not its seams against the real bar.
Writes OUT_PREFIX_1x.png and OUT_PREFIX_6x.png (nearest-neighbor).
"""
import argparse, sys, xml.etree.ElementTree as ET
from pathlib import Path
from PIL import Image, ImageDraw

sys.path.insert(0, str(Path(__file__).resolve().parent))
import counter_art

REGION = (10, 14, 24, 24)        # x0, y0, x1, y1 in tiles (x1/y1 exclusive)
LAYERS = ["Back", "Back2", "Buildings", "Front", "Front2"]

def load_map(path):
    root = ET.parse(path).getroot()
    sets = []
    for ts in root.findall("tileset"):
        src = ts.find("image").get("source")
        sets.append((int(ts.get("firstgid")), int(ts.get("columns")), src))
    layers = {}
    w = int(root.get("width"))
    for l in root.findall("layer"):
        layers[l.get("name")] = [int(t.get("gid", 0)) for t in l.find("data").findall("tile")]
    return w, sets, layers

def sheet_tile(sets, sheets, gid):
    for firstgid, cols, src in reversed(sets):
        if gid >= firstgid:
            i = gid - firstgid
            img = sheets.get(src)
            if img is None:
                return None
            return img.crop(((i % cols) * 16, (i // cols) * 16, (i % cols) * 16 + 16, (i // cols) * 16 + 16))
    return None

def standin_tile(gid, layer):
    """Neutral, clearly fake stand-ins. Not vanilla art."""
    im = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    if layer == "Back":
        light = gid % 2 == 1
        d.rectangle((0, 0, 15, 15), fill=(206, 146, 92) if light else (190, 130, 80))
        d.line((0, 15, 15, 15), fill=(160, 104, 62))
    elif layer == "Buildings" and gid in (910, 911, 912, 913, 914, 915, 916, 917, 942, 943, 944, 945, 946, 947, 948, 949):
        d.rectangle((0, 0, 15, 15), fill=(104, 62, 40)); d.line((0, 8, 15, 8), fill=(78, 46, 30))
    elif layer == "Buildings" and gid in (951, 3224):
        d.rectangle((0, 0, 15, 15), fill=(100, 58, 36), outline=(46, 28, 20))
        d.rectangle((3, 3, 12, 12), outline=(78, 46, 30))
    elif layer == "Front" and gid in (919, 920, 894):
        d.rectangle((0, 0, 15, 15), fill=(150, 98, 58))
        d.line((0, 0, 15, 0), fill=(46, 28, 20)); d.line((0, 14, 15, 14), fill=(190, 140, 80))
        d.line((0, 15, 15, 15), fill=(46, 28, 20))
    elif gid in (887,):
        d.ellipse((3, 4, 12, 13), fill=(138, 54, 44), outline=(46, 28, 20))
    elif gid in (855,):
        pass
    elif gid != 0:
        d.rectangle((0, 0, 15, 15), fill=(120, 90, 70, 200))
    return im

def old_kiosk():
    """The pre-redesign 32x16 art, reproduced from the original build script."""
    from PIL import ImageDraw as ID
    kiosk = Image.new("RGBA", (32, 16), (0, 0, 0, 0)); kd = ID.Draw(kiosk)
    kd.rectangle((2, 0, 29, 6), fill=(66, 42, 32, 255), outline=(190, 139, 77, 255))
    kd.rectangle((3, 1, 28, 5), fill=(119, 76, 48, 255))
    kd.rectangle((0, 8, 31, 15), fill=(91, 54, 38, 255))
    kd.line((0, 8, 31, 8), fill=(202, 153, 91, 255)); kd.line((0, 9, 31, 9), fill=(53, 36, 30, 255))
    kd.rectangle((0, 12, 31, 14), fill=(111, 68, 45, 255)); kd.line((0, 14, 31, 14), fill=(65, 42, 33, 255))
    kd.rectangle((3, 7, 12, 12), fill=(62, 55, 48, 255), outline=(38, 34, 31, 255))
    kd.ellipse((17, 9, 23, 13), fill=(112, 65, 48, 255), outline=(52, 38, 32, 255))
    kd.rectangle((26, 10, 29, 12), fill=(216, 194, 151, 255), outline=(78, 53, 39, 255))
    return kiosk

def render(tmx, assets, use_old):
    w, sets, layers = load_map(tmx)
    sheets = {}
    real = assets is not None
    if real:
        for _, _, src in sets:
            f = Path(assets) / (src + ".png")
            if f.exists():
                sheets[src] = Image.open(f).convert("RGBA")
    x0, y0, x1, y1 = REGION
    out = Image.new("RGBA", ((x1 - x0) * 16, (y1 - y0) * 16), (30, 20, 16, 255))
    art = counter_art.build()
    top_tiles = [art.crop((0, 0, 16, 16)), art.crop((16, 0, 32, 16))]
    front_tiles = [art.crop((0, 16, 16, 32)), art.crop((16, 16, 32, 32))]
    old = old_kiosk()
    for lname in LAYERS:
        data = layers.get(lname)
        for ty in range(y0, y1):
            for tx in range(x0, x1):
                gid = data[ty * w + tx] if data else 0
                override = None
                if use_old:
                    if lname == "Buildings" and ty == 19 and tx in (16, 17):
                        override = old.crop(((tx - 16) * 16, 0, (tx - 16) * 16 + 16, 16))
                else:
                    if lname == "Front" and ty == 18 and tx in (16, 17):
                        override = top_tiles[tx - 16]
                    if lname == "Buildings" and ty == 19 and tx in (16, 17):
                        override = front_tiles[tx - 16]
                if override is not None:
                    tile = override
                elif gid == 0:
                    continue
                else:
                    tile = sheet_tile(sets, sheets, gid) if real else standin_tile(gid, lname)
                    if tile is None:
                        continue
                out.alpha_composite(tile, ((tx - x0) * 16, (ty - y0) * 16))
    return out

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("prefix")
    ap.add_argument("--saloon-tmx", default=str(Path(__file__).resolve().parents[1] / "WHITE_COKE" / "Maps_Saloon.tmx"))
    ap.add_argument("--assets-dir")
    ap.add_argument("--old", action="store_true")
    a = ap.parse_args()
    img = render(a.saloon_tmx, a.assets_dir, a.old)
    if not a.assets_dir:
        ImageDraw.Draw(img).text((2, 2), "STAND-IN vanilla tiles", fill=(255, 255, 255, 255))
    img.save(f"{a.prefix}_1x.png")
    img.resize((img.width * 6, img.height * 6), Image.NEAREST).save(f"{a.prefix}_6x.png")
    print("wrote", a.prefix)

if __name__ == "__main__":
    main()
