# Opening02 — first substantial construction correction

Runtime source frozen for review/build: `UnityProject/Assets/Scripts/World/NightDistrict.cs`. Numerical recipe and outputs: `SourceAssets/environment-v6-context/audit_opening.py` and `placement-audit.json`. No Unity execution or commit was performed by this task.

## Problem and resulting geometry

Inspected opening01/02/03 in both preserved opening01 draft modes, target A, and independent `003c-opening-native-review.md`. The rejected draft exposed pale roof slabs and a plain cubby in .15; at .22 the occupied front was below the near barrier. Finishing those hidden walls would not correct visibility.

The same two group endpoints, real .200/.275 pier feet, and original service datums remain. Compact rooms now stand **on** the connected platforms: court floor 12.75 m / roof 21.25 m; return floor 22 m / roof 30.5 m. Four grounded columns and two transverse caps under each room replace the tall empty ground-level walls. Actual foot socket columns remain based on the instantiated `Pier foot` transforms. Continuous deep steel girders connect the supported intervals. No extra tower or third group was added.

Exposed walks narrow from 11.8 m to 7 m. Their dark mapped top, mineral edge and steel underside replace the broad pale shelves. Rooms have medium concrete returns, a recessed coated-steel front, two occupied window cassettes, a central service door and a dark supported roof. These remain the existing three module types: podium/interface, service frontage, supported span.

The source review caught overlapping coplanar podium/walk layers. Concrete walks and curbs are now split at room±12.9 m, the exact 25.8 m podium boundary; top skins split at room±12.6 m, matching the 25.2 m top. The mapped visible faces do not overlap. Girders retain intentional volumetric bearing contacts. Rear/end walls meet the roof at 7.6 m above floor; roof top is 8.5 m above floor.

## Material and lighting correction

Four owned material clones; no new maps. The mineral returns and dark tops reuse existing `CastConcrete` base/packed-finish maps, and the steel uses `ServiceCoating`. Opening boxes receive valid 4 m metric face UVs; off-mode batches still receive no added UV channel. Finishes explicitly remain normal-map-free. Concrete base color .32/.355/.37, dark top .10/.135/.155, steel .075/.095/.11; packed smoothness multipliers .72/.76/.82. The occupied clone reuses the existing workshop room maps with a complete 4.8 × 3.2 m front-pane fit.

Exactly four shadowless spots remain, excluding racer layer 8. Each group has a housed under-canopy source at 240 intensity / 27 m / 108°, aimed across the occupied front and return. Court second light is 430 / 25 m / 82°, directed down its front room column to the actual grounded base. Return second is 750 / 34 m / 80°, mounted below the endpoint bearing and directed to the original .275 foot/socket. Tall roof poles and the broad roof-centered floods were removed. Lens positions are below the canopy fascia and at the Light origins; dark housings sit behind the lenses. No fog, exposure, global light, route, camera, physics or outdoor caster exclusions changed.

## Audits and expected native evidence

Every emitted box is planned before the gate, including all eight rotated housing/lens boxes. Their rotation is included in world bounds and in conservative local-AABB footprints for the yaw-only landmark, transit and city OBB checks. Exact runtime reservations remain authoritative. The source screening checks all emitted volumes against 59 conservative possible near-city, district, landmark and transit envelopes; no conflicts were found. Far skyline exclusion is justified by radius separation. The entire 1,200-sample route is checked against the banked underbody plus horizontal/vertical padding; neither a selected camera nor only a nearby branch substitutes for this check.

| Group | Boxes, including lamps | World min XYZ | World max XYZ | Minimum gap below protected road envelope |
| --- | --- | --- | --- | --- |
| Court | 49 | (171.990,0,-61.580) | (263.726,21.250,-47.708) | 22.475 m |
| Return | 52 | (148.858,0,-39.362) | (203.120,30.500,61.646) | 18.762 m |

Total 101 boxes / 1,212 triangles, up to 12 material-batched mesh renderers, four Lights, four new owned material instances and zero new textures/colliders. New meshes/materials use NightDistrict's existing destruction lifecycle; no native object is allocated by a field/static constructor.

Source-computed upper-room rectangles, ignoring occlusion:

- Court .15: visible clipped X0–241 / Y578–714.
- Return .15: X53–264 / Y424–488.
- Return .22: X193–520 / Y612–743, replacing the rejected lower cubby's X168–520 / Y678–998. Native images must confirm the front and support remain visible against the rail.
- .30/.37/.43: both groups remain behind the recorded cameras; preserve these as regression views.

Successful numerical execution and `git diff --check` are complete. Source contains telemetry for all group bounds, service floors, exact socket contacts, box counts, four audited housing boxes per group, full-course sample count and number of actual city footprints checked. Expected runtime summary: groups=2, addedLights=4; group boxes=49/52. Require three `VECTOR_RUSH_OPENING_FINISH ... existingMaps=True` lines. Native appearance, exact runtime acceptance, motion and performance are still pending. This correction is not a visual pass.
