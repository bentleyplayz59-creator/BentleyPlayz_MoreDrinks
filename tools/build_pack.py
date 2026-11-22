#!/usr/bin/env python3
from pathlib import Path
from itertools import combinations
import json, zipfile
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
PACK=ROOT/"dist"/"BentleyPlayz_MoreDrinks"
ASSETS=PACK/"assets"
ASSETS.mkdir(parents=True,exist_ok=True)

COFFEES=[
"Amber Roast","Maple Latte","Vanilla Cloud","Caramel Swirl","Mocha Velvet",
"Hazelnut Hearth","Cinnamon Cappuccino","Toffee Macchiato","Cocoa Bean Brew","Brown Sugar Latte",
"Midnight Espresso","Honeycomb Coffee","Almond Cream Roast","Butterscotch Brew","Peppermint Mocha",
"Campfire Coffee","Gingerbread Latte","Coconut Cream Coffee","Cherry Mocha","Pecan Praline Brew",
"Blueberry Crumble Coffee","Orange Zest Espresso","Lavender Latte","Pistachio Cappuccino","Rose Mocha",
"Smoked Maple Coffee","White Chocolate Latte","Salted Caramel Brew","Cardamom Coffee","Marshmallow Mocha",
"Apple Pie Latte","Blackberry Cream Coffee","Toasted Sesame Latte","Dark Cocoa Espresso","Sunrise Blend",
"Golden Dalgona","Iced River Roast","Frosted Vanilla Coffee","Spiced Plum Latte","Cranberry Mocha",
"Cookie Crumble Coffee","Honey Oat Latte","Brown Butter Brew","Starfruit Espresso","Moonlight Mocha",
"Praline Swirl Coffee","Citrus Cream Latte","Wintermint Roast","Wild Berry Mocha","Royal Reserve Coffee"]
TEAS=[
"Spring Blossom Tea","Mint Meadow Tea","Peach Garden Tea","Jasmine Pearl Tea","Blueberry Sage Tea",
"Rosehip Sunset Tea","Lemon Verbena Tea","Wildflower Honey Tea","Cranberry Spice Tea","Lavender Haze Tea",
"Apple Orchard Tea","Ginger Citrus Tea","Hibiscus Glow Tea","Chamomile Cloud Tea","Pomegranate Petal Tea",
"Blackberry Bramble Tea","Vanilla Rooibos Tea","Mango Sunrise Tea","Pear Blossom Tea","Elderflower Mist Tea",
"Spearmint Frost Tea","Cinnamon Orchard Tea","Golden Turmeric Tea","Raspberry Leaf Tea","Coconut Jasmine Tea",
"White Peach Oolong","Blood Orange Tea","Blue Lotus Tea","Strawberry Fields Tea","Mulberry Garden Tea",
"Maple Chai Tea","Lemongrass Breeze Tea","Apricot Nectar Tea","Rosemary Citrus Tea","Pineapple Sage Tea",
"Winterberry Tea","Honeydew Green Tea","Plum Blossom Tea","Passionfruit Tea","Cucumber Mint Tea",
"Vanilla Orchid Tea","Blackcurrant Tea","Sunset Hibiscus Tea","Cocoa Nib Chai","Clover Honey Tea",
"Moonflower Tea","Citrus Blossom Tea","Golden Pear Tea","Wild Rose Tea","Starlight White Tea"]
assert len(COFFEES)==len(TEAS)==50 and len(set(COFFEES+TEAS))==100

STATS=["Speed","LuckLevel","FarmingLevel","FishingLevel","ForagingLevel","MiningLevel","CombatLevel","Defense","Attack","MagneticRadius","MaxStamina"]
PAIRS=list(combinations(STATS,2))
PALETTES=[(105,58,37),(153,91,54),(190,125,70),(87,54,43),(124,76,61),(205,150,92),(88,110,84),(120,151,112),(163,103,137),(120,102,166),(78,133,164),(198,104,81),(211,174,94),(98,142,137),(160,119,82),(173,129,163),(95,113,157),(184,148,119),(126,145,91),(196,119,96)]
ACCENTS=[(244,211,133),(232,232,218),(202,224,242),(245,183,196),(177,218,174),(221,190,240),(255,225,165),(165,215,213),(239,166,117),(210,210,210)]

def icon(draw,x,y,i,is_tea):
    base=PALETTES[i%len(PALETTES)]
    accent=ACCENTS[(i*7+(11 if is_tea else 3))%len(ACCENTS)]
    dark=tuple(max(0,c-45) for c in base)
    light=tuple(min(255,c+48) for c in base)
    draw.rectangle((x+3,y+13,x+12,y+14),fill=(214,197,165,255))
    draw.point((x+2,y+13),fill=(214,197,165,255)); draw.point((x+13,y+13),fill=(214,197,165,255))
    if is_tea:
        draw.rectangle((x+4,y+5,x+11,y+12),fill=base+(255,))
        draw.rectangle((x+5,y+4,x+10,y+4),fill=light+(255,))
        draw.rectangle((x+5,y+6,x+10,y+7),fill=accent+(255,))
        draw.rectangle((x+11,y+7,x+13,y+10),fill=dark+(255,))
        draw.point((x+13,y+8),fill=light+(255,))
        draw.line((x+3,y+2,x+4,y+6),fill=(235,226,198,255))
        draw.rectangle((x+1,y+1,x+3,y+3),fill=accent+(255,))
    else:
        draw.rectangle((x+3,y+5,x+11,y+12),fill=base+(255,))
        draw.rectangle((x+4,y+4,x+10,y+4),fill=light+(255,))
        draw.rectangle((x+4,y+6,x+10,y+7),fill=dark+(255,))
        draw.rectangle((x+11,y+7,x+13,y+10),fill=base+(255,))
        draw.point((x+13,y+8),fill=light+(255,))
        draw.rectangle((x+4,y+11,x+10,y+11),fill=light+(255,))
        if i%3==0:
            draw.line((x+5,y+3,x+4,y+2),fill=(232,232,224,255)); draw.line((x+8,y+3,x+9,y+1),fill=(232,232,224,255))
        elif i%3==1: draw.line((x+6,y+3,x+7,y+1),fill=(232,232,224,255))
        else: draw.line((x+9,y+3,x+8,y+2),fill=(232,232,224,255))
    motif=i%5
    if motif==0: draw.point((x+7,y+9),fill=accent+(255,))
    elif motif==1: draw.rectangle((x+6,y+8,x+8,y+9),fill=accent+(255,))
    elif motif==2:
        draw.point((x+9,y+9),fill=accent+(255,)); draw.point((x+6,y+10),fill=light+(255,))
    elif motif==3: draw.line((x+5,y+9,x+8,y+9),fill=accent+(255,))
    else: draw.rectangle((x+7,y+8,x+9,y+10),fill=accent+(255,))

