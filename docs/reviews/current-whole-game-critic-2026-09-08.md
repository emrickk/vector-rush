# Whole-game critique: 2026-09-08

Independent reviewer: Astra. **The AAA target is still far off. Vector Rush now has a coherent playable prototype, but its racing experience remains insufficiently proven and its environment still looks assembled from repeated modules.** The HUD is more resolved than the game surrounding it. Further work on its typography or the ship's minor contours has low priority.

The useful target is one exceptional three-lap race: rewarding control, credible opponents, a memorable circuit, convincing sound and reliable frame delivery. That is consistent with the [original vertical-slice brief](../GPT6-BUILD-PROMPT.md). More circuits, vehicles, weapons or multiplayer would expand the problem before this race earns the player's next attempt.

## Biggest gaps, ranked by development priority

### 1. Competition has weak evidence, with a substantial pacing problem in the recorded race

The [raw finish telemetry](../../evidence/night-v4-race-01/live-telemetry.txt) records the automated player at 3.0001 laps. The nearest rival is at 2.7417, a gap of 0.2584 lap; the farthest is at 2.2656, a gap of 0.7345 lap. Clearing the previously repeated wall stalls was necessary, but a quarter-lap lead over the closest rival does not demonstrate a convincing contest.

This is one automated run, so it does not establish human difficulty. The [AI implementation](../../UnityProject/Assets/Scripts/Gameplay/HoverVehicle.cs) also applies its corridor guard only to rivals, excluding the testing player. That asymmetry is a diagnostic lead, not proof of the entire gap's cause. [RaceDirector](../../UnityProject/Assets/Scripts/Gameplay/RaceDirector.cs) freezes everyone when the player finishes, so this run never establishes each opponent's independent completion.

**The next race needs evidence of pursuit, clean overtakes, mistakes with recoverable consequences and a reason to improve the next lap.** No sustained human handling assessment supports those claims yet. A stable automated lap proves that the system can navigate the course; it cannot tell us whether steering is satisfying or airbraking rewards skill.

### 2. The circuit communicates more gameplay than it currently implements

The luminous chevrons in [the lap-two view](../../evidence/hud-motion-01/selected-0300.png) look like an interactive boost surface. That expectation is a design inference. Their implementation is definite: [WorldBuilder.BoostStrip](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs) places seven groups of collider-free boxes; the inspected gameplay code has no track-triggered boost response. They are decorative.

That is a poor relationship between appearance and consequence. A player should learn whether a road marking changes their racing decision. The next pass should give these surfaces a real, clearly communicated role, or make their appearance unmistakably directional.

The [track source](../../UnityProject/Assets/Scripts/World/TrackPath.cs) supplies a constant-width course with curvature, banking and elevation. Those ingredients are present. The current evidence does not establish a strong sequence of corner commitments, overtaking opportunities and boost tradeoffs. Authored race decisions deserve more attention than adding another neon treatment to the same ribbon.

### 3. The environment is the largest visible quality gap

The [exterior](../../evidence/hud-motion-01/selected-0105.png), [gallery](../../evidence/hud-motion-01/selected-0200.png) and [boost straight](../../evidence/hud-motion-01/selected-0240.png) retain obvious construction repetition: sharp grids of windows on dark tower masses, similar skyline profiles, repeated light bars and broad dark structural planes. There is density, but limited architectural identity or clear separation between near, middle and distant layers.

The gallery has the strongest visual premise. Its warm enclosure changes the composition and makes the ship readable. However, black ceiling areas still swallow construction, repeated bays announce the kit, and diagonal road-sheen boundaries remain visible. The [road diagnosis](../road-dampness-diagnosis.md) correctly calls the last adjustment a contrast reduction, not a repaired root cause.

The craft now reads as a connected vehicle with legible armor, dark structure and propulsion. Broad panels and simple collars still limit close inspection, but rebuilding the silhouette again would miss the larger problem. The current scene needs one deliberately composed place that can be recognized without its title text.

### 4. Sound and speed feedback remain underdeveloped or unverified

The current preview is silent. I have not listened to the native game's audio or continuously watched the clip, so I cannot honestly call its sound bad, its motion smooth or its speed sensation exciting.

There is still a concrete implementation gap. [RaceAudio](../../UnityProject/Assets/Scripts/Presentation/RaceAudio.cs) contains synthesized player motor, turbine, wind, boost and event signals. Motor/turbine pitch and level derive from speed rather than throttle load. The visible exhaust now responds to throttle, so acceleration and coasting need a joined audiovisual review. The inspected audio implementation also supplies no spatial rival pass-by or authored music layer. That limits the available feedback vocabulary; it does not establish the audible mix's quality.

