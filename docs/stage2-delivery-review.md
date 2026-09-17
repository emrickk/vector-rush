# Stage 2 — propulsion and the complete player interface

## Candidate

Native app: `../builds/Vector Rush Stage 2 candidate-03.app`.
Convenience launcher: `../builds/Play Vector Rush Stage 2.command`.
Build GUID: `ebace6a5dfab441fa418b8c6cfed21e7`.
Runtime source: `1219ed1d6d9f320293d078eea02cb76f7e0573b6`.
Evidence: `../artifacts/stage2/candidate-03/`.

The original checkout and previous apps remain preserved. Changes are local; no public remote push or publication was made. Stage 1 city construction, course, craft geometry, handling and camera behavior remain outside this change.

## Visible changes

- **2A:** Six interface references establish the shared typography, warm-white/cyan/lime palette, spacing and screen hierarchy. References use actual native gameplay backgrounds.
- **2B:** Broader paired exhaust plumes have a shorter white-blue core, animated cyan shoulders, ignition, sustained boost, release and recharge behavior. The race HUD has large open speed/position instruments, segmented boost with actual state labels, nearby-rival distance, lap/time, minimap and honest record/ghost status.
- **2C:** Title, countdown, pause, results, audio settings, controls, bindings and display/comfort share the same visual treatment. Settings include working sliders, HUD scale and reduced interface motion. Race results use the actual finish ledger; return-to-title and retry reset the race lifecycle.
- **2D:** One identified default-on native build is the delivery candidate. Verification and remaining limits are recorded below.

## Native review

The first integrated capture exposed an inverted bottom gradient and incorrect native panel color handling. Corrective commit `1811150` removes the cyan band and restores the dark panels/lime controls. Candidate 03 title, race HUD, pause and settings/control screens were inspected at 1280×720, 1600×900, 1920×1080 and 2560×1080; no clipping or layout failures were found in those captures.

The first exhaust implementation read as long narrow beams. Corrective commit `1219ed1` shortens the hot core, broadens the shoulder and adds actual animated shape deformation and density breakup. Matched native frames show a substantial increase in flame volume, distinct propulsion states and a clear driving line. Temporal samples show irregular variation and extinguished thrust on release.

There is still visible material work to consider: overlapping plume sheets can form star-like rays and triangular facets around the bright core and outer flame. The result is suitable for owner review, not a claim of natural volumetric rendering or AAA quality.

## Verification

- The final runtime source passes **203/203 EditMode tests**, with no failures or skips. Coverage includes preferences, menu/modal input isolation, propulsion envelopes, finish lifecycle, record errors and evidence isolation.
- All four native screen captures pass external image/report validation.
- The same-binary propulsion comparison validates **432 matched natural frames**, including recorded vehicle/camera pose and input states. Cruise, acceleration, boost, empty boost and release were observed through actual virtual-gamepad input. Synthetic driving is excluded from personal records.
- Final input, full-race and performance findings are added after their native runs.

## Evidence boundaries

Programmatic menu screenshots establish appearance and screen coverage; direct pointer/keyboard checks are separate. Videos are silent 24 fps simulation-time captures and do not establish real-time performance, audio quality or human handling. Real-time frame measurements are delivered CPU frame intervals, not isolated GPU timings. Physical-controller comfort and owner artistic acceptance require the owner's review.

Stage 3 environment/material/lighting work is not started by this delivery.
