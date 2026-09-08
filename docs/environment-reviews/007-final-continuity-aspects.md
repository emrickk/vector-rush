# Environment review 007: final sampled continuity and aspect ratios

2026-09-08. **Both construction defects reported in review 006 are absent in the corrected native samples. The inspected full-circuit samples and ten aspect-ratio anchors preserve the player, route, gallery shell and main HUD.** This closes those two specific geometry findings; it does not establish continuous motion quality, crowded-gallery readability or an independent art-direction acceptance. The reviewer authored the gallery implementation.

## Evidence and identity

The final [full-lap report](../../evidence/environment-final-full-lap/environment-evidence.json), [16:10 report](../../evidence/environment-final-16x10/environment-evidence.json) and [ultrawide report](../../evidence/environment-final-ultrawide/environment-evidence.json) share native build GUID `153b77ea7d53463b8fc11a0ddae232be`. The [final source manifest](../../evidence/environment-final-full-lap/source-manifest.json) covers 163 files. Of the 82 entries also present in the candidate 03 manifest, only `EnvironmentEvidence.cs` differs: capture instrumentation was corrected after an earlier full-lap run triggered its anchors prematurely. That failed-anchor run is excluded here.

The final full-lap sequence contains 1,008 PNGs at 1920 × 1080. Race time advances from 0.120 to 42.079 seconds, lap 1 to lap 2; the lap increment occurs at frame 913. The corrected benchmark anchors occur at frames 646/816/836/856/886. This establishes actual circuit coverage in recorded state, rather than relying solely on the `COMPLETE` marker. I independently checked every PNG with Pillow's structural verification: all 1,008 full-lap images and all ten aspect images passed, with their expected dimensions. Structural verification is not visual inspection of every frame.

The capture uses automated driving and 24 captured frames per simulation second. It has no audio and does not measure real-time performance. The starting state is already racing, so it does not cover grid/countdown presentation. Baseline comparisons are not pixel matched: full-lap anchor cameras differ by approximately 0.055–0.150 m, up to 0.272° and 0.005° FOV, despite matching player positions/progress. Aspect reports contain no baseline comparison entries; their default false match field is not an evaluated mismatch.

## Images actually inspected

I viewed these 34 full-lap frames as labeled 640 × 360 contact-sheet samples in increasing order:

`0, 6, 12, 24, 36, 48, 72, 96, 144, 192, 240, 288, 336, 384, 432, 480, 528, 576, 624, 646, 690, 742, 790, 816, 836, 846, 856, 866, 886, 909, 930, 960, 990, 1007`.

I opened these 12 original full-resolution frames:

`0, 15, 24, 384, 646, 794, 816, 836, 850, 856, 866, 886`.

The union is **37 unique visually inspected final full-lap frames**. I also opened all five original approach/entry/middle/exit/reveal images in each supported alternate ratio: **five at 1280 × 800 and five at 1920 × 810**. These are still-only runs; their 264-frame metadata does not represent 264 saved or inspected screenshots per ratio.

Before the instrument-only rebuild, I inspected 12 original candidate 03 frames (`0, 144, 148, 150, 170, 190, 200, 204, 206, 210, 220, 240`) to check the old defects at their original sequence positions. Those predecessor images are corroboration, not additional final-build coverage. I did not watch continuous normal-speed playback or drive manually.

## Findings

| Finding | Final evidence and conclusion |
| --- | --- |
| **P2 projecting left plate: closed in inspected views** | Candidate 03 frames 144/148/150 no longer show the pale plate intruding over the gutter. Final full-lap frame [794](../../evidence/environment-final-full-lap/selected/frame-0794.png), supported by sample 790, shows a clear left road edge at the corrected location. This is visual closure; no collision-boundary test is claimed. |
| **P2 intersected left hatch: closed in inspected views** | Candidate 03 frames 200/204 show a complete dark backing. Final frame [850](../../evidence/environment-final-full-lap/selected/frame-0850.png), supported by sample 846, retains the rectangular dark hatch without the wall-colored V/wedge. |
| **P3 small-label contrast: remains** | The exterior diffuser still passes behind `BEST LAP` at final frame [866](../../evidence/environment-final-full-lap/selected/frame-0866.png). The main race-time digits remain legible. Some gallery fixture silhouettes cross the timing card without hiding its main digits. Preserve this as a bounded contrast issue for motion review, not a request for HUD redesign. |
| **Road tonal bands: unresolved motion limitation** | Broad angular tonal bands remain visible at 866/886 and the reveal anchors. These images do not establish their cause or temporal stability. They support neither an artifact-free road claim nor a proven material root-cause repair. |

Across the sampled circuit and all five final benchmark anchors, the forward road and white player remain readable. The cool gallery at [384](../../evidence/environment-final-full-lap/selected/frame-0384.png) has visible blue ceiling and upper-wall surfaces; the warm gallery retains readable ribs, wall panels and roof planes. I found no gross upper-shell gap, disappearing player or road obstruction in the inspected images. Lower service openings remain behind the barriers. This sample coverage cannot exclude brief popping, shimmer or uncomfortable light rhythm between frames.

There is actual occupied-start evidence: [frame 0](../../evidence/environment-final-full-lap/selected/frame-0000.png), samples 6/12 and original [15](../../evidence/environment-final-full-lap/selected/frame-0015.png)/24 show a lime rival beside the player on the right. Its outer portion is clipped by the screen edge and the speed HUD overlaps its rear, but the two craft remain distinguishable and the forward route readable. The adjacent rival is gone by samples 36/48. This establishes one nearby rival at the start, not six-craft pack readability or a crowded gallery passage.

| Aspect | Result across all five original anchors |
| --- | --- |
| [16:10, 1280 × 800](../../evidence/environment-final-16x10/03-middle.png) | Narrower horizontal coverage still fits the player, central road and gallery exit. Main HUD text and boost bar are not cut off. |
| [Ultrawide, 1920 × 810](../../evidence/environment-final-ultrawide/03-middle.png) | Additional side walls and district remain visible without losing the player or route. Main corner labels and gauges fit. |

Bright barrier lines cross portions of the speed-gauge/minimap backgrounds in the alternate-ratio images, but their major labels remain readable. Both sets cover the late benchmark passage only; neither establishes populated-start readability, full-lap motion or crowded-gallery behavior at that ratio.

## Acceptance boundary

The two P2 construction findings are closed for these corrected native views. Sampled route/craft/shell continuity and the ten aspect anchors pass this bounded inspection. Continuous playback, narrow-edge shimmer, road stability, lighting rhythm, audio, manual handling, crowded-gallery readability and an art benchmark verdict remain unassessed here. Real-time performance and race/restart tests are separate evidence owned by the integration review. No game source, assets, camera, HUD or lighting were changed during this review; review 006 remains the original failure record.
