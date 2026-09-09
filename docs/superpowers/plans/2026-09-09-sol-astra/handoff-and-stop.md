# Implementation handoff and owner-first stop

This file overrides the earlier production plan's automatic independent reviews. Technical worker self-checks and peer integration corrections are implementation work. Parent/independent artistic, code-quality and gameplay review starts only after the owner has reviewed the handoff and asks for it.

## Required art handoff — Astra

Write `docs/production-handoffs/ART_HANDOFF.md` with:

- Status: `ART_IMPLEMENTATION_COMPLETE`, `INCOMPLETE`, or `BLOCKED`.
- Exact immutable package revision, READY/checksums hash and course hash.
- Editable Blender source, authoring recipes, textures, meshes and complete layout paths.
- Every required family and four-zone coverage; list incomplete items explicitly.
- Self-check evidence: axis/scale, bounds, UVs/maps, export/reimport, native views inspected and known limitations.
- Last owned source commit if available; parent records integration commits separately.

Astra stops active production after its complete handoff. It may answer a specific Sol integration defect with a new package revision before final assembly; in that case it updates the handoff to the replacement revision. It does not start another aesthetic direction or a critic cycle.

## Required coding handoff — Sol

Write `docs/production-handoffs/CODING_HANDOFF.md` and `evidence/nocturne-production/final/FINAL_HANDOFF.json` with:

```json
{
  "contractVersion": 1,
  "status": "AWAITING_USER_REVIEW",
  "sourceCommit": "actual code/integration commit",
  "artRevision": "actual immutable revision",
  "courseHash": "actual course SHA256",
  "artReadySha256": "actual READY SHA256",
  "buildGuid": "actual Unity build GUID",
  "appPath": "absolute new app path",
  "launcherPath": "absolute launcher path",
  "previewPath": "absolute full-lap preview path",
  "nativeViewsDirectory": "absolute original PNG directory",
  "testReportPath": "absolute current test report",
  "raceReportPath": "absolute current race verification",
  "performanceReportPath": "absolute current measurement report",
  "knownLimitations": [],
  "humanPlayReview": "NOT_REVIEWED",
  "independentVisualReview": "NOT_REVIEWED",
  "independentCodeReview": "NOT_REVIEWED",
  "parentReviewStarted": false
}
```

This is a schema example; actual handoff values must be real, existing and verified by Sol. Sol's accompanying Markdown explains what changed, what technical checks passed/failed, how to run the app, controls, original-build preservation and remaining limitations. It must not claim AAA or independent acceptance.

## Technical readiness before the normal stop

- [ ] Astra reports its complete four-zone package; Sol integrated that same revision and matching course hash.
- [ ] The native candidate opens the new production scene directly, has the required art/features and completes the three-race/restart verification without a crash or broken race state.
- [ ] Relevant automated tests passed, or a failed test is identified as a specific blocking item. No missing required feature is buried under “known limitations.”
- [ ] Native original screenshots and an actual-game-sound preview are present and labeled correctly. Blender renders and generated targets remain separate.
- [ ] Current performance measurements are present with honest pass/fail values. Missing human play, physical controller coverage or unmet provisional performance targets can remain explicit acceptance limitations; they are not disguised as completed review.
- [ ] Launcher works, all paths exist and source/art/build identity is pinned.
- [ ] Worker-owned changes are committed/pushed through the parent coordination workflow; no historical app or unrelated dirty work was overwritten.

If a required implementation or delivery artifact cannot be completed, stop as `BLOCKED_IMPLEMENTATION` with named missing items and the usable partial artifacts. Do not wait silently, claim normal completion, request an automatic review, or silently reduce scope. A performance shortfall with measurements may be handed off as a declared limitation; an app that cannot run, missing full-course art, broken required feature or absent required preview is incomplete implementation.

## Parent's allowed packaging check

Parent may read worker reports, inspect task status, verify artifact existence/hashes, reconcile package versions and handle commits/push. This is coordination, not the independent review. Parent does not inspect source for quality, critically assess native images, launch a critic, or give a visual/gameplay pass before the owner-first gate.

Notify the owner once both complete handoffs agree, using this shape:

> Sol's coding/integration and Astra's modeling are handed off. The candidate is ready for your review.
> [Playable build] · [Launcher] · [Full-lap preview] · [Before/after views]
> Technical checks: [actual brief results]. Remaining limitations: [actual items].
> I have stopped. I will begin my review when you tell me to proceed after yours.

If blocked, replace the first sentence with the specific blocker and never say “ready” without qualification. The notification is in this task, not an email/Slack message or an unrequested automation.

## What happens after notification

End the active turn. Leave both workers idle. Do not schedule wake-ups, silently resume work, run additional benchmarks, make aesthetic changes, dispatch code/visual reviewers or publish a release.

The owner's explicit review-start request transitions to `REVIEW_REQUESTED`. Then parent inspects their feedback, reviews actual source/native output/technical evidence, reports findings, and proposes or executes corrections within the scope of that new request. Implementation-complete and independently accepted remain different states.

## Worker dispatch/skill precedence

If a skill says to review after every task, continue automatically, or start a whole-branch review, the owner's specific stop instruction overrides that sequence. Do not ask for permission to honor the stop. Do not use an automatic completion/goal mechanism to continue past it. The rebuild's full quality ambition stays in force; its independent evaluation is intentionally deferred.
