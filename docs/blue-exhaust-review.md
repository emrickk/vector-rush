# Blue exhaust: first reviewable candidate

Item 1 is implemented and ready for visual review. The camera, road, HUD, world and driving source were not changed. This is an exhaust candidate, not owner acceptance or an overall AAA verdict.

## What changed

The new default treatment replaces the visible crossed sheets with a directed particle jet and a sparse world-space wake. A deterministic 64-frame animated density atlas supplies internal motion. The nozzle keeps its white-blue core; the small central aperture stays compact rather than emitting beads. Particle birth positions include within-frame flow to prevent gaps/banding at 24 fps. The shader fades near the camera and against scene depth, which the effect explicitly requests from the camera. Throttle/boost envelopes and lighting intensities retain existing authority.

The paused blue sheet/wake version remains available in the same binary with `-vrBlueBaseline`. Its source snapshot is preserved outside the repository in the evidence folder. Removed ribbon files and the paused blue baseline were already working-tree changes when this task started; the commit includes the needed baseline for reproducible comparison.

## Evidence

Evidence folder: `../artifacts/exhaust-blue-v2` relative to this repository.

- `Vector Rush Blue Exhaust.app`: native macOS candidate; layered exhaust enabled on ordinary launch.
- `Blue exhaust before-after.mp4`: labelled 18-second side-by-side comparison.
- `Blue exhaust candidate 1080p.mp4`: full-resolution candidate sequence.
- `native-comparison`: original 1080p frame sequences, launch arguments and reports.
- `comparison-validation.json`: same build GUID; 432 frames per run; zero differences in every recorded player/camera position, velocity, FOV, energy and race time; identical stage sequence.
- `editor-tests.json`: 225/225 Editor tests passed, including five new emission/reset/pause/release/random-isolation tests.
- `performance`: separate real-time runs, no screenshot/encoder or open Editor workload.

On Apple M4 Max at 1920×1080, the sustained-boost sample window measured baseline mean 8.389 ms / p95 9.039 ms (298 samples), candidate mean 8.362 ms / p95 8.875 ms (299 samples). This short run shows no observed regression; it is not a whole-game benchmark or isolated GPU measurement.

Both native propulsion runs completed and observed cruise, acceleration, actual boost and release. Video uses scripted gamepad inputs and 24 Hz simulation time with no audio; it is not human driving or evidence of real-time capture performance.

## Visual assessment and limits

The candidate reads as a narrower, textured jet instead of the baseline's broad glowing sheets and large smoky particles. Sequential native frames show connected nozzle emission, persistent thrust through the turn and extinguished jets on release. The reference's holographic phase is a separate visual state; we did not copy it or assume it represented boost. The reference's stronger road response and broader scene finish remain outside this item. The new trail is deliberately bounded and narrower, not claimed to be longer than every frame of the paused baseline.

Unity Pipeline failed player compilation with missing metadata assemblies. The successful player build temporarily omitted that development package while explicitly retaining its existing Newtonsoft JSON dependency (3.2.2), used the current saved scene without running scene preparation, then restored manifest/lock bytes from the pre-build snapshot. The reproduction script and successful build log are preserved with the evidence. Two Editor-saved rendering-settings files were restored to their clean starting contents. Package and unrelated pre-existing asset changes are excluded from this milestone commit.

No GitHub remote is configured in this checkout; `origin` is a local integration path. Keep this milestone local rather than pushing to an unverified destination.
