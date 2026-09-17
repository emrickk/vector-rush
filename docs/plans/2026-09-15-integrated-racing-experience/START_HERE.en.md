# Four-task startup and collaboration guide

The user's explicit assignment is: **GPT-6 Astra handles all art assets and Blender; GPT-5.6 Sol handles Unity programming and integration.** This supersedes older instructions to use Sol for everything or to complete everything in one task. The overall experience goal, acceptance criteria, and no-new-gameplay boundary are unchanged. This packet only prepares handoffs; the user creates and starts the tasks.

This is the English version of [START_HERE.md](START_HERE.md), with the same assignments and scope. Use the [English overall plan](PLAN.en.md) and [English acceptance criteria](ACCEPTANCE.en.md).

## Startup order

1. Start [01: Coordination and Unity scene integration](01-integration-sol.en.md) with GPT-5.6 Sol. It preserves the baseline, creates independent workspaces, publishes coordination files and interface ownership, and returns WORKSPACES_READY.
2. After WORKSPACES_READY, start [02: All art assets](02-art-assets-astra.en.md) with GPT-6 Astra, [03: Speed and ship effects implementation](03-motion-vfx-sol.en.md) with GPT-5.6 Sol, and [04: UI and audio implementation](04-ui-audio-sol.en.md) with GPT-5.6 Sol. These three may run in parallel against the shared contracts.
3. Relay each task's staged deliveries or completion status to 01 so it can read the shared reports and continue integration. Bring Astra's early samples into the native scene first, then expand in batches; do not wait for every asset before merging.
4. 01 delivers the complete exemplar, native main footage with sound, collision footage, a playable app, and acceptance results. The user reviews the whole experience before deciding on full-circuit expansion.

Writing a file does not automatically wake another task. The user can continue 01 after its first WORKSPACES_READY report. If contributors have not delivered yet, it can work on its own environment/lighting foundations without fabricating completion or ongoing monitoring.

## Exclusive ownership

Paths refer to each task's repository with the same source baseline. Shared source files must not have competing writers. After auditing the actual project, 01 may specify new filenames in coordination without violating the model assignments.

| Task | Exclusive ownership | Work assigned elsewhere |
| --- | --- | --- |
| 01 / Sol | Unity scenes and Bootstrap; WorldBuilder, TrackPath, road/lighting/environment placement code; Packages/ProjectSettings/asmdef; importers and production assembly prefabs; diagnostics, builds, overall reports; shared preferences/event contracts | All models, textures, fonts, icons, VFX atlases, and art selection go to 02; camera/VFX code to 03; HUD/audio code to 04 |
| 02 / Astra | `SourceAssets/Experience/`; models/textures/fonts/audio and artistic materials in `UnityProject/Assets/Resources/ExperienceArt/`; their stable metadata; Blender, UV/LOD, export/asset-generation scripts; composition/placement manifests, design tokens, visual references | C#, runtime shader code, and production-scene wiring go to the relevant Sol task; global render settings to 01 |
| 03 / Sol | ChaseCamera; IonPropulsion, IonFlameParticles, VehicleVFX and new effects/camera code; associated shaders and focused behavior tests | Request visual textures/meshes/material design from 02; Bootstrap, Volume/renderer wiring, TrackPath, and PlayerPreferences changes go to 01 |
| 04 / Sol | RaceHUD, RaceAudio; new UI animation/audio playback/mixing code and focused tests | Fonts, graphics, and new audio assets come from 02; preference persistence and shared event contracts go to 01; collision emission/intensity events come from 03 |

Do not put runtime code in `Assets/Resources/ExperienceArt/`. Sol's shader code belongs separately in `Assets/Resources/Shaders/Experience/`. 03 owns effects shaders there; 01 uses a separate Environment subdirectory if environment shaders are required. Publish the file list to avoid name collisions. Astra specifies appearance and material inputs; Sol implements the mathematics/rendering code. Only technical format conversion and explicitly agreed parameter binding belong to Sol's import work. Return appearance changes to 02.

Astra delivers asset sources and importable payloads. 01 exclusively owns the production scene and assembly prefabs. Astra may create preview scenes in its own workspace but must not use them to overwrite the production scene. Native experiments are controlled by workspace ownership and the heavy-work lease.

## Shared coordination rules

Shared directory: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/coordination/integrated-racing-experience`. 01 writes `coordination.md` and `contracts.md`. 02/03/04 respectively write only `02-art.md`, `03-motion-vfx.md`, and `04-ui-audio.md`. Every report records status, commit, file list, asset/contract versions, requested responses, actual evidence, and unresolved issues. This directory is outside the source workspaces. Link stable files accessible to other tasks, not only temporary images they cannot access.

01 establishes the common baseline after checking current source and uncommitted changes. Do not copy Library/Temp/Builds as a new workspace. Unity workspaces are independent; only 01 operates the native integration workspace. Keep metadata/GUIDs stable after first import. Merge explicit commits/file lists; file owners resolve conflicts. Do not overwrite whole directories.

01 establishes early contracts, and 02's first visual specification supplies their artistic details:

- The same approximately 30-second route, three representative views, scene coordinates/units, track tangents, and clearance.
- Asset IDs, export coordinates, pivots, material slots, atlas formats/channels, HDR and roughness/smoothness conventions, LOD/collision proxies, font licenses, and sizes.
- A single authoritative source for existing throttle/boost/collision/rank states; event meanings, cooldowns, and pause/restart rules. Prefer read-only bridges; do not execute gameplay twice.
- Astra's event presentation specifications; a shared timeline implemented by 03/04; Volume, motion preferences, and final mix coordinated by 01.
- Budgets by domain and a lease for heavy workloads. Do not measure performance alongside Blender rendering, other game instances, or encoding.

When blocked by a dependency, continue independent interface, audit, or native-baseline work and record the specific need in your report. Do not invent another model's assets or use “waiting” to obscure unfinished work of your own.

## Quality responsibility

02 owns art direction and asset appearance. 01 owns the implemented complete native image and final delivery. Resolve conflicts together using native evidence; Sol must not establish a separate visual direction. The user has final artistic acceptance. Every task serves the same exemplar; individual task passes do not establish an overall pass.
