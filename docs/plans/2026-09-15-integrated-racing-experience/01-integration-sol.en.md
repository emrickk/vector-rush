# 01: Experience coordination and Unity scene integration

Assigned model: GPT-5.6 Sol. You own the complete runnable scene and final delivery. All art assets must be produced by 02 / Astra.

## Shared objective, locations, and authorization

Deliver an approximately 30-second complete, playable cyberpunk anti-gravity racing exemplar. Combine a dense and varied city, neon and atmospheric fog, smooth track banking, acceleration blur, full blue exhaust, a visible ship boost state, real collision feedback, racing typography and UI motion, and synchronized sound. Preserve existing gameplay. Expand to the full circuit and all existing interfaces only after the user reviews the exemplar.

Main repository (read-only initially; do not use it directly as your implementation workspace):
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city`

Overall plan: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/PLAN.en.md`
Acceptance criteria: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/ACCEPTANCE.en.md`
Startup order and ownership: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-integrated-racing-experience/START_HERE.en.md`

Shared coordination directory, created by 01:
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/coordination/integrated-racing-experience`

If `coordination.md` already exists, verify the actual workspaces, branches, baseline commit, status-report paths, and contracts, then continue from the recorded progress. If it does not exist, you are task 01: create it and the four workspaces under the first phase below. Do not wait for someone else to publish WORKSPACES_READY. Do not implement directly in the original main repository or invent workspace paths. A user request to “execute this handoff” in a new task authorizes implementation within this assignment. Preparing this handoff has not started implementation.

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

## First phase: prepare real workspaces for the other three tasks

This is the only task authorized to prepare the baseline before WORKSPACES_READY. Audit the main repository, latest commits, uncommitted files, existing native apps, and running Editor. Preserve them; do not blindly commit or discard the whole working tree, or restore an older version. Record decisions and provenance for necessary environment or asset differences. Save a recoverable baseline on a new integration branch, then create independent workspaces for 02/03/04 from that same starting point. Transfer source and required assets, excluding Unity caches and native builds.

Recommended workspace parent: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/experience-workspaces`. Create the actual paths and record them in coordination; do not claim they already exist. Record each task's branch, baseline commit, read/write ownership, and workspace state. Astra's Blender and asset directories must also be traceable and mergeable.

Create the shared directory and publish initial coordination and contracts files. Specify the 30-second route, three actual camera views, source and destination directories, asset IDs and coordinate conventions, existing event interfaces, preference integration points, render-pipeline versions, and the heavy-work lease. Do not design the final buildings, fonts, or textures yourself; 02 supplies the art specifications in its first delivery.

After verifying that each workspace can access its required source, specifications, and references, return WORKSPACES_READY and the startup order for 02–04 to the user. This milestone establishes collaboration readiness, not completed game quality.

## Subsequent execution: implementation and ongoing integration

When the user continues this task, work without waiting for all assets to be finished. Inspect WorldBuilder, road meshes/TrackFrame, existing lighting, fog, post-processing, build and capture paths, and implement the necessary integration interfaces. Use existing assets only to validate the technical pipeline. As soon as 02 supplies the first building and roadside modules, validate their scale, enclosure, parallax, lighting response, and guidance in the native game. Record the results in coordination for the user to relay.

You own environment instantiation and assembly code, track continuity and collision surfaces, global lighting/fog/exposure baselines, and Bootstrap/Volume/renderer wiring. Follow Astra's layout and art specifications. Send specific requests to 02 for environment models, material textures, or road asset changes; do not author those assets yourself.

Source lives under each workspace's `UnityProject/Assets/Scripts/`. Environment entry points include `World/WorldBuilder.cs`, `World/TrackPath.cs`, `World/OpeningRoadFinish.cs`, `World/NightTrackLighting.cs`, and `Presentation/OpeningRoadLightResponse.cs`; initialization starts in `VectorBootstrap.cs`. If existing gameplay needs a read-only event outlet, you own the minimal bridge. Preserve rules and state updates; do not leave this dependency without an owner.

03 owns camera and VFX code. Implement its requested Volume/renderer/depth/motion-vector wiring and preference persistence changes without creating competing edits to shared files. 04 owns UI/audio code. Coordinate 02's graphics/font package with 03's real events, resolving integration compilation and cross-component lifecycle problems.

Initially preserve the track centerline, racing rules, and speeds. If banking or mesh changes are necessary, validate TrackFrame, visible road, guardrails, and collision surfaces together. Do not hide geometry defects with camera roll. Lighting adjustments may rebalance the earlier road candidate, but preserve comparison evidence; higher brightness alone is not an improvement.

## Early validation and final gates

The first integrated native samples must include actual new buildings, neon/fog, 03's early propulsion implementation, and 04's typography/event styling. Combine deliveries incrementally in the same scene. Correct fundamental direction problems as soon as they appear, rather than deferring them to a final polish pass. Route asset appearance issues to 02 and code issues to the relevant file owner.

Provide evidence against each item in ACCEPTANCE.en.md. Deliver approximately 30 seconds of continuous driving with real game audio, a real-contact collision supplement, an app with the candidate enabled on ordinary launch, three comparison sets, and traceable source/asset manifests. Full UI expansion belongs to the later phase; this phase establishes a consistent HUD and representative menu while keeping all existing functions usable.

The performance target is sustained 60 fps-class operation at native 1080p on the current M4 Max. Remeasure the whole segment and collision stress segment, reporting long frames and available CPU/GPU data. Label baseline captures, simulation-time diagnostics, and real-time speed/audio demonstrations separately. If the camera or banking changes, do not claim identical pixels or poses.

After actually delivering phases 0–2, stop for user review. Do not expand automatically to the full circuit or substitute the three contributor completion reports for an overall assessment. Report any weak core dimension explicitly; do not declare owner acceptance.

## Tools and history

The known versions are Unity 6000.6.0f1 and URP 17.6.0. Pipeline versions and the working tree have changed; verify them before starting. Earlier native builds encountered a Pipeline metadata dependency issue. The evidence packages preserve a workaround that temporarily excluded Pipeline, explicitly retained Newtonsoft JSON 3.2.2, and restored manifest/lock afterward. Diagnose the current issue before reusing that procedure; do not blindly edit PackageCache or upgrade packages.

`VectorRushSetup.BuildMac()` calls `Prepare()` and rewrites scenes, materials, and settings. Do not use it for the current scene without checking those effects. Build the actual saved scene and verify ordinary-launch defaults. Align the lease used by `tools/opening-city-run.py` with the current project paths. You coordinate heavy workloads across workspaces; stop competing workloads during performance measurements.

Read `docs/road-light-response-review.md` and `docs/blue-exhaust-review.md` in the workspace for the actual scope of earlier validation. Reuse the input/frame/pose recording in `tools/capture-stage2.py` and `Stage2Evidence.cs` where useful. The new version still requires fresh tests, continuous motion/audio evidence, and performance measurements.

Only you update the integration workspace's overall implementation-plan and development-history. Contributors supply scoped commits and their own reports. Publish integration records, actual checks, and limitations at each stage. The origin was a local integration path, not an assumed GitHub destination; do not push externally without an authorized destination.