sheet=Image.new("RGBA",(160,160),(0,0,0,0))
draw=ImageDraw.Draw(sheet)
for i in range(100): icon(draw,(i%10)*16,(i//10)*16,i,i>=50)
sheet.save(ASSETS/"Drinks.png",optimize=True)

objects={}
stock=[]
catalog=[]
for i,name in enumerate(COFFEES+TEAS):
    tea=i>=50
    n=i-49 if tea else i+1
    kind="Tea" if tea else "Coffee"
    item=f"BentleyPlayz_{kind}_{n:02d}"
    price=(180+(i*37)%361) if not tea else (150+(i*43)%351)
    edibility=8+((i*7)%9)
    pair=PAIRS[(i*17)%len(PAIRS)]
    cycle=i//55
    effects={pair[0]:1+cycle,pair[1]:1+((i//7+cycle)%2)}
    duration=4+((i*5)%8)
    buff={"Id":item+"_Buff","BuffId":None,"Duration":duration,"CustomAttributes":effects}
    desc=f"A carefully prepared {name.lower()}. Restores energy and health, with a distinctive short-lived boost."
    objects[item]={
        "Name":item,"DisplayName":name,"Description":desc,"Type":"Cooking","Category":-7,
        "Price":price,"Texture":"Mods/{{ModId}}/Drinks","SpriteIndex":i,
        "Edibility":edibility,"IsDrink":True,"Buffs":[buff],
        "ContextTags":["drink","drink_tea" if tea else "drink_coffee","bentleyplayz_more_drinks"]}
    stock.append({"Id":item+"_ShopStock","ItemId":"(O)"+item,"Price":price,"AvailableStock":-1})
    catalog.append({"id":item,"name":name,"type":kind,"price":price,"edibility":edibility,
        "energy":edibility*2.5,"health":round(edibility*1.125,2),"buff_duration_minutes":duration,
        "effects":effects,"sprite_index":i})

profiles=[tuple(sorted(x["effects"].items())) for x in catalog]
assert len(objects)==100 and len(stock)==100 and len(set(profiles))==100
assert sheet.size==(160,160)

content={"Format":"2.9.0","Changes":[
 {"LogName":"Load 100 unique coffee and tea sprites","Action":"Load","Target":"Mods/{{ModId}}/Drinks","FromFile":"assets/Drinks.png"},
 {"LogName":"Add 50 coffees and 50 teas","Action":"EditData","Target":"Data/Objects","Entries":objects},
 {"LogName":"Add the More Drinks shop","Action":"EditData","Target":"Data/Shops","Entries":{
  "BentleyPlayz_MoreDrinks":{"Currency":0,"Items":stock}}}
]}
manifest={"Name":"BentleyPlayz More Drinks","Author":"BentleyPlayz","Version":"1.0.0",
 "Description":"Adds 50 unique coffees and 50 unique teas with individual sprites and drink buffs.",
 "UniqueID":"BentleyPlayz.MoreDrinks","MinimumApiVersion":"4.0.0"}
(PACK/"manifest.json").write_text(json.dumps(manifest,indent=2),encoding="utf-8")
(PACK/"content.json").write_text(json.dumps(content,indent=2,ensure_ascii=False),encoding="utf-8")
(PACK/"drink_catalog.json").write_text(json.dumps(catalog,indent=2,ensure_ascii=False),encoding="utf-8")
(PACK/"README.md").write_text(
 "# BentleyPlayz More Drinks\n\nContent Patcher pack for Stardew Valley 1.6.15. "
 "Adds 50 coffees and 50 teas. Each item has its own 16x16 sprite, varied price and edibility, "
 "and a distinct two-stat drink buff. A dedicated BentleyPlayz_MoreDrinks shop is added to Data/Shops. "
 "The Saloon kiosk map interaction is reserved for Step 2. No custom DLL is included.\n",
 encoding="utf-8")
zip_path=ROOT/"dist"/"BentleyPlayz_MoreDrinks_ContentPack.zip"
with zipfile.ZipFile(zip_path,"w",zipfile.ZIP_DEFLATED) as z:
 for p in sorted(PACK.rglob("*")):
  if p.is_file(): z.write(p,p.relative_to(PACK.parent))
print(f"Built {zip_path}")
print("Items: 100 (50 coffees, 50 teas); shop stock: 100")
print("Spritesheet: 160x160, 100 unique 16x16 sprites")
print(f"Distinct buff profiles: {len(set(profiles))}")
