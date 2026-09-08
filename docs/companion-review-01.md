# Vector Rush — independent companion review 01

Date: 2026-09-07 Pacific / 2026-09-08 UTC. Baseline source checkpoint: `39331771013dd683f780f99917cbebcb378673f8`, with an actively changing working tree.

Implementation and integration belong to the task **Build AAA racing game prototype**. This companion task owns independent visual and QA review. The review did not edit the game, launch Unity, control its UI, or rerun existing tests.

## Verdict

**AAA presentation: FAIL. Automated race-loop validation: supported within its recorded scope. Manual playability: incomplete evidence.**

The circuit, craft silhouette, readable race HUD, and three-lap loop form a useful prototype. The large reference-to-runtime gap is plainly visible in road shading, scenery construction, material response and propulsion. Existing negative review results should remain in the development history.

## Evidence inspected

- Generated art target: `Vector Rush/references/solstice-chase-concept.png` (direction only, not runtime proof).
- Actual gameplay: `evidence/run-03/03-race.png`, `04-results.png` and original recording frames `frame-0000.png`, `frame-0180.png`, `frame-0359.png` under `evidence/gameplay-recording/frames/`.
- `evidence/run-03/race-verification.txt`, `runtime-metrics.txt`, `evidence/editmode-results.xml`, and the capture implementation in `RaceEvidence.cs`.
- Gameplay/input source, track generation, presentation source, and previous runtime critiques.

The selected frames are sampled visual evidence, not a continuous playback inspection or a temporal-shimmer test. The 15-second recording uses automated steering at 24 simulation frames per second and contains no audio evidence. Physical-controller handling and audio quality were not assessed.

## Prioritized findings and acceptance criteria

### 1. Road geometry/shading — high priority, correction already owned by build task

The uphill race view and descending city bend contain broad alternating dark bands and diagonal triangle fans across the driving surface. They compete with the lane direction and dominate the view. This is visible across all three sampled recording frames, not just one isolated shot.

The build task independently reports a banking/tangent defect and is correcting it. Its geometric measurements are implementation-team evidence, not measurements independently rerun here. Earlier failed shadow adjustments do not count as a correction.

**Accept when:** fresh native captures show continuous road shading on both the uphill crest and descending city bend; a short consecutive-frame capture checks for temporal instability; a complete race with fresh time/recovery counts and restart/countdown checks verifies the changed driving surface. Preserve the old captures and identify the exact revised build. Passing this item does not confer AAA status.

### 2. Recovery input survives pause — confirmed source defect, medium priority

In baseline `HoverVehicle.cs`, `Update` latches controller north/Y at line 114 without checking race phase. Non-racing clearing is in `FixedUpdate` at lines 124–128. `RaceDirector.TogglePause` sets time scale to zero, so that clear cannot run during pause. Resuming allows the pending request to call recovery, moving the craft and deducting boost.

**Reproduce:** race; pause; tap and release Y; resume with Start. This is a control-flow finding, not a native reproduction.

**Accept when:** resuming preserves craft position, recovery count and boost. Only a fresh recovery action in Racing triggers recovery. Gate or clear transient gameplay input while non-racing, and add a targeted regression. Physical-controller validation should remain labeled separately if unavailable.

### 3. Controls and race restart — validation gap

The existing restart loop checks countdown freezing and zero laps. It repeatedly restarts before the previous countdown completes. It therefore does not establish that each restarted race enters Racing with usable controls and normal timing. Automated steering validates physics/race integration, not human steering comfort. The build task is separately diagnosing pointer activation on a Retina display.

**Accept when:** pointer Start, Resume, Restart, camera-shake toggle and Race Again visibly work or the precise automation limitation is documented; restart from results and from a paused race, let both countdowns complete, then check movement, timer, lap progress and resource reset. Obtain a manual driving pass covering steering, braking, boost and recovery before describing handling as validated.

### 4. Propulsion and craft material response — next visual milestone

The two long, solid teal trails read like rods behind the craft, while the engine cores stay relatively dark. The near craft lacks the controlled light, dark glass and bright propulsion hierarchy in the target image.

**Accept when:** normal-throttle and boost captures have distinguishable engine states; cores are clear at chase-camera distance; exhaust tapers/fades rather than reading as opaque rods; hull, canopy and machinery separate through material response. Check in both sun and shade and preserve HUD/road visibility. Review actual runtime captures, not isolated Blender renders.

### 5. Scenery structure and depth — next visual milestone

The city bend repeats tall blank slabs with horizontal strips. Coastal rocks have conspicuous large polygon facets; water and the horizon offer little depth. The current assets do not support the reference's detailed coastal setting.

**Accept when:** improve one representative city bend and one coastal vista first. Give buildings believable facade depth and varied silhouettes; add rock detail at the distances actually seen; establish shoreline/water/horizon separation. Compare matched native viewpoints before extending changes around the entire circuit.

## Evidence and claim corrections

- The XML records **17 passing EditMode tests**. Those tests cover race-progress rules, not the complete controls/rendering experience.
- Run 03 records an automated three-lap finish in **112.58 seconds**, zero player recoveries, and three countdown-pause/reset checks. The results screenshot shows **01:52.576** and best lap **00:37.181**.
- `RaceEvidence.Update` stops frame collection after 60 seconds. The **P95 16.62 ms / P99 16.72 ms** measurements are a **60-second in-race frame-interval sample at 1080p on M2 Max with VSync**, separate from full-race completion. They are neither a full-race performance trace nor isolated CPU/GPU timings. Update README's broader “full-race sample” wording.
- New track geometry invalidates reuse of old lap/recovery measurements as proof for the corrected build. Collect fresh evidence.
- Controller comfort, manual racing, audio mix, and temporal artifacts remain unverified in this companion review.

## Coordination

The above road acceptance criteria, paused recovery bug, and performance-label correction were sent directly to the build task. Fresh geometry/control evidence is requested for independent re-review. This report is outside the game repository to avoid concurrent edits; the integration owner can copy it into the project's critique history and commit it with the appropriate milestone.
