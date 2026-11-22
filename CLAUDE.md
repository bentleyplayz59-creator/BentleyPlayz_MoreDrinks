# Claude / Quad Handoff — Counter Design-Only Redesign

## Main task

Redesign **only the visual design of the existing More Drinks counter** in Gus's Stardrop Saloon. The counter and its current functionality/workflow are already working correctly. This task is about appearance, not fixing or replacing working behavior.

Use your own design judgment to make the counter look intentional, polished, attractive, and professionally integrated into the Saloon. Do not preserve a poor-looking design just because it exists, but preserve the working implementation and behavior.

## Non-negotiable scope boundary: design only

- **Do not change the existing counter workflow.**
- **Do not change how the counter opens, how the shop/menu is accessed, how orders or purchases work, or how the player interacts with it.**
- **Do not change the existing functional logic, event handling, triggers, input behavior, shop integration, item handling, or data flow.**
- **Do not alter NPC schedules, NPC behavior, Gus's normal stock, vanilla shop behavior, or unrelated game systems.**
- Do not refactor or rewrite working systems as part of this task.
- Do not split this into additional mods or change the mod architecture.
- Only modify the counter's visual presentation and the minimum visual assets/layout data strictly necessary to accomplish that redesign.
- If you believe a functional change is unavoidable, stop and explain why before making it. Do not silently change the workflow.

## References to study

Before redesigning the counter, inspect the existing BentleyPlayz projects—especially **Bentley's Fast Food**—and use their stronger interior presentation, counter design quality, visual consistency, and overall polish as references. Review the relevant assets and research already present in this repository. Adapt what works to the Stardrop Saloon rather than blindly copying another building.

## Visual quality expectations

- The counter must look like it belongs in the Stardrop Saloon and in Stardew Valley.
- It should read clearly as a purposeful, usable shop counter and look coherent with the surrounding room.
- Pay attention to proportions, alignment, tile-level detail, palette, visual hierarchy, and how the counter sits within the interior.
- The result should be a substantial visual improvement, not a cosmetic patch.
- Keep Android compatibility in mind, since the project is tested on Android.

## Project context

This is BentleyPlayz More Drinks, a Stardew Valley 1.6.15.3 / SMAPI 4.3.2.5 project. Its broader goal is to add 50 coffees and 50 teas and integrate them into Gus's Stardrop Saloon without removing vanilla content. The existing counter functionality is confirmed working; **do not change its workflow**.

## Deliverable and verification

Implement the visual redesign in the project and review it in context with the surrounding Saloon interior. Confirm that the counter still behaves exactly as before: same interaction, same access path, same menu/shop workflow, and same functional behavior. Report the visual files changed and explicitly list any functional files you avoided changing. Identify any Android visual testing still needed. Do not stop at a mockup or written proposal.
