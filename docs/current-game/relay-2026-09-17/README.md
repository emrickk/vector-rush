# GitHub relay 01 — fresh-checkout validation, 2026-09-17

Started from `game/current` publication `a7b8d70013703f17b65125e8a8b1ad90c9a53d4f` in an isolated clone. The owner supplied the branch after requesting agent-to-agent continuation. This milestone closes the publication handoff's missing current-source tests, native rebuild, recorded camera check and single three-lap performance race. Game code/assets were not changed.

## Verified

- All 2,757 published payload files matched the source manifest before work.
- Clean Unity 6000.6.0f1 import and complete current EditMode suite: **271/271 passed**.
- Fresh underground native build: **98d13df67244414c923b8e1b64f3c643**; course identity remains `e5b995eaedbda504165e40147acfbd697321b38a249fd1f08f770642b0a2dc4f`.
- Complete actual native automated lap: **43.2 s, 405 timestamped frames**, with 43.2 s of non-silent game audio. Encoded from actual recorded times.
- Existing `ValidateCapture` completed: 405 camera samples, 1,831.03 m swept path, no new-enclosure crossings and no six-axis proximity hits within 0.35 m. This checker does not cover all possible manual camera paths.
- Separate uncaptured 1920 × 1080 three-lap race: player **130.015 s**, six finishers, zero recoveries for all racers; finish/pause/initial-start checks passed. Frame time **8.334 ms mean, 9.312 ms P99, 16.676 ms maximum**, zero frames over 33.3 ms. One local sample, not a GPU benchmark or improvement claim.
- Completed HUD baseline scene SHA-256 remains `83195acf00ea5140cb4600a1d686ca1cd1641683c7b16729aa8b7a57e4c2f7b5`. No scene, source asset, script or project-setting changes remain after build cleanup.

Machine-readable results: [verification.json](verification.json). Initial sandboxed Unity licensing startup hung; only this task's processes were stopped and a normal-access retry completed successfully. This was an environment startup issue, not a test failure.

## Next baton

Continue the existing underground candidate, starting with the sparse upper foundation faces and terrace/city connection. The published reference and newly captured stills still show a gap in facade detail, massing and inhabited terraces. Preserve the road's banking/horizontal layout, recognizable ship, completed HUD and baseline scene. Do not regenerate the candidate without preserving hand edits.

Record a bounded visual hypothesis, implement in the candidate, and compare actual native descent → portal → gallery → exit against the supplied reference. This relay inspected sampled stills and created continuous footage; it does **not** claim a completed continuous-motion art review. Show the real comparison before full-circuit rollout. Repeat relevant checks after changes; run the three-race lifecycle to establish repeated restart behavior, which this single-race check does not establish. Manual driving/controller feel and owner artistic acceptance remain open.

Before the next commit, fetch `game/current`; preserve newer remote work and use a normal push. Update the current handoff and publication manifest for the final staged payload. Builds, raw captures and private licensing logs stay outside source control.

Local evidence on this machine is under `/Users/anping.wang/output/vector-rush-relay-2026-09-17/`: `VectorRushRelay.app`, `preview-01/full-lap.mp4`, `tests-03/results.xml`, `preview-01/camera-clearance.txt`, and `performance-01/validation.json`. These paths are conveniences, not clone dependencies.
