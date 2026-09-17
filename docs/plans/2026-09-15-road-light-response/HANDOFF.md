# Handoff: Vector Rush opening-road light response

> 状态更新，2026-09-15：道路候选已保留在 `7358f77`。用户现要求重新规划整体赛车体验。本文保留为道路任务的历史说明；新的制作入口为 [整体体验计划](../2026-09-15-integrated-racing-experience/PLAN.md) 与 [新交接](../2026-09-15-integrated-racing-experience/HANDOFF.md)。不要把本文当作下一轮独立修补任务启动。

Execute the plan at:
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/docs/plans/2026-09-15-road-light-response/PLAN.md`

You own implementation through a playable native candidate and matched moving evidence. Work autonomously within this bounded scope. The owner will run this task using GPT-5.6 Sol. Do not start additional conversations, revive older parallel workstreams or implement later polish items.

## Objective

Make one 15–20-second section of the opening bend's road receive convincing colored light/highlights from the existing track lighting. The improvement must be apparent from the normal driving camera. Preserve the blue exhaust, camera, HUD, world layout and gameplay. No feature additions.

## Exact working locations

Repository:
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city`

Unity project:
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/opening-city/UnityProject`

Baseline source commit: `b987c23`, branch observed as `art/opening-city`. Verify current HEAD before editing. Do not reset or switch branches over existing work.

Preserved native blue-exhaust candidate:
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/artifacts/exhaust-blue-v2/Vector Rush Blue Exhaust.app`

Its comparison, source hashes, test results and build notes:
`/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/artifacts/exhaust-blue-v2`

Create a fresh sibling evidence directory for this road task. Do not overwrite the exhaust evidence or app.

Reference video supplied by the owner:
`/Users/anping.wang/Library/Containers/com.tencent.xinWeChat/Data/Documents/xwechat_files/emrick_ee64/msg/video/2026-09/229355dc35da682a84e16c2cb0379898_raw.mp4`

Inspect the actual video/frames for the road's moving light response. It is visual direction, not a specification of its unknown rendering implementation.

## Start here

1. Read applicable AGENTS.md, PLAN.md and `docs/blue-exhaust-review.md`.
2. Check current working-tree changes and the running Editor. At handoff preparation, four opening-city FBX metadata files, package manifest/lock, and several untracked art/settings/review files were outside the exhaust milestone. Preserve them; establish provenance rather than committing or discarding them blindly.
3. Inspect the native baseline and the runtime road material before selecting a technique. Both road material templates currently disable environment reflections; direct specular is enabled. Older probe and SSR trials were visually unsuccessful. The plan lists the evidence and limits on retries.
4. Implement one bounded technique, validate it in the Editor, then build and record native A/B evidence. Keep the baseline exhaust enabled in both road variants. Do not reuse `-vrBlueBaseline` as the road switch because it changes exhaust.

## Tooling and delivery cautions

Unity Editor version is 6000.6.0f1; verify installed tools and current packages. The preceding task used Unity CLI for live iteration. Its native build encountered Pipeline runtime metadata-reference failures. A temporary player-only build omitted Pipeline while retaining the existing Newtonsoft JSON 3.2.2 dependency, then restored the pre-build manifest/lock bytes. The successful log and reproduction script are in the exhaust evidence folder. Inspect current state before reusing that workaround; do not blindly upgrade packages or modify their cache.

Build from the current saved scene. `VectorRushSetup.BuildMac()` calls `Prepare()`, which rewrites scene/material/settings state; it is not appropriate for a narrowly scoped comparison unless its consequences have been explicitly resolved.

`tools/capture-stage2.py` and `Stage2Evidence.cs` provide scripted input, source identity, native frames and camera/vehicle telemetry. Extend them narrowly for a dedicated road toggle if needed. `tools/opening-city-run.py` supplies the shared heavy-process lease; check its current ownership before launching builds/captures. Never run a build during tests or performance measurement alongside recording/encoding.

Keep `docs/implementation-plan.md` and `docs/development-history.md` current, and commit scoped completed steps. At preparation, origin pointed to a local integration path, not GitHub. Do not infer permission to push to another destination.

Deliver the named playable candidate, matched 15–20-second before/after video, three native still pairs, performance/regression evidence and a short honest verdict. Stop after this passage is ready for owner review. Do not claim artistic acceptance on the owner's behalf or expand to the full circuit.
