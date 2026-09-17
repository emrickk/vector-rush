# Final road: sampled-sequence verification

2026-09-08. **Pass for the bounded road-fixture caster change.** The final native ordinary and boosted exterior samples preserve useful road lighting, vehicle grounding and landmark depth. The diagnosed L-shaped fixture shadow is absent in the final boosted passage. No demonstrated visual blocker remains within this review’s scope. This is sampled-sequence verification, not continuous video playback or a performance verdict.

## Evidence and matching

The [final native report](../../evidence/landmarks-final/full-lap/environment-evidence.json) is complete and identifies build **`b709ef36660443b7bdec08170d00b8df`**. It records 1,440 frames spanning approximately 60 simulation seconds and a lap advance. I visually inspected **10 original 1920 × 1080 final PNGs**, in sequence within the groups below, and three original candidate02 comparison PNGs: 175, 641 and 1004. I did not visually inspect all 1,440 frames.

| Sampled passage | Final frames inspected | Recorded speed and state |
| --- | --- | --- |
| Mast approach and passing view | 148, 166, 175 | 174.55–175.16 km/h, boost off |
| Utility plant passing views | 606, 641 | 175.91–176.22 km/h, boost off |
| Final exterior straight | 995, 1000, 1004, 1008, 1016 | 224.62–244.82 km/h, boost on |

For all 10 selected frames, I independently verified that the final and [candidate02 report](../../evidence/landmarks-candidate-02/full-lap/environment-evidence.json) values exactly match for camera position/rotation, player visual position/rotation, FOV, progress, race time, speed and boost state. Candidate02 GUID is `a1325c823e8b4541b20c143f42f12470`. Matching capture metadata does not imply pixel-identical renders.

The source manifests share ten recorded paths; only `NightTrackLighting.cs` differs among those shared entries. The final manifest covers more files, so this is not a claim of a complete binary comparison. Current source applies `ShadowCastingMode.Off` to the three diagnosed fixture groups and retains the helper’s `On` default for other combined geometry.

## What the samples show

In the [mast approach](../../evidence/landmarks-final/selected/frame-0148.png), [warm-lit passage](../../evidence/landmarks-final/selected/frame-0166.png) and [near mast view](../../evidence/landmarks-final/selected/frame-0175.png), cool and amber light pools remain visible on the road and craft. The road curve, cyan boundaries, elevated structure and mast remain distinct. The [candidate02 mast view](../../evidence/landmarks-candidate-02/selected/frame-0175.png) has the same visible landmark composition; the caster adjustment produces no obvious loss of its depth.

The [utility plant approach](../../evidence/landmarks-final/selected/frame-0606.png) and [passing view](../../evidence/landmarks-final/selected/frame-0641.png) retain the cylinders’ shading, pipe silhouettes and separation from the road. Compared with [candidate02 frame 641](../../evidence/landmarks-candidate-02/selected/frame-0641.png), the unwanted narrow fixture stripe is reduced while the vehicle’s dark ground shadow remains clearly visible. Broader scene shading also remains; this was not global shadow removal.

Across boost samples [995](../../evidence/landmarks-final/selected/frame-0995.png), [1000](../../evidence/landmarks-final/selected/frame-1000.png), [1004](../../evidence/landmarks-final/selected/frame-1004.png), [1008](../../evidence/landmarks-final/selected/frame-1008.png) and [1016](../../evidence/landmarks-final/selected/frame-1016.png), the recognizable pole-and-arm L is absent as the camera passes the fixture. Candidate02 frame 1004 visibly contains that L at the exactly matching pose. Final road light pools, building recesses, visible lamp housings and the craft’s silhouette and ground shadow remain readable. These samples are consistent with the causal [A/B/A result](002-fixture-caster-native.md).

Thin transverse lines remain. Source explicitly builds them as `Deck expansion joint` geometry with shadow casting already disabled; their persistence is expected and does not indicate the diagnosed fixture shadow returned. This review does not recategorize every remaining tonal boundary as a seam or certify earlier unrelated road artifacts.

## Scope of the verdict

Keep the limited fixture change. The sampled ordinary and boosted passages support completing this visual step without another road experiment. No continuous playback, manual driving, audio, frame-time stability or full-circuit art acceptance is claimed. The parent owns those broader delivery checks. No runtime changes, Unity process or native capture were made by this reviewer; the report is the only new file written for this verification.
