# Independent review 010 — usable night prototype milestone

Reviewer: Astra. Date: 2026-09-07 PDT. **Decision: retain the 0.42–0.78 dampness-mask range and proceed to packaging this playable prototype milestone.** The final adjustment modestly improves road-highlight contrast. It does not resolve the repeated diagonal sheen, and this verdict does not award continuous-motion, manual-play or AAA acceptance.

## Reviewed evidence and revision

Directly compared the final candidate's [gallery middle, frame 105](../../evidence/night-v4-motion-02/selected/amber-mid.png) with the preceding candidate's [gallery middle](../../evidence/night-v4-motion-01/selected/amber-mid.png). Both show race time 35.380 s and 191 km/h in the HUD at a comparable pose. The final frame was inspected from its original PNG before the persistent selected copy was prepared; the link above is the retained selection.

The [final selection record](../../evidence/night-v4-motion-02/selected/selection.json) also retains [observed boost, frame 146](../../evidence/night-v4-motion-02/selected/observed-boost.png). The recording reports 31 frames with boost active. That final boost selection is a retained capture reference, not an additional directly inspected image for this narrow contrast decision. Earlier candidate entry/middle/exit and observed-boost stills were directly inspected during the preceding review work.

The [final identity record](../../evidence/night-v4-motion-02/asset-identity.json) identifies pass04 contact-corrected ship geometry, refreshed ship maps and the gallery/plume implementation already carried by the combined candidate. The last adjustment changes only the road dampness-mask range from 0.48–0.93 to 0.42–0.78. Direct specular rendering remains enabled; normal maps, scene lights and gameplay code are unchanged by this adjustment. The prior full-race evidence remains preserved separately in [night-v4-race-01](../../evidence/night-v4-race-01/asset-identity.json).

The footage is an automated-steering capture at 24 simulation frames per second. I did not continuously view the clip. Selected stills and boost telemetry cannot establish flicker, effect attachment between frames, transition smoothness, speed feel or real-time performance.

## Final contrast decision

**Keep the reduced range.** In the matched gallery view, the broad bright road sheen on the right is modestly softer and quieter. The damp surface still responds to light, the warm road pool remains visible, and the ship's armor, dark center and short plumes remain readable. The change provides a small useful improvement without visibly flattening the whole passage.

The repeated diagonal boundaries are still present. Describe this result as reduced contrast, not a corrected root cause or an artifact-free road. The comparison does not justify another road, lighting or scene rebuild before packaging this milestone.

## Basis for a playable milestone

The inspected [native race verification](../../evidence/night-v4-race-01/race-verification.txt) records a completed three-lap run in 112.60 s with zero recoveries, using automated steering through actual hover physics. It also records successful results-to-race and paused-race restarts and frozen countdowns while paused. The implementation owner reports 29 passing automated tests. These support a usable prototype milestone; they do not establish the quality of manual control or race design.

The separately inspected [runtime sample](../../evidence/night-v4-race-01/runtime-metrics.txt) records 3,529 frame intervals on Apple M2 Max at 1920 × 1080 with VSync enabled: mean 17.01 ms, p95 20.60 ms and p99 25.34 ms. This sample comes from the preceding combined candidate before the final material-range adjustment. It is an observed runtime sample, not isolated GPU timing or a guarantee for other hardware. The simulation-frame recording is not used as performance evidence.

## Remaining limits

- Repeated diagonal road sheen remains visible despite the modest contrast improvement.
- Dark ceiling surfaces, repetitive gallery bays and window-heavy exterior buildings still limit the scene's finish and depth.
- Continuous-motion stability and hands-on control feel have not received independent acceptance in this review.
- The whole game has not met or been accepted as AAA quality.

Preserve the earlier candidate, this final comparison and the separate race evidence. Package the current revision as a playable prototype milestone with these limits recorded. The bounded review is complete; no further scene work is required for this milestone decision.
