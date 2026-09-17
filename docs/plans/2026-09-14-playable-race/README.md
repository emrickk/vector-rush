# Vector Rush: one polished, competitive race

Date: 2026-09-14. Status: execution plan prepared; implementation has not started under this packet.

## Outcome

Deliver one coherent Nocturne Circuit race that looks intentional, makes steering/airbraking/boosting and overtaking legible, and gives the player a reason to try again. Preserve the original night-world identity. The warm gallery is the first visual milestone; the full circuit remains the delivery scope.

The owner approved this direction after reviewing both project folders and the article at https://x.com/builtbysketch/status/2098773631249854478. The owner will create separate conversations and selected **GPT-5.6 Sol for every conversation**, including modeling. This supersedes older model assignments for this packet. Reading a handoff does not switch the conversation's model; select GPT-5.6 Sol in the app before starting.

## Start order

1. Start **01 Integration** first. It inventories and preserves the current work, establishes a baseline, prepares isolated workspaces, and writes `coordination.md` beside this plan.
2. Once coordination says `WORKSPACES_READY`, start **02 Environment**, **03 Racing**, and **04 Replay and presentation** in parallel, one handoff per conversation. Each reads coordination to find its workspace.
3. Workers implement within their ownership, self-check, and write delivery reports. Integration combines the work and performs native acceptance checks.
4. The owner reviews the integrated race and drives it. Then conduct a five-person qualitative playtest before adding more tracks or progression.

Separate conversations do not automatically exchange messages. Workers put durable updates in this packet's `reports/` directory. Integration can inspect those files on follow-up. Nobody should claim to be monitoring after ending their turn; no automation is requested.

## Verified starting point and uncertainty

- Main project: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush`.
- Historical staging: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Review`. Its September 7 handoff consolidated ownership into the main project. Preserve it; do not treat old coastal priorities as current direction.
- Original-world launch script: `tools/production/play-production.command` currently opens `Builds/Vector Rush.app`. Verify the binary identity before assigning it a source revision.
- Direct-production visuals were owner-rejected. Gameplay corrections in that branch may still be useful.
- Gallery02 Blender package exists; its handoff claims technical completion only. Native integration/appearance acceptance remains unproven.
- Saved `evidence/aaa-rebuild/resume-02/tests-03.xml` records 114 passes. Saved direct-03 evidence records three automated races on another build. Neither is current-source or human-play proof.
- The September 14 read found branch `codex/nocturne-production-handoff` with no HEAD commit and extensive pre-existing staged/untracked work. Historical commit/push claims cannot be assumed to describe this checkout.
- Unity project pins 6000.6.0f1. Verify installed tools and package versions locally.

## Ownership

Paths below are relative to each isolated project's root. Workers never mutate the shared original checkout.

| Conversation | Implementation ownership | Shared report |
| --- | --- | --- |
| 01 Integration | Baseline/repository preservation; workspace setup; shared bootstrap, scenes, project/render settings, build tools; shared contracts; final integration and evidence | `reports/01-integration.md` |
| 02 Environment | `SourceAssets/aaa-nocturne/`; `Assets/Editor/AAA/`; `Assets/Scripts/World/AAA/`; gallery/world materials and generated art resources; `NightTrackLighting.cs`; lane-specific art/import tests | `reports/02-environment.md` |
| 03 Racing | `Assets/Scripts/Gameplay/` except `RaceRecords.cs`; `PaceEvidence.cs`; lane-specific physics, race, input and AI tests | `reports/03-racing.md` |
| 04 Replay and presentation | `RaceRecords.cs`; new `Assets/Scripts/Replay/`; `Assets/Scripts/Presentation/` except opening/road/ship diagnostic render tools; lane-specific replay, records, settings and presentation tests | `reports/04-replay-presentation.md` |

`Assets/...` means `UnityProject/Assets/...`. Existing shared tests, `VectorBootstrap.cs`, `EnvironmentEvidence.cs`, `ProductionEvidence.cs`, scenes, assemblies, Packages and ProjectSettings belong to Integration. Request an integration patch in the report rather than editing another lane's files. Replay's rendering never owns `ChaseCamera.cs`; Racing owns the camera's behavior. Environment holds camera/craft/HUD fixed for art comparisons.

## Delivery sequence and gates

### M0: reproducible baseline

Inventory source, packages, assets, staged state and native apps. Preserve a recoverable copy and establish isolated lane workspaces from exactly the same source snapshot. Record hashes and known mismatches. Produce a current baseline build if possible without replacing the preserved original. Do not let repository cleanup consume the whole project: source work can proceed in protected copies while remote-access issues are documented.

### M1: integrated gallery

