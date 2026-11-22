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

# Three-tile-wide, two-tile-tall drinks counter, cropped from the
# vanilla townInterior shop-display tilesheet.
vanilla_interior = Image.open(ROOT / "WHITE_COKE" / ".._.._.._.._.._.._.._.._Maps_townInterior.png").convert("RGBA")
assert vanilla_interior.size == (512, 1088)
kiosk = vanilla_interior.crop((224, 896, 272, 928))
assert kiosk.size == (48, 32)
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
root = ET.Element("map", {"version":"1.4","tiledversion":"1.4.2","orientation":"orthogonal","renderorder":"right-down","width":"3","height":"2","tilewidth":"16","tileheight":"16","infinite":"0","nextlayerid":"2","nextobjectid":"1"})
ts = ET.SubElement(root,"tileset",{"firstgid":"1","name":"z_bentley_more_drinks_kiosk","tilewidth":"16","tileheight":"16","tilecount":"6","columns":"3"})
ET.SubElement(ts,"image",{"source":"Kiosk.png","width":"48","height":"32"})
for tile_id in ("0","1","2","3","4","5"):
    t=ET.SubElement(ts,"tile",{"id":tile_id})
    props=ET.SubElement(t,"properties")
    ET.SubElement(props,"property",{"name":"Action","value":"OpenShop BentleyPlayz_MoreDrinks none"})
