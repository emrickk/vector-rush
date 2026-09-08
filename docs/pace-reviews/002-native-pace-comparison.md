# Pace review 002: native baseline and player-speed candidate

2026-09-08. **The final 53/72 calibration sustains a nearby rival for 80.9% of post-launch racing time, compared with 0% in the baseline, with zero recoveries.** All three laps and both gallery passages retain nearby rivals in the recorded race. The nearest rival at player finish is 100.85 m behind, narrowly outside the proposed 100 m gate, and no pass is recorded. Stop further speed calibration for this bounded closer-racing objective; preserve the literal finish-gate miss and overtaking limitation rather than claiming every gate passed.

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

The bounded next physical hypothesis was a rival-only lateral guard force toward the final clearance-approved inward target, using normal rigidbody acceleration with a small cap and lateral damping. This could create space to rotate without teleporting, changing colliders, or removing the guard. It must honor blocked corrections and adjacent-craft clearance and be validated natively for actual edge escape, contacts, recoveries and later-lap proximity. Candidate 03 does not include that force. No root-cause closure is claimed for candidate 03.

## Candidate 04: physical edge assistance works; pace still needs alignment

Source review of the implemented helper found no blocker to the bounded experiment. `RivalCorridorAcceleration` returns zero for the player, an inactive guard, or a target that is not meaningfully inward; it clamps `(targetLane − currentLane) × 3 − lateralSpeed × 2` to ±6 m/s². `DriveAI` applies it with normal rigidbody acceleration after the existing collision-clear target resolution. The damping term can oppose fast inward travel, so this is lateral proportional/damping assistance, not an unconditional inward force. Existing player controls, steering clamp, corridor braking and clearance decisions remain authoritative.

Candidate 04 [native report](../../evidence/pace-candidate-04/pace-evidence.json) and [analysis](../../evidence/pace-candidate-04/analysis.json) are complete: build GUID `e095e1d456934daaafa2808f83afa405`, 1,208 samples, player 60/82, the same pursuit controller as candidate 03, finish **120.716 s**, all six recovery counts zero. The direct change from candidate 03 is the bounded rival guard force.

| Rival | Active racing time with absolute lane >8 m: candidate 03 → 04 | Longest continuous guarded outer-edge episode in 04, s | Actual guard duty in 04 |
| --- | ---: | ---: | ---: |
| 1 | 27.5% → 5.0% | 2.00 | 41.8% |
| 2 | 32.8% → 3.3% | 1.21 | 56.1% |
| 3 | 20.0% → 4.0% | 0.83 | 65.6% |
| 4 | 40.5% → 3.9% | 0.80 | 61.1% |
| 5 | 37.2% → 3.4% | 0.79 | 44.0% |

This is strong native support that the added lateral room mitigates the sustained wall state. It is not proof of zero contacts or universally solved driving: guard duty remains high, and the observation covers one deterministic native race.

Post-launch within-30/40/60 m fractions are **20.6% / 25.6% / 38.3%**, with mean nearest gap **78.4 m**. Within-60 fractions by player lap are **76.1% / 23.5% / 18.8%**. Every observed close rival is behind the player; the fraction with an unfinished rival ahead within 60 m is **0%**. The cool gallery has a rival within 60 m for 100% of sampled duration, while the warm gallery reaches only 23.7%. This supports nearby metadata in the cool passage, not a claim that a rival is visible ahead in the chase camera.

At finish, signed rival gaps are **−240.0 / −563.7 / −774.1 / −626.7 / −456.3 m**. No rival has finished first; there are no frozen-progress proximity artifacts. The nearest finish gap is 240.0 m, with no rivals within 200 m. All three proposed closeness gates still fail despite the physical improvement.

The player averages 164.2 km/h after five seconds, versus 160.4 for the fastest rival and 156.6 for the next fastest. Increasing player speeds is not justified by these measurements. Retesting player 55/75 with the now-supported guard assistance is a bounded, evidence-based next balance experiment: the earlier setting produced roughly 158.2 km/h player mean, but its previous pack failed under the older prolonged wall state. That earlier failure cannot be assumed to recur or to be fixed without the new native run. Retain the physical assistance and verify sustained proximity, actual ranks and the same safety gates before wrapping the pace work.

## Candidate 05: sustained proximity passes; nearest finish gap remains open

Candidate 05 [native report](../../evidence/pace-candidate-05/pace-evidence.json) and [analysis](../../evidence/pace-candidate-05/analysis.json): build GUID `049ee37656b14137b55efd59abaa4d9a`, 1,257 samples, complete without error, player 55/75, pursuit plus the same lateral guard assistance as candidate 04. Player finish is **125.567 s** and all six recovery counts remain zero.

