# Opening service construction — rough native candidate

This is an editable procedural source recipe, not a finished asset kit or accepted visual pass. Runtime source is `UnityProject/Assets/Scripts/World/NightDistrict.cs`, methods `BuildOpeningConstruction` through `OpeningWorldBounds`. No exported mesh or extra texture is needed. All new geometry and four practical lights require `OpeningFinishPreview.ConstructionEnabled`; off retains the original city geometry, lights and materials.

## Source and placement audit

Inspected original SSR02 OFF frames 0149 (.15), 0211 (.22), 0283 (.30), generated target A, and `docs/visual-target-reviews/002c-improvement-environment-critique.md`. Original pixels show actual existing track piers descending into a dark ground plane. The service avenue meshes are emissive details without street-light components. `BuildApproachServiceGroup` is at .735, outside this opening. The mast has an existing authored foundation; this pass does not duplicate it or fill its reserved site.

Reproduced TrackPath's 1,200-sample arc-length lookup and the recorded camera position/quaternion/FOV from `evidence/nocturne-v2/ssr-02/off/capture-validation.json`. Nearby-looking grid candidates (152,0,14) and (82,0,84) fail the original 41 m city clearance. The accepted .20 industrial block is centered at (151.253,0,-54.532), faces approximately +X, and already overlaps the existing 34 m inner-grid tower at (152,0,-56). This overlap is existing control content. Its industrial roof/east facade and the tower's full-height north blade provide actual connection endpoints. The industrial east edge is approximately X=171.26; the tower face at X=172 meets the first service level. The north blade reaches Z=-37.82 at the second service datum.

- **Crossing service court:** connects existing frontage (172,0,-54.563) to the actual .200 pier-foot socket (262.623,1.917,-54.817). Top datum 12.75 m matches the existing industrial roof; span path 90.62 m, broken by a grounded room and an intermediate support. Room placed at 32% of the path to avoid the X=224 service avenue.
- **Inner frontage return:** connects the existing tower's north blade (152,0,-37.82) to the actual .275 foot socket (199.803,1.918,59.610). Top datum 22 m, path 108.53 m, broken by a grounded room and two intermediate supports. Room at 70% brings its top/front/recess into the .22 left-city area.

The foot socket is computed from the instantiated `Pier foot` transform at local (-.33,.46,0), not an invented ground coordinate. Its new inboard column bears on the existing foot, away from the existing center pier. The contact footprint is 1.8 × 2.2 m. No second foot is added there. Intermediate supports have their own small ground feet. The two total path lengths are not unsupported single spans; the open intervals are approximately 16–30 m.

Three module types: podium/interface top with exposed fascia, deep recessed service frontage, supported platform spans. Reuses the existing industrial/grid construction and transit concrete/steel/glass plus the existing warm source material. Does not instantiate another authored tower/workshop because the actual contact building already exists; the 8 × 18 × 6.985 m workshop would be a smaller redundant room in this composition.

## Bounds, projection and clearance

Detailed results and source hashes are in `placement-audit.json`. These are source-computed pre-build bounds; runtime logs provide actual chosen bounds and contacts.

| Group | Min XYZ | Max XYZ | Minimum gap below protected road volume |
| --- | --- | --- | --- |
| Court | (171.983,0,-65.079) | (263.726,18.175,-47.209) | 22.909 m |
| Return | (146.703,0,-40.423) | (205.109,31.361,62.208) | 25.938 m |

Screen projections are deliberately labeled **unoccluded bounding rectangles**, not visible pixel coverage. In .15, the court is approximately X=0–740/Y=648–919 (9.66% rectangle), and the return X=0–343/Y=430–720 (4.79%). In .22 the return is X=0–680/Y=639–1080 (14.44%); its actual room is X=168–520/Y=678–998, so a meaningful facade lies toward the left city while the foreground barrier may hide its lower portion. Native review must establish visible coverage. Both groups are behind the recorded .30/.37/.43 cameras; they are depth/clutter regression checks, not promised improvement views.

Existing `Clear` and `Reserve` remain unchanged for all original content. Their 2D lateral-only exclusion cannot admit any true pier-foot connection, so these specific low volumes use a separate conservative full-course preflight: 1,200 route samples; 18 m horizontal envelope plus one sample step; maximum 17-degree banking, 13.3 m deck half-width, 1.8 m underbody and 3 m extra clearance plus one sample step. Each entire world AABB must lie below that protected volume on every nearby course branch. Landmark reservations and transit benchmark exclusions remain mandatory. Existing industrial, district and skyline footprints are recorded only in preview mode and checked per new piece. Only the two existing co-located contact buildings are exempted to permit intentional facade contact. Existing service avenue locations were checked before final room placement. No racing colliders are created.

## Lighting, ownership and regeneration

Four new shadowless spotlights, two per group, excluding racer layer 8. Broad court source: 900 intensity (court) / 1552.94 (return), range 58, cone 100 degrees. Recess source: 480 / 828.24, range 36, cone 92 degrees. Each has a supported dark housing and visible warm lens. Existing transit fills are remote; the mast's washes model its existing landmark, so none was moved. This bounded added light cost needs native measurement. Fog, exposure, sun, postprocessing and existing fixture caster exclusions are untouched.

49 boxes total (19 + 22 preflight pieces, plus 8 small housing/lens boxes), 588 triangles, up to eight batched mesh renderers and four Light components. Reuses four material references; no new materials, maps, native allocations in field constructors, or colliders. Existing NightDistrict mesh ownership/OnDestroy handles the new combined meshes; WorldBuilder retains its existing material ownership.

Regenerate the audit with `python3 SourceAssets/environment-v6-context/audit_opening.py` from the repository root (script also supports any working directory). Runtime recipe regenerates meshes whenever the world builds with `-vrOpeningFinish construction` or `combined`. Telemetry starts `VECTOR_RUSH_OPENING_GROUP` and `VECTOR_RUSH_OPENING_CONSTRUCTION`; rejected groups name the failed reservation/envelope. Expected accepted groups=2, addedLights=4. To change geometry, edit the runtime recipe and the numerical audit together, then rebuild through the separate opening-preview build entry point. Do not overwrite the original application. No Unity execution was performed by this source task.
