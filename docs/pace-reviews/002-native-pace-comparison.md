# Pace review 002: native baseline and player-speed candidate

2026-09-08. **Player speed reductions and speed-aware pursuit steering have not yet produced stable close racing.** The 60/82 m/s candidate improves proximity, but misses the proposed gates; slower 55/75 and subsequent pursuit steering each have closer early laps followed by separation. All six recovery counts remain zero in every run. Candidate 03 telemetry shows rivals requesting inward steering while their long box colliders remain almost flush with the barrier. This supports a physical wall-lock hypothesis rather than another blind speed reduction or a claim that pursuit steering closed the cause.

## Evidence and calculation

Baseline [native report](../../evidence/pace-baseline-01/pace-evidence.json) and [derived analysis](../../evidence/pace-baseline-01/analysis.json): build GUID `db45f4d51f4a4c1e98aca0a6d43f3fad`, 1,127 samples, three-lap player finish at **112.603 s**, six optional pack screenshots. The report is complete with no error. All vehicle settings remain cruise 78 and boost 108 m/s. The harness drives the real player with testing autopilot through normal physics and uses `Time.captureFramerate=24`; these are simulation-time pace data, not real-time performance or manual-play acceptance.

I recomputed each rival's signed gap as `(rival.RaceProgress − player.RaceProgress) × track.Length` and verified the recorded values to within 0.000061 m. **Closeness uses the minimum absolute validated race-distance gap to any rival.** It uses neither the harness's headline Euclidean proximity nor wrapped track proximity, which can count another course branch or a lapped rival. Positive signed gaps indicate rivals ahead.

Metrics below are duration weighted after race time 5 s. Each observed state applies until the next sample, with the first interval clipped to the five-second boundary. Player-lap and gallery attribution use the interval's starting state, so boundary precision is limited by the approximately 0.1 s sampling. Gallery ranges are normalized progress 0.390–0.432 (cool) and 0.860–0.902 (warm), across every player lap. Analysis records per-lap/gallery values and the source report's SHA-256. Candidate analysis excludes already-finished rivals from active racing proximity and independently unwraps physical track progress to detect misleading gaps when validated progress freezes at the finish. All five rivals remain unfinished in baseline and candidate 01, so this exclusion does not change their results.

Candidate 01 [native report](../../evidence/pace-candidate-01/pace-evidence.json) and [analysis](../../evidence/pace-candidate-01/analysis.json): build GUID `aa6abad9842a4e8e8d2649114ede5c05`, 1,210 samples, complete without error, actual player settings 60/82 m/s throughout. The comparison uses the same 24 fps capture-rate protocol. Changes in pack interactions are part of the observed result; this is not a replay with identical rival trajectories.

## Baseline versus candidate

| Metric | Baseline 78/108 | Candidate 60/82 |
| --- | ---: | ---: |
| Player finish, simulation seconds | 112.603 | 120.912 |
| Mean player speed after 5 s, km/h | 176.9 | 164.8 |
| Rival within 30 / 40 / 60 / 80 m after 5 s | 0% / 0% / 0% / 0% | 0% / 5.3% / 26.6% / 53.6% |
| Rival within 100 m after 5 s | 4.0% | 73.0% |
| Mean nearest absolute race gap, m | 280.1 | 81.5 |
| Nearest rival at player finish, m | 476.6 | 177.1 |
| Rivals within 100 / 200 m at player finish | 0 / 0 | 0 / 1 |
| Entire field distance spread at player finish, m | 1,352.2 | 1,085.2 |
| Recoveries: player, then rivals 1–5 | 0 / 0 / 0 / 0 / 0 / 0 | 0 / 0 / 0 / 0 / 0 / 0 |

| Player lap | Mean nearest race gap, m | Within 60 m | Cool-gallery mean gap, m | Warm-gallery mean gap, m |
| --- | ---: | ---: | ---: | ---: |
| 1, after 5 s | 165.7 | 0% | 102.3 | 184.0 |
| 2 | 276.5 | 0% | 162.9 | 271.3 |
| 3 | 386.0 | 0% | 278.4 | 344.6 |

Every individual gallery/lap combination also has 0% within 60 m. Across all laps, the cool-gallery mean is 182.7 m and the warm-gallery mean 266.4 m. The player progressively leaves the field behind; launch-only screenshots would understate the gap.

## Actual corridor-guard measurements

| Racer | Actual guard duty after 5 s | Actual guard duty, 5–42 s | Prior 24 fps inferred duty, 5–42 s | Mean speed after 5 s, km/h |
| --- | ---: | ---: | ---: | ---: |
| Player | 0% | 0% | Exempt; 32.7% counterfactual | 176.9 |
| Rival 1 | 83.9% | 81.8% | 82.2% | 139.2 |
| Rival 2 | 15.6% | 13.5% | 12.3% | 167.6 |
| Rival 3 | 70.4% | 45.6% | 45.7% | 144.1 |
| Rival 4 | 31.1% | 32.7% | 32.8% | 164.8 |
| Rival 5 | 63.6% | 37.1% | 36.4% | 146.8 |

