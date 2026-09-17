# Opening-road light response: review candidate

The opening bend now receives alternating cyan and amber light pools from its existing overhead fixtures. This is a direct-light treatment, not screen-space reflection or reflected scene imagery. It is ready for owner review on one 18-second passage; owner artistic acceptance and circuit-wide rollout remain open.

## Change and boundary

The treatment reuses the 21 existing fixture lights around the start-line wrap and opening bend. It moves each light to the center of its visible diffuser, aims it obliquely across the banked deck, and gives it a broad outer falloff around a narrower core. Source count, road material, road texture detail, geometry, collision, exposure, bloom, camera, HUD, blue exhaust and gameplay remain unchanged.

Ordinary launch enables the candidate. `-vrRoadLightBaseline` restores only the prior road-light setup. It does not select `-vrBlueBaseline`; the layered blue exhaust remains enabled in both road variants.

The first 1,180/1,320 cd trial executed but was rejected as too weak at gameplay scale. The second trial established visible color but exposed a hard 54-degree cone edge. The retained candidate uses the same 3,600/3,000 cd cyan/amber sources with an 82-degree outer penumbra and 24-degree inner cone. Rejected native runs remain in the evidence directory.

## Evidence

Evidence directory: `../artifacts/road-light-response`

- `Vector Rush Road Light.app`: native macOS candidate, road response enabled on ordinary launch.
- `Road light before-after.mp4`: labelled 18-second same-build comparison.
- `Road light candidate 1080p.mp4`: full-resolution candidate passage.
- `still-pairs`: approach, mid-bend and exit comparisons from original native frames.
- `comparison-validation.json`: identical build GUID, 432 frames per run, and zero recorded deltas for player/camera position, rotation, velocity, FOV, race time, input, energy and exhaust response.
- `tests/editor-tests-clean-cache.xml`: 233/233 Unity EditMode tests passed.
- `performance`: separate real-time A/B runs without screenshot or encoding work.

On Apple M4 Max at 1920 x 1080, the baseline measured mean 8.336 ms / p95 9.112 ms across 300 sustained-boost samples. The candidate measured mean 8.361 ms / p95 9.068 ms across 299 samples. This short CPU frame-interval comparison shows no observed regression; it is not an isolated GPU measurement or whole-game benchmark.

Both visual runs completed normal cruise, acceleration, boost, release and reboost through actual vehicle input. They used a scripted virtual gamepad and 24 Hz simulation-time capture, so the footage is not human-driving or real-time performance evidence. One real-time baseline attempt boosted during the cruise phase and invalidated its sample; it remains preserved as `performance-failed-input-anomaly-01`, and the clean repeat is the reported result.

## Verdict and limits

The retained pass creates a clear moving sequence of cool pools through the approach and mid-bend, followed by a warm pool at the exit. Road seams and the dark aggregate remain visible, and the widened penumbra removes the obvious cone edge seen in the rejected second trial. Because these are real direct lights, nearby vehicles also receive their color while crossing a pool. The treatment does not claim building, vehicle or offscreen reflections.

The native player build used the established player-only package workaround after Pipeline metadata-reference failures: Pipeline was temporarily omitted, the existing Newtonsoft JSON 3.2.2 dependency was retained, and the pre-build manifest/lock bytes were restored. The saved scene was built without `VectorRushSetup.Prepare()`.

Pre-existing FBX metadata, package manifest/lock, untracked art/settings and Stage 2 review files remain outside this milestone.
