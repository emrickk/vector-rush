# Independent review 008 — gallery faces and attached plume now readable

Reviewer: Astra. Date: 2026-09-07 PDT. **Decision: advance with named follow-ups. The bounded gallery-face and plume-readability correction is visibly successful in the inspected native stills.** Retain this direction and keep the ship's major form frozen. Continuous-motion stability and overall game quality remain pending; this is not an AAA pass.

## Evidence and exact scope

Directly inspected motion03 [frame 82 — gallery entry](../../evidence/night-v3-motion-03/selected/amber-entry.png), [frame 105 — gallery middle](../../evidence/night-v3-motion-03/selected/amber-mid.png), [frame 128 — gallery exit](../../evidence/night-v3-motion-03/selected/amber-exit.png) and [frame 146 — observed boost](../../evidence/night-v3-motion-03/selected/observed-boost.png). Compared middle and boost directly with their motion02 [middle](../../evidence/night-v3-motion-02/selected/amber-mid.png) and [boost](../../evidence/night-v3-motion-02/selected/observed-boost.png) counterparts. The earlier entry/exit evidence is retained in [review 007](007-native-gallery-and-boost-stills.md). All four persistent selected copies match their source PNG hashes, and their [selection record](../../evidence/night-v3-motion-03/selected/selection.json) preserves the frame association.

All inspected PNGs are actual 1920 × 1080 native gameplay captures with the ordinary HUD. The [motion03 metadata](../../evidence/night-v3-motion-03/replay-metadata.json) confirms automated steering through normal physics, 24 simulation frames per second and 360 requested frames. The run is a simulation recording, not a real-time performance test. It was captured 2026-09-08 04:29:23–04:30:40 UTC. These selected states match the earlier run's telemetry:

| Frame | Race time | Track progress | Recorded state |
| --- | --- | --- | --- |
| 82 | 34.430 s | 0.857726 | 194.2 km/h, grounded, boost off, energy 100%. |
| 105 | 35.390 s | 0.885914 | 190.6 km/h, grounded, boost off, energy 100%. |
| 128 | 36.350 s | 0.917377 | 242.6 km/h, grounded, boost off, energy 100%. |
| 146 | 37.100 s | 0.949849 | 334.5 km/h, grounded, boost on, energy 84.4%. |

The [asset identity](../../evidence/night-v3-motion-03/asset-identity.json) records the unchanged pass03 ship FBX (`0415a4a829347f6556f0f8ba1274ebbbd949b5bf6cf6ae128218c4bbd5eb436e`) and anchor file, together with the revised gallery/lighting/propulsion sources and built data. This capture **does not include pass04 ship-surface contact corrections**. It cannot validate that separate work. No continuous passage was viewed for this review, and the stills do not establish plume attachment or light stability between samples.

## Why this correction can advance

**The gallery's wall construction is now readable.** Where motion02 middle showed almost black slabs bounded by orange emitters, motion03 middle reveals warm charcoal panel faces, recessed perimeter lines, dark support divisions, upper vent/fixture slots and lower returns. Entry shows that this treatment extends along the curved passage. The washed faces and their darker borders establish useful depth at ordinary chase size. The existing route and exit remain legible, and the warm response on the ivory ship is retained.

This is enough to move beyond the previous gallery-face blocker. It does not yet make the entire module kit richly finished: the wall bays are repetitive and large roof areas remain nearly black. Those should now be ranked with the other scene work rather than provoking a replacement of the whole gallery.

**Exhaust now reads as an attached short body.** Entry and middle have visible soft cyan tails extending from the two main engine mouths. In boost frame 146 the tails are also clearly legible, with compact bright origins and fading edges. The chamber and collar remain readable around them. The old broad blue filling of the nozzle bowls and the two distant bright blue road spots are substantially reduced. This shifts attention from colored spill toward propulsion at the craft.

The new effect is still narrow and beam-like through its bright center, but its short extent and soft outside fade are a useful improvement over the previously almost absent volume. The selected boost state does not turn the effect into an opaque cone or hide the ship's silhouette. Retain this balance for the motion check; do not enlarge the white disks or restore the dominant blue bowl lighting merely to increase impact.

## Remaining work, in current priority order

1. **Review the actual corrected effect and lighting in continuous motion.** Check attachment through yaw/bank, cruise-to-boost changes, tail flicker, and warm/cool transitions. The two engine axes project at different angles in a banked view; a still cannot distinguish stable attachment from frame-to-frame drift. No temporal pass is awarded here.
2. **Correct the road's hard repeated wet-highlight boundaries.** The broad right-side streak in gallery entry/middle still contains conspicuous diagonal/rectangular subdivisions. The current correction did not resolve that earlier observation. Soften and break the visible pattern while preserving actual warm pools and darker interruptions. Isolate the cause before changing the entire road response.
3. **Improve the next largest scene surfaces.** The gallery ceiling still reads mainly as black overhead slabs between bright strips, and the exterior boost view still consists largely of crisp window grids over simple dark building masses. Give selected roof returns and a few near building service faces controlled readable values; quiet the more distant window layers. Keep the now-useful wall wash and passage proportions.

The previously observed canopy/intake/nozzle contact defects remain outside this correction's visual scope. Assess them separately in native close views of the corrected ship export. No new broad shape work is justified by the four inspected player views.

## Stopping point for this review

The bounded still-image goal is met: close gallery walls now communicate construction, and short propulsion is visible at the nozzle in both non-boost and observed-boost states. Preserve this revision and proceed to motion evidence and the higher-value scene follow-ups. A clean build, matched telemetry and these improved stills do not establish the complete game's visual or playability target.
