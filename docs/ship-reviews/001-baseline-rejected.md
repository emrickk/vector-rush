# Independent ship review 001 — V2 baseline rejected

Reviewer: Astra. Date: 2026-09-07. Scope: direct static visual review and production planning. **Verdict: reject. The current ship is materially below reference C's hero-asset standard in both Blender and native Unity.** This is not a motion review.

## Evidence inspected

- [Reference C — primary ship/material target](../../references/nocturne/C-craft-materials.png).
- [Reference B — warm/cool lighting context](../../references/nocturne/B-amber-corridor.png).
- Current V2 Blender renders: [rear three-quarter](../../evidence/hero-v2/renders/hero-rear-three-quarter.png), [front three-quarter](../../evidence/hero-v2/renders/hero-front-three-quarter.png), [top](../../evidence/hero-v2/renders/hero-top.png), [chase](../../evidence/hero-v2/renders/hero-chase.png).
- Prior native night02: [start](../../evidence/night-02/02-start.png), [city descent](../../evidence/night-02/06-city-descent.png).
- Fresh native baseline03: [start](../../evidence/ship-baseline-03/02-start.png). This shows 0 km/h at the start with nearby opponents. It is useful close gameplay-camera evidence, not evidence of racing motion. It is a different moment/framing from night02 and cannot be treated as a precisely matched lighting comparison.
- Read the V2 asset manifest, engine-anchor contract and inspection camera script for dimensional/evidence context. Source settings are not substituted for what the images show.

The aspirational references differ in exact construction. C controls the ship review; B provides a material/lighting scenario. Neither image is evidence of this game's implemented quality.

## Observed failures, ranked

| Rank | Direct visual observation | Why it misses C | Required next change |
| --- | --- | --- | --- |
| 1 | Top/front views resolve as three pointed surfboard-like bodies. White nacelle caps swell around their ends; the intake's raised white rails are thick and rounded. | C has substantial integrated propulsion housings with controlled plane transitions and shell layering. V2's big shapes read molded and soft before small detail can help. | Rebuild nacelle crown/shoulder/return sections and integrate aft fairings. Recess the intakes. Review clay massing before finish. |
| 2 | The central canopy is a bulbous teal lens with a broad rounded metallic surround. In native baseline03 it becomes a tall glossy black bubble above a separate pale rear cap. | C's dark cockpit/spine supplies a long, low, continuous center. V2 breaks that line into a jewel-like cockpit and small white taper. | Lower and lengthen the canopy, tighten the frame, connect its surface language to the center fuselage. |
| 3 | Large thin fins rise from each rear nacelle. Their roots look attached to the outer shell, and they become dominant black slabs in all native rear views. | They interrupt the lower coherent envelope in C and exaggerate the model-kit silhouette. | Remove them in the first shape candidate; only reintroduce low integrated stabilizers if they improve the full silhouette. |
| 4 | Visible diagonal tubes have black open ends and sparse bright collars; simple crossbars bridge large gaps. The rear crossbar and little vertical paddles are particularly exposed in chase. | The reference's dark areas contain readable mounting and layered mechanical construction. V2 shows loose-looking rods without enough designed terminations or structural hierarchy. | Model a few substantial nacelle mounts and routed feeds that visibly meet housings. Keep the gaps open and purposeful. |
| 5 | Armor is broadly uniform pale white; trim and frame frequently share a smooth silvery response. The canopy reads highly saturated teal in studio. Large surfaces have little authored finish or panel hierarchy. | C separates maintained coated armor, glazing, graphite and exposed metal, with subtle panel-level variation. V2 looks like clean colored pieces of one toy material family. | Build material separation after surface cleanup. Add restrained panel finish variation tied to construction, not global noise. |
| 6 | Main engines have prominent shiny circular lips and broad turquoise annuli. In studio chase much of each tube reads bright; in native views sharp blue rings dominate the mouth. | C retains a dark cavity and restrained collar around a compact bright core. V2 exposes depth, but the brightness distribution turns it into a luminous tube/outline. | Integrate the fairing, darken and articulate chamber stages, reduce luminous ring dominance, preserve the small core. |
| 7 | Gray forward number fields have ragged white intrusions along their borders. Aft numeral shapes are visibly incomplete in top/rear renders and poorly oriented for rear legibility. Citron strips look raised above the armor. | C's sparse accents sit within a believable panel/marking system. Broken lettering and sticker-like thickness are readily visible finish defects. | Correct the surface conformity/geometry and lay out clean rear-readable marks. Verify both sides and the exported asset. |

These defects are visible in favorable neutral studio conditions. Changing the night lighting alone cannot remove them. The fresh native baseline03 makes the canopy bubble, thick vent surrounds, slab fins, disconnected-looking pipework and large luminous nozzle rims easy to see. Native night02 also shows the same silhouette at smaller racing-camera size; the nearby rival exposes the long pointed front and smooth stacked shell construction.

## What is already useful

The twin-nacelle arrangement and central cockpit are recognizable. The open channels are genuine visible spaces. Main nozzles do have recessed interiors and distinguishable cores; this review does not claim their cavities are absent. Pale hull panels remain visible in the current night captures, and engine effects are compact in the inspected stills. Those foundations can be retained while the form and material execution are rebuilt.

This is a list of usable foundations, not partial evidence of a final quality pass. The overall object still has a toy-like primary-form reading.

## Observations versus hypotheses

- **Observed:** ragged/incomplete marking edges. **Not established:** whether projection offsets, overlapping surfaces, mesh clipping, or another authoring issue caused them. Inspect the exported mesh; do not credit a presumed fix without fresh images.
- **Observed:** puffy highlights and softened shell transitions. **Not established:** the relative contribution of cross-section design, beveling and normal treatment. The rendered correction is what matters.
- **Observed:** flatter and darker material separation in native views than in studio. **Not isolated:** runtime remapping versus the different light/reflection environment. The parent's fixed native inspection rig should distinguish those causes.
- **Observed:** engine cavity depth exists but loses visual hierarchy. The V2 anchor contract places the main throat 0.685 m behind its exit. That distance does not prove that the engine looks convincing.
- **Not reviewed:** motion shimmer, material/reflection stability, banked VFX alignment, lighting transitions or performance. Static screenshots cannot pass those requirements.

## Immediate bounded correction

Rebuild primary massing only: nacelle shells and aft integration, lower long canopy/spine, recessed narrow-lip intakes, and a low coherent silhouette without the tall slab fins. Submit full-ship clay top/side/front/rear/chase views, then the same views in restrained neutral materials. Preserve the current camera set for before/after comparison and add a reference-C perspective view.

The worker should not spend this iteration on dirt maps, dozens of fasteners, more trim or stronger emission. The first gate is a clean, convincing large-form object under neutral light. [The production plan](../ship-art-plan.md) defines subsequent assembly, material, engine and native gates. No AAA pass is earned by this baseline.
