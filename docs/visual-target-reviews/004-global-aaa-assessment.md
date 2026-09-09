# Global assessment after the opening04 rejection

Date: 2026-09-09. Scope: critique and production diagnosis, with no new game implementation.

**Verdict: Vector Rush is a functioning prototype with some resolved presentation elements. The current native images remain substantially below the intended AAA visual standard; the complete racing experience has not been demonstrated at that standard either. The opening04 work is not a successful global visual milestone.**

Here, AAA refers to the quality ambition in the project's brief, not a certification or a numerical score. WipEout Omega Collection is the stated reference. Its [official announcement](https://blog.playstation.com/2016/12/03/wipeout-omega-collection-revealed-for-ps4/) describes the combined track/ship content of three games, nine modes, multiplayer and a soundtrack; its [release announcement](https://blog.playstation.com/2017/03/30/wipeout-omega-collection-coming-to-ps4-on-june-6th/) also specifies remastered effects and a 60 fps target. Matching that product's breadth is a different undertaking from making one convincing racing slice. Adding more content is not the proposed remedy here.

## Findings, in order of visible impact

### 1. The environment is still at blockout quality in too many dominant areas

In opening04's [opening](../../evidence/nocturne-v2/opening-04/draft/combined/selected/opening01.png), [descent](../../evidence/nocturne-v2/opening-04/draft/combined/selected/opening03.png), [thermal passage](../../evidence/nocturne-v2/opening-04/draft/combined/selected/thermal01.png), and [station](../../evidence/nocturne-v2/opening-04/draft/combined/selected/station.png), the road, barriers, towers and landmark bodies occupy substantial screen space but remain broad simple planes, boxes and cylinders. Window grids supply much of the building identity. There are authored models and deliberate placement rules in the source, but their presence does not establish a finished architectural image.

The missing layer is largely between silhouette and tiny detail: credible wall thickness, facade depth, structural transitions, designed bases and roofs, track-edge assemblies, and material boundaries that remain readable from the racing camera. More bolts or more repeated towers would leave the main problem intact. The new frontage improves a small peripheral region while the dominant image stays similar.

### 2. Lighting, materials and atmosphere do not yet form a convincing whole

The scene has a coherent blue/amber palette, but emissive rails and fixtures carry much of its visual identity. Large exterior surfaces sit in narrow dark ranges; the sky has little structure, and the space beneath the track often becomes a black void. The [gallery](../../evidence/nocturne-v2/opening-04/draft/combined/selected/warm-gallery.png) is more legible, but repeated soft pools on broad wall panels and weak construction shading expose its simplicity.

This is a joint asset-and-rendering problem. A roughness map cannot create the architectural forms or environmental light information that are absent, and better geometry can remain ineffective under the current lighting. SSR's rejected result does not prove that the engine cannot achieve the target; it proves that this experiment did not provide useful visual gain. No isolated rendering feature has earned the status of a global solution.

### 3. The circuit lacks enough visual differentiation as a journey

There are real bends, grades, banking, galleries and landmarks. Nevertheless, most sampled outdoor sections share the same elevated ribbon, cyan edge, overhead lamp rhythm and distant window towers. The industrial cylinders change an object beside the road more than they change the space through which the player races. The gallery is the strongest departure because enclosure and illumination change together.

The next design needs a deliberate sequence of anticipation, compression, reveal and release, connected to driving decisions. This does not imply filling every view with objects or requiring a new circuit. The current route may support much stronger framing and environmental treatment; its limitations need to be tested rather than permanently exempted from discussion.

### 4. Quality is uneven across the frame

The readable HUD and recognizable craft are ahead of the world, making the world's simplification more conspicuous. The craft still has broad, simply shaded panels and stylized exhaust, so it is not finished either. However, another isolated craft remodel or HUD pass has lower global value than bringing road, barriers, nearby architecture and lighting to a consistent level. A stronger hero object cannot carry an otherwise unfinished scene.

### 5. A convincing race is a separate outstanding requirement

