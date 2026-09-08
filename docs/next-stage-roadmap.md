# Next path: prove one excellent racing section

Date: 2026-09-08. Baseline: `14b1212`, the packaged HUD milestone. This is a critique and planning checkpoint; no new gameplay or art is implemented here.

## Verdict

The current build is a coherent, playable prototype. It is substantially below the requested AAA presentation and complete-race target. The strongest parts are the corrected craft, consistent color identity, readable HUD and functioning race flow. They sit inside a visibly procedural environment, with competition and human control feel still insufficiently demonstrated.

The next investment should make a short piece of the game convincing as a race. Another ship remodel, another HUD reskin, more repeated buildings or stronger bloom would have limited value before that gate is met.

## Harsh critique, tied to evidence

1. **The city is the largest visible quality gap.** Current racing views show repeated window patterns on mostly dark block masses, little architectural differentiation, and limited near/middle/far visual layering. They communicate a generated skyline rather than a memorable district. Elevated track construction and adjacent urban spaces are sparse. Night lighting reduces how much is visible; it does not supply the missing design.
2. **The track has too little character at racing distance.** Long, broad deck surfaces and continuous cyan rails carry most of the composition. Those rails help navigation, but the route needs authored changes in enclosure, landmarks, structural scale and light rhythm. The gallery is the strongest starting point, yet repeated bays, dark ceiling and diagonal road sheen remain obvious.
3. **There is more evidence of a reliable lap than a competitive race.** In the recorded 112.60-second finish, opponents had covered only 2.27–2.74 laps while the player had covered three. That particular run is not a compelling demonstration of pressure or overtaking. All racers freeze at the player's finish, so the missing opponent finish times are unknown. Zero recoveries is valuable, but it is an insufficient race-quality gate.
4. **A safety correction may have made the opponents too cautious.** The outer-corridor guard can cap rivals at 42 m/s and suppress boost; the player/testing autopilot is exempt. Existing runs do not isolate causation. Measure how long that guard engages and how much time is lost before changing it or adding broad speed bonuses.
5. **Feedback is incomplete across senses.** Throttle now drives the visible exhaust. Source inspection shows motor/turbine pitch and level are still principally speed-driven; the current audio layer uses non-spatial sources and supplies no rival pass-by system. These are confirmed implementation limits, not a claim that the unheard mix sounds bad. The current exported preview is silent and cannot establish sound quality or input feel.
6. **Some scenery promises an interaction it does not deliver.** Seven luminous chevron groups are built by `WorldBuilder.BoostStrip` as decorative meshes; no corresponding track boost mechanic is implemented. Their boost-pad reading is a visual inference, but the decorative implementation is confirmed. Replace that treatment with unambiguous route markings for the present scope rather than casually adding a new boost system.
7. **The HUD is now adequate to freeze.** Its layout, map, typography and state hierarchy work in the inspected conditions. Small label/background interference, low-energy coverage and further motion refinement remain. They are lower priorities than the city and racing. More decorative instrumentation would not fix either.
8. **Polish is not yet proven over a complete human-driven race.** The present record is strongest for automated physics, isolated input sequences and still-image layout. Continuous audiovisual review, physical-controller feel and current-build performance need explicit acceptance. Previous frame-time measurements predate the new HUD.

Sources: [final current still review](hud-reviews/004-final-timer-and-contrast.md), [current gallery frame](../evidence/hud-motion-01/selected-0200.png), [current boosted exterior](../evidence/hud-motion-01/selected-0240.png), [full-race evidence](../evidence/night-v4-race-01/), and [current HUD capture scope](../evidence/hud-native-02/hud-evidence.json). [Independent whole-game critique](reviews/current-whole-game-critic-2026-09-08.md) is retained separately.

## Milestone 1 — a credible race and responsive craft

**First bounded implementation step: measure the opponent pace loss.** Add an opt-in analysis run recording per-racer checkpoint times, active outer-corridor guard duration, speed cap duration, blocked-following time, contact events, recoveries and position changes. Separate solo pace from six-craft traffic. Allow an evidence mode to observe every craft finish without awarding false progress or changing normal presentation rules. Preserve the current version as the control.

Then tune only the demonstrated causes: safe guard release/re-entry, target line, corner speed and passing/following decisions. Do not hide the problem with position-dependent catch-up acceleration. Keep the player's physics unchanged while isolating opponent changes.

