# VECTOR RUSH

A playable, original anti-gravity racing prototype for Apple Silicon macOS. Pilot Kestrel 07 through three laps of Nocturne Circuit against five rivals. The current development direction is a night urban circuit.

[Private GitHub repository](https://github.com/emrickk/vector-rush) · [Build brief](docs/GPT6-BUILD-PROMPT.md) · [Development history](docs/development-history.md) · [Ship art plan](docs/ship-art-plan.md)

## Current milestone — landmarks and road finish

Two original landmarks now give the circuit distinct places: a split signal mast with an elevated room beside the opening bend, and a three-drum thermal exchange works with a service podium and pipe bridge beside the middle sector. Their footprints keep nearby blocks clear. A lower, quieter skyline and removal of the oversized slab beside the mast give the structures more space. The final-sector station and both galleries remain.

A native before/off/restored experiment identified the conspicuous L-shaped road band as the streetlamp family's moon shadow. Disabling casting on those three fixture mesh groups cleans up the satin road's light pools while retaining visible lamps, architectural shadows and vehicle grounding. Road geometry, surface material and racing physics remain unchanged by that correction.

[Current 24-second landmark preview](evidence/landmarks-final/VectorRush-landmarks-preview.mp4) · [Current 42.75-second full lap](evidence/landmarks-final/VectorRush-full-lap.mp4) · [Current local macOS archive](Builds/VectorRush-macOS-landmarks-2026-09-08.zip). Both videos are continuous excerpts of actual 1080p native rendering, with silent automated steering at 24 simulation frames per second. The full lap spans two recorded start-line crossings.

[Independent landmark critique](docs/environment-reviews/012-landmarks-native-critique.md) · [Landmark correction verification](docs/environment-reviews/013-landmark-correction-verification.md) · [Road shadow experiment](docs/road-reviews/002-fixture-caster-native.md) · [Final road sample review](docs/road-reviews/003-final-road-verification.md).

The closer-racing preset remains53m/s cruise and 72 m/s boost. Its prior accepted automated race kept a rival within 60 m of validated progress for 80.9% of post-start racing, with no recoveries. Nearby rivals are often behind the camera; the named distance cue exposes their position. The nearest prior finish gap was 100.85 m and no overtake was demonstrated. [Pacing evidence and limitations](docs/pace-reviews/002-native-pace-comparison.md).

## Play

Open **[Builds/Vector Rush.app](Builds/Vector%20Rush.app)** or unpack **[the current local archive](Builds/VectorRush-macOS-landmarks-2026-09-08.zip)**. Source, editable assets and selected evidence are pushed to GitHub. The native build is ready locally; no GitHub release has been published. Earlier archives are historical.

| Action | Keyboard | Gamepad |
|---|---|---|
| Start / confirm | Enter | A / south button |
| Accelerate | W / Up | Right trigger |
| Steer | A/D or Left/Right | Left stick |
| Brake | S / Down | Left trigger |
| Airbrakes | Q / E | Left / right shoulder |
| Boost | Space | A / south button |
| Recover | R | Y / north button |
| Pause | Escape / P | Start |

Menus support the pointer. Pause includes Resume, Restart, camera shake and Quit. Results supports another race.

## Current validation

- Native build **b709ef36660443b7bdec08170d00b8df** captures 1,440 frames / 60 simulation seconds. All 1,440 camera, racer and timestamp records exactly match the pre-road-correction candidate; all five station anchors match exactly.
- **42/42 Unity tests passed** on the final source, including pursuit geometry, rival corridor assistance and existing input/race/restart checks.
- Complete current circuit at **1920×1080**, plus five final-sector views each at **1280×800** and **1920×810**. All 1,450 original PNGs passed chunk-CRC and compressed-data validation. The inspected smaller warm-gallery and ultrawide reveal views remain readable. Other aspect-ratio landmark approaches were not newly inspected.
- Separate real-time native race completed **three laps in 128.32 seconds**, with **zero recoveries across all six racers**. Both restart launches and all three countdown-pause checks passed. Both final runs passed these behavior checks. The first performance sample had intermittent long frames (P95/P99 16.66/188.14 ms); the repeat returned to **9.23/9.33 ms**, close to the prior 9.22/9.32 ms. The first hitch cause is undetermined; both samples and competing-process observations are preserved in the [performance comparison](evidence/landmarks-final/performance-comparison.json).
- Final macOS archive integrity passes: **55,318,267 bytes**, 441 ZIP entries. Runtime source/resources/settings and app hashes are recorded in the [delivery manifests](evidence/landmarks-final/build-manifest.json).

[Current evidence](evidence/landmarks-final/) · [Real-time race and metrics](evidence/landmarks-final/performance/) · [Previous close-racing milestone](evidence/close-race-final/).

This remains a working prototype. Landmark surfaces and foundations are simplified, distant buildings still repeat, and ordinary road seams remain. The fixture experiment explains the selected current shadow shape; it does not retroactively resolve every earlier damp-road artifact. Visual acceptance here covers documented sampled images, not continuous playback, subjective human handling or audio quality. Earlier physical-input checks remain historical coverage.

## Previous coastal delivery and evidence

- Revised Blender ship with layered armor, open structure and recessed engines; two authored tower designs grouped on substantial waterfronts; textured cliffs, additive exhaust, bloom and contact shading.
- **19/19 Unity tests passed**, covering race progress, finish order, recovery and paused input.
- **Three laps in 112.64 seconds, zero player recoveries**, using automated steering through ordinary hover physics. Both complete restart launches and three countdown/pause checks passed. Several rivals used recovery; AI tuning remains provisional.
- **1920×1080 on Apple M2 Max:** 60-second standalone sample, VSync enabled, mean 16.69 ms, P95 16.76 ms, P99 16.96 ms. These are observed frame intervals, not isolated GPU timings. Unity reported 203.6 MiB allocated memory. No Editor or asset generation ran during this sample.
- Native pointer activation checked at 1280×800 and 1920×1080, including outside-click rejection. Keyboard start/pause/resume and throttle were checked. Physical gamepad hardware and a sustained human handling assessment remain untested.

[Previous coastal gameplay preview](evidence/gameplay-final/VectorRush-gameplay-current.mp4): 15 seconds, 1280×720, 24 fps, silent. This is actual native gameplay with automated steering, recorded at fixed simulation time; it is separate from the real-time performance sample and does not show the newer night candidate.

![Previous coastal native race view](evidence/run-09-coastal/06-city-descent.png)

The native race evidence is in [run-09-coastal](evidence/run-09-coastal). The unobstructed dedicated cliff inspection is in [coast-inspection-10](evidence/coast-inspection-10); the earlier obstructed inspection is retained as a failed capture. Earlier runs and negative diagnostics remain labeled history, not current presentation evidence.

## Previous coastal review and general limits

This is a working prototype, **not an AAA-quality game**. The independent critic accepts the inspected visual integration but rejects the AAA target. Broad cliff planes and texture repetition, sparse quays, dark tower glazing and simple water remain visible gaps. Minor close-up craft details and some environment export cleanup also remain. The previous severe road reflection artifact is absent from the inspected final views after using a matte Lit deck; other materials retain reflections.

The slice includes one course, one craft shape, five AI rivals and original procedural sound effects. It has no weapons, multiplayer, campaign, licensed soundtrack or additional circuits. Audio mix and human driving feel need further assessment.

## Project and editable sources

- [UnityProject](UnityProject/) — Unity **6000.6.0f1**, URP **17.6.0**. Open `Assets/Scenes/Solstice.unity` and press Play.
- [SourceAssets/hero-v2](SourceAssets/hero-v2/) — preserved previous ship, export generator, engine anchors and inspection metadata.
- [SourceAssets/hero-v3](SourceAssets/hero-v3/) — current ship source, immutable finish/bake payloads and preserved earlier passes.
- [SourceAssets/environment-v2](SourceAssets/environment-v2/) — preserved tower/cliff kit and texture sources.
- [SourceAssets/environment-v3](SourceAssets/environment-v3/) — current editable transit/service and night-tower sources, export recipes and placement audits.
- [SourceAssets/environment-v4](SourceAssets/environment-v4/) — current editable landmark kit, preserved rejected/corrective passes, live export identity and full-course placement audit.
- [Asset regeneration guide](docs/asset-regeneration.md) — safe staged rebuilds that protect the reviewed live assets.
- [Reference concept](references/solstice-chase-concept.png) — generated art direction, distinct from actual runtime and Blender inspection images.
- [Toolchain and provenance](docs/toolchain.md), [test results](evidence/editmode-results.xml), [current build manifest](evidence/landmarks-final/build-manifest.json).

The craft, environment geometry, interface and sound synthesis are original. Cliff surface maps are **Rock 3 by Rob Tuytel / Poly Haven (CC0)**; original maps, hash manifests and the named Unity mask derivative are retained with [provenance](SourceAssets/environment-v2/textures/Rock3_PROVENANCE.md). No Wipeout assets or branding are included.

## Build

Run `./tools/unity.sh prepare`, `./tools/unity.sh test`, then `./tools/unity.sh build` with the pinned Editor installed. Close this project's Editor before batch commands. `./tools/unity.sh open` opens it interactively.

`./tools/blender.sh` shows explicit versioned asset operations. They write to fresh staging directories; review and copy payloads separately while preserving Unity metadata. The original V1 generator remains historical source and must not replace the current ship accidentally.
