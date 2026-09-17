# Final landmark and road delivery verification

2026-09-08. Parent integration verification; independent landmark acceptance is in 013, and the diagnostic author's bounded final road sample review is in road-reviews/003. This report does not replace those critiques.

## Implemented result

Two imported landmark families use 41,316 triangles across two renderers: the 123 m split signal mast beside the opening right bend and 90.95 m thermal exchange works beside the middle left approach. Whole-course footprint audits remain in environment-v4. Nearby procedural placement respects their reserved sites. Far skyline windows and heights recede, and the specific transformed slab crowding the mast is removed. Final-station/gallery architecture is retained.

The three road-fixture combined renderers now cast no shadows. Native frozen A/B/A demonstrates the removal and return of the selected pole-and-arm road shape with every recorded light, camera and road material/mesh identity fixed. This is a deliberate shadow-composition finish; road material, topology, collider, other casting groups and race tuning are unchanged. It does not explain all historical damp-road boundaries.

## Exact final build and captures

Native GUID `b709ef36660443b7bdec08170d00b8df`. The 60-simulation-second full-circuit capture contains 1,440 frames, with ordinary physics and existing testing autopilot. All 1,440 camera/racer/timestamp records match candidate02 exactly; all five station anchor comparisons pass with zero deltas. First and second recorded start-line crossings are frames 15 and 1040, producing a 1,026-frame / 42.75-second full-lap video. The 24-second continuous preview is frames 130–705 and shows both landmark passages. Videos are silent and encoded at 24 fps simulation timing; they are not performance samples.

Parent directly inspected final 1080p frame 175 and the boosted reveal, the 1280×800 warm-gallery middle and 1920×810 reveal. Parent also inspected corrected-candidate 175/606 and all three original road-control images earlier. These show a clear mast silhouette, off-road thermal pipe/podium composition, the removed road L and intact fixture illumination/HUD. These are sampled still judgments; no continuous playback, human driving or audio acceptance is claimed. Other-aspect landmark approaches were not visually reviewed.

All 1,440 full-circuit PNGs plus five anchors at each alternate aspect pass PNG chunk-CRC and compressed-data validation. The two encoded videos report expected 1080p/24fps frame counts and durations. Capture identity, selected indices and hashes are in the final capture manifest. The complete raw image sequence remains local; selected originals and videos are versioned.

## Behavior and package

All 42 Unity tests pass on the final source. A separate real-time native race completes three laps in 128.32 s; every racer has zero recoveries. Both complete restart launches and all three countdown-pause checks pass. Both final real-time runs pass those behavior checks. First-sample P95/P99 was 16.66/188.14 ms; the unchanged-build repeat returns to 9.23/9.33 ms, near prior 9.22/9.32 ms. Chrome GPU-helper CPU activity is recorded during the successful repeat as well as observed after the first sample, so it does not establish a delay cause. Preserve both runs and leave the first hitch cause undetermined. No Editor build,bake or video encoding ran concurrently. Do not infer guaranteed smoothness from the simulation-time recording or passing behavior checks.

The local macOS archive is 55,318,267 bytes with 441 entries and a clean ZIP CRC check. Exact runtime/resources/settings and app-file SHA-256 manifests identify the packaged build. No GitHub release has been published.

## Limits

Landmark foundations and surfaces remain simplified, distant tower repetition persists and thin authored road joints remain. The bounded landmark and selected road-shadow changes are implemented and reviewable. Their sampled acceptance does not establish subjective continuous-motion quality or AAA visual completion.
