# Stage 1 opening city: owner review

Status: **OWNER_REVIEW_READY**. Stage 2 has not started.

## Result

The opening now has a landmark that remains visible at the bend, connected viaduct supports, a less dominant lower transit route, and stepped buildings that add depth to the reveal. These art corrections were already committed at `88d6399`; this stage brought them into a fresh native candidate and verified the comparison rather than duplicating the art work.

The independent visual verdict is a useful composition/construction checkpoint, not finished-game acceptance. Flat road response, primitive support and rooftop details, dark lower-city gaps and the large blank foreground roof remain visible weaknesses. See [independent review](reviews/stage1-opening-visual-review.md).

## Open the result

- [Comparison page with three image pairs and both videos](../../artifacts/opening-city/stage1-candidate-01/index.html)
- [All three before/after pairs](../../artifacts/opening-city/stage1-candidate-01/before-after.jpg)
- [Playable app](../../builds/Vector%20Rush%20Meridian%20stage1-candidate-01.app)
- [Candidate identity and validation summary](../../artifacts/opening-city/stage1-candidate-01/candidate-summary.json)

Use this named app to see Stage 1. The preserved original app has not been replaced.

## Verification

- Fresh Unity 6000.6.0f1 macOS build passed, GUID `c366cc453e5145029c61e7f0b627b198`.
- Runtime source at build: `69bafb2420f37d14681d310e9b0a3b5f392b0cd4`; art checkpoint: `88d6399`. Dirty provenance includes this stage's planning document and a preserved pre-existing untracked art metadata file; no uncommitted composition changes were introduced.
- All four opening FBX imports passed bounds and collider checks.
- Both captures completed 432 frames at 1600 × 900 and 24 Hz. The external validator verified 870 PNGs and all recorded camera, FOV, physical racer and visual poses. Maximum paired component difference was zero.
- Three final candidate anchors are byte-identical to the corresponding images inspected by the independent critic from the first checkpoint build.
- Two H.264 videos contain 432 frames and last 18 seconds each. They are silent simulation-time recordings with test autopilot, not human driving or performance measurements.
- No runtime exceptions were found in the capture logs.
- Normal launch without diagnostic flags remained healthy for 45 seconds. The native title/menu was inspected, and the log confirmed the opening scenery enabled by default.
- Automated validator tests exercise missing/corrupt images, altered poses, build/mode mismatches, cadence and malformed data.

The capture itself initially exposed a real bug: two screenshot requests in the same frame dropped the named anchor image. The failed checkpoint is preserved. Commit `69bafb2` changes full capture to one screenshot per frame, verifies the sequence and copies the exact saved anchor frame. Commit `8d733a8` adds independent evidence validation. A successful build alone is not used as a visual verdict.

## Scope and stop

Track layout, handling, camera behavior, craft and HUD remain fixed. Gallery rollout and race tuning are outside this stage. No claims are made for continuous watched-motion quality, audio, real-time performance, human handling or owner acceptance.

Source milestones remain local because the existing coordination packet limits pushes to an authorized private repository and the recorded remote does not meet that condition. No replacement remote or release was created.

Stop here for owner review. Any next stage should be defined from the owner's response to this playable candidate.
