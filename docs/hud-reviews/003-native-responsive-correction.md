# Independent HUD review 003 — responsive correction

Reviewer: Astra. Date: 2026-09-07 PDT. **Decision: the corrected HUD passes the native visual layout gate for the seven captured states and three tested sizes.** Keep this composed four-instrument direction. One small timing-spacing refinement remains; no new blocking visual defect was observed. This is bounded HUD acceptance, not an assertion that the game or interface meets a complete AAA production bar.

## Evidence reviewed

Directly inspected all 21 images recorded in [hud-native-02](../../evidence/hud-native-02/hud-evidence.json): menu, countdown, racing, observed boost, pause, a real final-lap event and results, each at 1920 × 1080, 1280 × 800 and 1920 × 810. All recorded image files are present.

The record identifies Unity 6000.6.0f1 and a 2026-09-08 05:37–05:38 UTC capture. Automated steering uses ordinary vehicle physics at 24 simulation frames per second between snapshots, with simulation temporarily frozen for matched aspect stills. Boost and the final-lap transition are observed; the latter was detected at race time 75.368 and pictured at 75.698 seconds. Results follow an actual three-lap finish at 112.603 seconds with zero player recoveries. These qualifications remain essential: a complete capture run does not itself establish visual quality, input correctness or performance.

## Corrections verified in native images

| Prior finding | Refreshed evidence | Verdict |
| --- | --- | --- |
| P1 map/arc/arrow fragmentation outside the 1920 × 1080 reference size | [1280 × 800 racing](../../evidence/hud-native-02/03-racing-1280x800.png) and [1920 × 810 racing](../../evidence/hud-native-02/03-racing-1920x810.png) now show complete, correctly placed route outlines, player arrows and speed arcs. The boost chevron also renders at both sizes. | Resolved in the native player. The failed first-build captures and [review 002](002-native-first-pass.md) remain the historical record. |
| P2 malformed zero/unavailable time display | [Countdown at 1080p](../../evidence/hud-native-02/02-countdown-1920x1080.png) now shows zero minutes/seconds and milliseconds, with a simple `--` for an unavailable best lap. The same state is intact at both other sizes. | Resolved, with a separate minor spacing refinement below. |
| P2 very small control hints | The [1280 × 800 menu](../../evidence/hud-native-02/01-menu-1280x800.png) and [pause view](../../evidence/hud-native-02/05-pause-1280x800.png) have visibly larger hints and action labels. Both legend lines fit, and primary/secondary button labels remain unclipped. | Improved sufficiently for the tested desktop layout. This is not a claim about couch-distance or accessibility testing. |

The scale correction retains the intended map proportions and does not merely hide broken elements. A player can now associate their arrow with the circuit at every tested size. Speed and energy remain one readable lower-right cluster, while position/lap and race timing occupy the upper corners without crowding the driving line.

## New-state and composition check

The [1080p final-lap view](../../evidence/hud-native-02/06-final-lap-1920x1080.png) and [1280 × 800 version](../../evidence/hud-native-02/06-final-lap-1280x800.png) show `FINAL LAP` above a subordinate last-lap value, with lap `3 / 3` in the persistent instrument. The event is high enough to leave the vehicle and immediate road readable. The valid best/last value is `00:37.230`; it is visually subordinate to the race clock. Its fade timing and disappearance are not established by a still.

The [boost state](../../evidence/hud-native-02/04-boost-1920x810.png) communicates activity through text, color and the nearby chevron as well as the live energy rail. The percentage is 99% because this is an early observed boost frame. The final-lap frame also demonstrates a partly depleted 78% rail. Neither is a low/empty-energy case.

Menu, pause and [results](../../evidence/hud-native-02/07-results-1920x1080.png) share the condensed font, restrained rows and citron primary action. Their enlarged action labels and hints remain in bounds across all three sizes. The results total `01:52.603` matches the race record. No additional blocking control-layout, clipping or overlap defect appeared in the complete refreshed set.

## Remaining bounded work

**P3 — tighten the race clock's seconds/fraction gap.** In [ordinary racing](../../evidence/hud-native-02/03-racing-1920x1080.png), `00:03` and `.039` sit far enough apart to read as two independent values. Keep the smaller fraction and fixed alignment, but bring the visible gap to roughly 4–8 reference pixels. This is a small typography change, not another responsive-layout failure. If changed, verify it in fresh native output; this report accepts only the pictured version.

Native pointer hit areas, keyboard/controller actions, low/empty energy and continuous HUD transitions are not verified by these stills. Record those checks separately from the visual acceptance above, including any independent native input run already completed by the parent. There is no reason from this review to reopen world, lighting or vehicle work. Preserve the two native HUD runs and separate corrective history, verify the small final timing change, and advance this HUD as a usable presentation milestone.
