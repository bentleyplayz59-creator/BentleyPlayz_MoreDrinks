# Research folder

Vanilla Stardew Valley 1.6 reference files for the More Drinks mod, especially the Stardrop Saloon work.
Nothing here is mod content. These are read-only references; do not edit them in place.

## Layout
- `saloon/` Saloon map (Maps_Saloon.tmx), its tilesheets, full Data/Locations, plus small extracts:
  - `Saloon_tmx_summary.json`: map size (46x25), tilesets, layers, map properties (Warp, Doors, DayTiles/NightTiles), and all 17 Action objects with tile coordinates.
  - `Data_Locations_Saloon_entry.json`: the Saloon entry from Data/Locations.
- `tea-coffee/` everything that touches vanilla coffee/tea:
  - `vanilla_tea_coffee_objects.json`: compact summary of vanilla Coffee (395), Triple Shot Espresso (253), Green Tea (614), Tea Leaves (815), Tea Sapling (251), Coffee Bean (433), Tea Set (341) and Stardrop Tea, with sprite sheet coordinates.
  - `TileSheets_crops.png`, `Data_Crops.json`: crop data and sprites.
  - Full `Data_Objects.json` and `Maps_springobjects.png` are in `research/` (root).
- `town/` Town map and outdoor tilesheets (Gus's building exterior).
- `other/` Fish shop map (kept from earlier research).

## Not yet supplied
`Maps/indoor` (listed in the project instructions) has not been provided.

## Notes
- `Maps_springobjects.png`: 24 columns of 16x16 sprites; sprite index N is at x=(N%24)*16, y=(N//24)*16.
- `saloon/tilesheets/` file names (`.._.._..`) are kept exactly because Maps_Saloon.tmx references them by those names.
- No beverages, names, or assets have been designed here.
- Large and binary files (TMX maps, PNG tilesheets, full Data_Locations/Data_Crops) are added from `MoreDrinks_research.zip`; see the status list in the project notes for what is uploaded yet.
