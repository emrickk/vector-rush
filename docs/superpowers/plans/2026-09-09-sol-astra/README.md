# Sol coding / Astra modeling execution plan

**Owner instruction, 2026-09-09:** GPT-5.6 Sol handles coding; Astra handles modeling/Blender work. Make the assignments concrete. When implementation is handed off, stop and notify the owner. The owner reviews first; the parent begins its independent review only after the owner asks it to proceed.

**Status: PLAN READY — WORKERS NOT DISPATCHED.** This packet is the concrete execution specification for the [production rebuild](../2026-09-09-nocturne-production-rebuild.md). It preserves the full-circuit scope. It changes the earlier automatic reviewer/iteration sequence to the owner's explicit stop gate.

## Assignments and ownership

| Worker | Exact model | Owns | Must not edit |
| --- | --- | --- | --- |
| Coding / integration | `gpt-5.6-sol` | All Unity C#, import/build/validation scripts, Unity scenes/prefabs/material assets/render settings, gameplay/audio code, automated tests, final native build and evidence | Astra's source blends, mesh geometry, texture sources, art manifests and authoring recipes |
| Modeling / art | `gpt-6-astra` | Blender modeling and Blender Python, UVs/textures, architectural asset family, complete circuit placement design, lighting intent, art source/export package and art self-checks | Unity source, Unity scene/settings/assets, game logic, import settings, final app/build commands |
| Parent / coordinator | Existing parent | Dispatch, dependency/resource coordination, immutable handoff bookkeeping, scoped commits/push and notifying the owner | Implementation substitution or independent code/art/game review before the owner-first gate |

Astra owns Python that generates art. Sol owns Python/shell/C# that imports or validates the production package. This prevents “all code belongs to Sol” from splitting Blender geometry construction between workers.

Sol is the only Unity writer/operator. Astra passes world-space placements and lighting specifications through the shared contract; Sol creates the corresponding Unity objects without inventing a competing art direction. One scene owner therefore remains accountable for integration while the roles stay separate.

## Read order

Both workers read `AGENTS.md`, this file and [contract-v1.md](contract-v1.md), then their own assignment: [Sol](sol-coding.md) or [Astra](astra-modeling.md). The [handoff checklist](handoff-and-stop.md) defines completion and the stop. The broader plan and critique are context, not permission to override this packet's ownership or review timing.

The workspace is `/Users/anping/Documents/Stuff/AI Space/Vector Rush`, branch `build/first-playable`. Parent records the actual starting commit and existing dirty paths at dispatch. The planning base is `b3f5998`; it is not a command to reset. Never overwrite prior dirty scene/evidence files or run a broad clean/reset/stash. Original and historical apps remain unchanged.

## Scope of this implementation run

Deliver one complete new native candidate with all four production zones, substantial replacement art, persistent scene/light data, integrated racing feedback and the specified records/settings changes. Preserve the original route, collision envelope, ship identity, baseline driving constants and camera for this run. Human-feedback-driven route/handling changes belong to review and correction after the owner gate; do not invent such feedback while implementing.

All assets and code are **candidate implementation**, not accepted AAA quality. Worker self-checks are required. Independent acceptance, human control feel and the parent's critical visual review remain pending when work stops. The earlier plan's independent exemplar/scene gates are deferred to the requested review phase; worker native self-inspection before replication remains required.

## Sequence and dependencies

