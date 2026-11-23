#!/usr/bin/env python3
"""Gideon, Gus's cousin: the villager who staffs the More Drinks counter.

Called from build_pack.py. Adds only NEW content (a new NPC); it does not touch Gus,
his stock, any existing NPC's schedule, or the counter's Action/shop workflow.

Gideon has no schedule file on purpose. A villager without a schedule stays on his Home
tile all day and night, which is exactly the "here 24/7" behaviour: he is placed at
Saloon (16,18) every morning and never leaves.
"""
import json
from pathlib import Path
import gideon_art

NPC_ID = "BentleyPlayz_Gideon"
HOME_LOCATION, HOME_X, HOME_Y = "Saloon", 16, 18

DIALOGUE = {
    "Introduction": "Name's Gideon. I'm Gus's cousin. I'll be running the drink counter, so come by any time.$h",
    "Mon": "Fresh pots are on. What can I get you?",
    "Tue": "Gus says I'm not allowed to leave the counter. Not that I'd want to!$h",
    "Wed": "The secret to a good mustache is wax. The secret to a good coffee is patience.",
    "Thu": "Fifty coffees and fifty teas. I've tried them all. Twice.",
    "Fri": "Always open, always brewing. That's the Gideon promise.$h",
    "Sat": "Busy night ahead. Better grab your drink early!",
    "Sun": "Day of rest? Not at this counter. I'm here 24/7!$h",
}

def write_assets(assets_dir: Path):
    sprite, portrait = gideon_art.build()
    sprite.save(assets_dir / "Gideon.png", optimize=True)
    portrait.save(assets_dir / "Gideon_Portrait.png", optimize=True)
    (assets_dir / "Gideon_Dialogue.json").write_text(
        json.dumps(DIALOGUE, indent=2, ensure_ascii=False), encoding="utf-8")

def patches():
    return [
        {"LogName": "Load Gideon's sprite sheet", "Action": "Load",
         "Target": f"Characters/{NPC_ID}", "FromFile": "assets/Gideon.png"},
        {"LogName": "Load Gideon's portraits", "Action": "Load",
         "Target": f"Portraits/{NPC_ID}", "FromFile": "assets/Gideon_Portrait.png"},
        {"LogName": "Load Gideon's dialogue", "Action": "Load",
         "Target": f"Characters/Dialogue/{NPC_ID}", "FromFile": "assets/Gideon_Dialogue.json"},
        {"LogName": "Add Gideon, Gus's cousin, standing behind the drinks counter 24/7",
         "Action": "EditData", "Target": "Data/Characters", "Entries": {
            NPC_ID: {
                "DisplayName": "Gideon",
                "Gender": "Male",
                "Age": "Adult",
                "Manner": "Polite",
                "SocialAnxiety": "Outgoing",
                "Optimism": "Positive",
                "HomeRegion": "Town",
                "CanBeRomanced": False,
                "CanSocialize": "FALSE",
                "CanReceiveGifts": False,
                "SocialTab": "HiddenAlways",
                "EndSlideShow": "Hidden",
                "FriendsAndFamily": {"Gus": "cousin"},
                "Home": [{
                    "Id": "Default",
                    "Location": HOME_LOCATION,
                    "Tile": {"X": HOME_X, "Y": HOME_Y},
                    "Direction": "down"
                }]
            }
        }},
    ]

def append_docs(out_dir: Path):
    note = (
        "\n\n## Gideon (Gus's cousin)\n\n"
        f"NPC `{NPC_ID}` stands at Saloon tile ({HOME_X},{HOME_Y}) facing down, behind the counter, "
        "all day and night (Home tile, no schedule file). That tile is the decorative Front-layer "
        "counter top: no collision, no Action, so the solid Buildings station at (16,19)-(17,19) "
        "and its shop workflow are unchanged.\n"
    )
    for name in ("Saloon_Collision_Grid.md", "README.md"):
        with open(out_dir / name, "a", encoding="utf-8") as f:
            f.write(note)