Resolve the package's canonical course identity versus JSON-byte checksum using the existing contract, not by bypassing guards. Validate/import Gallery02 and compare baseline/candidate in the same binary at fixed camera/route positions. Inspect entry, interior and exit, plus continuous motion.

Acceptance: preserved enclosure and cool exit; connected construction; clear material roles; localized warm pools with dark intervals; readable cyan edge and craft; no doubled geometry, corridor intrusion, unstable LODs or road-wide artifacts. A studio render cannot close this gate. If the change is negligible, change the production hypothesis. Do not replicate rejected work.

### M2: competitive racing

Validate controls, airbrakes, boost, recovery and camera on the existing course. Demonstrate reachable rivals ahead, a clean player pass, a rival response/pass opportunity, and recovery after a mistake through ordinary physics. Record actual passes and visible action separately from proximity telemetry. Never script a showcase and label it normal play.

Use controlled scenarios for diagnosis, then full races with varied starting conditions. Three automated completions establish regression coverage, not fair competition or human comfort. Preserve finish order, pause/restart correctness and manual-only record eligibility.

### M3: replay and sensory feedback

Add a personal-best lap ghost and three fixed course-sector deltas on top of existing records. The ghost is optional, non-colliding, visually subordinate and hidden until a compatible valid manual lap exists. Key recordings to course and driving rules; invalidate incompatible data. Add readable result comparisons and quick retry.

Improve boost/pass/impact/finish feedback using existing audio/VFX systems. Keep volume controls, camera comfort and road readability. Validate saving, restart, incompatible/corrupt recordings and disabled-ghost behavior.

### M4: full-circuit consistency

After M1's exemplar passes, apply its construction/material/light discipline around the circuit, with a distinct opening-city composition solution. Preserve elevated road, layered skyline and altitude cues. Evaluate the weakest foreground assets, not only the most photogenic section. Check at least opening, both galleries, thermal section, crest/descent and station.

### M5: owner-ready build and playtest

One identified native build, controls, full-lap audiovisual recording, functional findings and isolated performance measurements. Separate presentation capture from real-time performance runs. Assess normal and boost driving, visible traffic, menus, records, ghost and recovery. Record unsupported hardware explicitly.

Owner handling review is required before claiming human feel. With owner-arranged participants, watch five fresh players: can they start without coaching, describe what boost/airbraking do, improve a lap, recover from mistakes, and voluntarily choose another race? Proposed directional gate: at least three choose another attempt without prompting and can name something they want to improve. This small sample is a design decision aid, not retention evidence. If the loop fails, fix it before expanding content.

## Shared contract and integration rules

Integration publishes an initial `contracts.md` in this directory after reading current APIs:

- Keep race timing/progress authoritative in gameplay. HUD/ghost/audio observe it.
- Agree on monotonic race/lap time, ordered progress, pause/restart events, manual-run eligibility, course identity and driving-rules revision.
- Replay may record existing transforms/progress without waiting on a large new event framework. Racing supplies only the minimal necessary hooks through Integration.
- Sector boundaries default to one-third and two-thirds of normalized course progress plus start/finish. Only ordered forward crossings count; recovery/invalid laps follow a documented eligibility rule.
- Ghost playback follows lap time and pauses with the race; it must never participate in collisions, race ranking, AI avoidance or records.
- Any handling change changes the driving-rules identity so prior ghosts/records cannot silently compare incompatible physics.

Only one GPU-heavy Blender render, Unity import/build or native capture runs on this Mac at once. Integration publishes a lease protocol and owns baseline/final capture slots; source work continues in parallel. Separate Unity project copies do not remove hardware contention.

## Working method

Keep useful existing work and preserve rejected evidence. Work in bounded changes with visible acceptance criteria. Run relevant tests and native checks, but do not manufacture visual, audio, performance or human-play acceptance. When a capability blocks verification, report exactly what remains unverified and continue independent work.

Workers commit only their own changes in their prepared lane repositories. Integration handles combined history and any authorized push to the existing private repository, preserving user work and never merging to main. Do not create a replacement remote or change access permissions. No publishing, releases, purchases, participant outreach or new automations are requested by this packet.

Each report contains: status; workspace/baseline identity; changed files and commits; what visibly/behaviorally changed; checks with evidence paths; remaining defects; integration dependencies; exact next action. Statuses: `IN_PROGRESS`, `READY_FOR_INTEGRATION`, `BLOCKED`, `OWNER_REVIEW_READY`. Do not label work accepted before it is reviewed.

The article's cost/time figures are one author's account, not a schedule estimate. Its transferable practices here are a stable playable baseline, separate art and mechanics, reproducible assets, concrete render review, targeted replacement when tooling plateaus, and flourishes that support replay.