| Metric | Candidate 04, 60/82 | Candidate 05, 55/75 |
| --- | ---: | ---: |
| Within 30 / 40 / 60 m after 5 s | 20.6% / 25.6% / 38.3% | 43.9% / 54.7% / 66.4% |
| Within 60 m, player laps 1 / 2 / 3 | 76.1% / 23.5% / 18.8% | 82.7% / 77.3% / 40.7% |
| Mean nearest gap after 5 s, m | 78.4 | 49.2 |
| Cool / warm gallery within 60 m | 100% / 23.7% | 100% / 57.3% |
| Nearest rival at player finish, m | 240.0 | 160.4 |
| Rivals within 200 m at player finish | 0 | 2 |

Candidate 05 passes the proposed overall within-60 m gate, two-rivals-within-200 m finish gate and zero-recovery gate. It **does not** pass the nearest-finish-rival-within-100 m gate. Per-lap nearest-gap means are 37.9 / 41.2 / 67.4 m, so the third lap still separates more than the earlier laps. Every observed close rival is behind; ahead-within-60 m remains 0%, and no observed rank change or actual pass is claimed.

Signed rival gaps at player finish are **−552.0 / −449.3 / −190.5 / −160.4 / −288.7 m**. Physical and validated gaps agree; all rivals remain unfinished. Player mean speed after five seconds is **158.0 km/h**, compared with Rival 4 at **157.8**, Rival 3 at **156.5**, and Rival 5 at **154.7**. This near-equal pace preserves much of the launch separation. A final actual-player **53/72 m/s** candidate is justified as a small calibration intended to give the leading rivals enough pace to close that remaining gap; its exact effect and ability to produce a visible pass must be observed, not assumed. No further AI architecture change is indicated by candidate 05 alone.

## Candidate 06: final bounded pace verdict

Candidate 06 [native report](../../evidence/pace-candidate-06/pace-evidence.json) and [analysis](../../evidence/pace-candidate-06/analysis.json): build GUID `2e193f7e0b434c90889e503ce4668749`, 1,285 samples, complete without error, actual-player cruise/boost **53/72 m/s**, the same pursuit and lateral guard assistance as candidate 05. Player finish is **128.320 s**. All six recovery counts are zero, and no rival finishes before the player; independently unwrapped physical progress agrees with validated race gaps.

| Metric | Original baseline | Final candidate 06 |
| --- | ---: | ---: |
| Rival within 30 m after 5 s | 0% | 56.0% |
| Rival within 40 m after 5 s | 0% | 66.4% |
| Rival within 60 m after 5 s | 0% | 80.9% |
| Rival within 100 m after 5 s | 4.0% | 98.1% |
| Mean nearest absolute race gap, m | 280.1 | 38.4 |
| Within 60 m, player laps 1 / 2 / 3 | 0% / 0% / 0% | 86.2% / 77.7% / 79.2% |
| Cool / warm gallery within 60 m | 0% / 0% | 100% / 100% |
| Nearest rival at player finish, m | 476.6 | 100.85 |
| Rivals within 200 m at player finish | 0 | 2 |
| Entire field distance spread at player finish, m | 1,352.2 | 469.2 |

The nearest-gap mean remains consistent across player laps at **35.2 / 40.4 / 39.3 m**, rather than collapsing late as earlier candidates did. The cool-gallery nearest gap averages 30.6 m, range 18.5–43.8 m; the warm-gallery gap averages 50.4 m, range 41.7–58.2 m. These are duration-weighted metadata measurements, not a claim that those rivals appear ahead in every rendered view.

Signed finish gaps for Rivals 1–5 are **−248.87 / −361.31 / −100.85 / −112.95 / −469.23 m**. The two strongest rivals average 155.4/155.5 km/h after five seconds, compared with the player's 154.6 km/h; enough pace is retained to keep the recorded race close through all three laps, although the player remains first. All close-rival observations are behind the player: ahead-within-60 m is **0%**, and an actual overtake is not demonstrated.

| Proposed gate | Final result |
| --- | --- |
| At least 50% of post-launch time with a rival within 60 m | **Pass: 80.9%** |
| Nearest rival within 100 m at player finish | **Fail narrowly: 100.85 m** |
| At least two rivals within 200 m at player finish | **Pass: two** |
| Zero player and rival recoveries | **Pass: all six zero** |

Do not round 100.85 m into a passing 100 m result or attribute that miss to unmeasured error. Equally, the arbitrary 0.85 m overrun alone does not justify another balance cycle: the intended sustained-proximity behavior is directly demonstrated, and late-lap separation is controlled. The bounded recommendation is to retain this calibration and proceed to the requested visual critique/improvement round. Keep actual visible passing, occupied-camera readability, manual-player feel and broader nondeterministic race coverage as explicit unproven limits. This review certifies neither a six-craft fighting pack nor subjective racing quality; it documents a substantial, sustained improvement over the baseline without recoveries in the measured native run.
