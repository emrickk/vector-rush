# Global gameplay and production critique

**Vector Rush implements a functioning race prototype, but its complete player experience is substantially less developed than its presentation work.** The largest supported gaps are competition that has not earned a human quality verdict, a limited audiovisual feedback model, a replay loop that discards useful performance context, and incomplete player-facing configuration. Increasing asset count would not establish quality in these systems. A single excellent three-lap race remains a sufficient target.

## Six supported findings

### 1. Race correctness is ahead of demonstrated race quality

The current source has real strengths: ordered lap gates, valid-track checks, recovery protection, interpolated finish order, curvature-aware AI braking, traffic-aware lane changes and bounded wall-clearance assistance. These support a functioning contest. They do not establish satisfying manual control, credible pressure or interesting overtaking.

The current balancing arrangement is asymmetric: the player uses 53/72 m/s cruise/boost, while the default rival values are 78/108 m/s; rivals also have an explicitly non-player corridor-assist force. The AI selects among three target lanes, follows local speed/traffic rules and has a fixed boost latch. This is a workable prototype model, but its fairness and skill curve require human evidence. Those constants alone do not prove the race is unfair or easy. [Bootstrap](../../UnityProject/Assets/Scripts/VectorBootstrap.cs:71), [vehicle/AI](../../UnityProject/Assets/Scripts/Gameplay/HoverVehicle.cs:203).

The later historical close-race record improves on the old critique: it reports sustained proximity and zero recoveries, while explicitly reporting no demonstrated overtake. It is not a measurement of the opening04 build. RaceDirector still freezes the field when the player finishes, so that path does not establish the remaining rivals' independent finish times. **“Rivals stay near the player” is a weaker outcome than a convincing race the player wants to master.** [Historical scope](../../evidence/close-race-final/release-notes.md), [finish behavior](../../UnityProject/Assets/Scripts/Gameplay/RaceDirector.cs:137).

### 2. Audio and visual feedback do not yet describe one coherent vehicle and racing space

All RaceAudio sources are nonspatial, with Doppler disabled; the system follows the player and attaches its impact listener to the player. There is no rival-position-driven pass-by layer or environment-dependent gallery/exterior response in this implementation. Motor/turbine pitch and level follow speed, while the visible exhaust follows throttle. Coasting therefore leaves the motor model driven by speed after the exhaust demand has fallen. Wind following speed is appropriate; propulsion-load feedback needs a different relationship. [Audio](../../UnityProject/Assets/Scripts/Presentation/RaceAudio.cs:46), [speed-driven mixing](../../UnityProject/Assets/Scripts/Presentation/RaceAudio.cs:69), [throttle-driven exhaust](../../UnityProject/Assets/Scripts/Presentation/IonPropulsion.cs:22).

Impact sounds, sparks, suspension lights, boost transitions and camera response are implemented. Their existence does not prove their timing, weight, mix, fatigue or opponent-awareness value. The concrete shortfall is a limited event/space model, not the mere use of synthesized sound. I have not listened to this candidate and cannot claim its audible mix is bad.

### 3. The circuit has geometry and scenery, but its driving identity is weakly demonstrated

The course is a constant-width 22 m spline with fourteen authored knots, elevation and derived banking. The road and its control law are broadly uniform; boost drains and regenerates continuously at fixed rates. That can support a demanding racer, but the available evidence does not explain which corner sequences reward different commitments, where a skilled driver earns an overtake, or why saving boost changes a later decision. Environmental landmarks make locations distinct visually; distinct driving decisions are a separate question. [Track](../../UnityProject/Assets/Scripts/World/TrackPath.cs:18), [boost/handling](../../UnityProject/Assets/Scripts/Gameplay/HoverVehicle.cs:178).

This is an inference about unproven design depth, not proof that constant width or a single circuit is inherently dull. The old decorative boost-strip complaint is obsolete: current WorldBuilder uses non-emissive painted `DirectionMarkings`. No additional tracks, weapons or multiplayer are prerequisites to resolving this gap. [Current markings](../../UnityProject/Assets/Scripts/World/WorldBuilder.cs:168).

### 4. The replay loop discards the player's most useful improvement context

