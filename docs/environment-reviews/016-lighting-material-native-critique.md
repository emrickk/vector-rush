# Environment review 016: native lighting and material critique

Independent review, 2026-09-08. **Accept candidate 01 for this bounded lighting and material step. No required corrections.** The thermal plant now has readable service recesses and supported pipework; the mast gains a modest warm separation beneath the skyroom. The inspected race views retain the existing night-road clarity. This is a visible improvement, not a claim of complete material realism or overall release readiness.

## Evidence and comparison limits

Directly inspected eight original **1920 × 1080** candidate images against the same eight baseline originals identified in [review 015](015-lighting-material-direction.md): frames **148, 175, 193, 534, 570, 606 and 641**, plus **05-reveal.png**. All fourteen selected frame copies, seven per capture, were verified byte-identical to their original sequence frames by SHA-256.

The [candidate report](../../evidence/lighting-depth-candidate-01/full-lap/environment-evidence.json) identifies build GUID `5c49b92643bd4d41820ba81857fdf8c7`, Unity 6000.6.0f1 and Apple M2 Max. Its complete 1,440-frame sequence ran from 20:06:00 to 20:08:17 UTC, with normal hover physics, automated steering and 60 simulation seconds at a 24 fps simulation capture rate. It records 1,247 renderers and 103 active lights, compared with 1,247 and 101 in the [baseline](../../evidence/landmarks-final/full-lap/environment-evidence.json). The [source manifest](../../evidence/lighting-depth-candidate-01/source-manifest.json) declares four shared texture pairs, finish and glazing changes, a mast fill, a thermal portal fill and a retargeted thermal rear wash, with geometry, road finish and global lighting fixed.

These are nearby views, not exact pixel matches. The seven frame-index comparisons are approximately 0.322–0.326 m apart in camera position and 0.010 s apart in race time; their FOV differences are below 0.00031°. The reveal uses frame 1004 and the same race time and progress, but its camera differs by 0.442 m and its FOV by 0.00901°. All five automated anchor comparisons report `matched: false`. This supports a qualitative visual comparison, not subtraction-based attribution of every changed pixel.

Only the sixteen identified pictures were visually inspected here. No continuous playback, audio, other aspect ratios, performance or human driving was reviewed.

## Thermal plant: the main improvement

At [534](../../evidence/lighting-depth-candidate-01/selected/frame-0534.png) and [570](../../evidence/lighting-depth-candidate-01/selected/frame-0570.png), the previously black service face now separates into an outer wall, dark opening frames and recessed shutter panels. The shallow ledge also reads more clearly. The brightest pool sits above the left two openings at 570, with a soft falloff and darker interiors. It is concentrated, but does not produce a hard circular boundary or flatten the entire facade. No softness correction is required from these views.

At [606](../../evidence/lighting-depth-candidate-01/selected/frame-0606.png), local highlights begin to connect the pipes to their support posts and deck. At [641](../../evidence/lighting-depth-candidate-01/selected/frame-0641.png), the long pipe highlights, warm coupling rings and lit support faces make those connections substantially easier to read. The drum faces retain broad curved shading while the pipes gain a different finish response. The combined treatment gives the equipment more material distinction and physical depth.

The pipe highlights and the support cap at 641 are strong local accents, but the road boundary and approaching bend remain clear. No conspicuous repeated texture seam or checker pattern is visible in these four views; this does not establish texture stability in motion.

## Mast: restrained improvement, remaining limitation

The [148](../../evidence/lighting-depth-candidate-01/selected/frame-0148.png), [175](../../evidence/lighting-depth-candidate-01/selected/frame-0175.png) and [193](../../evidence/lighting-depth-candidate-01/selected/frame-0193.png) views show a modest warmer separation at the room underside and inner support area. The treatment preserves the open silhouette and dark internal cavity. It is most useful in the closer view at 193.

The braces remain predominantly dark and the occupied amber pane still reads fairly flat at this distance. Fine material texture is not a demonstrated benefit in these views. These are limits on the magnitude of the improvement, rather than defects introduced by this candidate; avoid describing the tower as fully resolved realistic construction.

## Composition and decision

Across the inspected pairs, there is no obvious new broad light spill on the player or road, and the HUD remains readable. The [station reveal](../../evidence/lighting-depth-candidate-01/full-lap/05-reveal.png) retains the workshop's warm interior and the canopy's lit supports, with no visible contextual regression. This is an observation of the rendered images, not proof that a light-culling setting isolates every surface.

**Keep candidate 01 and close this bounded step.** Preserve the current local light hierarchy; no additional assets, global exposure change or required follow-up correction emerges from this review. Only this review document was authored by the reviewer; no runtime source, assets or Unity state were changed.
