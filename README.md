# VECTOR RUSH

A playable, original anti-gravity racing prototype for Apple Silicon macOS. Pilot Kestrel 07 through three laps of Nocturne Circuit against five rivals. The current development direction is a night urban circuit.

[Private GitHub repository](https://github.com/emrickk/vector-rush) · [Build brief](docs/GPT6-BUILD-PROMPT.md) · [Development history](docs/development-history.md) · [Ship art plan](docs/ship-art-plan.md)

## Current night milestone — racing HUD

The HUD now groups speed and boost in one instrument, adds the actual circuit map with live racer markers, and uses bundled Rajdhani typography. Compact position/lap and race-time panels keep the road center clear; actual lap transitions trigger a brief final-lap cue. Countdown, pause and results share the same typography.

The current environment candidate adds an authored transit station, service buildings, terrace/split towers, continuous deck construction and rebuilt warm/cool galleries with visible ceilings. Near glazing now has restrained room-light artwork. The road uses a deliberate satin finish; the old diagonal-sheen cause remains unresolved. The corrected Kestrel ship, throttle-driven exhaust and compact racing HUD remain in use.

[Current 11-second environment passage](evidence/environment-final-full-lap/VectorRush-environment-current.mp4) · [Current 42-second full-lap preview](evidence/environment-final-full-lap/VectorRush-full-lap-current.mp4). Both show the final native build at 1080p, with silent automated steering at 24 simulation frames per second. The full recording spans two start-line crossings and a completed lap.

The five-view composition has passed review for further verification. **Finished visual acceptance and full-circuit city expansion remain pending normal-speed playback review.** Frame inspection does not establish motion comfort, highlight stability or human driving feel. Rejected candidates and corrective history are preserved in [environment reviews](docs/environment-reviews/).

[Previous HUD preview](evidence/hud-motion-01/VectorRush-HUD-current.mp4) and [HUD phase/aspect verification](evidence/hud-native-02/) predate this environment candidate. The [native throttle check](evidence/throttle-native-01/throttle-evidence.json) covers normal-physics exhaust response; gameplay and propulsion are unchanged in this environment pass.

## Play

Open **[Builds/Vector Rush.app](Builds/Vector%20Rush.app)** or unpack **[the current environment review archive](Builds/VectorRush-macOS-environment-2026-09-08.zip)**. The **[VectorRush-macOS-2026-09-07.zip](Builds/VectorRush-macOS-2026-09-07.zip)** archive contains the earlier coastal delivery and is historical. Build products stay local and are excluded from Git; source and selected evidence are in the private repository.

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

- Final native build **153b77ea7d53463b8fc11a0ddae232be**: a verified complete circuit across **1008 frames / 42 simulation seconds**, with five correct benchmark anchors. An 11-second passage is extracted from that same recording.
- Five current native views each at **1920×1080, 1280×800 and 1920×810**. Source/build hashes and PNG validation are preserved. The baseline comparison reports camera drift outside exact-match tolerance; it is a nearby actual-physics comparison, not pixel-exact A/B.
- Separate real-time native race: **three laps in 112.60 seconds, zero player recoveries**; both restart launches and all three countdown-pause checks pass.
- **Apple M2 Max, 1920×1080, VSync enabled:** 6,804 observed frame intervals; mean **8.82 ms**, P95 **15.14 ms**, P99 **15.99 ms**, 240.1 MiB Unity allocation. Fresh baseline P95/P99 were 16.67/17.01 ms. No build, bake or encoding job ran during the sample. This is no observed regression in this run, not an isolated GPU timing or locked-frame-rate guarantee.
- **32/32 Unity tests passed again on the final source**, including throttle response, rival safety and race/restart behavior.

[Final capture and manifests](evidence/environment-final-full-lap/) · [Real-time race and metrics](evidence/environment-final-performance/) · [Art review 008](docs/environment-reviews/008-final-environment-art.md) · [Continuity/aspect review 007](docs/environment-reviews/007-final-continuity-aspects.md) · [Capture validation](evidence/environment-final-full-lap/capture-validation.json).

Native frame inspection accepts the corrected glazing, bearing and hatch. Broad diagonal road bands, a brief lamp/BEST LAP overlap and the conventional station silhouette remain. Continuous normal-speed playback, crowded-gallery readability, human driving/audio feel and full-circuit city expansion are **not accepted** by these checks. Earlier HUD pointer/phase checks remain historical coverage; this environment stage changes no controls, handling or AI.

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
- [Toolchain and provenance](docs/toolchain.md), [test results](evidence/editmode-results.xml), [current build manifest](evidence/environment-final-full-lap/build-manifest.json).

The craft, environment geometry, interface and sound synthesis are original. Cliff surface maps are **Rock 3 by Rob Tuytel / Poly Haven (CC0)**; original maps, hash manifests and the named Unity mask derivative are retained with [provenance](SourceAssets/environment-v2/textures/Rock3_PROVENANCE.md). No Wipeout assets or branding are included.

## Build

Run `./tools/unity.sh prepare`, `./tools/unity.sh test`, then `./tools/unity.sh build` with the pinned Editor installed. Close this project's Editor before batch commands. `./tools/unity.sh open` opens it interactively.

`./tools/blender.sh` shows explicit versioned asset operations. They write to fresh staging directories; review and copy payloads separately while preserving Unity metadata. The original V1 generator remains historical source and must not replace the current ship accidentally.
