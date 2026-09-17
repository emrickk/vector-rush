# 03: Speed, ship state, and collision effects implementation

Assigned model: GPT-5.6 Sol. You own runtime and shader implementation. All visual source assets, models, atlases, material appearance, and Blender production belong to 02 / Astra.

## Shared objective, locations, and authorization

Deliver an approximately 30-second complete, playable cyberpunk anti-gravity racing exemplar. Combine a dense and varied city, neon and atmospheric fog, smooth track banking, acceleration blur, full blue exhaust, a visible ship boost state, real collision feedback, racing typography and UI motion, and synchronized sound. Preserve existing gameplay. Expand to the full circuit and all existing interfaces only after the user reviews the exemplar.

Main repository (read-only initially; do not use it directly as your implementation workspace):
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city`

Overall plan: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/PLAN.en.md`
Acceptance criteria: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/ACCEPTANCE.en.md`
Startup order and ownership: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/START_HERE.en.md`

Shared coordination directory, created by 01:
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/coordination/integrated-racing-experience`

First read `coordination.md` in that directory to obtain your actual workspace, branch, baseline commit, status-report path, and shared contracts. Do not guess workspace paths. If WORKSPACES_READY has not been published, perform read-only analysis and state the dependency; do not implement in the main repository or create a separate baseline. A user request to “execute this handoff” in a new task authorizes implementation within this assignment. Preparing this handoff has not started implementation.

The known runtime starting point is `7358f77` (the direct road-lighting candidate), following `b987c23` (blue particle exhaust). Subsequent documentation commits do not imply runtime changes; check the current HEAD and uncommitted files. The earlier visual candidates have not received overall user acceptance. Their presentation may be reworked, but preserve recovery points.

Reference video:
`/Users/anping.wang/Library/Containers/com.tencent.xinWeChat/Data/Documents/xwechat_files/emrick_ee64/msg/video/2026-09/229355dc35da682a84e16c2cb0379898_raw.mp4`

Collision reference screenshot:
`/var/folders/yx/hf7ht73550q2156rvymtwnrr0000gn/T/codex-clipboard-33ecef45-723d-430c-b58f-336234386fa1.png`
If the temporary screenshot is unavailable, extract the collision moment from the video. The actual mechanics of HOLOGRAPHIC PHASE are unknown. Use its appearance only as inspiration for the existing boost state; do not add shield or transformation gameplay.

Existing evidence and apps:
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/artifacts/road-light-response`
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/artifacts/exhaust-blue-v2`

The model assignment is an explicit user requirement. GPT-6 Astra owns selection, design, creation, and modification of all art assets, including Blender and its generation scripts, models, UVs, LODs, textures, artistic material parameters, VFX atlases, UI graphics, fonts, and any new source audio assets. GPT-5.6 Sol owns programming, shader code, technical import, binding, and runtime integration. Sol must not generate a replacement art package or independently change Astra's visual design. Procedural building geometry used to produce assets still belongs to Astra; Unity code that reads and instantiates its manifest belongs to Sol. Existing assets may temporarily validate interfaces, but must not be presented as completed final asset work.

Edit only your assigned files and your own status report. Record changes needed in another owner's files as requests for that owner to implement. Do not automatically create or wake user tasks, send cross-task messages, or spawn agents: the user will create the four tasks. Shared files support handoff; they do not imply continued monitoring after a task ends.

## File ownership

The source paths below are relative to `UnityProject/Assets/Scripts/` in your own workspace.

You own `Gameplay/ChaseCamera.cs`, `Presentation/IonPropulsion.cs`, `IonFlameParticles.cs`, `VehicleVFX.cs`, new camera/VFX code, Experience VFX shader code, and corresponding tests. Do not edit Bootstrap, WorldBuilder, TrackPath, the production scene, ProjectSettings/Packages, PlayerPreferences, or RaceHUD/RaceAudio. Send attachment-point, global Volume, and persistent-setting requests to 01; coordinate audiovisual/UI event contracts with 04.

## Start from existing behavior

The code already has damping/FOV/shake, continuously banked road geometry, boost, and collision particles. Inspect the native presentation first and identify whether each deficit concerns amplitude, timing, shape, spatial relationships, or incorrect input. The user wants speed, energy, and contact feedback; adopting new technology or adding particles is not the objective.

Once 01 prepares the workspaces, audit existing inputs/events, camera behavior, lifecycle, and render order, and establish testable interfaces. Use existing resources for diagnostics; do not author placeholder art to replace Astra's work. Request required VFX atlas formats, frame counts/orientation/channels, material parameters, and screen-scale constraints from 02 early. 02 decides artistic appearance.

## Implementation order

1. **Speed and turns.** Coordinate follow damping, look-ahead, bounded FOV, event impulses, and boost transitions against actual foreground geometry and bends. Distinguish road up, ship banking, and camera roll to avoid doubling the tilt. Do not increase actual racing speed to manufacture improvement. Send road geometry or banking discontinuities to 01.
2. **Motion blur.** Verify the available URP path, depth/motion vectors, and UI/transparent-effect order. If using peripheral directional blur instead, identify it as an approximation and keep the central route readable. Implement your shader/controller; 01 attaches renderer/Volume components. Respect existing reduced-motion/comfort semantics for disabling or scaling the effect; 01 owns preference changes.
3. **Ship boost state.** Implement the existing boost's activation, sustained state, and release using Astra's appearance specification. Synchronize hull material/outline/energy changes with propulsion. Preserve collision, mass, speed, consumption, and existing authoritative state. Add no ability and do not infer the reference holographic phase's gameplay.
4. **Full propulsion.** Use Astra's coherent atlases and assets to layer a bright core, turbulent main body, and outward-dispersing bright particles/trails. Control relative velocity, lifetime, near-camera/depth fading, overdraw, and distant-opponent budgets. Eliminate visible sheet edges, nozzle detachment, low-frame-rate beading, static water-column appearance, and large foreground blobs. Review continuous motion before tuning local parameters.
5. **Real collisions.** Drive light grazes, sustained scraping, and heavy impacts from actual contact point/normal, normal impact, and tangential sliding. Stop sustained emission when contact ends. Together with 01, verify that wall contact actually reaches the event path; do not assume OnCollisionEnter is sufficient. Define cooldowns/pooling, repeated-contact coalescing, and screen-occlusion limits. Supply 04 with the same real impact event for audio/UI. Do not add a damage system.

## Collaboration and checks

Map changes to the single event source in the contracts. VFX random streams must not affect race/AI randomness. Pause, return to title, restart, automatic recovery, boost depletion, and rapid repeated contact must remain stable without resource leaks. Do not duplicate input sampling or calculate conflicting collision intensity values.

Deliver early runnable code and Astra samples to 01 for native integration, then refine them. In `03-motion-vfx.md`, record the commit, changed files, contract version, required 01 wiring/02 assets/04 consumers, native footage, and unresolved issues. Show cruise/acceleration/boost/turn/release in the main sequence. Real collision demonstrations may use a separate controlled sequence, with the input method clearly labeled.

Completion requires behavior/lifecycle validation of the current source and direct evidence of speed, propulsion, and collisions from the actual camera. Measure your local performance cost; 01 owns final whole-scene measurement. Compilation or unit tests do not establish visual acceptance. Do not start city modeling, UI art, or full-circuit expansion.
