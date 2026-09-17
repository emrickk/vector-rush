# Complete-circuit lighting review, 2026-09-16

Decision: the accepted test-field infrastructure treatment now continues around the full circuit and is ready for owner review. The protected first 310 spans keep the same start, spacing, construction, materials, lights and ordering. No track, handling, camera, ship, HUD or city-architecture system was changed.

## Native result

- Build: `Vector Rush Circuit Lighting 01.app`
- Build GUID: `655f9a6ebeed4bbeb6b7678b957c6dca`
- Scene revision: `stage1-circuit-lighting-01`
- Infrastructure: 926 spans from normalized progress `-0.012` through `0.98808`
- Protected test field: spans 0 through 309, with continuation beginning at progress `0.3228`

The complete-lap preview finished successfully with 401 native 1920 by 1080 frames, 12 route anchors, stereo audio and progress from `0.00020` through `0.99701`. Direct inspection confirms cyan and magenta rails, edge wash, supports, warm markers and reflective deck response remain visible through the formerly dark circuit sections. No missing rail run or unlit fallback section was found in the 12 route anchors.

The three protected-field comparison anchors use exactly matching race time, player and rival transforms, camera transform and field of view. The first two frames retain the accepted field itself. The 12.66-second frame can now see newly populated infrastructure beyond the protected boundary. This is the intended downstream continuation, not a change to a protected span.

## Verification

- EditMode: 252 passed, 0 failed.
- Full native race: complete, six finishers, zero recoveries, player second at 129.166 seconds.
- Result freeze, pause freeze and clean start invariants passed.
- Isolated 1920 by 1080 frame delivery: 8.349 ms mean, 9.151 ms P95, 9.310 ms P99, 17.514 ms maximum, with zero frames above 33.3 ms.
- Full-lap preview and the performance race use build GUID `655f9a6ebeed4bbeb6b7678b957c6dca`.

## Remaining limits

The circuit treatment is deliberately repetitive because this pass duplicated the approved test-field system and preserved city architecture. Several outer sections therefore remain skyline-light compared with the opening district. During off-line or sustained-contact moments, the wide local rail-response band can occupy much of the lower frame. Neither point blocks this bounded rollout, but both remain presentation issues for a later art pass if the owner wants more route variety or a subtler near-rail response.

Automated steering and native screenshots establish route coverage, race completion and frame delivery. They do not establish manual handling, controller feel, audio quality or owner artistic acceptance.
