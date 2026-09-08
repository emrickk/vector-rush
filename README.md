# VECTOR RUSH

A playable, original anti-gravity racing prototype for Apple Silicon macOS. Pilot Kestrel 07 through three laps of Nocturne Circuit against five rivals. The current development direction is a night urban circuit.

[Private GitHub repository](https://github.com/emrickk/vector-rush) · [Build brief](docs/GPT6-BUILD-PROMPT.md) · [Development history](docs/development-history.md) · [Ship art plan](docs/ship-art-plan.md)

## Stage 1 checkpoint — paused at owner request

The owner has selected the supplied night-racing video as the target for a more coherent, polished version of this same game. The [production finish plan](docs/superpowers/plans/2026-09-08-night-production-finish.md) keeps the night direction and existing work. It starts with one representative 15-second opening-city passage: broad lighting and road response, connected construction, craft materials, then visible racing, motion and sound. Full-circuit rollout follows separate visual, watched-motion/manual-play and technical gates. The [design and acceptance criteria](docs/superpowers/specs/2026-09-08-night-production-finish-design.md) and [independent planning critique](docs/environment-reviews/019-production-finish-plan-critique.md) define the standard.

The baseline and lighting experiments are complete as a resumable checkpoint. The accepted fog correction adds city depth; the clearer reflection-off road is restored after the reflection candidates failed to show a worthwhile benefit. Stage 1 remains incomplete against its production road-response target. Work is paused at the owner's request; see the [current handoff](docs/HANDOFF.md) and [independent A/B/C review](docs/environment-reviews/021-stage1-lighting-road-native.md). The earlier coastal-restoration suggestion is superseded by the chosen night direction.

## Previous accepted delivery — lighting and material depth

The signal mast and thermal exchange works now use distinct ceramic, cast-concrete, satin-metal and service-coating finishes. Selective light reveals the thermal plant's recessed entrances and pipe supports, with a restrained warm fill beneath the mast's upper rooms. Independent native review accepts this pass; the mast's amber pane remains comparatively flat.

Two original landmarks now give the circuit distinct places: a split signal mast with an elevated room beside the opening bend, and a three-drum thermal exchange works with a service podium and pipe bridge beside the middle sector. Their footprints keep nearby blocks clear. A lower, quieter skyline and removal of the oversized slab beside the mast give the structures more space. The final-sector station and both galleries remain.

A native before/off/restored experiment identified the conspicuous L-shaped road band as the streetlamp family's moon shadow. Disabling casting on those three fixture mesh groups cleans up the satin road's light pools while retaining visible lamps, architectural shadows and vehicle grounding. Road geometry, surface material and racing physics remain unchanged by that correction.

[Previous accepted 24-second lighting preview](evidence/lighting-depth-final/VectorRush-lighting-preview.mp4) · [Previous accepted 42.75-second full lap](evidence/lighting-depth-final/VectorRush-full-lap.mp4) · [Previous accepted local macOS archive](Builds/VectorRush-macOS-lighting-depth-2026-09-08.zip). Both videos are continuous excerpts of actual 1080p native rendering, with silent automated steering at 24 simulation frames per second. The full lap spans two recorded start-line crossings.

[Current lighting/material critique](docs/environment-reviews/016-lighting-material-native-critique.md) · [Independent landmark critique](docs/environment-reviews/012-landmarks-native-critique.md) · [Landmark correction verification](docs/environment-reviews/013-landmark-correction-verification.md) · [Road shadow experiment](docs/road-reviews/002-fixture-caster-native.md) · [Final road sample review](docs/road-reviews/003-final-road-verification.md).

The closer-racing preset remains53m/s cruise and 72 m/s boost. Its prior accepted automated race kept a rival within 60 m of validated progress for 80.9% of post-start racing, with no recoveries. Nearby rivals are often behind the camera; the named distance cue exposes their position. The nearest prior finish gap was 100.85 m and no overtake was demonstrated. [Pacing evidence and limitations](docs/pace-reviews/002-native-pace-comparison.md).

## Play

The local **[Builds/Vector Rush.app](Builds/Vector%20Rush.app)** contains the Stage 1 checkpoint; consult its limited validation in the [handoff](docs/HANDOFF.md). The **[previous accepted local archive](Builds/VectorRush-macOS-lighting-depth-2026-09-08.zip)** retains the earlier fully checked lighting/material delivery. Source, editable assets and selected evidence are pushed to GitHub. No new native archive or GitHub release is part of this wrap-up.

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

## Previous accepted delivery validation

- Build **5c49b92643bd4d41820ba81857fdf8c7** records a complete 1,440-frame native circuit at 1920×1080. All PNG chunk-CRC/decompression checks pass. Small camera drift remains against the previous build; exact anchor comparisons fail and are preserved as such.
- **42/42 existing Unity tests pass** on that delivery's source. The 193 source/resource/settings hashes and 189 app-file hashes remain unchanged after tests.
- Independent review016 compares eight native views. Additional parent inspections cover the cool-gallery approach/interior, crest and station reveal. This is sampled visual acceptance, not subjective continuous playback or human driving assessment.
- The 42.75-second full lap and 24-second lighting excerpt pass complete video decoding. Both are silent, continuous native rendering with automated steering at 24 simulation frames per second, separate from performance measurement.
- Current local archive passes integrity checks: **56,664,092 bytes**, 441 ZIP entries. [Delivery manifest](evidence/lighting-depth-final/build-manifest.json).
- Five station-context views each at **1280×800** and **1920×810** complete successfully on the same build; all 10 PNGs pass validation. The inspected warm-gallery and ultrawide reveal retain road, craft and HUD readability.
- A separate real-time native race finishes **three laps in 128.32 seconds**, with **zero recoveries across all six racers**. Both restart launches and all three countdown-pause checks pass. The 60-second performance sample averages **8.35 ms**, with **P95 9.21 ms / P99 9.33 ms**, consistent with the prior accepted repeat. [Race/performance comparison](evidence/lighting-depth-final/performance-comparison.json).

[Previous accepted evidence](evidence/lighting-depth-final/) · [Reviewed native circuit](evidence/lighting-depth-candidate-01/) · [Previous landmark milestone](evidence/landmarks-final/).

This remains a working prototype. Foundations and glazing are simplified, distant buildings still repeat, and ordinary road seams remain. The fixture experiment explains the selected current shadow shape; it does not retroactively resolve every earlier road artifact. Fine texture stability in motion, subjective handling and audio quality are outside the inspected evidence. Earlier physical-input checks remain historical coverage.

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
- [SourceAssets/environment-v5-materials](SourceAssets/environment-v5-materials/) — current original architecture finish textures, generator and validation.
- [Asset regeneration guide](docs/asset-regeneration.md) — safe staged rebuilds that protect the reviewed live assets.
- [Reference concept](references/solstice-chase-concept.png) — generated art direction, distinct from actual runtime and Blender inspection images.
- [Toolchain and provenance](docs/toolchain.md), [test results](evidence/editmode-results.xml), [current build manifest](evidence/lighting-depth-final/build-manifest.json).

The craft, environment geometry, interface and sound synthesis are original. Cliff surface maps are **Rock 3 by Rob Tuytel / Poly Haven (CC0)**; original maps, hash manifests and the named Unity mask derivative are retained with [provenance](SourceAssets/environment-v2/textures/Rock3_PROVENANCE.md). No Wipeout assets or branding are included.

## Build

Run `./tools/unity.sh prepare`, `./tools/unity.sh test`, then `./tools/unity.sh build` with the pinned Editor installed. Close this project's Editor before batch commands. `./tools/unity.sh open` opens it interactively.

`./tools/blender.sh` shows explicit versioned asset operations. They write to fresh staging directories; review and copy payloads separately while preserving Unity metadata. The original V1 generator remains historical source and must not replace the current ship accidentally.