The results screen shows final position, total time and best lap, then offers another race or quit. `StartRace` resets `BestLap`, `LastLap` and race time. No persistent personal-best or race-history path appears in the inspected gameplay/presentation source. A player can repeat the activity, but the game retains little context for judging whether that repetition was better or where time was lost. This is a direct product-loop limitation inside the existing race, not a request for a campaign or reward economy. [Reset](../../UnityProject/Assets/Scripts/Gameplay/RaceDirector.cs:58), [results](../../UnityProject/Assets/Scripts/Presentation/RaceHUD.cs:395).

### 5. The player-facing wrapper remains prototype-level

The menu/pause flow exposes a camera-shake toggle and fixed control instructions. Driving bindings are hard-coded; the inspected source supplies no remapping, separate audio controls, sensitivity settings or persistent user preferences. Basic gamepad driving/start/restart/pause actions exist, but no explicit controller menu-focus/navigation implementation covers all visible choices. That is materially different from a verified controller-only product flow. [Input](../../UnityProject/Assets/Scripts/Gameplay/HoverVehicle.cs:83), [menu/pause/options](../../UnityProject/Assets/Scripts/Presentation/RaceHUD.cs:206), [controls](../../UnityProject/Assets/Scripts/Presentation/RaceHUD.cs:421).

These are usability/comfort and completeness gaps. The use of IMGUI itself is not evidence of poor quality, and I did not perform a new physical-controller session.

### 6. The evidence pipeline is much stronger at proving implementation than experience

The latest opening04 artifacts reviewed contain a successful separate build and nine native diagnostic stills per mode with 1,440 pose records. They explicitly do not record motion or sound and run automated steering with `Time.captureFramerate=24`. This cannot establish responsiveness, pacing over three laps, audiovisual coherence, manual recovery, physical-controller comfort or real-time frame delivery. The 42 passing EditMode tests are meaningful correctness evidence; they do not measure those experiences. [Capture scope](../../UnityProject/Assets/Scripts/EnvironmentEvidence.cs:56), [opening04 build](../../evidence/nocturne-v2/opening-04/build-validation.json), [test scope](../../evidence/nocturne-v2/opening-02/test-validation.json).

Current matching has improved and should be described accurately: opening04 combined passes the supplied exact-pose tolerance at eight of nine anchors; station fails because of approximately 0.294 degrees of camera rotation. Construction-only fails that tolerance at all nine, including approximately 2.66 m displacement at station. This allows broad composition judgments with stated limits; it does not support treating every small edge/pool difference as an isolated art effect. Neither file counts nor rendered keyword logs are visual acceptance. [Combined manifest](../../evidence/nocturne-v2/opening-04/draft/combined/capture-validation.json), [construction manifest](../../evidence/nocturne-v2/opening-04/draft/construction/capture-validation.json).

## Evidence absent from this current assessment

- A recent sustained human keyboard and physical-controller assessment of the actual candidate, including deliberate airbraking, boost tradeoffs, side-by-side passing and recoverable mistakes.
- An independently heard full race demonstrating the combined propulsion, wind, boost, contact, race-state and opponent-awareness feedback.
- Independently watched continuous candidate motion through crowded racing, region transitions, pause/finish/restart and adverse recovery cases.
- Exact-candidate isolated real-time frame-delivery evidence for opening04. Historical close-race or earlier night-production timing must not be relabeled as its measured performance.
- Evidence that repeated human attempts produce understandable improvement and voluntary replay. Automated proximity and completion cannot answer that question.

## Methodology problems

The project repeatedly treats a rendering or source hypothesis as the next visible improvement before the whole-race quality question is established. The recent shader-retention failures show the practical danger: an implementation can compile, and one material can pass its guard, while the comparison's baseline loses its intended features. Source checks are necessary, but independent native output remains necessary too. Similarly, a better passage still does not establish a stronger race.

The retrospective must also distinguish versions. I formed the source findings above before reading `current-whole-game-critic-2026-09-08.md`. Its early quarter-lap gap, decorative BoostStrip finding, 32-test count and old frame-time numbers are superseded or belong to older candidates; I have not recycled them as current defects. The later close-race result is acknowledged as historical improvement, with its own stated limits. The original road has now been restored, so rejected regional-atlas behavior is also not automatically a defect of current opening04 source.

This critique reads current source at `54c5347` and available evidence manifests/reports. It does not make a new visual judgment of the latest PNGs. No Unity execution, implementation, test rerun, human play, audio audition, continuous-video inspection or commit was performed. A separate visual critique should determine how far the latest complete frames still fall short; this report establishes why local art progress alone cannot justify a whole-game AAA-quality claim.
