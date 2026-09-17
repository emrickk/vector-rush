# 04: UI animation and audio implementation

Assigned model: GPT-5.6 Sol. You implement styling bindings, animation, and audio playback/mixing for existing interfaces. Font selection, graphics, textures, audio source assets, and art specifications all come from 02 / Astra.

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

You own `Presentation/RaceHUD.cs`, `RaceAudio.cs`, new UI animation/audio control code, and focused tests. Do not edit camera/VFX, World/TrackPath, Bootstrap, the production scene, ProjectSettings/Packages, PlayerPreferences, or original font/graphics/audio assets. Shared settings and event bridges belong to 01; authoritative collision intensity and boost events come from 03/existing gameplay.

## First action

Audit the existing HUD, main menu, pause, settings, results, keyboard/mouse/controller navigation, and reduced-motion behavior. Barlow typography, boost/rank feedback, and layered audio already exist; do not describe them as absent. Inventory current information/actions and actual audio paths, and provide that inventory to 02 as design input. Start with animation/binding interfaces and existing events. Add no gameplay, pages, or data fields.

## First-round presentation scope

Bind Astra's display/small-text font pairing, size/tracking/color tokens, and graphics package at actual screen pixel sizes. Establish a coherent racing identity for the exemplar HUD—speed, position, boost, and existing prompts—and selection/entry/exit animation for one representative menu. Keep other existing interfaces functional and basically consistent. Full art expansion across all interfaces belongs to phase 3 after user acceptance of the exemplar; do not expand this delivery prematurely.

Existing event feedback needs consistent onset, emphasis, hold, exit, and interruption rules. Set reasonable frequency and priority for overtaking feedback, boost changes, and collision warnings; do not animate every element at once. Collision UI expresses existing events only, without new shield/damage values. Astra provides the visual timing storyboard; adapt it to actual interaction and return any differences.

Retain IMGUI if it can support the result. Do not turn this into a UI framework migration. Advance animation in the appropriate Update/time source, not repeated OnGUI callbacks. Respect pause/reduced-motion, text size, and safe areas, keeping the central driving view clear. Return missing glyphs or incorrectly sized graphics to 02 rather than selecting an unrelated replacement resource yourself.

## Audio

Keep the existing engine, wind, boost, impact, and notification playback structure. Remix around actual throttle, speed, boost transitions, contact intensity, and UI events. Astra owns timbre specifications, selection of new material, and source audio creation. You implement playback, existing runtime synthesis, and mixing code with agreed parameters; do not independently create new source audio assets.

Light grazes, sustained scrapes, and heavy impacts need distinct sustained/transient behavior. Do not stack a new one-shot every frame. Limit concurrency and peaks; verify that repeated impacts do not clip. Leave space for engine, wind, and event sounds; music must not mask feedback. Synchronize with the same event sources and timestamps used by 03. Pause, resume, and restart must not leave stale looping sounds. Do not claim a listening-quality pass from waveforms or source code without hearing the result.

## Incremental delivery and validation

When 02's first fonts/graphics arrive, deliver a runnable HUD and representative menu sample to 01; do not wait for the entire graphics package. Use 03's real boost/collision events to verify coordinated UI/audio response, and return appearance issues in native footage to Astra.

Record the commit, file list, asset/contract versions, required 01 settings wiring, actual visual/audio validation, and unresolved items in `04-ui-audio.md`. Check common resolutions/scaling, existing keyboard/mouse/controller navigation, rapid repeated events, pause, restart, reduced motion, and audio resource lifecycle.

Deliver native event clips with sound and necessary behavioral checks for 01 to integrate into the approximately 30-second main sequence. 01 and Astra coordinate final whole-image style and audio balance; assist with corrections. Source-only checks and static UI screenshots do not establish animation/audio quality, and you cannot declare overall artistic acceptance on the user's behalf.
