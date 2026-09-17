# Billboard architectural finish and pacing — September 17, 2026

Owner approved cabinet/mount detail, balanced brightness, distinct animation pacing and investigation of the two candidate 05 timing hitches. Candidate 05 positions, dimensions and facade headings remain the layout baseline; this is still a review candidate, not artistic acceptance.

Scene `Assets/Scenes/AnimatedBillboardsStage6.unity`; revision `animated-billboards-stage6-06`; build `e2e0c129118247348e77af4c98d57deb`. Local app: `/Users/anping.wang/output/vector-rush-billboards-2026-09-17/Billboards06.app`. Local native review: `http://127.0.0.1:8773/review/`; previous reviews remain at `review-04/` and `review-05/`.

## Physical integration and motion

Inset cabinet lips, rain hoods and bottom ledges surround the existing screen surfaces. Steel arms, diagonal braces and anchor plates reach actual office-facade triangles. Eight displays attach directly to facades; the corner return and blade connect to neighbouring facade-anchored frames. Measured gaps range from 3.94 to 14.37 m, so these are projecting structures rather than flush posters. Supports are combined into one persistent mesh/render batch and add no colliders. Original building meshes are retained; the visible recess is in the cabinet framing, not a newly carved building opening.

Screen gain is campaign-specific: AURORA 1.20, VOLT .90, ECHO 1.10, ORBIT 1.22. Accent emission and facade washes are reduced; fog is unchanged. Trilinear mip filtering softens transitions at distance. Playback periods are 7.5 s AURORA, 4.5 s VOLT, 12 s forward/back ECHO and 4.25 s ORBIT. Ticker scroll speed drops from .035 to .018 UV/s; orbital-object rotation slows from 22 to 15 degrees/s. The same global pacing drives direct animated reflections. Original videos/atlases are unchanged.

[Native driving comparison](comparison.jpg) · [Native cabinet and support detail](details.jpg)

## Verification

286/286 Unity tests passed, including collider/course/weather preservation and animation cycles. Native driving capture completed: 46.101 s, 971 source frames, 21.12 Hz average (970 non-zero-duration encoded frames). The recorder had uneven intervals, including a 384 ms maximum; this is capture evidence, not a performance benchmark. Motion close-ups contain 399 frames over 35.000 s. Screen readability is judged from the driving capture; close-ups supplement structure and pacing review. The updated browser report was visibly verified: driving frames advanced from the opening to 23.9 s, the animation panel advanced through 14.7 s, and cabinet/comparison images rendered. Both replays were left paused for owner review.

The original course, collision geometry, handling, ship, HUD, fog/rain and dry tunnel behavior are preserved. Game P5 blur remains separate.

## Hitch investigation

Candidate 05 had two frames over 33.3 ms, maximum 190.926 ms. Its aggregate report had no per-hitch timestamp, GC or focus data, so it cannot identify a cause.

The opt-in performance recorder now reserves space for 32,768 frame samples before measurement, suppresses its periodic synchronous diagnostic log during performance mode, and retains up to 32 hitch records with time, course progress/lap, focus state and GC collection-count deltas. It does not capture images or encode media. These changes reduce measurement interference; they do not establish what caused the old hitches. The first diagnostic run recorded one 41.47 ms frame on lap three at course progress .373745, with the application focused, zero focus changes, and no recorded GC collection-count delta. This is beyond the billboard district. The existing route switch activates at .3228, which does not coincide with this hitch; no causal claim or unrelated route edit follows from that observation. The repeat run completed without a frame over 33.3 ms. No repeatable cause is established; candidate 05's 190.926 ms stalls did not recur in either new sample.


Both uncaptured 1920×1080 runs completed a three-lap automated race with six finishers, zero recoveries, frozen results, pause freezing and clean restart. The Unity Editor, media capture/encoding and browser playback did not overlap these runs; ordinary OS activity was not eliminated. Values measure delivered frame intervals, not GPU execution time.

| Run | Mean | P95 | P99 | Maximum | Frames >33.3 ms |
| --- | --- | --- | --- | --- | --- |
| [A](performance-06a.json) | 8.35 ms | 9.04 ms | 9.24 ms | 41.47 ms | 1 |
| [B](performance-06b.json) | 8.33 ms | 8.33 ms | 8.33 ms | 8.33 ms | 0 |

## Relay

Local source milestone only. Automatic approval review previously blocked the GitHub publication; direct owner approval remains pending. Do not automatically publish, dispatch agents or expand to other city work. New authoring helper `BillboardMounts.Build` validates direct facade attachments and shared structural connections before scene save. `mounts.json` records those connections. Use the portable wrapper to build the serialized scene; explicit authoring regenerates only this Stage 6 candidate.
