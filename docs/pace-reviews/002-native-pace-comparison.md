# Pace review 002: native baseline and player-speed candidate

2026-09-08. **Reducing the actual player's cruise/boost to 60/82 m/s improves racing proximity substantially, but does not meet the proposed closeness gates.** The fraction of post-launch time with a rival within 60 m rises from 0% to 26.6%; the nearest finish gap drops from 476.6 to 177.1 m. All six recovery counts remain zero. A second bounded setting, 55/75 m/s, is recommended for native validation; it is not yet accepted.

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

The candidate still fails the three proposed closeness gates from review 001: ≥50% within 60 m after launch, nearest finish rival within 100 m, and at least two within 200 m. It passes the zero-recovery gate. The player now averages 164.8 km/h, close to Rival 4's 163.7 and Rival 2's 160.8 km/h, but the launch separation is not recovered consistently. Try actual-player **55/75 m/s** next, preserving the same safety behavior, and measure again rather than accepting the improvement alone. Native visual inspection of an occupied passage, active-rival finish handling and manual play remain separate acceptance work.
