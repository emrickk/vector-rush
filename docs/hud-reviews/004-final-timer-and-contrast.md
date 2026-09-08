# Independent HUD review 004 — final timing and contrast

Reviewer: Astra. Date: 2026-09-07 PDT. **Decision: accept the final timing refinement and advance the HUD milestone to packaging.** The seconds/fraction spacing issue from [review 003](003-native-responsive-correction.md) is resolved in the final 1080p native images. No serious new HUD defect was observed, and no further implementation is requested by this review.

## Directly inspected evidence

| Final native image | Visible HUD state | Assessment |
| --- | --- | --- |
| [Frame 0105 — exterior](../../evidence/hud-motion-01/selected-0105.png) | Race time `00:31.400`, speed 144, full energy, lap 1 | The smaller fraction now sits close to seconds and reads as one time. Digits, speed arc and map remain clear against dark road and detailed buildings. |
| [Frame 0200 — warm gallery](../../evidence/hud-motion-01/selected-0200.png) | Race time `00:35.350`, speed 192, full energy | The same timing hierarchy holds in the brighter gallery. Main speed and energy remain readable over the blue road highlight; the interface leaves the craft and route open. |
| [Frame 0240 — observed boost](../../evidence/hud-motion-01/selected-0240.png) | Race time `00:37.019`, speed 323, `BOOST ENGAGED`, 86% energy | Boost text, chevron and changed rail accent are clearly grouped. The complete arc and map remain intact, and the timer fraction remains subordinate. |
| [Frame 0300 — lap 2](../../evidence/hud-motion-01/selected-0300.png) | Race time `00:39.519`, lap 2, best/last `00:37.400`, speed 239, 91% energy | The lap event and persistent lap count agree. Best/last and race time have a clear hierarchy without covering the immediate road. |

The listed speed values are the displayed HUD readings, which are smoothed; they are not substitutions for raw per-frame vehicle telemetry.

One minor contrast limitation remains visible: the bright overhead lamp intersects the small `RACE TIME` label in frame 0300, making that label less clean than the main digits. The primary clock and best-lap value remain readable. This does not warrant another implementation pass for the current milestone.

## Capture qualification and scope

Read the [capture metadata](../../evidence/hud-motion-01/replay-metadata.json) and [capture description](../../evidence/hud-motion-01/replay-info.txt). The run contains 360 records at 24 simulation frames per second, with automated steering through normal vehicle physics. Metadata reports 32 frames with observed boost. All 360 referenced frame files exist, and each of the four selected PNGs is byte-identical to its corresponding source frame.

This is a 15-second simulation-time recording, not a real-time performance test. Only the four selected stills were directly viewed in this review; the clip was not continuously watched. Audio, animation smoothness, transient disappearance, low/empty energy and native input are outside this four-image acceptance. Separate input evidence remains separate.

The final spacing check is at 1920 × 1080. It supplements, rather than replaces, review 003's seven-state, three-size responsive layout acceptance. Preserve [review 002's first-build failure](002-native-first-pass.md), the corrected native evidence and this final narrow verification as separate history. The result is a usable, coherent racing HUD milestone; a broader AAA production claim is not established by these checks.