Actual sampled guard duty closely agrees with the short-recording reconstruction in [review 001](001-baseline-diagnosis.md). The new observation is that Rivals 3 and 5 spend still more time in guard later: Rival 3 reaches 83.7%/84.9% during player laps 2/3, while Rival 5 reaches 69.0%/77.8%. Rival 1 remains above 77% in every lap. These sustained speed caps explain much of the widening field, although they do not establish the underlying steering/contact cause of remaining at the outer edge. The fields are directly observed controller outputs sampled approximately every 0.1 s, not a continuous physics-tick trace.

## Finish evidence and limits

Signed rival gaps at player finish, in rival-index order, are **−1,352.2 / −476.6 / −1,227.9 / −496.9 / −1,138.2 m**. All five rivals have completed two laps; the player has completed three and finishes first. The first-observed player lap completions occur at 38.210 / 75.408 / 112.603 s. Rival 2's first two are observed at 42.539 / 82.039 s; these sampled times are not exact crossing timestamps.

`RaceDirector` freezes the field at player finish. Unfinished rivals therefore have no observed full-race finish time, and no time-based final spread is invented here. The analysis reports their validated distance spread instead. Zero recoveries does not prove no glancing contact or good subjective racing feel. This telemetry review does not certify screenshots, close overtakes, crowded-gallery motion or manual handling.

## Candidate 01 decision and next measurement

Candidate 01 preserves acceleration and the rival safety/traffic rules. Its per-lap within-60 m fractions rise to **11.7% / 25.1% / 41.6%**, with mean nearest gaps **91.5 / 80.0 / 74.1 m**. The cool-gallery within-60 m fraction reaches 48.2%, but the warm gallery remains 0%, with mean gap 82.4 m. At player finish, signed rival gaps are **−1,085.2 / −335.8 / −974.4 / −177.1 / −845.4 m**. Independent physical gaps agree with validated gaps; nobody has finished ahead of the player.

The candidate still fails the three proposed closeness gates from review 001: ≥50% within 60 m after launch, nearest finish rival within 100 m, and at least two within 200 m. It passes the zero-recovery gate. The player averages 164.8 km/h, close to Rival 4's 163.7 and Rival 2's 160.8 km/h, but the launch separation is not recovered consistently. This motivated a 55/75 m/s native candidate; its observed failure is recorded below. Native visual inspection of an occupied passage, active-rival finish handling and manual play remain separate acceptance work.

## Candidate 02: slower player, later pack collapse

Candidate 02 [native report](../../evidence/pace-candidate-02/pace-evidence.json) and [analysis](../../evidence/pace-candidate-02/analysis.json) are complete: build GUID `087dd1ec2d2644fc8959fabaf4b15ce4`, 1,261 samples, player settings 55/75 m/s, finish 125.969 s, all recovery counts zero. There are no finished rivals before player finish; independent physical and validated race gaps agree.

| Metric | Candidate 01, 60/82 | Candidate 02, 55/75 |
| --- | ---: | ---: |
| Within 30 / 40 / 60 m after 5 s | 0% / 5.3% / 26.6% | 19.8% / 24.1% / 33.1% |
| Mean nearest gap after 5 s, m | 81.5 | 152.9 |
| Within 60 m, player laps 1 / 2 / 3 | 11.7% / 25.1% / 41.6% | 77.2% / 26.1% / 0% |
| Mean nearest gap, player laps 1 / 2 / 3, m | 91.5 / 80.0 / 74.1 | 40.5 / 88.5 / 319.0 |
| Nearest rival at player finish, m | 177.1 | 494.1 |
| Rivals within 200 m at player finish | 1 | 0 |

The aggregate within-60 fraction improves slightly, but hides the loss of all nearby rivals during lap 3. Signed gaps at finish are −881.5 / −806.6 / −984.5 / −668.6 / −494.1 m. Across all gallery passages, within-60 fractions are 65.5% cool and 28.2% warm, again including the closer early laps rather than proving sustained occupied passages. This candidate fails every proposed closeness gate.

Actual guard duties for Rivals 1–5 become **82.5% / 78.2% / 81.8% / 64.8% / 62.8%**. Rival 2 rises from 30.8% in candidate 01 to 78.2%; Rival 4 rises from 33.2% to 64.8%. Their mean speeds fall from 160.8/163.7 to 141.1/145.0 km/h. Rival 2's median lateral position is +7.71 m and Rival 4's +6.90 m; roughly 48% and 37% of their post-launch samples respectively have guard active and absolute lateral position above 8 m. Their reserved lane at those outer-edge samples is +5.8 m. Changed interaction timing can send previously competitive rivals into a persistent outer-edge state, making speed-only tuning non-monotonic.

One telemetry limitation matters: `AITargetLane` currently returns the reserved `aiLane`, not the temporary corridor target after `KeepCorrectionClear`. Thus these reports cannot tell whether an inward correction was requested and understeered, or whether adjacent-racer clearance temporarily blocked that correction. Record the final resolved target in the next run before claiming a unique steering root cause.

## Bounded steering hypothesis