| Order | Sol coding lane | Astra modeling lane | Handoff to continue |
| --- | --- | --- | --- |
| 1, parallel | C1: production branch and exact route/camera export | A1: asset replacement inventory and four-zone design; inspect retained references | `geometry-context.json` from C1 enables exact placement design |
| 2, parallel | C2: implement contract importer with a temporary diagnostic package; build persistent scene proof | A2: author all mesh/material families and export tested exemplars | Immutable `art-package-01/manifest.json`, complete payload and READY marker |
| 3, integrated | C3: import package, bake representative area, native exemplar/self-check and technical feedback | A3: consume raw native feedback; correct export/material/layout defects; finish full-course layout and lighting | Corrected package uses a new revision, never mutates imported payload |
| 4, parallel | C4: records, settings, race lifecycle, audio and feedback implementation | A4: finish all four zones, transitions, texture/LOD payload and art handoff | `ART_HANDOFF.md` declares complete or blocked with exact revision |
| 5, Sol only GPU work | C5: assemble whole course, bake/build, run tests/races/performance, create preview and launcher | Stay idle after art handoff; answer concrete integration defects through a new package revision if needed | `CODING_HANDOFF.md` and `FINAL_HANDOFF.json` pin matching art revision and app |
| 6, mandatory | Stop implementation | Stop implementation | Parent checks presence/status only, not quality; notify owner and end turn |
| 7, later | No work automatically | No work automatically | Owner reviews and explicitly asks parent to start independent review |

C2's diagnostic meshes must never appear in the final candidate. A partial imported scene is not the final deliverable. Sol may implement C4 while waiting for an immutable art revision. Astra may finish reusable asset families while waiting for geometry context. Neither worker silently redesigns the other's interface or edits the other's files to get unstuck.

## Shared contract changes

Both start with contract version 1. If a field is insufficient, the discovering worker sends a concrete proposed field/change and migration to its peer and parent. Parent records the coordination decision; both workers adopt the same version before publication/import. This is interface coordination, not a parent quality review. Breaking unilateral changes or undocumented guesses are forbidden.

## Resource coordination

Blender modeling/export and source coding can run concurrently. Before a Cycles render, Unity bake, native capture or performance sample, request the shared heavy-work slot from parent. Only one heavy render/bake/build/native measurement runs at a time. Never profile while Blender is rendering, Unity is baking, a build is running or video is encoding. Parent schedules resources, not artistic approval.

## Dispatch instructions

When execution is requested, create two fresh subagents with no inherited conversation history, so old review loops cannot leak into their instructions:

```text
coding_sol: model=gpt-5.6-sol, fork_turns=none
Workspace: /Users/anping/Documents/Stuff/AI Space/Vector Rush
Read /Users/anping/Documents/Stuff/AI Space/Vector Rush/AGENTS.md.
Read every .md file in /Users/anping/Documents/Stuff/AI Space/Vector Rush/docs/superpowers/plans/2026-09-09-sol-astra/.
Implement the coding/integration lane in sol-coding.md through C5. Work only
in your owned paths. Coordinate immutable packages with modeling_astra.
Do not spawn reviewers or new tasks. Return implementation handoff and stop;
parent review is deferred until the owner has reviewed and asks for it.

modeling_astra: model=gpt-6-astra, fork_turns=none
Workspace: /Users/anping/Documents/Stuff/AI Space/Vector Rush
Read /Users/anping/Documents/Stuff/AI Space/Vector Rush/AGENTS.md.
Read every .md file in /Users/anping/Documents/Stuff/AI Space/Vector Rush/docs/superpowers/plans/2026-09-09-sol-astra/.
Implement the modeling/art lane in astra-modeling.md through A4. Do not touch
Unity files. Blender Python belongs to your modeling ownership. Coordinate
context/package handoff with coding_sol. Do not spawn reviewers or new tasks.
Return your pinned art handoff and stop; do not claim AAA acceptance.
```

Keep worker model choices unchanged. Do not replace Sol's implementation with parent coding because a task becomes difficult; send a clarified contract or return a named blocker.

## Permanent stop semantics

The state sequence is:

`PLANNED → IMPLEMENTING → PACKAGING → AWAITING_USER_REVIEW → REVIEW_REQUESTED → INDEPENDENT_REVIEW`

Only the owner's explicit request to begin review can transition out of `AWAITING_USER_REVIEW`. Worker completion, a passing test, an informal critic message, elapsed time or a skill's automatic review instructions cannot trigger that transition. Do not launch review agents, self-start a polish round, publish a release, or schedule a wake-up at the handoff boundary.

If implementation is blocked, use `BLOCKED_IMPLEMENTATION`, notify the owner with the exact missing requirement and completed artifacts, and do not claim both workers are done. Routine technical self-fixes and peer integration fixes remain part of implementation.
