#!/usr/bin/env python3
from pathlib import Path
from itertools import combinations
import json, zipfile, xml.etree.ElementTree as ET
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "dist" / "BentleyPlayz_MoreDrinks" / "Content"
ASSETS = OUT / "assets"
ASSETS.mkdir(parents=True, exist_ok=True)

COFFEES = [
"Amber Roast","Maple Latte","Vanilla Cloud","Caramel Swirl","Mocha Velvet","Hazelnut Hearth","Cinnamon Cappuccino","Toffee Macchiato","Cocoa Bean Brew","Brown Sugar Latte",
"Midnight Espresso","Honeycomb Coffee","Almond Cream Roast","Butterscotch Brew","Peppermint Mocha","Campfire Coffee","Gingerbread Latte","Coconut Cream Coffee","Cherry Mocha","Pecan Praline Brew",
"Blueberry Crumble Coffee","Orange Zest Espresso","Lavender Latte","Pistachio Cappuccino","Rose Mocha","Smoked Maple Coffee","White Chocolate Latte","Salted Caramel Brew","Cardamom Coffee","Marshmallow Mocha",
"Apple Pie Latte","Blackberry Cream Coffee","Toasted Sesame Latte","Dark Cocoa Espresso","Sunrise Blend","Golden Dalgona","Iced River Roast","Frosted Vanilla Coffee","Spiced Plum Latte","Cranberry Mocha",
"Cookie Crumble Coffee","Honey Oat Latte","Brown Butter Brew","Starfruit Espresso","Moonlight Mocha","Praline Swirl Coffee","Citrus Cream Latte","Wintermint Roast","Wild Berry Mocha","Royal Reserve Coffee"]
TEAS = [
"Spring Blossom Tea","Mint Meadow Tea","Peach Garden Tea","Jasmine Pearl Tea","Blueberry Sage Tea","Rosehip Sunset Tea","Lemon Verbena Tea","Wildflower Honey Tea","Cranberry Spice Tea","Lavender Haze Tea",
"Apple Orchard Tea","Ginger Citrus Tea","Hibiscus Glow Tea","Chamomile Cloud Tea","Pomegranate Petal Tea","Blackberry Bramble Tea","Vanilla Rooibos Tea","Mango Sunrise Tea","Pear Blossom Tea","Elderflower Mist Tea",
"Spearmint Frost Tea","Cinnamon Orchard Tea","Golden Turmeric Tea","Raspberry Leaf Tea","Coconut Jasmine Tea","White Peach Oolong","Blood Orange Tea","Blue Lotus Tea","Strawberry Fields Tea","Mulberry Garden Tea",
"Maple Chai Tea","Lemongrass Breeze Tea","Apricot Nectar Tea","Rosemary Citrus Tea","Pineapple Sage Tea","Winterberry Tea","Honeydew Green Tea","Plum Blossom Tea","Passionfruit Tea","Cucumber Mint Tea",
"Vanilla Orchid Tea","Blackcurrant Tea","Sunset Hibiscus Tea","Cocoa Nib Chai","Clover Honey Tea","Moonflower Tea","Citrus Blossom Tea","Golden Pear Tea","Wild Rose Tea","Starlight White Tea"]
assert len(COFFEES) == len(TEAS) == 50 and len(set(COFFEES + TEAS)) == 100

PALETTES = [(105,58,37),(153,91,54),(190,125,70),(87,54,43),(124,76,61),(205,150,92),(88,110,84),(120,151,112),(163,103,137),(120,102,166),(78,133,164),(198,104,81),(211,174,94),(98,142,137),(160,119,82),(173,129,163),(95,113,157),(184,148,119),(126,145,91),(196,119,96)]
STATS = ["Speed","LuckLevel","FarmingLevel","FishingLevel","ForagingLevel","MiningLevel","CombatLevel","Defense","Attack","MagneticRadius","MaxStamina"]

# Use the original 16x16 Stardew Valley Coffee (395) and Green Tea (614)
# item sprites as the source silhouettes. Recolor only their existing pixels;
# transparent pixels and dark outlines remain untouched.
SOURCE_ASSETS = ROOT / "DecodedAssets"
coffee_source = Image.open(SOURCE_ASSETS / "VanillaCoffee.png").convert("RGBA")
tea_source = Image.open(SOURCE_ASSETS / "VanillaGreenTea.png").convert("RGBA")
assert coffee_source.size == tea_source.size == (16, 16)

