# BentleyPlayz More Drinks

A Stardew Valley 1.6.15 mod project by Bentley Playz, targeting SMAPI on Android as well as compatible desktop installations.

## Project goals

- Add 100 original beverages: 50 coffees and 50 teas.
- Give every beverage its own name, description, price, edible/drink data, and individual sprite.
- Integrate the drinks into Gus's Stardrop Saloon while preserving the vanilla shop's existing stock, behavior, and character interactions.
- Keep the project organized as one cohesive mod rather than splitting related features into unrelated mods.
- On initialization, show a prominent shoutout to HunnyBwat.

## Current priority: visual redesign of the Saloon counter

The existing counter's functionality and workflow are working correctly. Its visual design is not acceptable yet, so the current task is to redesign **the appearance of the counter only**.

Use Bentley's Fast Food and the other existing BentleyPlayz projects as visual and quality references. The result should feel cohesive with those projects while still fitting Gus's Saloon and Stardew Valley's visual language. Review the existing project assets and context before making design decisions.

**Do not change the counter workflow or functional behavior.** Do not change how the counter is accessed, how it opens, how the shop/menu works, how purchases or orders are handled, or any related event handling, triggers, input behavior, data flow, NPC schedules, Gus's stock, or vanilla shop behavior. Do not refactor working systems. Only change the counter's visual presentation and the minimum visual assets/layout data required for the redesign. If a functional change seems necessary, stop and explain why before making it.

The specific visual solution is intentionally left to the developer. See [CLAUDE.md](CLAUDE.md) for the detailed handoff and strict scope boundaries.

## Gideon, Gus's cousin

A new villager, **Gideon**, staffs the More Drinks counter. He is Gus's cousin: same build, a teal jacket with a red tie, and a curly handlebar mustache. He stands at Saloon tile **(16,18)** facing down, behind the counter, all day and night (no schedule file, so he never leaves his Home tile). That tile is the decorative Front-layer counter top, so the solid Buildings station at (16,19)-(17,19) and its shop Action are untouched.

Gus's intro event now also says: "I won't be running the drink shop. My cousin, Gideon, will be running it. He'll be here 24/7 whenever you need it." Saves that already watched the intro can replay it with `debug ebi BentleyPlayz_MoreDrinks_GusIntro`.

- Art: `tools/gideon_art.py` builds the sprite sheet and portraits from the vanilla Gus files in `WHITE_COKE/`.
- NPC data, dialogue and patches: `tools/gideon_npc.py` (called from `tools/build_pack.py`).

## Development notes

- Target game version: Stardew Valley 1.6.15.3.
- Target mod loader: SMAPI 4.3.2.5.
- Android compatibility is an important requirement; test on the user's Android setup where possible.
- Preserve vanilla Saloon functionality and Gus's existing stock.
- Use the repository's researched game assets and references rather than guessing about map layout or tile usage.
- The drink roster and final custom sprites are supplied/approved separately; do not invent a replacement set of 100 finished drink assets without the user's input.
