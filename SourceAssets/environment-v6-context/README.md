# Opening construction — correction 02 source recipe

Editable runtime source: `UnityProject/Assets/Scripts/World/NightDistrict.cs`, from `BuildOpeningConstruction` through the opening preflight helpers. The current frozen correction is documented in `evidence/nocturne-v2/opening-02/source-construction-correction.md`. The original failed blockout is preserved in commit `ce50b9e` and the opening01 native evidence; independent rejection is `docs/visual-target-reviews/003c-opening-native-review.md`.

## Existing source/contact audit

Inspected the original SSR02 OFF .15/.22/.30 full frames, target A, and the preserved opening01 off/combined opening frames. The original supports and feet already exist. Service avenues contain emissive meshes without street-light components; the authored `BuildApproachServiceGroup` at .735 does not address the opening. The existing mast foundation and its reservation remain untouched.

The actual accepted .20 industrial block is centered at approximately (151.253,0,-54.532), using horizontally projected **banked** TrackFrame.Right. It already overlaps the original grid tower at (152,0,-56). The apparent grid candidates (152,0,14) and (82,0,84) fail original city clearance and are not contact endpoints. The existing industrial/tower east face at X=172 provides the 12.75 m court contact. The grid tower's 34 m north blade reaches Z=-37.82 and provides the return's 22 m contact.

- Court: frontage (172,0,-54.563), .200 foot (266.253,1,-54.710), actual inboard socket (262.623,1.917,-54.817), path length 90.624 m.
- Return: frontage (152,0,-37.82), .275 foot (202.328,1,62.219), actual socket (199.803,1.918,59.610), path length 108.525 m.

Sockets use the instantiated `Pier foot.TransformPoint(-.33,.46,0)`, including actual scale and support rotation. There is no second foundation at either existing foot. Grounded room frames and intermittent span columns divide the path into approximately 16–30 m supported intervals; these are not 90/109 m unsupported slabs. Room positions remain 32%/70% along their paths; the court room stays clear of the X=224 service avenue.

## Correction and ownership

Same two groups, four lights and three module types. Rooms now stand above the connected platforms, with 8.5 m fronts and grounded open frames below. Walks are 7 m wide with dark tops, visible mineral edges and deep steel girders. Dark roofs, concrete returns and recessed occupied fronts replace the pale cubbies/roof pools. Actual light housings and lenses are included in the planned geometry/preflight. All construction, material creation and footprint collection require `OpeningFinishPreview.ConstructionEnabled`.

The correction uses four new owned material clones and **no new maps**. Existing CastConcrete/ServiceCoating resources receive 4 m metric UVs on all opening box faces; the occupied window map is fitted to its 4.8 × 3.2 m pane. No normal map is introduced. Opening materials/meshes use NightDistrict's existing ownership lists and OnDestroy. Off-mode geometry, UV formats, lights, random sequence and original reservations remain unchanged.

101 boxes / 1,212 triangles / up to 12 batched renderers; exactly four shadowless spotlights; no colliders. Three mapped structural variants plus one occupied material clone; borrowed glass/source materials remain shared. The front light in each group has intensity 240, range 27, cone 108°. Court base light is 430/25/82°; return foot light is 750/34/80°. Fog/global lighting and existing road fixture shadow exclusions are unchanged. Appearance and performance need native judgment.

## Bounds and validation

`placement-audit.json` records exact source hashes, source-computed per-group bounds, all four light origins/targets, projected room rectangles and conservative reservation screening. It reconstructs TrackPath's arc-length lookup, banking, actual foot basis and recorded SSR02 camera quaternions/FOV. Screen rectangles are unoccluded bounding boxes, not measured visible pixels.

| Group | Min XYZ | Max XYZ | Gap below protected road volume |
| --- | --- | --- | --- |
| Court | (171.990,0,-61.580) | (263.726,21.250,-47.708) | 22.475 m |
| Return | (148.858,0,-39.362) | (203.120,30.500,61.646) | 18.762 m |

The runtime preflight checks **every emitted volume**, including rotated light boxes, against the original landmark and benchmark reservations and recorded existing city OBBs. It then checks all 1,200 course samples: 18 m lateral envelope plus a sample-step margin, and maximum 17-degree bank/13.3 m deck half-width/1.8 m underbody plus 3 m vertical margin and one sample step. Exact actual site acceptance still requires native telemetry. Only the two co-located original contact buildings are exempted to permit intended facade contact; no global Clear/Reserve rule is weakened.

The source script additionally screens all boxes against 59 conservative possible near-city/district/landmark/transit envelopes, with no conflicts; its far-skyline exclusion uses radius separation. This conservative numerical screen supplements the actual runtime reservation sequence, not a claim that Python instantiated the city. Coplanar podium/walk overlap is removed: concrete and curbs stop at the podium's ±12.9 m boundary; dark skins stop at its top's ±12.6 m boundary. Continuous girders retain intended bearing intersections.

Upper front rectangles: court .15 X0–241/Y578–714; return .15 X53–264/Y424–488; return .22 X193–520/Y612–743. Both groups remain behind .30/.37/.43 cameras. Native review must confirm frontage, dark top and support-contact readability rather than crediting rectangle area.

## Regeneration

Run `python3 SourceAssets/environment-v6-context/audit_opening.py` from the repository root (any working directory is also supported). It writes `placement-audit.json` beside itself. Update the runtime recipe and numerical mirror together when changing dimensions; do not treat stale hashes/projections as proof.

Build through the integrator's separate opening-preview entry point; do not overwrite the original app. Runtime world generation creates the correction under `-vrOpeningFinish construction` or `combined`; off/surface retain original construction. Expect three `VECTOR_RUSH_OPENING_FINISH ... existingMaps=True` logs, two accepted `VECTOR_RUSH_OPENING_GROUP` logs with boxes=49/52 and auditedHousingBoxes=4 each, and `VECTOR_RUSH_OPENING_CONSTRUCTION groups=2 addedLights=4`. Any failed reservation skips the full affected group. No Unity execution was performed by this source task.
