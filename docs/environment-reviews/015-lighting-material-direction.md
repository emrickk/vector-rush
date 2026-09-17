# Environment review 015: lighting and material depth

Independent direction, 2026-09-08. **Prioritize the thermal plant's visible podium and pipe supports, then the mast's exposed bracing and skyroom.** Improve the distinction between concrete, metal and glass while retaining the accepted silhouettes, placement and night-road clarity. This is a completed baseline direction review, not a review of implemented changes.

## Inspected baseline

Directly inspected eight original 1920 × 1080 images from [the final landmark capture](../../evidence/landmarks-final/full-lap/environment-evidence.json): frames **148, 175, 193, 534, 570, 606 and 641**, plus **05-reveal.png**. All seven selected frame copies were verified byte-identical to their source frames by SHA-256.

The metadata identifies build GUID `b709ef36660443b7bdec08170d00b8df`, Unity 6000.6.0f1 and Apple M2 Max. Its 1,440-frame sequence completed at 17:38:04 UTC and covers 60 simulation seconds with automated steering, normal hover physics and a 24 fps simulation capture rate. Only the eight identified pictures were visually inspected for this direction. No continuous playback, audio, other aspect ratios, performance or human driving was reviewed here.

## 1. Reveal the thermal plant's existing service base

At [534](../../evidence/landmarks-final/selected/frame-0534.png), progress 0.4993, and [570](../../evidence/landmarks-final/selected/frame-0570.png), progress 0.5399, the front and side of the podium are clearly exposed below the drums. The large rectangular service recesses and their surrounding wall currently merge into nearly black shapes. **This is the best visible foundation target.** A restrained local wash across one facade and its top ledge should distinguish the outer concrete face, recessed openings and upper equipment platform. Retain dark recess interiors so the facade gains depth rather than becoming evenly bright.

At [606](../../evidence/landmarks-final/selected/frame-0606.png), progress 0.5802, and [641](../../evidence/landmarks-final/selected/frame-0641.png), progress 0.6194, the camera rises and the platform top, pipe runs, support posts and drum feet dominate. Give those existing connections enough local contrast to read as equipment supported by a service deck. The pipes are currently much harder to distinguish than the drums. Aim the treatment at these visible surfaces rather than spending light on the hidden rear or ground below the near barrier.

Keep the drums' broad curved light response, but separate their satin metal from the rougher concrete platform and warmer insulated mains. Subtle directional roughness and modest surface variation should support the existing shape; additional bright bands or fine repeated seams would compete with it.

## 2. Deepen the mast above the barrier

The [148](../../evidence/landmarks-final/selected/frame-0148.png), [175](../../evidence/landmarks-final/selected/frame-0175.png) and [193](../../evidence/landmarks-final/selected/frame-0193.png) references span progress 0.1497–0.1999. The room, projecting floor and roof edges, upright faces and inner bracing are visible. The actual ground foundation is concealed behind the road and barrier throughout these views. Lighting that hidden foundation cannot establish its connection for this camera.

Prioritize the existing cross-bracing, where it meets the uprights, and the underside and side returns of the skyroom. Preserve a darker internal cavity and a quieter outer face so the structure reads in layers. The occupied pane is currently a flat amber rectangle beside nearly black glazing. Give it a restrained perimeter/value change and distinguish reflective smoked glass from the surrounding opaque cladding. Keep the sparse occupied-room pattern and clear gap between the uprights.

## Material and lighting boundaries

The current [landmark setup](../../UnityProject/Assets/Scripts/World/NightLandmarks.cs) maps eight named finishes and supplies four architectural spot washes. Use the existing material regions to establish a consistent hierarchy: rough concrete at the service base, restrained ceramic cladding, darker recessed structure, satin metal drums, warmer mains and reflective smoked glazing. Judge the materials in the race views before adding finer texture detail.

Preserve the [final-station reveal](../../evidence/landmarks-final/full-lap/05-reveal.png) as context: the workshop and canopy already have useful warm interior depth. The new local treatment should fit that night scene. Keep light concentrated on architecture, retain the current road finish and fixture-shadow correction, and preserve the visibility of the barrier, next turn, player and HUD. No new landmarks, geometry rebuild or layout change is needed.

## Handoff state

Baseline inspection and this direction document are complete. No lighting/material candidate was inspected, and no implementation was performed by this reviewer. After the implementation owners finish their local lighting and material work, the next step is one independent native candidate review using these same references, with full-lap evidence available and the exact inspection scope recorded. Acceptance should depend on readable podium recesses and equipment supports, deeper mast construction, distinct finishes and preserved night-road clarity.

Only this direction document was authored for the current task. No runtime source or assets were edited.