Correct the decorative boost-pad affordance in this milestone: keep directional road markings visually distinct from actual boost feedback.

In parallel, design a bounded feedback pass: throttle/load-responsive motor layer, retained speed-driven wind, spatial rival approach/pass-by, clear boost onset/release and distinct impact feedback. Compare the same accelerate/coast/brake/boost sequence before and after, with audible native capture. Tune camera and handling only against a recorded manual baseline; shake remains optional.

**Proposed acceptance targets (targets, not measured results):**

- Ten reproducible race starts spanning several grid/line arrangements; every racer completes all required ordered checkpoints and finishes in every evidence run. No unresolved wall stop over two seconds. Log recovery use separately rather than hiding it in a completion flag.
- In the standard benchmark, at least two opponents finish within five percent of the reference driver's time in at least eight of ten runs. This is an initial tuning target, not proof of fun or a demand that a human always wins or loses.
- Test passing and side-by-side cornering explicitly; position-change logs exclude start-grid sorting, recoveries and lap-count transitions. Review actual exchanges for contact, fairness and readability instead of optimizing an arbitrary overtake count.
- A continuous human-driven race with audible throttle, lift-off, boost, passing and impacts. Record the driver's ability to predict turn-in, braking and boost, and any loss of control or discomfort. Automated steering cannot sign off this part.

Stop after the bounded correction if the evidence does not improve. Diagnose the remaining loss before widening the physics changes.

## Milestone 2 — one authored 10–15-second visual benchmark

Use the existing final-sector turn, warm gallery and exit straight. A lap is approximately 37 seconds in the automated benchmark, so a 10–15-second section is already a substantial sample; a 20–30-second rebuild would cover most of the circuit. Keep the course centerline and driving behavior fixed during the art comparison so the race work remains testable.

Author a distinct entry landmark, a constructed gallery with readable ceiling/supports, and an exit skyline reveal. Build three depth layers from the race camera: close trackside structure, a believable adjoining district, and a quiet distant skyline. Use a small, deliberate set of architectural families with differences in silhouette, facade construction and lighting. More random windows are not an art direction.

Resolve the road sheen through controlled material/mesh/light experiments, retaining failed hypotheses. Add scale through curbs, joints, barriers, service details and supported lighting with consistent dimensions. Concentrate detail where it changes the racing view. Preserve the current craft silhouette and HUD.

**Acceptance:** capture the exact entry/middle/exit camera poses plus an uninterrupted native passage with sound. The section must be identifiable without reading its sign, hold near/middle/far separation, make the next turn obvious, keep opponents and the craft readable, and show coherent surfaces under both warm and cool lighting. Review the road pattern and distant aliasing continuously. A hero still alone does not pass. Independent critique may reject the section; make at most two substantive corrections before reassessing the approach.

## Milestone 3 — carry the standard through one complete race

Expand the accepted visual language around the existing circuit with three distinct landmark sequences and deliberate quiet intervals. Preserve a personal best across restarts and show a useful comparison in results so another attempt has a clear target. Match race-start, lap, opponent pass and finish feedback to the same identity. Add information to the HUD only when manual testing demonstrates a driving need; measure it correctly before displaying it.

Complete a continuous native audiovisual review from menu to finish. Recheck low/empty boost, bright-background text, pause/restart, collision/recovery and physical controller operation. Profile the exact delivered build at 1080p on the M2 Max after warm-up, report frame-time percentiles and bottlenecks, and compare with the pre-HUD baseline. Keep simulation-time video and real-time performance evidence separate. Package only after the relevant regressions and a full manual race are assessed.

## Scope and ownership

Retain one circuit, one player-craft form and five rivals. Defer additional tracks, ships, weapons, multiplayer, weather systems and another HUD redesign. None is needed to establish the next quality gate.

Parent owns the benchmark, shared integration and camera contracts. Gameplay owns pace instrumentation and rival behavior. Environment author owns only the nominated section/assets. Audio/presentation work runs after the gameplay owner or under explicitly non-overlapping file ownership. Independent reviewer owns critique and may reject the result. Define ownership before each implementation step; preserve every accepted or rejected comparison and corrective commit.

The immediate next action is the opponent pace diagnosis. The near-term deliverable is a convincing short race section with real competition, responsive audiovisual feedback and authored scenery. Do not label that result AAA until the broader quality claim is supported.
