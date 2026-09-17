# Lighting field and original-map hybrid review

Status: hybrid ready for owner review. Three repeated native races and an isolated full-race performance run pass; presentation limitations and the prior unreproduced timeout remain documented.

The candidate uses the Lighting 01 scene as its base, with the original map's canyon/gallery, thermal works, station/civic approach and distant-city districts added after the protected opening. It retains one existing road course, ship handling, chase camera, HUD and the completed circuit lighting treatment.

## Build and source

- App: `../../builds/Vector Rush Hybrid Route 02.app`.
- Native build GUID: `37d40c498a7c49d1b51678d91eda0f4f`.
- Scene: `UnityProject/Assets/Scenes/Stage1HybridRoute.unity`.
- Protected `Stage1City.unity` SHA-256 remains `a05eafe1ffa7864ee580f98a149110dfc749efbb6c4acf4d849e6276297373ca`, identical to the pre-hybrid committed scene.
- Imported `NocturneProduction.unity` is byte-identical to the original project's source scene. The two maps have different environment art even though their course spline is shared.
- Both original apps are retained. Build 02 differs from the previous hybrid candidate only by opt-in race diagnostics.

## Protected opening and transition

The protected boundary is normalized course progress 0.3228, not a fixed elapsed-time timer. The three repeatable approach, bend and reveal camera anchors from build `4a60f744d24949fcbe74834f67eada4d` exactly match the Lighting 01 anchor transforms and FOV. Direct image comparison shows matching scene composition; transient racer/ghost effects differ. The protected scene file is unchanged in the current candidate.

The imported districts remain inactive inside the field, preventing distant original-map structures from changing its skyline. Beyond the boundary a 0.16-second fade-out and 0.24-second fade-in hides the map switch. At progress 0.975 the route returns to the lighting field for the lap wrap. This is a visible blackout transition; seamless streaming is not claimed. Original geometry that overlaps the protected section is trimmed from the hybrid copy.

The previous hybrid binary completed a native full-lap capture with 408 frames and all 12 route anchors. Its silent [handoff excerpt](../artifacts/hybrid-route/map-handoff.mp4) preserves the capture timestamps, at approximately 10 samples per second. It is automated driving and does not establish motion smoothness or manual handling.

## Validation

All 253 editor tests pass for the current source (`artifacts/hybrid-route/test-results-02.xml`). The earlier native three-race check failed after its first completed six-finisher race; it must not be reported as a pass. A repeat run adds phase, gate, speed and restart telemetry to identify whether the timeout reproduces.

The current binary completed three consecutive native races, each with all six racers finishing, zero recovery resets and passing result-freeze, pause and clean-restart checks. Each automated player race time was 129.166 seconds. Source: `artifacts/hybrid-route/races-02/validation.json`. The earlier timeout did not reproduce; its cause is unresolved, and no gameplay fix is claimed.

The isolated current-binary 1920 × 1080 performance race completed with six finishers and zero recoveries. Mean delivered frame interval was 8.35 ms, P95 8.93 ms and P99 9.30 ms. Two intervals exceeded 33.3 ms; maximum was 183.89 ms. These brief hitches remain a limitation, and their cause has not been isolated. This measures CPU frame delivery rather than GPU execution. No editor tests, screenshot capture or encoder ran concurrently; the other running game instance was temporarily suspended and restored afterward. Source: `artifacts/hybrid-route/performance-01/validation.json`.

## Scope and limits

This work combines the authorized map sections. It does not redesign the original district assets, handling, HUD or lighting test field. Human input/controller feel, audio quality and owner artistic acceptance remain unverified. Some later areas retain sparse or repetitive scenery and strong near-rail glow.