A fast number, a wider field of view and blue exhaust are only components of speed communication. Their combined timing, impact feedback, opponent awareness and fatigue over a full race remain open questions.

### 5. The race wrapper and production proof remain thin

The [menu](../../evidence/hud-native-02/01-menu-1920x1080.png) and [results](../../evidence/hud-native-02/07-results-1920x1080.png) are clear and consistent. The result currently gives a position, total time and best lap, then another race or quit. Best lap resets on every restart in the director. There is little persistent performance context to make that next attempt purposeful. A stored personal best and useful race comparison would serve the existing loop better than a larger menu.

The [32 passing tests](../../evidence/editmode-results.xml), [native race/restart checks](../../evidence/night-v4-race-01/race-verification.txt) and [pointer check](../../evidence/hud-input-01/scope.md) are real strengths. They cover correctness, not overall experience. Physical controller hardware, sustained human driving, low-energy HUD presentation and results-screen pointer activation remain outside the current native evidence.

The last separate [performance sample](../../evidence/night-v4-race-01/runtime-metrics.txt) measured 3,529 frame intervals at 1080p on M2 Max: P95 20.60 ms and P99 25.34 ms. Those are not locked-60 results, and the sample predates the final presentation changes. The current 24-fps simulation recording does not update that measurement.

## Recommended next three milestones

These are proposed acceptance gates for the project, not industry standards or measured results. Keep the existing craft, HUD direction and single-course scope while testing them.

### Milestone 1: prove a rewarding, contested race

Diagnose where rivals lose time, evaluate manual steering/braking/boost, and resolve the decorative boost-surface promise. Instrument before tuning; increasing every rival's top speed would obscure whether time is lost to traffic, cornering or repeated safety intervention.

**Pass criteria:** Ten varied-grid diagnostic races let all six racers complete independently, with no unrecovered wall stall or invalid progress. On a declared standard difficulty, at least two rivals' median clean race times fall within 5% of an agreed practiced-human reference. Human play shows legal overtaking and recoverable mistakes through ordinary physics. Three short human sessions, collectively covering keyboard and a physical controller, demonstrate intentional braking/boost decisions and repeatable improvement; these sessions are qualitative design probes, not population statistics. Every apparent boost surface has an observable, explained function, with inside/outside passes checked in the native game.

**Stop condition:** If the race remains uninteresting after the pacing/handling changes, revise one corner or boost decision before commissioning more environment art.

### Milestone 2: author one signature track passage

Build one approximately 10-to-15-second entry, gallery and exit sequence around a clear driving decision. Concentrate on a distinctive nearby structure, readable structural depth, quieter distant architecture, controlled warm/cool transitions and a road surface whose highlights have a plausible cause. This is one section, not a rebuild of the entire city.

**Pass criteria:** Fixed native chase views distinguish foreground, middle distance and background. The passage remains identifiable without its signs; the driving line and nearby opponents stay clear. The diagonal sheen is either corrected through an isolated diagnosis or replaced by a deliberately stable surface finish. An independently watched continuous native traversal confirms no distracting shimmer, disappearing light response or camera obstruction. The section also passes a crowded traversal at its intended racing speed.

**Stop condition:** A better still with unstable motion or unreadable opponents fails the section gate. Review the cause before extending the kit elsewhere.

### Milestone 3: finish and prove the complete race experience

Join engine load, wind, boost, impacts, rival proximity and race signals into an audible experience; add a coherent original music direction if it supports the race. Give results useful comparison and a persistent personal target. Close the remaining controller, low-energy, transition and performance gaps on the same candidate.

**Pass criteria:** Independently play and watch a full three-lap native race with audible sound, including a crowded start, pass, wall contact, low-energy event, pause/resume, finish and restart. Validate physical controller operation and results controls. Re-measure the exact candidate at 1080p on M2 Max across representative full-race samples; proposed frame-delivery gate is P95 at or below 17.5 ms and P99 at or below 20 ms, with every larger spike explained rather than hidden by an average. Three short play sessions produce clear accounts of how to improve the next attempt, with at least two players choosing another race when offered a stopping point. Treat that as an early design signal, not proof of broad appeal.

## Evidence and limits

This critique directly assesses the four current 1080p HUD-motion selections and the native menu/results images. It also reads current source, README, raw race/metrics/test evidence and the preserved ship/HUD review history. Visual judgments apply to those views; source findings describe implementation, and future improvements are explicit bets.

There was no new build, source change, human play session, audio audition or continuous-video inspection during this review. Earlier approvals remain valid for their bounded model/import/HUD milestones. They never established whole-game AAA quality. The recommended priority is to earn the racing experience first, then improve its strongest visual passage, then validate the complete slice.
