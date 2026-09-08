# VECTOR RUSH

A playable, original anti-gravity racing prototype for Apple Silicon macOS. Pilot Kestrel 07 through three laps of Solstice Circuit against five rivals. The current development direction is a night urban circuit.

[Private GitHub repository](https://github.com/emrickk/vector-rush) · [Build brief](docs/GPT6-BUILD-PROMPT.md) · [Development history](docs/development-history.md) · [Ship art plan](docs/ship-art-plan.md)

## Current checkpoint — V3 finish integrated, night polish underway

The native app now uses the coherent V3 ship with flush citron/07 paint, baked surface maps and authored engine anchors. Eight [calibrated native inspection views](evidence/ship-native-v3-01/inspection-scope.json) and [independent review 006](docs/ship-reviews/006-source-baked-native-finish.md) support advancing the material hierarchy and import. Local canopy/intake/nozzle surface overlaps remain under correction; the accepted primary shape stays fixed.

[Native amber-gallery preview](evidence/night-v3-motion-02/VectorRush-V3-amber-candidate.mp4): 15 seconds at 1080p, recorded with automated steering at 24 simulation frames per second. This preserves the pass 03 baseline before the current gallery/plume correction and does not measure real-time performance. [Review 007](docs/ship-reviews/007-native-gallery-and-boost-stills.md) records black structural panels, weak attached exhaust and repeated road highlights as remaining issues. The night build's final race, performance and packaging checks are pending.

## Play

Open **[Builds/Vector Rush.app](Builds/Vector%20Rush.app)** for the current night development candidate. The **[VectorRush-macOS-2026-09-07.zip](Builds/VectorRush-macOS-2026-09-07.zip)** archive contains the earlier coastal delivery and is historical. Build products stay local and are excluded from Git; source and selected evidence are in the private repository.

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
- [SourceAssets/environment-v2](SourceAssets/environment-v2/) — editable tower/cliff kit, export scripts and texture sources.
- [Asset regeneration guide](docs/asset-regeneration.md) — safe staged rebuilds that protect the reviewed live assets.
- [Reference concept](references/solstice-chase-concept.png) — generated art direction, distinct from actual runtime and Blender inspection images.
- [Toolchain and provenance](docs/toolchain.md), [test results](evidence/editmode-results.xml), [build manifest](evidence/build-manifest.json).

The craft, environment geometry, interface and sound synthesis are original. Cliff surface maps are **Rock 3 by Rob Tuytel / Poly Haven (CC0)**; original maps, hash manifests and the named Unity mask derivative are retained with [provenance](SourceAssets/environment-v2/textures/Rock3_PROVENANCE.md). No Wipeout assets or branding are included.

## Build

Run `./tools/unity.sh prepare`, `./tools/unity.sh test`, then `./tools/unity.sh build` with the pinned Editor installed. Close this project's Editor before batch commands. `./tools/unity.sh open` opens it interactively.

`./tools/blender.sh` shows explicit versioned asset operations. They write to fresh staging directories; review and copy payloads separately while preserving Unity metadata. The original V1 generator remains historical source and must not replace the current ship accidentally.
