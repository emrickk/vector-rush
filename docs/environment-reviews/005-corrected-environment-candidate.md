# Environment review 005: corrected native candidate

Independent review, 2026-09-08. **Accept candidate 02 as the composition to take into motion, aspect-ratio and performance verification. Do not yet accept the finished benchmark or expand it around the circuit.** The specific blank-wall, unreadable-station and harsh gallery-lighting failures in [review 004](004-first-environment-candidate.md) have been substantially corrected. The passage now has an identifiable nearby district and a constructed enclosure. Its nearest facades still look conspicuously simple, and the transit structure has not yet become a strong signature landmark. This is a better prototype environment, not AAA presentation.

## Evidence and limits

Directly inspected all five 1920 × 1080 candidate anchors and their five corresponding [locked baseline anchors](../../evidence/environment-baseline-01/baseline-manifest.json). The [candidate report](../../evidence/environment-candidate-02/environment-evidence.json) records a complete 264-frame passage captured from 08:38:44 to 08:39:17 UTC, using Unity 6000.6.0f1 on Apple M2 Max. The anchors remain frames 0, 170, 190, 210 and 240.

Player position and track progress match the baseline anchors, but the report correctly retains `matchedWithinTolerance: false`. Camera offsets are 0.055–0.150 m; the largest reported angle difference is 0.163° and FOV difference is 0.00455°. This is a useful comparison of visible composition, not an exact pixel-controlled material experiment.

The automated capture uses normal hover physics and 24 simulation frames per second. All five anchors are grounded; only 05 is boosting. I reviewed still images, not continuous normal-speed playback. I did not hear audio, inspect a crowded passage, verify other aspect ratios or measure this candidate's performance. The 11-second picture sequence and its successful capture do not establish those checks.

## Five-view comparison

| View | Visible result | Remaining issue and judgment |
| --- | --- | --- |
| [01 approach](../../evidence/environment-candidate-02/01-approach.png), [baseline](../../evidence/environment-baseline-01/01-approach.png) | The large terrace on the left has setbacks, floor divisions and a clearer silhouette. Low structures and the transit grouping toward the right add a middle layer around the banked road. The bend remains legible. | Several broad amber floor bands read as flat luminous rectangles. Nearby building scale still conflicts with the dense small windows in the far city. Composition can advance; near-facade finish needs attention before the same family is multiplied. |
| [02 entry](../../evidence/environment-candidate-02/02-entry.png), [baseline](../../evidence/environment-baseline-01/02-entry.png) | The deep portal, primary ribs, ceiling surfaces and mounted fixtures establish enclosure. Softer upper illumination preserves more structure than candidate 01's row of bright pools. The next curve stays visible. | Bays remain regular, but the repetition no longer overwhelms the portal in this still. Keep this construction and light direction. Motion must establish whether the fixture rhythm becomes distracting at speed. |
| [03 middle](../../evidence/environment-candidate-02/03-middle.png), [baseline](../../evidence/environment-baseline-01/03-middle.png) | The ceiling has readable depth and joints. The right service recess/grille breaks the broad wall span with a functional cue. Warm structure, cooler road and bright craft remain separated. | The wall and ceiling surfaces remain rather uniform and new-looking. This is finish work, not a reason to replace the gallery shell. The satin road is quieter in the image, but its stability remains a motion question. |
| [04 exit](../../evidence/environment-candidate-02/04-exit.png), [baseline](../../evidence/environment-baseline-01/04-exit.png) | The close portal frames open sky and the continuing road. The canopy and illuminated supports are visible in the middle distance on the right. The adjacent tower now has readable setbacks and facade divisions. Upper-right light spill is less aggressive and the timing fields remain clear. | The transit building is a small, partial introduction here; the adjacent tower is more visually prominent. The missing-destination failure is reduced, while landmark hierarchy remains modest. Preserve the open driving line. |
| [05 reveal](../../evidence/environment-candidate-02/05-reveal.png), [baseline](../../evidence/environment-baseline-01/05-reveal.png) | The former left blue slab is now a service frontage with roof depth, framed openings and ventilation. On the right, a canopy, lit supports and occupied bays establish trackside infrastructure. The district has a stronger near layer while the central road and sky remain open. | The large amber pane on the left is the most conspicuously unfinished object in the passage. The right frontage also uses broad, flat amber openings. The station now reads as a constructed place, but its cropped, conventional profile does not yet provide the memorable silhouette requested by the plan. |

## What blocks acceptance, and what can be polished

**The documented composition failures no longer require a wholesale scene rebuild.** The left service frontage faces the camera usefully, the exit introduces the transit structure, and the gallery has a lit ceiling with functional variation. Preserve these corrections while collecting the remaining verification.

**Verification still blocks benchmark acceptance and expansion.** A still cannot establish window shimmer, road highlight stability, lamp strobing, parallax, disappearing surfaces, rival readability or frame cost. Perform those checks on the exact candidate. This report supplies no motion or performance pass.

Two bounded art priorities remain:

1. **Finish the nearest amber openings first.** Start with the left workshop in 05, the right station frontage and the large terrace bands in 01. Retain the current massing; add a believable recess or dark interior break, a few meaningful frame divisions, restrained value variation and consistent opening scale. Avoid a dense replacement window grid. Acceptance: the closest opening reads as glazing or an occupied interior at native gameplay size, rather than a single colored panel, and the near/far scale relationship is less abrupt.
2. **Give the station one clearer structural identity.** Its canopy and supports are now sufficient to locate the same architectural grouping across 04 and 05. A stronger asymmetric roof termination, visible support relationship or separated upper mass should make that grouping memorable without relying on a sign or brighter emission. Judge both cameras together. This is a focused identity pass, not a request for more buildings or a larger object beside the road.

The gallery's residual surface uniformity is lower-priority polish. Keep the softer lighting, visible ceiling and service recess. Do not reintroduce equally intense lamp pools to manufacture detail. The original dense skyline also remains generic, but adding further towers would not improve the nearest unfinished architecture.

## Road treatment and next gate

The quieter satin road is a legitimate material direction under the [environment plan](../visual-environment-plan.md). Accept it as a deliberate finish change for further evaluation. Do not describe it as a proven cure for the old diagonal sheen: [review 003](003-road-normal-shadow-controls.md) left the root cause unresolved, and this candidate changes more than one visual condition.

Next, review the complete passage at normal playback speed, with ordinary and boosted travel distinguished. Check nearby craft, supported 16:10 and ultrawide views, and fresh frame-time evidence against a current baseline. If the targeted facade/identity polish changes the candidate, refresh the affected anchors and the relevant motion evidence before accepting it. Preserve candidate 01's rejection and candidate 02's matching limitations.

Only this critique document was authored for the review. No art, lighting, camera, gameplay, HUD or capture source was edited.
