# Stage 7 city integration

Local playable candidate on `integration/p4-night-city-20260917`, based on Game P4 `b787b5f` and NightCityKit `250d221`. Scene: `Assets/Scenes/NightCityStage7.unity`. Revision: `night-city-stage7-02`. Unity 6000.6.0f1 native build: `f86cee2c911749a783c67594a6e48fd1`.

## Integrated scope

Five facade-connected service terraces add 30 NightCityKit prefab instances in the opening district. Shopfronts have structural rooms and roofs, wet coping, parapets, rooftop cooling/duct equipment, cabinets, tank/extractor variants, small signs and five doorway practicals. Placement uses the actual P4 facade triangles, with both terrace ends supported and the full driving corridor kept clear. The selected course positions are .0375 left, .095 right, .160 left, .240 right and .285 left.

The first authoring attempt rejected an invalid facade site. The completed survey now evaluates placement clearance before selecting sites. Review also caught unsupported rooftop equipment in the prepared source; service-room roofs now support those assets. Native candidate 01 showed the right-side frontage too low behind the rail. Candidate 02 raises only those two terrace bases by four metres, preserving the animated displays and sparse opening composition.

This is the bounded opening-district integration. It does not replace every P4 tower, place every kit asset, or introduce Game P5's separate speed-blur work. Larger skyline replacement and owner artistic acceptance are not claimed.

## Preservation and tests

All **2,169 original P4 Unity files** under Assets, Packages and ProjectSettings remain unchanged according to Git. The new scene and art are additive. Course hash stays `a43cff0540e9b86cc0d60810b1f19e6cd49ae52d69e41ea43f6fb5b590790abc`.

Both candidate revisions pass **290/290 EditMode tests**. Four Stage 7 tests check original course/colliders/weather/animated screens, LOD/material completeness with no added physics, actual mesh bounds against 2,400 road samples across 13 lateral positions and the animated display bounds, and structural support for rooftop equipment. P4 handling, ship, HUD, weather, road and original scene assets are preserved.

## Native validation

Candidate 01 completed an automated native driving lap: 43.435 seconds, 787 source frames, captured game audio and timestamp-based encoding. The capture averaged 18.109 Hz despite requesting up to 24 Hz; it is visual evidence, not a performance benchmark. Matched P4/native comparisons exposed the right-side height issue addressed in revision 02.

The first revision 02 performance attempt launched the binary directly, remained unfocused in every retained hitch sample, and failed with `Rival finish/timeout did not complete`. Its report contains no completed race and its zero aggregate metrics are not valid performance results. [The failed report is retained](performance-unfocused-failed.json). No causal fix or gameplay change is inferred from that failure.

The [foreground Launch Services repeat](performance-foreground.json), using the same binary, completes the player's three laps in **128.667 seconds** with all six finishers, zero recoveries, frozen results, pause freezing and clean restart checks. It records 8.333 ms mean, 9.025 ms P95, 9.317 ms P99 and 9.400 ms maximum delivered-frame intervals, with zero frames above 33.3 ms and zero focus changes. The Metal log changes from requested 1920×1080 to a 3326×2104 surface. This is a successful native runtime sample, not a controlled 1080p benchmark or GPU timing. Editor, capture, encoding and browser playback did not overlap the run; ordinary OS activity was not eliminated. The unfocused failure still needs separate investigation before claiming background-play reliability.

Final revision 02 [native capture](preview-validation.json): **43.029 seconds, 780 source frames**, 1920×1080 throughout the logged surface setup, and captured game audio. Encoding retains 779 non-zero-duration frames with recorded timestamps. Capture averages 18.108 Hz, with an 85.333 ms maximum interval; do not substitute this for uncaptured performance. Native driving-camera frames show the raised right-side storefronts above the rail, the unchanged large animated displays, and the dry covered road with no visible rain in the sampled tunnel interior at course .8204. The [P4 comparison sheet](comparison.jpg) labels nearest course progress and preserves image aspect ratio; it is not an exact simulation-time comparison.

The browser replay decoded through its complete 43.029 seconds and all 12 comparison images loaded. Browser screenshot capture timed out, so appearance was checked directly from the native source frames and generated comparison images. Owner aesthetic judgment and human/controller feel remain open.

## Local outputs and reproduction

Output root: `/Users/anping.wang/output/vector-rush-p4-city-integration-2026-09-17`.

- Native app: `NightCityStage7-02.app`.
- Authoring evidence: `authoring-03/integration.json`.
- Build identity: `build-02/build-identity.json`.
- Full regression results: `tests-final.xml`.
- Source preservation: `p4-preservation.json`.
- Final native review: `http://127.0.0.1:8774/review-02/`, or `review-02/index.html` locally.
- Native video: `preview-02/full-lap.mp4`.

The portable wrapper now selects Stage 7: `bash tools/current-game.sh build <fresh absolute .app> <fresh absolute evidence>`. `p4-build` preserves the original Stage 6 comparison; `baseline-build` remains the older HUD baseline. `bash tools/current-game.sh test <fresh absolute directory>` runs the complete EditMode suite. The changed shell wrapper passed syntax validation; the final app was built by the same underlying Unity method with the Stage 7 scene explicitly selected.

Open the serialized Stage 7 scene explicitly in Unity; no asset regeneration is needed. For explicit regeneration, run `VectorRush.Editor.NightCityIntegrationSetup.Prepare` with `-productionEvidence <fresh absolute folder>`. It starts from the preserved Stage 6 scene. Build with `VectorRush.Editor.ProductionSceneSetup.BuildExperienceCandidate`, `-experienceScene Assets/Scenes/NightCityStage7.unity`, `-experienceBuildOutput <fresh absolute .app>` and `-productionEvidence <fresh absolute folder>`.

Launch native checks through `open -W -n <app> --args -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -productionValidation <fresh absolute folder> performance`. Do not overlap performance with the Editor, capture, encoding or browser playback. For a driving recording use `preview -previewHz 24` instead of `performance`, then run `tools/production-preview.py` after the app exits. `tools/night-city-review.py` packages that recording and the preserved P4 preview into a labeled local review.

The owner approved committing and pushing this milestone on September 17, 2026. The historical publication manifest is unchanged. Human/controller handling and owner artistic acceptance remain separate from automated native validation.
