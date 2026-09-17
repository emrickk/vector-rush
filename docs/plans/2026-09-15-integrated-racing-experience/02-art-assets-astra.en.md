# 02: Unified art direction, all assets, and Blender production

Assigned model: GPT-6 Astra. The user explicitly requires Astra to handle all assets and modeling. Your scope includes the exemplar's buildings, ship appearance, materials/textures, VFX atlases, fonts, and UI graphics. Sol implements and integrates the runtime behavior.

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

## Ownership and first action

You own SourceAssets/Experience, the ExperienceArt payload, Blender files and generation/export scripts, artistic materials, design tokens, and composition manifests. Follow the specific boundaries in START_HERE.en.md. Do not overwrite the production Unity scene, write gameplay/C#, or change camera/global rendering configuration.

After WORKSPACES_READY, inspect the actual native exemplar route and the reference video, then audit available assets. Classify each key existing asset as retain, rework, replace, or demote to background use. Deliver one complete style board, three route compositions, a short asset list, and material/typography specifications, then immediately begin the first production batch. Do not stop at another general plan waiting for someone else.

## Deliver in batches, with the first batch usable in Unity

**A: Shared visual language and a minimum visible asset package.** Establish consistent silhouette, material, color, and value rules for the city, track, ship, effects, and UI. Prioritize a visibly differentiated set of buildings, podiums, and close guardrail guidance, plus exhaust/collision atlas samples, font selections, and HUD numeral/title styling. These let 03/04 start validation without waiting for the entire city package.

**B: City assets and layout.** Use the overall plan's suggested building families to create genuinely different masses, rooflines, and facade divisions. Add a landmark, transport structures, bridges, rooftop equipment, advertisements, and guidance modules. Variants must do more than change height or window color. Supply an instance placement manifest along the actual route, including stable IDs, position/rotation/scale, material slots, and LODs. 01 assembles it in the production scene. Avoid evenly spaced buildings. Overlap near, middle, and distant city layers while prioritizing track sightlines, clearance, and route readability.

**C: Ship, propulsion, and collision assets.** Retain the anti-gravity identity and blue propulsion direction. Design a fuller main jet with layered particles, and produce temporally coherent flame atlases, elongated spark/bright-fragment assets, and necessary meshes. Specify UV orientation, frame count/FPS, channels, and opacity/brightness conventions. Avoid flicker from unrelated independently generated frames. Define the boost material/energy/outline treatment and visual timing for light grazes, sustained scraping, and heavy impacts. If mechanical articulation or hull changes would help, establish their value in a native sample before investing; add no gameplay.

**D: UI and audio assets.** Provide distinctive racing display typography paired with legible small text, license and glyph coverage information, icon/gauge/panel/selection-state graphics, size/safe-area/color tokens, and motion storyboards with timing suggestions for existing boost/rank/collision feedback. Cover the HUD and a representative menu first. Do not include data for nonexistent gameplay in the design. If new audio assets are necessary, you own selection or creation, provenance, licensing, and source files. 04 implements playback, existing runtime synthesis, and mixing code to your timbre specifications; it does not independently create new source audio assets.

## Asset production requirements

- Deliver `.blend` sources, reproducible generation/export scripts, and actual exports together. Record units, axes, pivots, applied transforms, normals/tangents, UVs, material slots, texture color spaces, and channels.
- Deliberately design roughness, metal, glass, and emission regions. Blender appearance does not establish URP appearance. Supply reproducible material data, compatible textures, and explicit shader behavior requirements for Sol.
- Provide necessary LODs and simple collision proxies. 01 owns the actual track collision surfaces; do not use detailed high-polygon buildings as movement collision geometry.
- Record provenance, licenses, texture dimensions, and memory/triangle budgets. Prefer suitable existing assets. Do not purchase without an explicit budget. The user's reference is for visual observation, not an asset pack to extract and reuse.
- Keep GUIDs and filenames stable for fonts, textures, graphics, and previews. Report technical import failures separately from artistic shortcomings. Validate in your own preview scene without overwriting 01's production scene.

## Native feedback and completion

Each batch must include a commit, manifest, sample images/footage, and status in `02-art.md`, with requests for 01/03/04 to validate in actual cameras and return issues. Revise against native results, especially close construction joints, repetition, silhouettes in distant fog, blown-out neon, flat-looking exhaust, and actual on-screen font sizes.

Do not declare the asset package complete based only on Blender studio renders. It requires evidence of successful native import and usability in moving scenes. If integration feedback is pending, report “assets ready for integration / native validation pending,” not a fabricated pass. The user has not asked you to create more asset tasks; complete this assignment yourself and deliver in batches.
