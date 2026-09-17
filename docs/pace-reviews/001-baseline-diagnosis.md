# Pace review 001: baseline diagnosis and bounded first candidate

2026-09-08. **The recorded player is effectively alone after the launch. Rival speed loss is concentrated around the rival-only edge guard and early following behavior; the vehicles do not have lower rival engine settings.** Try a fixed actual-player cruise/boost reduction before changing rival lane reservations or weakening their safety controller. This report is read-only: no gameplay source was modified.

## Evidence and method

The input is the final environment [native full-lap report](../../evidence/environment-final-full-lap/environment-evidence.json), build GUID `153b77ea7d53463b8fc11a0ddae232be`: 1,008 frames at 24 captured frames per simulation second, with all six rigidbody velocities, positions and normalized track progress. The player used the existing testing autopilot through ordinary physics. These data describe that automated baseline; they do not establish manual-player balance or real-time performance.

I reconstructed `TrackPath`'s 1,200-segment Catmull–Rom arc-length table from its source knots, producing approximately **1,844.517 m** total length. Each racer's normalized progress was unwrapped by adding successive differences in `[-0.5, 0.5)`. Longitudinal separation is the absolute difference in unwrapped progress times track length; it is not Euclidean distance across nearby sections of the circuit. Speed is rigidbody velocity magnitude multiplied by 3.6. Post-launch averages use the 891 frames with race time greater than five seconds.

For the edge analysis, I reproduced `TrackPath.Evaluate`'s tangent, grade-independent bank and right vector, then calculated lateral position and lateral velocity. Projected excursion is `max(abs(lane), abs(lane + lateralSpeed * 0.45))`. The source activates the rival guard above 6.2 m and releases below 5.6 m. **The report does not record guard state, so duty estimates replay that hysteresis at sampled 24 fps, not every physics step.** Exact instrumentation may differ around transitions. The raw excursion fraction does not require inferred hysteresis. No telemetry for throttle, desired speed, blocker, collision or guard cause was available in this recording.

## Measured separation and speed

| Racer | Mean speed after 5 s, km/h | Distance behind at 42.079 s, m | Projected excursion >6.2 m | Estimated guard duty |
| --- | ---: | ---: | ---: | ---: |
| Player | 179.1 | — | 31.2% | 32.7% counterfactual; player is exempt |
| Rival 1 | 130.5 | 628.0 | 79.5% | 82.2% |
| Rival 2 | 164.7 | 287.5 | 9.5% | 12.3% |
| Rival 3 | 143.6 | 524.9 | 44.4% | 45.7% |
| Rival 4 | 155.1 | 389.9 | 29.9% | 32.8% |
| Rival 5 | 149.6 | 461.2 | 30.2% | 36.4% |

Across the entire recording, a rival is within 30/40/60 m on only **5.7% / 7.0% / 8.8%** of frames. After five seconds, all three fractions are **0%**; a rival is within 100 m on only **11.3%** of frames. At 5 s, the nearest rival is already approximately 82 m behind. The nearest gap averages **102.7 m** in the cool gallery (progress 0.390–0.432, range 92.0–116.0 m) and **183.2 m** in the warm gallery (0.860–0.902, range 180.1–192.4 m).

Rival 1's post-launch median lateral position is **+8.99 m**, with observed range −1.16 to +9.08 m. It spends sustained stretches at the outer edge, rather than suffering only brief curve corrections. Rival 2, the fastest rival, has median lane +2.12 m and the lowest estimated guard duty. This supports a relationship between edge exposure and pace loss, but does not prove why the outside rival remains there. Its actual steering, blocking and contact states need the instrumented run.

Source inspection explains the mechanisms:

- Every craft currently has cruise 78 m/s, boost 108 m/s and acceleration 34 m/s². Normal AI desired speed starts at `75 + gridIndex * 0.5`, so rivals have a slightly higher initial request than the testing player.
- Curve preview, heading alignment and following limits can reduce that request for every AI-driven craft. The launch following distance is `11 + speed * 0.32` m; the trailing rows have a clear early acceleration deficit in the recording.
- The additional rival-only corridor guard caps desired speed at **42 m/s** and forbids boost while active. The testing player explicitly bypasses that guard. Rival 1's sampled behavior is consistent with persistent use of that limit.
- The preferred outer lanes are ±5.8 m, only 0.4 m inside the guard activation boundary. Curvature, heading and lateral velocity can exceed the predictive boundary even before the craft's center crosses it.

## First candidate

Set the **actual player's** cruise to **60 m/s** and boost to **82 m/s**. Keep acceleration 34, player steering/hover physics, rival settings, guard thresholds and lane reservations unchanged. Apply the settings to the real player vehicle, not just its testing autopilot. The user explicitly permits slowing the player.

This is a bounded first experiment, not a validated balance solution. The ideal full-throttle straight equilibrium, ignoring grade, contact and AI throttle regulation, solves `34 * multiplier * (cap − speed) / 16 = 2 + 0.0017 * speed² + 0.05 * speed`. It falls approximately **71.3→55.3 m/s** in cruise and **101.0→77.4 m/s** in boost, where the boost multiplier is 1.65. These are physical estimates, not observed maxima or promised lap times; the testing autopilot also regulates toward its desired speed.

Moving all preferred lanes inward to ±4.6 m is not an equally isolated change. The target lies inside the corridor, but adjacent lane spacing would fall below the controller's 5.5 m occupancy threshold and can conflict with collider-based clearance. That would require reevaluating passing, reservations and side-by-side interactions. Raising the guard speed or removing the guard risks reintroducing the earlier wall/stall recoveries. Preserve those behaviors for this limited balance pass.

## Native validation and acceptance criteria

First record actual baseline guard duty, desired speed, following limits and impacts to check the inferred diagnosis. Then run the same deterministic player autopilot and three-lap sequence with the candidate's real player settings. Use actual unwrapped race distance, not modulo-only gaps. Suggested gates are deliberately practical rather than guarantees:

- **Closeness:** after the first five seconds, at least one rival within 60 m on ≥50% of racing samples, with within-30 m and within-100 m fractions reported alongside it. Report each lap and each gallery separately so a crowded launch cannot conceal a solitary race.
- **Competitive finish:** at player finish, nearest rival within 100 m and at least two rivals within 200 m, counting rivals ahead or behind by absolute unwrapped distance. Report the entire field spread and actual recorded lap-crossing times. `RaceDirector` freezes all vehicles when the player finishes, so unfinished rivals have no measured finish time; do not present extrapolated finish times as an observed finish spread.
- **Safety and playability:** complete three valid laps with zero player and rival recoveries; report sustained stalls, side contact and edge-guard duty rather than allowing slower racing to hide those faults. Preserve restart and countdown/pause checks. Inspect at least one genuinely occupied gallery passage and one nearby overtake in native motion.
- **Actual-game scope:** confirm the settings apply with testing autopilot disabled. Automated success establishes repeatable balance evidence, while manual acceleration, boost contrast and cornering feel still require a direct play check.

If the candidate fails closeness, use its measured nearest-rival and lap-specific gaps to choose the next bounded adjustment. Do not accept a staged screenshot, an autopilot-only handicap or an uneventful race completion as proof of closer racing.