The existing proportional command is `steering = clamp(angleDegrees / 26, −1, 1)`. Above 26 m/s, the subsequent yaw target is `steering × 1.12`, so its small-angle yaw gain is approximately **2.468 radians/second per radian of target-heading error**. For a no-slip pure-pursuit approximation, following a point at planar chord distance `L` calls for yaw rate `2 × forwardSpeed × sin(angle) / L`. Using the current lookahead estimate `12 + speed × 0.33` gives small-angle gains approximately **3.248 at 42 m/s** and **4.082 at 75 m/s**. The existing proportional controller therefore requires additional heading/lateral error to supply the necessary curve yaw, consistent with outward tracking bias.

Test that speed-aware yaw command using the actual planar destination chord length and nonnegative forward speed, then convert through the existing 1.12 yaw scale, preserving its steering clamp, avoidance, braking and corridor guard. Account for the existing low-speed yaw multiplier when mapping desired yaw to steering. Hover slip, yaw response delay and wall contact make pure pursuit an approximation; the algebra does not prove the native behavior will be stable. Keep the rival guard and validate zero recoveries plus actual lateral/guard duty reduction. Restore player 60/82 for this candidate because it produced the better sustained prior result. Do not accept another isolated close first lap or a frozen finished-rival race gap as evidence of success.

## Candidate 03: pursuit mapping does not close the problem

Candidate 03 [native report](../../evidence/pace-candidate-03/pace-evidence.json) and [analysis with slip/contact-geometry diagnostics](../../evidence/pace-candidate-03/analysis.json): build GUID `aea49fc2e0c84f72ae6eb820453b0b70`, 1,208 samples, complete without error, player settings restored to 60/82, player finish **120.708 s**, all six recovery counts zero. The new steering function affects every AI-driven craft, including the testing player. This is a comparison at the same player speed settings as candidate 01, not a replay of identical player inputs or rival trajectories. The report now includes the final resolved target lane.

Within 60 m after five seconds is **27.9%**, compared with 26.6% in candidate 01. Per-lap fractions are **69.7% / 15.6% / 2.5%**; nearest-gap means are **53.0 / 125.5 / 155.6 m**. The all-race mean is 113.3 m, worse than candidate 01's 81.5 m. At player finish, the nearest rival is 167.1 m behind and only one is within 200 m. Signed finish gaps are −751.5 / −611.9 / −167.1 / −914.0 / −1,021.9 m. No rival has finished, so physical and validated race gaps remain consistent. All proposed closeness gates still fail.

Actual rival guard duties are **68.6% / 58.2% / 37.5% / 73.9% / 72.0%**. Which rival remains competitive changes, but persistent outer-edge guarding remains. The pursuit formula's unit checks can establish its mathematical mapping; these native results do not establish stable pack behavior or a solved steering root cause.

### Slip hypothesis checked against native geometry

I reconstructed planar hull heading from the native quaternion, projected velocity into the track plane, and reconstructed the normal pursuit destination using the recorded final target lane. Across all guarded samples, mean absolute hull-to-travel slip is 2.25–2.90° across the five rivals. This can affect curve tracking, but it is small in the sustained outer-edge state:

| Rival | Guarded time with absolute lane >8 m, s | Mean absolute slip, degrees | Mean hull-to-target angle, degrees | Mean travel-to-target angle, degrees | Mean resolved lateral error, m |
| --- | ---: | ---: | ---: | ---: | ---: |
| 1 | 31.83 | 0.89 | 13.12 | 13.90 | 4.35 |
| 2 | 38.00 | 0.95 | 13.12 | 13.89 | 4.34 |
| 3 | 23.16 | 1.45 | 12.87 | 14.01 | 4.11 |
| 4 | 46.85 | 0.80 | 13.26 | 13.90 | 4.38 |
| 5 | 43.08 | 0.85 | 12.92 | 13.60 | 4.24 |

Median absolute slip in those outer-edge samples is only 0.18–0.37°. An approximately 13° inward target angle already exists; changing to velocity-heading pursuit would add roughly 0.7–1.1° on average in that state. The resolved target remains more than four meters inward while observed mean absolute lateral velocity is only 0.4–0.6 m/s. These data do not support travel-vector targeting as the main remedy for the sustained wall state.

The craft box collider is 5.2 × 1.2 × 7.2 m, with local center (0, 0.12, 0.1); the barrier's inner lateral face is at ±11.7 m. Projecting the oriented collider onto the local track-right axis gives mean barrier clearance of **0.064–0.104 m** during these guarded outer-edge samples. Between **82% and 94%** of that time has projected clearance below 0.15 m. This approximates the local barrier plane; it is not recorded collision/contact telemetry. Nevertheless, the geometry supports a long-box yaw lock: turning inward initially sweeps the stern outward into the wall, while forward thrust cannot move the hull inward until it turns.

The bounded next physical hypothesis is a rival-only lateral guard force toward the final clearance-approved inward target, using normal rigidbody acceleration with a small cap and lateral damping. This could create space to rotate without teleporting, changing colliders, or removing the guard. It must honor blocked corrections and adjacent-craft clearance and be validated natively for actual edge escape, contacts, recoveries and later-lap proximity. That force has not been implemented or accepted by this review. No root-cause closure is claimed for candidate 03.
