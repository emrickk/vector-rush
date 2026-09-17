# Environment review 006: candidate 02 sampled sequence continuity

2026-09-08. **The sampled frames preserve the gallery shell, the forward route and the player craft, but two visible construction details need correction or explanation before this can be treated as a clean passage.** This is a bounded read-only continuity review by the gallery implementation author, not an independent art-direction verdict or a full motion acceptance.

## Evidence actually inspected

The [candidate 02 status](../../evidence/environment-candidate-02/status.txt) was `COMPLETE` before inspection. Its [native report](../../evidence/environment-candidate-02/environment-evidence.json) records 264 frames, 1920 × 1080, Unity 6000.6.0f1, build GUID `4e0ec5aa2ed544baa7e0b9c32368e96e`, captured 08:38:44–08:39:17 UTC. The sequence uses automated steering, ordinary physics and `Time.captureFramerate=24`: eleven simulation seconds, without audio. It is not a performance sample.

I inspected these **40 frames as labeled 640 × 360 contact-sheet samples**, in increasing sequence order:

`0, 24, 48, 72, 96, 100, 112, 124, 136, 148, 156, 164, 168, 170, 172, 176, 180, 184, 188, 190, 192, 196, 200, 204, 208, 210, 212, 216, 220, 224, 228, 232, 236, 240, 244, 248, 252, 256, 260, 263`.

I then opened these **12 original 1920 × 1080 PNGs** for detail inspection:

`144, 148, 150, 180, 190, 200, 204, 206, 210, 219, 220, 240`.

That is **44 unique inspected frames out of 264**. The contact sheets were temporary inspection aids; the source PNGs were unchanged. I did not watch the complete sequence at normal playback speed. I did not inspect the remaining 220 frames, other aspect ratios, audio, player-controlled traversal or a nearby pack of rivals. These samples cannot rule out short-lived popping, flicker, shimmer or discomfort between them, and they do not establish how fast or convincing the passage feels.

The report also says `matchedWithinTolerance: false`. The five anchor player positions/progress match the baseline, but camera displacement is approximately 0.055–0.150 m. This is not an exact camera/pixel comparison. The older first-candidate road-control still was inspected as context; it is not evidence for candidate 02 continuity.

## Actionable findings

| Priority | Evidence | Observation and bounded next check |
| --- | --- | --- |
| P2 | [Frame 144](../../evidence/environment-candidate-02/frames/frame-0144.png), [148](../../evidence/environment-candidate-02/frames/frame-0148.png), [150](../../evidence/environment-candidate-02/frames/frame-0150.png); progress 0.8205–0.8258 | A pale blue rectangular plate projects inward from the left road-edge assembly, over the gutter and into the visible running surface. It remains present as the camera approaches and passes, so it is not merely a single sampled-frame flash. It reads as a loose flap or an unintended projection. Identify the source object and check its extent against the actual driving/collision boundary; then place it consistently within the edge assembly or give the intended service cover a convincing flush fit. No collision was tested, and the sample does not prove a collision hazard. |
| P2 | [Frame 200](../../evidence/environment-candidate-02/frames/frame-0200.png), [204](../../evidence/environment-candidate-02/frames/frame-0204.png), [206](../../evidence/environment-candidate-02/frames/frame-0206.png); progress 0.8853–0.8927 | The left gallery service hatch has a large wall-colored V/wedge through its dark backing while its horizontal bars remain visible. The shape persists and grows as the hatch gets closer. In contrast, the right hatch in [190](../../evidence/environment-candidate-02/frames/frame-0190.png) reads as a complete dark rectangle. The left one looks intersected or partially missing, undermining the new service detail. Check backing depth and orientation against the curved wall panels; the visual evidence does not by itself prove the implementation cause. |
| P3 | [Frame 220](../../evidence/environment-candidate-02/frames/frame-0220.png), with [219](../../evidence/environment-candidate-02/frames/frame-0219.png) immediately before it | An exterior lamp's bright diffuser passes behind the small `BEST LAP` label and lowers its contrast at 220. The main race-time digits remain readable. Gallery fixture silhouettes also pass behind the timing card in 200/204/206, but I could still read its digits. Check these crossings in full-speed playback before deciding whether lamp placement needs adjustment. This is not a request to redesign the HUD. |

## What the samples support

- **Gallery construction:** samples 176–212 retain visible ceiling planes, chamfered ribs and side panels as the camera enters, traverses and exits. I found no gross roof-shell disappearance or opening to the sky through the upper shell in those samples. The left hatch defect above remains an exception to clean detail continuity. Lower service gaps and supports are visible behind the barriers; they do not obscure the road in the inspected views.
- **Route and craft:** the left bend and then the exit straight remain interpretable. No sampled frame loses the player silhouette against the wall, road or open skyline, including the warm interior and boosted exterior samples. The bright portal lamps are conspicuous at entry/exit, but neither hides the player or closes off the route in these views. There is no close rival pack here to establish occupied-track readability.
- **Lighting hierarchy:** the ceiling remains a lit surface rather than a black void. Upper-wall pools are still repetitive, but their sampled appearance does not wash out the primary ribs or the central road. This is a still-image observation, not a lamp-strobing verdict.
- **Exit construction:** across samples 208–256, the visible nearby buildings and service roofs change position and apparent size in the expected direction as the camera advances; I found no gross disappearance among those samples. Whether the transit structure reads as a strong continuous landmark is an art-direction question for the separate review.
- **Road limitation:** broad diagonal tonal bands remain visible on the roadway in original frames 219/220/240. This review cannot attribute them to lighting, shadows, mesh interpolation or the material, nor decide their motion stability. Treat the deliberately satin road as the stated material redesign; these samples are not evidence of a proven root-cause repair or an artifact-free surface.

## Required follow-through

Resolve or explicitly account for the projecting plate and the left hatch intersection, then inspect those exact progress ranges in the corrected native build. Preserve a complete normal-speed viewing for shell popping, narrow luminous-edge shimmer, lamp rhythm, road stability and the transition into boost. That viewing, an occupied passage, the supported aspect ratios, audio and separate performance measurements remain outside this report. No gameplay, art, lighting, camera or HUD source was changed for this review.
