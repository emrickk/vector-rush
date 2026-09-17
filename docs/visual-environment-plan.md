# Visual environment plan — Nocturne Circuit

**Current governing direction — 2026-09-08:** [Production night finish plan](superpowers/plans/2026-09-08-night-production-finish.md) and [acceptance spec](superpowers/specs/2026-09-08-night-production-finish-design.md). The owner selected improving the existing night scene toward the supplied racing video. The new primary benchmark is the opening city bends, mast and first gallery approach, with five natural crossings at .15, .22, .30, .37 and .43. The final station is now a control view, not the primary acceptance passage. Global light/material experiments and later bounded motion/racing work follow the new staged scope. The implementation described below is historical; the new production plan has not started.

## Historical environment plan and completed bounded passes

Date: 2026-09-08. Current playable baseline: `14b1212`; planning checkpoint: `6fa91de`.

**User priority: visuals and feeling first.** This plan replaces the previous AI-first order. The current work targets the generated-looking city, underdeveloped track, repetitive gallery, black ceiling and diagonal road sheen. Implementation is active. Baseline, authored transit assets, both galleries and track construction are built; corrected composition has advanced to final verification. Full-circuit city expansion remains gated by benchmark acceptance. See the implementation plan and numbered reviews for current status.

## Intended result

Make the race camera show a believable elevated night circuit with weight, depth and a distinct identity. The player should feel the scale of the city, the proximity of trackside structure, the compression of the gallery and the release into open space. The first deliverable is one excellent 10–15-second native passage; the accepted design then expands around the existing circuit.

Direction: a working urban transport district after dark. Dark concrete, charcoal ceramic and restrained metal provide the material base. Cool exterior lighting establishes the route and structural edges; warm service/interior lighting gives the city life. Cyan is a navigation accent. Use large readable forms, deliberate gaps and a few distinct landmarks.

For this stage, “feeling” primarily means perceived scale, speed, enclosure and visual clarity in motion. Keep the craft, HUD, player handling and AI as stable references. Diagnose a camera problem if the new scenery exposes one, but do not turn this into a racing-balance or feature-development project.

## 1. Fix the camera benchmark and design the passage

Use the current final-sector turn, warm gallery and exit straight. Preserve the same start progress, vehicle pose, camera, resolution and initial lighting settings for comparisons. Save reference views at approach, gallery entry, middle, exit and skyline reveal. Include the current defects instead of choosing flattering angles.

Produce a small set of paintovers or composition studies based on those actual views. Specify what fills each depth layer, what the player looks toward, and which edges communicate the next turn. Choose one direction before detailed asset production. References are design targets, not evidence of native quality.

Deliverables: five baseline images, an annotated passage layout, a compact palette/material sheet and a bounded asset list. Add a 10–15-second continuous baseline at ordinary and boosted speed. Capture silent picture comparisons if necessary, but assess native presentation with audio separately when judging the overall experience.

## 2. Rebuild city composition from the race camera

Arrange three visible depth layers. Existing source already generates skyline rings; success means those layers read in the actual image, not simply that they exist in code.

| Layer | Build | Visual purpose |
| --- | --- | --- |
| Near track | Service platforms, structural buttresses, retaining edges, utility rooms and supported lamps, safely outside the driving envelope | Weight, scale and fast parallax |
| Middle district | A few composed blocks with podiums, setbacks, roof equipment, recesses, courtyards and a visible street/service level | A place the circuit belongs to |
| Far skyline | Quiet silhouettes with deliberate height groups, gaps and reduced detail/contrast | Depth and a readable destination |

Use three architectural families: stepped terrace blocks, taller split-mass towers, and low service/transit buildings. Inspect and reuse the existing authored terrace/split assets where they fit; author missing geometry in Blender. Each family needs a distinct silhouette and construction logic, not merely a different window seed.

