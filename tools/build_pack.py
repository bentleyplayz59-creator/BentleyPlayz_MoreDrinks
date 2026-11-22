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
ACCENTS = [(244,211,133),(232,232,218),(202,224,242),(245,183,196),(177,218,174),(221,190,240),(255,225,165),(165,215,213),(239,166,117),(210,210,210)]
STATS = ["Speed","LuckLevel","FarmingLevel","FishingLevel","ForagingLevel","MiningLevel","CombatLevel","Defense","Attack","MagneticRadius","MaxStamina"]

def draw_drink(draw, x, y, i, tea):
    base = PALETTES[i % len(PALETTES)]
    accent = ACCENTS[(i * 7 + (11 if tea else 3)) % len(ACCENTS)]
    dark = tuple(max(0,c-45) for c in base)
    light = tuple(min(255,c+48) for c in base)
    draw.rectangle((x+3,y+13,x+12,y+14), fill=(214,197,165,255))
    if tea:
        draw.rectangle((x+4,y+5,x+11,y+12), fill=base+(255,))
        draw.rectangle((x+5,y+4,x+10,y+4), fill=light+(255,))
        draw.rectangle((x+5,y+6,x+10,y+7), fill=accent+(255,))
        draw.rectangle((x+11,y+7,x+13,y+10), fill=dark+(255,))
        draw.point((x+13,y+8), fill=light+(255,))
        draw.line((x+3,y+2,x+4,y+6), fill=(235,226,198,255))
        draw.rectangle((x+1,y+1,x+3,y+3), fill=accent+(255,))
    else:
        draw.rectangle((x+3,y+5,x+11,y+12), fill=base+(255,))
        draw.rectangle((x+4,y+4,x+10,y+4), fill=light+(255,))
        draw.rectangle((x+4,y+6,x+10,y+7), fill=dark+(255,))
        draw.rectangle((x+11,y+7,x+13,y+10), fill=base+(255,))
        draw.point((x+13,y+8), fill=light+(255,))
        draw.rectangle((x+4,y+11,x+10,y+11), fill=light+(255,))
        if i%3 == 0:
            draw.line((x+5,y+3,x+4,y+2), fill=(232,232,224,255)); draw.line((x+8,y+3,x+9,y+1), fill=(232,232,224,255))
        elif i%3 == 1:
            draw.line((x+6,y+3,x+7,y+1), fill=(232,232,224,255))
        else:
            draw.line((x+9,y+3,x+8,y+2), fill=(232,232,224,255))
    motif = i % 5
    if motif == 0: draw.point((x+7,y+9), fill=accent+(255,))
    elif motif == 1: draw.rectangle((x+6,y+8,x+8,y+9), fill=accent+(255,))
    elif motif == 2:
        draw.point((x+9,y+9), fill=accent+(255,)); draw.point((x+6,y+10), fill=light+(255,))
    elif motif == 3: draw.line((x+5,y+9,x+8,y+9), fill=accent+(255,))
    else: draw.rectangle((x+7,y+8,x+9,y+10), fill=accent+(255,))

sheet = Image.new("RGBA",(160,160),(0,0,0,0))
d = ImageDraw.Draw(sheet)
for i in range(100):
    draw_drink(d,(i%10)*16,(i//10)*16,i,i>=50)
sheet.save(ASSETS/"Drinks.png", optimize=True)

# Two adjacent 16x16 tiles form one 32x16 kiosk front, matching Saloon tiles (16,19) and (17,19).
kiosk = Image.new("RGBA",(32,16),(0,0,0,0))
kd = ImageDraw.Draw(kiosk)
kd.rectangle((0,2,31,14), fill=(91,55,38,255))
kd.rectangle((1,1,30,4), fill=(180,119,68,255))
kd.rectangle((2,5,29,13), fill=(122,77,48,255))
kd.rectangle((3,6,28,11), fill=(52,42,35,255))
kd.rectangle((4,7,27,10), fill=(231,193,127,255))
kd.rectangle((6,7,9,10), fill=(101,65,45,255))
kd.rectangle((12,7,15,10), fill=(112,78,51,255))
kd.rectangle((18,7,21,10), fill=(101,65,45,255))
kd.rectangle((24,7,27,10), fill=(112,78,51,255))
kd.rectangle((0,14,31,15), fill=(61,40,31,255))
kd.line((15,2,15,13), fill=(74,46,34,255))
kd.rectangle((2,2,13,3), fill=(223,177,104,255))
kd.rectangle((18,2,29,3), fill=(223,177,104,255))
kiosk.save(ASSETS/"Kiosk.png", optimize=True)

objects, stock, catalog = {}, [], []
seen_effects = set()
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
        "Buffs": [{
            "Id": item_id + "_Buff", "Duration": duration, "IsDebuff": False,
            "CustomAttributes": effects
        }],
        "ContextTags": ["drink", "drink_tea" if is_tea else "drink_coffee", "bentleyplayz_more_drinks"]
    }
    stock.append({
        "Id": item_id + "_ShopStock", "ItemId": "(O)" + item_id,
        "Price": price, "AvailableStock": -1
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
  {"LogName":"Add all drinks to the More Drinks kiosk shop","Action":"EditData","Target":"Data/Shops","Entries":{
    "BentleyPlayz_MoreDrinks":{"Currency":0,"Items":stock}
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
 "The custom shop is opened by the two-tile Saloon kiosk at (16,19) and (17,19).\\n",
 encoding="utf-8")
assert len(objects)==len(stock)==len(catalog)==100
assert len(seen_effects)==100
assert len(list(ASSETS.glob("*.png")))==2
print("Generated 100 unique drink objects, 100 shop entries, 100 unique buff profiles.")
print("Generated Drinks.png (160x160), Kiosk.png (32x16), and kiosk-overlay.tmx.")
