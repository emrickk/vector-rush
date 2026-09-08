# VECTOR RUSH

A playable, original anti-gravity racing prototype for Apple Silicon macOS. Pilot Kestrel 07 through three laps of Nocturne Circuit against five rivals. The current development direction is a night urban circuit.

[Private GitHub repository](https://github.com/emrickk/vector-rush) · [Build brief](docs/GPT6-BUILD-PROMPT.md) · [Development history](docs/development-history.md) · [Ship art plan](docs/ship-art-plan.md)

## Current milestone — close racing and final visual polish

The player now uses a 53 m/s cruise and 72 m/s boost preset, with responsive acceleration retained. Rival steering follows the pursuit geometry, and bounded lateral assistance helps AI craft clear outer-wall contact through normal physics. In the final automated race, a rival stayed within 60 m of validated race progress for **80.9%** of post-start racing, sustained across all three laps. All six racers had zero recoveries. The nearest finish gap was 100.85 m; no overtake was demonstrated.

The final independent visual critique led to two changes: a compact nearest-rival name/distance cue beneath POS/LAP and softer cool-gallery lighting. The authored transit/service architecture, rebuilt galleries, deck construction, corrected Kestrel ship and throttle-driven exhaust remain in the current build.

[Current 15-second preview](evidence/close-race-final/VectorRush-preview.mp4) · [Current 42.75-second full lap](evidence/close-race-final/VectorRush-full-lap.mp4) · [Current local macOS archive](Builds/VectorRush-macOS-close-race-2026-09-08.zip). Previews show actual native rendering at 1080p, with silent automated steering at 24 simulation frames per second. The full lap spans two recorded start-line crossings.

[Final critique](docs/environment-reviews/009-close-race-critique.md) · [Focused correction verification](docs/environment-reviews/010-close-race-verification.md) · [Pacing comparison and preserved candidates](docs/pace-reviews/002-native-pace-comparison.md). Broader city expansion and subjective continuous-motion, human driving and audio assessment remain future work.

## Play

Open **[Builds/Vector Rush.app](Builds/Vector%20Rush.app)** or unpack **[the current local archive](Builds/VectorRush-macOS-close-race-2026-09-08.zip)**. Source, editable assets and selected evidence are pushed to GitHub. The native build is ready locally; GitHub release publication was blocked by automatic approval review pending separate user authorization. Earlier environment/coastal archives are historical.

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

- Final native build **7da2883ade4f4223b7a611db8cf60eb6** completed a fresh pacing race in **128.32 simulation seconds**, with **zero recoveries across all six racers**. All 1,285 sampled racer states and timestamps exactly match the accepted pacing candidate after presentation changes.
- **42/42 Unity tests passed** on the final source, including pursuit geometry, rival corridor assistance and existing input/race/restart checks.
- The final 60-second traversal produced five correct anchors, all matching the pre-polish camera/player transforms exactly. The exported full lap contains **1,026 frames / 42.75 seconds**; the short preview contains **360 frames / 15 seconds**.
- Five current native views each at **1920×1080, 1280×800 and 1920×810**, with build identity and PNG integrity checks. The cue fits in inspected smallest/ultrawide warm-gallery views, and sampled labels agree with racer telemetry.
- Separate real-time native race: **three laps in 128.32 seconds, zero player recoveries**; both restart launches and all three countdown-pause checks passed. At 1080p on Apple M2 Max with VSync, 7,189 observed intervals yielded mean **8.35 ms**, P95 **9.22 ms**, P99 **9.32 ms**, and 239.5 MiB Unity allocation. No build or encoding ran concurrently. Prior environment P95 was 15.14 ms; changed race pace makes this an observed comparison, not an identical-trajectory benchmark.
- Native archive integrity and source/build hashes are recorded in the [delivery manifests](evidence/close-race-final/build-manifest.json).

[Final evidence](evidence/close-race-final/) · [Real-time race and metrics](evidence/close-race-final/performance/) · [Previous environment milestone](evidence/environment-final-full-lap/).

This remains a working prototype. Nearby rivals are usually behind the camera in automated driving; the cue exposes that pressure without inventing a visible fighting pack. Broad road shading bands, repeated skyline patterns and a modest station silhouette remain. Sampled visual checks do not establish subjective motion comfort or human driving/audio quality. Earlier pointer/gamepad checks remain historical coverage; no new physical gamepad validation is claimed.

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
- [Asset regeneration guide](docs/asset-regeneration.md) — safe staged rebuilds that protect the reviewed live assets.
- [Reference concept](references/solstice-chase-concept.png) — generated art direction, distinct from actual runtime and Blender inspection images.
- [Toolchain and provenance](docs/toolchain.md), [test results](evidence/editmode-results.xml), [current build manifest](evidence/close-race-final/build-manifest.json).

The craft, environment geometry, interface and sound synthesis are original. Cliff surface maps are **Rock 3 by Rob Tuytel / Poly Haven (CC0)**; original maps, hash manifests and the named Unity mask derivative are retained with [provenance](SourceAssets/environment-v2/textures/Rock3_PROVENANCE.md). No Wipeout assets or branding are included.

## Build

Run `./tools/unity.sh prepare`, `./tools/unity.sh test`, then `./tools/unity.sh build` with the pinned Editor installed. Close this project's Editor before batch commands. `./tools/unity.sh open` opens it interactively.

`./tools/blender.sh` shows explicit versioned asset operations. They write to fresh staging directories; review and copy payloads separately while preserving Unity metadata. The original V1 generator remains historical source and must not replace the current ship accidentally.