layer=ET.SubElement(root,"layer",{"id":"1","name":"Buildings","width":"3","height":"2"})
data=ET.SubElement(layer,"data",{"encoding":"csv"})
data.text="1,2,3,4,5,6"
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
  {"LogName":"Add Abigail drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Abigail","Entries":{"Saloon_Entry":"Gus has been experimenting with all sorts of new coffees and teas. The whole place smells wonderful!$h"}},
  {"LogName":"Add Alex drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Alex","Entries":{"Saloon_Entry":"Gus says some of the new coffees are great before a workout. I might try one."}},
  {"LogName":"Add Caroline drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Caroline","Entries":{"Saloon_Entry":"The new tea selection at the Saloon is lovely. I hope Gus keeps a few herbal blends in stock."}},
  {"LogName":"Add Clint drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Clint","Entries":{"Saloon_Entry":"I stopped by Gus's new drinks counter. The coffee aroma almost made me forget about work."}},
  {"LogName":"Add Demetrius drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Demetrius","Entries":{"Saloon_Entry":"Gus has introduced an interesting range of beverages. The variety of tea ingredients is impressive."}},
  {"LogName":"Add Dwarf drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Dwarf","Entries":{"Saloon_Entry":"Gus sells strange surface-world drinks now. Some are surprisingly good!"}},
  {"LogName":"Add Elliott drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Elliott","Entries":{"Saloon_Entry":"A well-made tea can make an ordinary afternoon feel rather poetic. Gus has quite a selection."}},
  {"LogName":"Add Emily drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Emily","Entries":{"Saloon_Entry":"The new drinks at the Saloon have such lovely colors. I want to try the floral teas!"}},
  {"LogName":"Add Evelyn drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Evelyn","Entries":{"Saloon_Entry":"Gus has a new coffee and tea menu. It's nice to see something new in the old Saloon."}},
  {"LogName":"Add George drink dialogue","Action":"EditData","Target":"Characters/Dialogue/George","Entries":{"Saloon_Entry":"I hear Gus has new drinks. As long as he still makes a proper cup of coffee."}},
  {"LogName":"Add Gus drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Gus","Entries":{"Saloon_Entry":"I've expanded the menu with fifty coffees and fifty teas! Take a look at the new drinks counter whenever you're in the Saloon.$h"}},
  {"LogName":"Add Haley drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Haley","Entries":{"Saloon_Entry":"The new drinks counter is cute. I tried one of the iced coffees and it was pretty good."}},
  {"LogName":"Add Harvey drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Harvey","Entries":{"Saloon_Entry":"Gus has added several new coffees and teas. Remember, a little caffeine goes a long way."}},
  {"LogName":"Add Jas drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Jas","Entries":{"Saloon_Entry":"Uncle Gus has lots of new drinks! I like looking at all the colorful names."}},
  {"LogName":"Add Jodi drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Jodi","Entries":{"Saloon_Entry":"Gus's new coffee and tea menu gives everyone something different to try."}},
  {"LogName":"Add Kent drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Kent","Entries":{"Saloon_Entry":"I stopped at the Saloon and noticed Gus's new drinks counter. It's good to see the town trying something new."}},
  {"LogName":"Add Krobus drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Krobus","Entries":{"Saloon_Entry":"The surface-dwellers have many kinds of tea. Gus has shown me a few interesting blends."}},
  {"LogName":"Add Leah drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Leah","Entries":{"Saloon_Entry":"I like the idea of a quiet tea at the Saloon after a walk through the forest."}},
  {"LogName":"Add Leo drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Leo","Entries":{"Saloon_Entry":"Gus has drinks with fruit flavors? That sounds like something I'd enjoy."}},
  {"LogName":"Add Lewis drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Lewis","Entries":{"Saloon_Entry":"Gus has expanded the Saloon menu with a whole collection of coffees and teas. Pelican Town is lucky to have him."}},
  {"LogName":"Add Linus drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Linus","Entries":{"Saloon_Entry":"The smell of fresh tea reminds me to slow down and enjoy the simple things."}},
  {"LogName":"Add Marnie drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Marnie","Entries":{"Saloon_Entry":"Gus told me about his new drinks menu. I might stop in for a cup after closing up."}},
  {"LogName":"Add Maru drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Maru","Entries":{"Saloon_Entry":"Gus's new menu has quite a range. I'd like to compare how he prepares each coffee."}},
  {"LogName":"Add Pam drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Pam","Entries":{"Saloon_Entry":"Gus has new coffees? Now that's the kind of news I like to hear."}},
  {"LogName":"Add Penny drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Penny","Entries":{"Saloon_Entry":"The new teas at the Saloon sound lovely. It's nice that Gus has made room for so many choices."}},
  {"LogName":"Add Pierre drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Pierre","Entries":{"Saloon_Entry":"Gus has expanded his menu again. I should ask him how the new drinks are selling."}},
  {"LogName":"Add Robin drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Robin","Entries":{"Saloon_Entry":"I noticed the new counter in the Saloon. Gus has made it feel like a little shop of its own."}},
  {"LogName":"Add Sam drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Sam","Entries":{"Saloon_Entry":"Gus has fifty new coffees and fifty new teas? That's a lot of options. I need to try them."}},
  {"LogName":"Add Sandy drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Sandy","Entries":{"Saloon_Entry":"I wish the Oasis had a menu like Gus's new one. The tea selection sounds wonderful."}},
  {"LogName":"Add Sebastian drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Sebastian","Entries":{"Saloon_Entry":"The Saloon has a new drinks counter. Maybe I'll try one of the darker roasts."}},
  {"LogName":"Add Shane drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Shane","Entries":{"Saloon_Entry":"Gus has new coffee on the menu. Maybe one of these will get me through a long shift."}},
  {"LogName":"Add Vincent drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Vincent","Entries":{"Saloon_Entry":"Gus has lots of new drinks! I wonder which one has the brightest color."}},
  {"LogName":"Add Willy drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Willy","Entries":{"Saloon_Entry":"A good cup of tea is welcome after a day on the water. Gus has added some new ones."}},
  {"LogName":"Add Wizard drink dialogue","Action":"EditData","Target":"Characters/Dialogue/Wizard","Entries":{"Saloon_Entry":"Gus has expanded his collection of brews. Even ordinary drinks can have curious properties."}},
  {"LogName":"Play Gus's one-time sponsored coffee and tea introduction on the first Saloon visit","Action":"EditData","Target":"Data/Events/Saloon","Entries":{
    "BentleyPlayz_MoreDrinks_GusIntro/Time 600 2600/!SawEvent BentleyPlayz_MoreDrinks_GusIntro": 'continue/16 18/farmer 15 20 2 Gus 17 18 2/skippable/pause 500/faceDirection Gus 2/faceDirection farmer 0/speak Gus "Hey, @! I have some exciting news for you!"/pause 500/speak Gus "The Stardrop Saloon has a brand-new coffee and tea menu!"/pause 500/speak Gus "We got sponsored by Bentley Playz! He taught me how to make all of these new drinks."/pause 500/speak Gus "There are 50 coffees and 50 teas, and every one has its own special boost."/pause 500/speak Gus "You can try them all at the new kiosk right here in the Saloon!"/pause 500/emote Gus 20/end'
  }},
  {"LogName":"Place the three-by-two drinks counter in the Saloon","Action":"EditMap","Target":"Maps/Saloon",
   "FromFile":"assets/kiosk-overlay.tmx","FromArea":{"X":0,"Y":0,"Width":3,"Height":2},
   "ToArea":{"X":15,"Y":19,"Width":3,"Height":2},"PatchMode":"Overlay"}
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
 "The custom shop has unlimited stock and is opened by a three-tile-wide vanilla Saloon drinks counter at (15,19) through (17,20). Gus introduces the menu in a one-time-per-save event on the first Saloon visit.\\n",
 encoding="utf-8")
assert len(objects)==len(buffs)==len(stock)==len(catalog)==100
assert len(seen_effects)==100
assert len(list(ASSETS.glob("*.png")))==2
print("Generated 100 unique drink objects, 100 shop entries, 100 unique buff profiles.")
print("Generated Drinks.png (160x160), Kiosk.png (48x32), villager dialogue, and kiosk-overlay.tmx.")
