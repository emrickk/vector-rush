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

## Active order — user correction, 2026-09-08

The user explicitly prioritizes **visuals and feeling first**, targeting the generated-looking city and underdeveloped track. The former AI-first order is superseded. Follow [the detailed visual environment plan](visual-environment-plan.md).

1. Lock five native race-camera views and compose the final-sector/gallery/exit passage.
2. Build a layered, authored district and convincing track/gallery construction; diagnose the road sheen in an isolated parallel experiment.
3. Integrate surface lighting, atmospheric depth and speed cues; independently review a continuous 10–15-second passage.
4. Extend the accepted language across the existing circuit and verify a full native lap plus current-build performance.

Keep the current ship, HUD, driving physics and AI as stable references for this stage. Opponent pace diagnosis, broader handling work and race-loop improvements remain valid deferred work; they do not block the visual milestone.

## Deferred race-quality work

After the visual milestone, revisit solo-versus-traffic opponent pace, safety-guard duty and independent opponent finishes; throttle/load audio, spatial rival feedback and human handling; persistent personal-best comparison and full controller coverage. Use the independent critique for the original evidence, and define fresh bounded implementation steps when this work resumes.

The immediate next action is the visual benchmark and composition plan. No gameplay or environment implementation has begun in this planning revision.