def recolor_vanilla_sprite(source, palette, variant, tea):
    base = tuple(int(c) for c in palette)
    accent = (236, 220, 174) if tea else (245, 224, 166)
    dark = tuple(max(0, c - 48) for c in base)
    out = Image.new("RGBA", source.size, (0, 0, 0, 0))
    src = source.load()
    dst = out.load()
    for y in range(16):
        for x in range(16):
            r, g, b, a = src[x, y]
            if a == 0:
                continue
            lum = (r * 299 + g * 587 + b * 114) // 1000
            if lum < 52:
                color = (r, g, b)
            elif lum > 220:
                color = accent
            elif lum < 105:
                color = dark
            else:
                factor = (lum - 105) / 115
                color = tuple(round(dark[c] * (1-factor) + base[c] * factor) for c in range(3))
            # Small, restrained pixel-level highlight variation; silhouette stays vanilla.
            if (x + y + variant) % 11 == 0 and lum >= 105:
                color = tuple(round(color[c] * 0.72 + accent[c] * 0.28) for c in range(3))
            dst[x, y] = (*color, a)
    return out

sheet = Image.new("RGBA", (160, 160), (0, 0, 0, 0))
for i in range(100):
    is_tea = i >= 50
    palette = PALETTES[(i * 7 + (11 if is_tea else 3)) % len(PALETTES)]
    source = tea_source if is_tea else coffee_source
    sprite = recolor_vanilla_sprite(source, palette, i, is_tea)
    sheet.alpha_composite(sprite, ((i % 10) * 16, (i // 10) * 16))
sheet.save(ASSETS / "Drinks.png", optimize=True)

# Build the kiosk from two adjacent 16x16 regions of the original vanilla
# furniture tilesheet. This retains the game's wood grain, outline, and shading.
vanilla_furniture = Image.open(SOURCE_ASSETS / "furniture.png").convert("RGBA")
assert vanilla_furniture.width == 512 and vanilla_furniture.height >= 352
kiosk = vanilla_furniture.crop((0, 320, 32, 336))
assert kiosk.size == (32, 16)
kiosk.save(ASSETS / "Kiosk.png", optimize=True)

objects, buffs, stock, catalog = {}, {}, [], []
seen_effects = set()
buff_icon_count = 48
for i, name in enumerate(COFFEES + TEAS):
    is_tea = i >= 50
    number = i - 49 if is_tea else i + 1
    kind = "Tea" if is_tea else "Coffee"
    item_id = f"BentleyPlayz_{kind}_{number:02d}"
    price = (180 + (i*37)%361) if not is_tea else (150 + (i*43)%351)
    edibility = 10 + ((i*7)%36)
    primary = STATS[i % len(STATS)]
    primary_value = 1 + (i // len(STATS))
    effects = {primary: primary_value}
    if i % 3 == 0:
        secondary = STATS[(i+4) % len(STATS)]
        effects[secondary] = 1 + ((i//3) % 2)
    profile = tuple(sorted(effects.items()))
    assert profile not in seen_effects, f"duplicate buff profile at {item_id}"
    seen_effects.add(profile)
    duration = 180 + ((i*73)%781)
    objects[item_id] = {
        "Name": item_id, "DisplayName": name,
        "Description": f"A freshly prepared {name.lower()}. Restores energy and health and grants a distinctive temporary boost.",
        "Type": "Cooking", "Category": -7, "Price": price,
        "Texture": "Mods/{{ModId}}/Drinks", "SpriteIndex": i,
        "Edibility": edibility, "IsDrink": True,
        "Buffs": [{"BuffId": item_id + "_Buff"}],
        "ContextTags": ["drink", "drink_tea" if is_tea else "drink_coffee", "bentleyplayz_more_drinks"]
    }
    buffs[item_id + "_Buff"] = {
        "DisplayName": name + " Boost",
        "Description": "A distinctive boost from " + name + ".",
        "Duration": duration * 1000,
        "IconTexture": "TileSheets\\BuffsIcons",
        "IconSpriteIndex": i %  buff_icon_count,
        "Effects": effects
    }
    stock.append({
        "Id": item_id + "_ShopStock", "ItemId": "(O)" + item_id,
        "Price": price
    })
    catalog.append({
        "id": item_id, "name": name, "type": kind, "price": price,
        "edibility": edibility, "energy": round(edibility*2.5,2),
        "health": round(edibility*1.125,2), "buff_duration_minutes": duration,
        "effects": effects, "sprite_index": i
    })

# Tiny overlay map; Content Patcher copies its Buildings layer and custom tilesheet into Maps/Saloon.
root = ET.Element("map", {"version":"1.4","tiledversion":"1.4.2","orientation":"orthogonal","renderorder":"right-down","width":"2","height":"1","tilewidth":"16","tileheight":"16","infinite":"0","nextlayerid":"2","nextobjectid":"1"})
ts = ET.SubElement(root,"tileset",{"firstgid":"1","name":"z_bentley_more_drinks_kiosk","tilewidth":"16","tileheight":"16","tilecount":"2","columns":"2"})
ET.SubElement(ts,"image",{"source":"Kiosk.png","width":"32","height":"16"})
for tile_id in ("0","1"):
    t=ET.SubElement(ts,"tile",{"id":tile_id})
    props=ET.SubElement(t,"properties")
    ET.SubElement(props,"property",{"name":"Action","value":"OpenShop BentleyPlayz_MoreDrinks none"})
layer=ET.SubElement(root,"layer",{"id":"1","name":"Buildings","width":"2","height":"1"})
data=ET.SubElement(layer,"data",{"encoding":"csv"})
data.text="1,2"
ET.ElementTree(root).write(ASSETS/"kiosk-overlay.tmx",encoding="utf-8",xml_declaration=True)

content = {
 "Format":"2.9.0",
 "Changes":[
  {"LogName":"Load 100 unique drink sprites","Action":"Load","Target":"Mods/{{ModId}}/Drinks","FromFile":"assets/Drinks.png"},
  {"LogName":"Add 50 coffees and 50 teas","Action":"EditData","Target":"Data/Objects","Entries":objects},
  {"LogName":"Add 100 unique custom drink buffs","Action":"EditData","Target":"Data/Buffs","Entries":buffs},
  {"LogName":"Add all drinks to the More Drinks kiosk shop with unlimited stock","Action":"EditData","Target":"Data/Shops","Entries":{
    "BentleyPlayz_MoreDrinks":{"Currency":0,"Items":stock}
  }},
  {"LogName":"Play Gus's one-time sponsored coffee and tea introduction on the first Saloon visit","Action":"EditData","Target":"Data/Events/Saloon","Entries":{
    "BentleyPlayz_MoreDrinks_GusIntro/Time 600 2600/!SawEvent BentleyPlayz_MoreDrinks_GusIntro": 'continue/16 18/farmer 15 20 2 Gus 17 18 2/skippable/pause 500/faceDirection Gus 2/faceDirection farmer 0/speak Gus "Hey, @! I have some exciting news for you!"/pause 500/speak Gus "The Stardrop Saloon has a brand-new coffee and tea menu!"/pause 500/speak Gus "We got sponsored by Bentley Playz! He taught me how to make all of these new drinks."/pause 500/speak Gus "There are 50 coffees and 50 teas, and every one has its own special boost."/pause 500/speak Gus "You can try them all at the new kiosk right here in the Saloon!"/pause 500/emote Gus 20/end'
  }},
  {"LogName":"Place the two-tile kiosk at the specified Saloon bar-front coordinates","Action":"EditMap","Target":"Maps/Saloon",
   "FromFile":"assets/kiosk-overlay.tmx","FromArea":{"X":0,"Y":0,"Width":2,"Height":1},
   "ToArea":{"X":16,"Y":19,"Width":2,"Height":1},"PatchMode":"Overlay"}
 ]
}
manifest = {
 "Name":"BentleyPlayz More Drinks - Content",
 "Author":"BentleyPlayz","Version":"1.0.0",
 "Description":"Content Patcher content for 50 coffees, 50 teas, their shop stock, and the Saloon kiosk map edit.",
 "UniqueID":"BentleyPlayz.MoreDrinks.Content",
 "ContentPackFor":{"UniqueID":"Pathoschild.ContentPatcher","MinimumVersion":"2.9.0"}
}
(OUT/"content.json").write_text(json.dumps(content,indent=2,ensure_ascii=False),encoding="utf-8")
(OUT/"manifest.json").write_text(json.dumps(manifest,indent=2),encoding="utf-8")
(OUT/"drink_catalog.json").write_text(json.dumps(catalog,indent=2,ensure_ascii=False),encoding="utf-8")
(OUT/"README.md").write_text(
 "# BentleyPlayz More Drinks\\n\\nOne combined SMAPI mod folder with a Code DLL and Content Patcher content. "
 "Adds 50 coffees and 50 teas, each with its own 16x16 sprite, positive Edibility, and a unique buff profile. "
 "The custom shop has unlimited stock and is opened by the two-tile Saloon kiosk at (16,19) and (17,19). Gus introduces the menu in a one-time-per-save event on the first Saloon visit.\\n",
 encoding="utf-8")
assert len(objects)==len(buffs)==len(stock)==len(catalog)==100
assert len(seen_effects)==100
assert len(list(ASSETS.glob("*.png")))==2
print("Generated 100 unique drink objects, 100 shop entries, 100 unique buff profiles.")
print("Generated Drinks.png (160x160), Kiosk.png (32x16), and kiosk-overlay.tmx.")