Create one signature transit structure at the gallery exit, composed to frame an asymmetric skyline reveal without hiding the racing line. Buildings should have bases, middle sections and roof terminations. Nearby windows get recessed frames or limited interior depth; distant windows become quieter grouped light patterns. Light occupancy in plausible floor/room groups instead of covering every facade with equally sharp random dots.

Deliverable: the benchmark's nearby district and skyline read as a composed place, including an actual relationship to the ground/service layer. Do not rebuild the entire city in this step.

## 3. Give the track and gallery visible construction

Keep the current centerline, bank and driving clearance while improving visible construction. Establish a consistent deck cross-section, edge thickness, barrier base and service ledge. Visible road/barrier boundaries must agree with collision boundaries; inspect any decorative additions from the driver’s approach before accepting them. Use expansion joints, drainage channels, restrained wear and access panels at plausible scale. Add support connections where visible from the chase camera. Directional markings should clearly read as navigation; redesign the decorative boost-like chevrons so they do not promise a new mechanic.

For the gallery, author a coherent structural kit: primary ribs, supported ceiling cassettes, inset wall panels, service channels and integrated light housings. Define three distinct conditions—entry portal, interior span and exit frame—rather than repeating an identical decorated bay. Smaller material changes support that construction hierarchy.

The ceiling must remain visible as a surface with depth. Aim low-intensity light at the ceiling and upper structure, balance the material response, and use local occlusion where it clarifies joints. Bright strips need physical fixtures and appropriate spill. Keep a readable route through the entire enclosure; avoid a featureless black roof or a uniformly glowing tunnel.

Deliverable: entry, middle and exit feel different while belonging to the same structure. Compare both empty and occupied track views so new details do not bury opponents or the craft.

## 4. Resolve road shading with a controlled experiment

Run this technical diagnosis alongside city asset authoring. Keep it isolated from the new art while identifying the cause. The earlier periodic-noise fix did **not** remove the diagonal sheen; the 0.42–0.78 smoothness range reduced its contrast only. Retain [the diagnosis and failed hypothesis](road-dampness-diagnosis.md).

At the same native gallery pose, compare one variable at a time: constant smoothness, normal mapping disabled, simple calibrated material, single-light versus overlapping-light contribution, and then mesh normals/tangents/interpolation if earlier tests point there. Check texture import/normal encoding and mip behavior. Compare triangle boundaries with the observed pattern before blaming geometry. A diagnosis needs a repeatable change in the actual artifact.

Restore each condition before the next test. Fix the identified cause, then rebuild the intended damp-surface finish with restrained roughness variation. If the original damp treatment remains unstable, adopt a deliberately authored satin road material that still responds coherently to warm/cool lighting. Record that as a material redesign, not a proven root-cause repair.

Deliverable: stable highlights with no distracting diagonal panel pattern in the matched view or moving passage. Global darkening, fog or extra bloom are not acceptance evidence.

## 5. Tune lighting, depth and perceived speed together

Use a controlled progression: cool open approach, a readable warm enclosure, then a cooler open skyline reveal. Light surfaces and construction, not only luminous strips. Maintain useful shadow detail without flattening the night palette. Separate the player craft from both dark structures and bright pavement; account for the existing HUD's small labels over lamps.

Make speed legible through near-field parallax, passing structural markers, light-pool rhythm and the changing enclosure. Keep markers far enough from the driving envelope to preserve navigation and comfort. Avoid evenly spaced high-contrast elements that flicker or feel like a strobe at speed.

Keep the camera fixed for the first environmental A/B. If it hides the exit, clips structure or undercuts scale, make one separate bounded camera comparison, preserving optional shake. Do not use stronger shake or a larger FOV as a substitute for depth. Assess ordinary speed, boost, a corner and gallery entry/exit continuously.

Deliverable: a passage that feels larger, faster and more spatially convincing while the road and opponents remain easy to read. This subjective acceptance needs an actual continuous viewing; selected stills alone cannot establish it.

## 6. Review the benchmark, then extend the language

Independent reviewer compares the five fixed views and actually watches the full traversal at normal playback speed. Review at native 1080p and the previously supported 16:10/ultrawide shapes. Check the following before expansion:

