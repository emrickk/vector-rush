# Start here — Nocturne production rebuild transfer

## Owner's objective and non-negotiable workflow

Rebuild the **whole Vector Rush Nocturne circuit** to a substantially stronger production standard. The owner rejected several small opening/material passes because the game still looked essentially the same. Do not resume that local-polish loop or treat shader repairs/import success as a global visual milestone.

Staffing is explicit: **GPT-5.6 Sol codes and integrates Unity; Astra models in Blender and authors textures/layout/lighting intent.** Blender generation Python belongs to Astra. Coding and modeling use separate file ownership and a versioned immutable handoff contract.

**The owner reviews the implementation candidate first.** Once both workers' outputs are integrated into a playable candidate, notify the owner and stop. Parent/independent code, art and gameplay review starts only when the owner asks. Do not auto-spawn critics, start a polish cycle or schedule continued work. The present transfer request authorizes packaging a draft PR, not an independent quality review.

## Required reading, in order

1. [Project rules](../../AGENTS.md).
2. [Concrete execution packet](../superpowers/plans/2026-09-09-sol-astra/README.md).
3. [Shared contract](../superpowers/plans/2026-09-09-sol-astra/contract-v1.md).
4. Your assigned [Sol coding brief](../superpowers/plans/2026-09-09-sol-astra/sol-coding.md) or [Astra modeling brief](../superpowers/plans/2026-09-09-sol-astra/astra-modeling.md).
5. [Handoff and mandatory stop](../superpowers/plans/2026-09-09-sol-astra/handoff-and-stop.md).
6. [Whole-course production plan](../superpowers/plans/2026-09-09-nocturne-production-rebuild.md).
7. [Global quality diagnosis](../visual-target-reviews/004-global-aaa-assessment.md), [visual critic](../visual-target-reviews/004-global-aaa-independent-critique.md), [gameplay critic](../visual-target-reviews/004a-global-gameplay-production-critique.md).
8. [Current Sol snapshot and coordination caveat](SOL_CHECKPOINT.md). Treat unvalidated/incomplete sections exactly as recorded; do not restart completed work or infer a green build.

The packet's owner-first review order supersedes older plans/skills that prescribe automatic review. README's original “workers not dispatched” line is historical: the owner subsequently started the separate coding task. Astra was not started at the latest owner update.

## State at transfer

- The global assessment and six-milestone rebuild plan are complete planning artifacts, not completed production work.
- Sol's coding task is `Implement Sol Astra gameplay`, Codex task ID `01a0855e-ce6a-7f71-8d08-4b53840f3be7`. Parent coordination task is `Review Vector Rush work record`, ID `01a0840d-c54a-7e43-be3b-f57b2ff2e520`. These are optional local coordination references; all necessary plan/context artifacts are in Git.
- Sol delivered C1 route/camera context and has been working on C2 importer/scene diagnostics and C4 race lifecycle, records, preferences/input, audio and telemetry. The checkpoint report records exact validation/completion boundaries.
- Astra's modeling/art package does not exist yet. The owner said they would start Astra separately. Do not use diagnostic import assets as production art.
- No finished NocturneProduction candidate, full-course art acceptance or whole-game AAA verdict is established by this transfer.
- Opening04 is a rejected global visual milestone. Its native stills and comparison remain useful baseline/history, not the new rebuild output.

## Modeling pickup

Use [geometry-context.json](../../evidence/nocturne-production/context/geometry-context.json) and [course-data.json](../../evidence/nocturne-production/context/course-data.json). Geometry context SHA-256 is `5a22d7779b229a7f7d38342a393414d99c5804f13badab5e6d8040ded782c764`; course hash is `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`. Parent verified these file hashes at the original handoff. They contain 1,201 actual route frames; do not redraw the route from images. See [C1 status](../../evidence/nocturne-production/context/C1_STATUS.md).

Start A1/A2 using the modeling brief. Final layout must cover viaduct, canyon, thermal and station zones. Publish immutable `SourceAssets/nocturne-production/art-package-NN/` revisions with READY/checksums last. Sol alone imports them into Unity. Keep course/camera/driving constants unchanged for this first candidate; later user review can justify changes.

## Coding pickup

Read the Sol checkpoint before continuing. Finish independent C2/C4 work first, then C3/C5 against Astra's actual immutable package. Preserve worker ownership. Do not rebuild a placeholder city to satisfy a final-art dependency. The narrow production scene path must not invoke legacy Prepare, regenerate the old city, lose persistent material/light data or change the original app.

Unity version is 6000.6.0f1 with existing URP 17.6; Blender is the existing installed tool. Unity editor/import/bake/build is owned by Sol. Serialize heavy Blender renders, Unity bakes/builds, native capture and performance work. No code/asset quality review was performed as part of PR packaging.

## Evidence, files and portability

This PR includes current source, Unity metadata, planning/review history, C1 context, available test XML/checkpoint reports and opening04 selected native PNGs/manifests. Test directories include intentionally failing red-step results and intermediate green results; filenames are not a claim that the latest complete tree passes. Use the coding checkpoint for exact latest results.

Native `.app` builds, Unity Library/Temp/Logs, licensing/editor logs, duplicate raw capture frames and other caches stay local/ignored. They are not required to understand the work, and must not be committed. Rebuild from source on a configured Unity machine when needed. The retained original local app is `Builds/Vector Rush.app`; never overwrite it. Future output is a fresh `Builds/Vector Rush-production-NN.app`.

Preserve history and rejected candidates. The draft PR is a **work-in-progress transfer**, not a request to merge or a completion claim. Read the actual PR branch state before running tools; do not reset to the old planning-base commit from the packet.
