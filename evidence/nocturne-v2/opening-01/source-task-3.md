# Task 3 — first surface candidate, source frozen for native review

Implemented a bounded candidate. Native compilation, appearance and performance remain unverified; the root integrator owns those checks. Do not interpret this report as a visual pass.

## Owned files

- `UnityProject/Assets/Scripts/World/WorldBuilder.cs`
- `UnityProject/Assets/Scripts/World/NightTrackLighting.cs`
- New `UnityProject/Assets/Scripts/World/OpeningRoadFinish.cs` and `.meta`

No other runtime files, settings, craft, camera, fog, SSR or package source changed. No Unity execution, commits, worktrees or delegation.

## Audit and design

Reviewed original native opening149/283, warm943, other selected nine views and generated A/B/C in the preceding critique; read 002a/002b and the task/global constraints. Native road shows soft mostly uniform patches, and warm gallery has usable geometry but weak broad face/structure separation.

The original road mesh has 960 rows, 12 transverse subdivisions, UV U=0..1 and V=0..240. `_BaseMap` scale is (3,1), which URP Lit uses for the main material maps; its current repeated noise is therefore small relative to the course. `RoadSurface` enables normal and metallic/gloss maps, BumpScale=.25, and disables environment reflections. This candidate preserves that reflection state. It cannot provide reflected fixture/building structure.

Gallery swept shell and rib meshes have no UVs/tangents; gallery cassettes/returns use scaled primitive cubes with 0..1 face UVs. The landmark metric normal-map convention is invalid here. No gallery texture/normal maps were added. Existing broad geometry is assigned distinct warm-study materials instead.

## Road treatment

`OpeningRoadFinish` partitions rendering into unchanged road plus two existing road regions: rows110..327 (progress .114583..340625) and rows825..866 (.859375..902083). Original position, normals and tangent values are copied exactly; a separate complete original mesh remains on the original MeshCollider. This adds two road renderers/materials, no new collider and no extra geometric surface over the same region. The unchanged renderer excludes the same triangles. Boundary vertices retain original shading normals.

Opening map atlas is 512x2048; warm is 512x512. Each has base, normal and smoothness maps with mipmaps, clamp wrapping, trilinear filtering and 8x anisotropy. Maps use physical deck distance and 22 m transverse width. Broad, interrupted service-polish areas wander through the middle lanes; maintenance margins remain matte. They are functions of physical deck coordinates, not camera or lamp projection. A 12 m fade at each region end blends back toward sampled original control maps. Because those original repeating maps are rebaked into a coarser regional atlas at the transition, exact texel/mip identity at the transition is not claimed; inspect transitions natively.

Nominal smoothness endpoints remain .35..50 with material multiplier .90, metallic remains zero, and BumpScale remains .25. Broad albedo variation is restrained (.83 plus up to .13 spatial modulation); no emissive bright streaks. Continuous metric relief replaces tiny random grain as the new-region normal contribution. Region UV positive scaling preserves the source tangent orientation. Original generated map arrays are retained only during atlas construction, then released; textures/meshes are tracked and destroyed with their owner, and materials use WorldBuilder ownership.

## Opening light rhythm

Reuses the existing 12 outdoor practicals at indices7..18 (progress .12069..31034). No new light count or emission geometry. Every changed source is now centered immediately below its visible diffuser at lateral +/-9.5 m, height12.48 m. The group is explicitly ordered: lit approach7–9, quieter transition10–12, lit bend13–15, descent16–18. Actual intensity values: 520,780,860,400,190,330,820,900,530,260,650,640. Group intensity sum6880 versus control5680 (+21.1% across that bounded group); this is redistribution plus controlled peak increase, not only translated pools. Lamp colors stay the existing cool/amber family.

Coverage aims 5–16 m forward and 1.5 m across center toward the opposite side; cones are92 degrees except quieter indices11/16 at82, inner44, range43. All other outdoor sources stay on their original construction path. Three exterior combined fixture groups remain explicitly ShadowCastingMode.Off. No global exposure, fog, moon, ambient or reflection-probe tuning.

## Warm-gallery calibration

Cool gallery is unchanged. Warm study uses neutral ceramic faces (.265,.255,.228), dark structural ribs (.065,.075,.087), recessed shell/ceiling (.15,.162,.178), and satin metal returns (.20,.215,.23), with separate smoothness/metallic values appropriate to those existing parts. The original panel material still exists but the study uses the new material only for warm gallery.

Keeps the same 18 lights (two concealed point washes and one road spot per bay). Concealed points move to the actual diffuser vicinity, use less saturated warm-white light and bounded range18, preserving ceiling/shoulder contribution. Six road spots use intensities360,440,260,420,360,240, alternating diagonal targets, narrower88/42 cones, range31, and less saturated warm-white color. The total spot intensity is2080 vs1710 (+21.6%), concentrated into distinct cassette responses. No tunnel, hatch, fixture or craft geometry redesign.

## Checks and native hypotheses

- `git diff --check` passed for the owned source changes. New helper/meta inspected for ownership, managed initialization, region triangle coverage and lifecycle. Compilation pending root.
- Off creates no helper, new road maps, new material group or lighting overrides. Existing road map generation and the off gallery call remain intact. The only shared-path refactor stores the same MeshRenderer local to attach the optional preview after original collider creation.
- Expected visible effect: opening approach/bend gain a deliberate bright/dark sequence, with road-scale satin areas breaking broad light coverage; warm943 separates bright ceramic faces, recessed shell and dark ribs/metal returns, with clearer intervals on the road.
- Risks for native review: regional atlas can lose some original fine detail; narrower cones may under-light part of the bend; material contrast may read too stark in the warm gallery; restrained smoothness still may not produce a sufficient road gain. The new source study must be rejected if whole-frame benefit remains negligible. It does not resolve reflected structure.
- First review should inspect surface-only149/211/283 and943, then .37/.43/thermal/station controls and region transitions. No further texture/material finishing before that feedback.