- Architecture remains recognizable with signs hidden; three depth layers are visually distinct.
- The deck, barriers, gallery ceiling and fixtures read as constructed objects at racing distance.
- Entry, enclosure and exit have distinct compositions with a clear next turn.
- The road sheen is stable; no conspicuous shimmer, popping, disappearing surfaces or lamp strobing in motion.
- Craft, rivals and HUD stay readable in bright and dark portions. Scenery does not intrude into the race or camera path.
- Re-measure the exact candidate on the M2 Max after warm-up. Compare frame-time percentiles with a fresh baseline and identify regressions before multiplying assets/lights. Keep simulation-time recordings separate from performance samples.

Allow two substantial review-and-correction cycles for the benchmark. If it still fails, reconsider its composition or construction approach before adding more detail. Keep the rejection and matching evidence.

After acceptance, extend the architectural and track kit across the existing circuit. Compose three recognizable districts—open skyline section, close service district and enclosed gallery—with quieter transitions. Reuse families with controlled placement, scale and lighting changes; every stretch does not need landmark density. Review an uninterrupted full lap so the repeated kit does not become obvious again.

Deliverables: updated native build, a short benchmark comparison, a current full-lap preview, editable Blender assets/export scripts, updated evidence and independent critique. Commit and push each completed milestone with Unity metadata and asset provenance preserved.

## Ownership and implementation order

Parent owns the passage composition, shared WorldBuilder/material changes, render settings and integration. Environment author owns nominated Blender assets, exports and NightDistrict composition. A track/light author may own NightTrackLighting and gallery assets under a separate contract. Independent reviewer changes critique files only. If code boundaries require shared edits, integrate them sequentially; do not let two owners alter the same scene or WorldBuilder file.

Order: baseline/composition first; city/track assets and isolated road diagnosis can then run in parallel; lighting and speed impression follow integration; native benchmark acceptance precedes full-circuit expansion.

AI pace, race-rule changes, new ship forms, another HUD redesign and new gameplay systems are deferred. The first implementation task is the five-view benchmark and scene composition for the final-sector/gallery/exit passage.

## Subsequent user-selected landmark / road milestone

2026-09-08: the user prioritized distinctive roadside landmarks, then road finish. The first/middle sectors now contain an original split signal mast and thermal exchange works with reserved footprints, quieter skyline layers and an independently verified mast sky gap. The final station remains. Native A/B/A isolates the current L-shaped road strip to the fixture caster family; disabling those three renderer casters preserves light pools without changing the road material/mesh or race physics. Reviews012/013 and road-reviews002/003 record acceptance and boundaries; final build b709ef36660443b7bdec08170d00b8df is packaged locally. All42 tests and both final race/restart checks pass. One performance sample has unexplained long intervals; the repeat returns near prior timings, with both preserved. This closes the requested bounded implementation, not every earlier broad city or subjective-motion ambition.

## Lighting/material depth acceptance — 2026-09-08

Candidate01 uses original ceramic/concrete/titanium/service finish maps with selective mast soffit and thermal portal/support fills. Review016 accepts the actual native images with no correction request. Its strongest benefit is the thermal service facade and connected pipework; the mast remains a subtler improvement. Keep the current light hierarchy. No new global exposure, geometry, road or race tuning is authorized by this bounded step. Preserve the small camera drift in comparisons and the limitations on fine texture, amber-pane depth and continuous motion. Final test/performance/package evidence is recorded separately.

## Production finish plan supersedes prior next-step recommendations

Retain the night circuit, original craft and landmarks, useful HUD and proven race/road corrections. Improve how lighting, road surfaces, connected construction and actual racing work together in ordinary camera views. Independent planning critique019 identifies six observable gates and explicitly rejects accepting isolated assets or sampled stills as proof of the entire moving experience. The earlier coastal comparison remains useful evidence about readability; restoring the coast is no longer the active proposal. See the linked production plan for ownership, iteration limits, rollout order and final delivery requirements.