The source implements steering, braking, boost, opponents, laps and results. Their existence is not evidence of rewarding mastery or competition. The current selected captures show a largely empty road ahead; they cannot establish the experience of traffic, overtaking, braking commitment, recovery or sustained pressure. Older automated telemetry is historical diagnostic evidence, not a current-build verdict on difficulty.

The current `RaceAudio.cs` synthesizes player engine, wind, boost and event signals, sets its sources to non-spatial, and drives motor/turbine pitch and level principally from speed. The inspected presentation code does not supply an authored music system or spatial rival pass-bys. Synthesis itself is not a quality defect; the limited reactive and spatial coverage is the concrete gap. No fresh continuous audiovisual review or human handling session occurred in this critique, so sound quality, responsiveness and speed sensation remain unjudged.

### 6. This is still prototype production, not final production

A complete three-lap loop is a useful foundation. Whole-product finish also involves meaningful replay motivation, persistent performance comparison, predictable controls, accessibility/settings, robust state transitions and stable delivery on declared hardware. These should be scoped to one excellent race, not used to justify adding tracks, weapons or multiplayer. Current opening04 still validation is not a current full-race performance test. There is no new performance result from this review.

## Why the method failed

1. **The benchmark shifted from the desired game to the previous build.** A small improvement over a weak baseline was allowed to consume a full milestone without demonstrating that it closed the target gap.
2. **We interpreted the scope too narrowly for the promised result.** The opening plan fixed route, camera, fog, postprocessing, craft and much of the surrounding construction, while allowing only a few modules and local surface changes. Those controls were useful for attribution and did not make a much better image impossible. We treated diagnostic isolation as the production strategy, without first proving an integrated treatment across the remaining variables. This was our scoping decision, not a failure of the owner’s constraints.
3. **We decomposed a coupled visual problem too early.** Construction, materials and light were assigned as separate small fixes before a convincing integrated section existed. Technically correct parts did not produce a convincing whole.
4. **Verification displaced evaluation.** Hashes, shader retention, tests and repeated captures establish integrity and correctness. They cannot establish visual impact, entertainment or taste. The independent critics did report failures; integration kept refining the same direction instead of changing the underlying proposal soon enough.
5. **Generated targets supplied appearance without a proven production recipe.** They remain useful references, but invented light transport, detail and geometry cannot be converted into a few shader settings. The native asset and lighting pipeline had not demonstrated equivalent behavior on a finished section.
6. **My immediate response to the user's rejection was also too simple.** The generated opening and gallery targets retain broadly similar compositions yet look much richer. Therefore “change composition” alone is not a sufficient diagnosis. Similarly, one impressive still would be an early visual probe, not the next proof of whole-game quality.

## Resulting recommendation

Stop the opening04 promotion/rollout path. Preserve it as a small local construction experiment, without describing it as a major step toward AAA.

The next proposal should be an integrated 15–20-second racing section: a clear spatial idea, production-quality nearby assets, coherent surface/light response, readable opponents and synchronized feedback. Rough composition should earn further asset work; finished native stills should earn continuous motion review; the continuous audible race should earn broader rollout. These are proposed gates, not work completed or a new implementation authorization.

Keep the functioning race framework, current craft identity and readable HUD as reusable foundations. Reopen environmental composition and lighting assumptions where necessary, and retain the existing track only to the extent it supports the intended experience. The realistic nearer goal is one exceptional vertical slice. No evidence supports predicting how many further iterations would make the whole project AAA.

## Evidence and independent checks

Parent directly inspected all nine current combined native views, the earlier displayed opening02 view, generated targets A/B, current track/camera/audio/world source, and prior plans and critique history. The independent visual critic reviewed all eighteen current off/combined originals. A separate source critic reviews racing and production coverage in [004a](004a-global-gameplay-production-critique.md); the independent visual findings are retained in [004](004-global-aaa-independent-critique.md).

Stills are original native captures, with natural pose differences between runs. Generated targets are explicitly not game evidence. No game source, build, play session, audio audition or new performance measurement was produced during this critique.
