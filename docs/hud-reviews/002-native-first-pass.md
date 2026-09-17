# Independent HUD review 002 — first native pass

Reviewer: Astra. Date: 2026-09-07 PDT. **Decision: the four-instrument direction is materially clearer, but this first native build fails responsive HUD acceptance.** The minimap and speed arc break at both non-reference sizes. Retain these captures and this failure even after a later correction passes. This review covers HUD presentation only.

## Evidence and scope

Directly inspected all 18 images in [hud-native-01](../../evidence/hud-native-01/hud-evidence.json): menu, countdown, racing, observed boost, pause and results, each at 1920 × 1080, 1280 × 800 and 1920 × 810. The capture record reports Unity 6000.6.0f1, 2026-09-08 05:33–05:34 UTC, a genuine three-lap finish in 112.603 seconds and zero player recoveries.

The existing testing autopilot drives normal vehicle physics at 24 simulation frames per second between capture states. Simulation is frozen briefly for matched aspect views. Start and pause use the normal director transitions; boost is observed, and results follow a completed race. Phase, lap, speed, energy and VFX are not injected. These are native stills, not a human-input, continuous-motion or performance test.

## Findings

| Priority | Native finding | Required response |
| --- | --- | --- |
| P1 | The complete map and speed arc visible at [1920 × 1080](../../evidence/hud-native-01/03-racing-1920x1080.png) become displaced fragments in [1280 × 800](../../evidence/hud-native-01/03-racing-1280x800.png) and [1920 × 810](../../evidence/hud-native-01/03-racing-1920x810.png). Much of the map falls beyond the bottom edge, the arc largely disappears, and the player arrow/boost chevron also lose their intended shape. Text, rectangular bars and ordinary panels remain positioned correctly. The same failure appears in countdown and boost. | Correct the rotated-line coordinate transform under the root HUD scale, then inspect complete native map, arc and arrow at all three sizes. The failure is not evidence of incorrect route data. |
| P2 | The zero clock and empty best-lap value use a visually broken dash/punctuation string, especially obvious in the [countdown](../../evidence/hud-native-01/02-countdown-1920x1080.png). This looks like a rendering fault in an otherwise clean instrument. | Show a valid zero race time and a simple unavailable best-lap marker. Keep milliseconds subordinate to minutes/seconds. |
| P2 | The [1280 × 800 menu](../../evidence/hud-native-01/01-menu-1280x800.png) and [pause view](../../evidence/hud-native-01/05-pause-1280x800.png) reduce the bottom control legend to very small text. Primary controls remain clear, but discovering recovery, pause and controller commands requires close inspection. | Increase the control-hint minimum size and recheck full-line fit at the smallest tested viewport. |

The line-transform diagnosis follows from the shared failure of rotated strokes while axis-aligned elements remain correctly scaled, plus inspection of the original `Line()` helper using `GUIUtility.RotateAroundPivot`. It is an implementation diagnosis, not proof that a later source edit fixes the native output. The parent has reported corrections in progress; they belong to the next review.

## What is working

- Speed and boost now form one lower-right instrument. In the [1080p boost state](../../evidence/hud-native-01/04-boost-1920x1080.png), the text `BOOST ENGAGED`, changed accent and percentage make the action and remaining resource easy to associate. The ordinary racing line and player craft remain unobstructed.
- The bundled condensed font gives the instrument labels, menu and results a consistent identity. Position and lap share one compact group; the upper center is free of persistent interface branding.
- The 1080p map shows a recognizable circuit outline, a distinct player arrow, quieter rival markers and a start/finish mark. Its geometry is useful when correctly drawn.
- Menu, pause and results use a clear primary action and consistent secondary rows. All primary labels and controls remain within their intended bounds in all three sizes; no further blocking layout defect was observed in these states.
- The [results view](../../evidence/hud-native-01/06-results-1920x1080.png) gives final position, total time and best lap a readable hierarchy. It displays `01:52.603`, consistent with the capture record's completed race.

## Next bounded gate

Rebuild the three identified HUD corrections and inspect the same three native sizes. Add a real final-lap event so the transient and timing hierarchy can be judged with a valid best lap. Preserve this first-pass evidence separately.

Low/empty energy is not covered here: the boost capture is near state entry at 99% energy. Drawn button appearance also does not verify native pointer hit areas, keyboard/controller navigation, or restart behavior. A short continuous passage is still needed before claiming animation or temporal refinement. These are evidence limits, not newly observed failures. Do not turn them into a new world or vehicle polish pass, and do not label the first build AAA-ready.
