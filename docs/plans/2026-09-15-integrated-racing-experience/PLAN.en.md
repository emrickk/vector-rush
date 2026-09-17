# Vector Rush: Integrated racing experience rebuild plan

2026-09-15. Status: planning and four role-specific handoffs complete. The user will authorize execution in new tasks. This work changes only planning and handoff documentation.

## Objective

Establish a coherent, distinctive cyberpunk anti-gravity racing experience: a dense, varied city surrounds the track; smooth bends convey banking and speed; neon affects surrounding materials; acceleration and collisions produce full, fast, layered ship feedback; and typography and UI motion share the same racing identity.

The first finished delivery is an approximately 30-second complete, playable exemplar covering cruise entry, acceleration, boost, a banked turn, release, and recovery. If collisions do not occur naturally during that drive, provide a separately labeled demonstration of real contact rather than forcing an accident into the main sequence. This validates the whole game's quality standard; it does not reduce the final objective to one attractive camera shot.

## Root causes and production principles

1. **Insufficient shared art direction.** Define common rules for building silhouettes, materials, neon, the ship, typography, and motion before producing content.
2. **Insufficient asset variety, construction, and layout.** Explicitly retain, rework, replace, or demote weak assets to background use. Do not assume existing buildings merely need more windows.
3. **Iteration dominated by still images.** Judge the scene through continuous driving from the normal camera. Stills diagnose issues; they cannot establish acceptance by themselves.
4. **Event feedback lacks shared timing.** Drive acceleration, boost, wall grazing, heavy impacts, and rank changes from existing real events, with coordinated onset, peak, and decay.
5. **Technical completion has substituted for quality judgment.** Builds, tests, and performance are delivery requirements. A clear whole-image improvement toward the target is a separate required judgment.

Rework the foundations of presentation while retaining reusable gameplay logic and reliable tools. Add no tracks, items, shield mechanics, transformation ability, progression, or other gameplay. Include existing sound in synchronization and mixing without opening a separate audio-feature project.

## Starting point and evidence boundaries

- The runtime source commit observed during planning was `7358f77`, adding the opening-road lighting candidate over `b987c23` blue particle exhaust. At execution, recheck HEAD, the working tree, running project, and app identity.
- The road candidate provides direct lighting, not building or vehicle reflections. Earlier probe/SSR configurations did not establish sufficient visual benefit.
- The user finds the blue exhaust candidate too narrow, lacking volume, jet speed, and particle impact. Preserve it as a comparison, not a frozen art standard.
- Source already contains a continuous spline, curvature-driven banking currently capped at ±17°, camera following/FOV/shake, collision particles, Bloom, rank/boost UI feedback, and layered audio. Do not describe weak effects as nonexistent systems. MotionBlur was not found in initialization during inspection; verify the actual runtime Volume.
- The reference shows a solid ship changing to a cyan-white outline/translucent state and displays HOLOGRAPHIC PHASE. Its trigger and implementation are unknown. Treat the hull appearance as inspiration for existing boost, not a new gameplay mechanic to copy.
- Building variety, sparseness, and the prototype impression are grounded in user feedback and inspected native views. A complete asset-library quality audit is still pending; the production inventory must come from examining actual assets.

## Art direction

Use a neon racing corridor through a dense vertical city. Retain the anti-gravity racing, night-city, and blue-propulsion identity.

- **Structure:** credible thickness, supports, and connections nearby; clearly different building masses in the middle distance; overlapping high and low silhouettes at the horizon.
- **Color:** cool city atmosphere; cyan-blue guidance and propulsion; selective magenta signage; warm windows and collision sparks. Give colors distinct purposes and priorities rather than equal brightness everywhere.
- **Light:** recognizable bright cores, local colored halos, and response on adjacent surfaces. Bright objects retain their shapes, and dark city areas retain substance.
- **Detail:** spend effort on silhouettes, facade divisions, signs, guardrail rhythm, and passing foreground objects visible at driving distance. Distant buildings do not need individually sharp windows.
- **Motion:** keep building structures stable. Selective screen animation, low-frequency light changes, and non-colliding distant traffic lights provide activity. Speed comes primarily from actual parallax, camera response, limited blur, and coordinated event feedback.

## Production phases

### Phase 0: establish the finished standard and asset decisions

Choose a continuous route in the native game and record entry, turn, and exit progress positions with the normal camera. Watch the full reference video and a current drive of matching length. Produce one comparison board, three composition sketches, and a retain/rework/replace inventory.

The board must cover city, track/guardrails, neon/fog, ship/propulsion, speed/collisions, and typography/UI. Generated images may communicate direction; motion footage and native results determine adoption. Keep planning outputs brief and proceed to scene production rather than generating another long report.

### Phase 1: complete the same native experience

The following are production dependencies for one complete experience. Use the four-task assignment below, with 01 integrating the work. Bring everything into one runnable scene early; individual passes cannot replace overall judgment.

**A. Rebuild the spatial and asset foundation first.**

Audit actual assets and develop a reusable city kit. A suggested starting budget is four clearly distinguishable building families—broad commercial towers, slender residential/hotel towers, stepped service buildings, and transport/industrial facilities—plus one directional landmark, podiums, bridges, rooftop equipment, signs, and guardrail guidance modules. Counts are an initial production budget, not acceptance criteria. Scale, roofline, massing, and facade divisions must differ; height changes and alternate window textures are insufficient. Compose foreground, middle distance, and background within the segment, avoiding evenly spaced roadside repetition.

Use suitable existing assets, reproducible Blender hard-surface production, and external assets with verified licenses. Validate large forms and lighting in the native game before refining materials. Avoid spending effort on invisible microdetail. Record external asset sources and licenses; paid purchases require an explicit budget and are not a default step.

**B. Integrate the road, city materials, lighting, and atmosphere.**

Calibrate exposure and tone mapping, then coordinate roughness, light cores, Bloom, road lighting response, and distance fog. First validate effects available in the existing URP path. Reduce distant contrast progressively into the fog color while retaining middle-distance silhouettes and city lights. Do not use dense fog to hide a sparse layout or brighten the entire image. Rebalance the existing road-lighting candidate where necessary to avoid blown-out ship highlights. If reflections are needed, prove valid source data and receiving materials before further SSR parameter experiments.

Add purposeful guardrail arrows, segmented lights, numbering/sector markers, and advertising panels. Guidance must follow the real track curve and banking, remaining continuous and readable. Advertisement brightness and animation frequency should stay subordinate to driving-critical feedback.

**C. Design complete speed and ship event responses.**

Check actual banking, surface normals, and mesh transitions before deciding whether to change them. Distinguish road banking, visual ship banking, and camera roll so they do not accumulate into excessive tilt. Bound and damp camera following, look-ahead, and boost FOV. Foreground movement and environmental flow provide sustained speed; collision impulses should be brief rather than constant shake.

Trial controlled motion blur and verify depth/motion vectors, transparent effects, and UI render order. Do not assume enabling URP MotionBlur reproduces the reference. If using peripheral directional blur, identify it as a stylized approximation and preserve central track and ship readability. Respect existing reduced-motion and camera-comfort settings; every new effect requires an off or reduced-quality path.

Express boost through hull brightness/energy textures, local outlines, a brief activation, and wake response. First establish an obvious hull change with existing models and materials. Material effects must not change colliders, mass, speed, handling, or ability rules. Produce mechanical articulation only if later asset evaluation establishes its value; do not add transformation gameplay to imitate the reference.

Rebuild the proportion of full main plume, bright core, and dispersing particles to convey rapid rearward ejection, turbulence, breakup, and decay. Give ignition, sustained propulsion, and release distinct shapes. Keep the blue direction. More particles, a narrower plume, or a longer plume are not quality criteria by themselves.

**D. Coordinate collisions, sound, and HUD feedback.**

Trigger flashes, slender gold-orange sparks, and fragment-like bright points at real contact locations, using contact intensity and sliding direction. Sustained scraping uses bounded ongoing emission that stops when contact ends; heavy impacts produce stronger brief bursts. Define pooling, cooldowns, distant-opponent degradation, and screen-occlusion limits. Decorative fragments must not add damage or physics gameplay. Synchronize sound, ship response, and UI warnings with distinct severity levels.

Pair a distinctive racing display face with legible small text. Specify weight, slant, tracking, size, and color roles. Cover speed, rank, boost, and existing event feedback first; motion must follow real events with explicit entry, emphasis, hold, and exit. Use existing game data only, without a shield bar or invented metrics. IMGUI may remain; a framework migration needs a demonstrated presentation obstacle. Verify font licenses and required language/glyph coverage.

Coordinate the existing engine, wind, boost, impact, and UI sound envelopes and intensity. Do not replace mixing work with a new music system. Final footage must include real game sound.

### Phase 2: integrated refinement and user review

Deliver approximately 30 seconds of continuous native driving, a collision supplement, a playable app, and current/candidate comparisons. Fix issues during technical self-checks. The final normal-camera assessment must examine all dimensions together; individual subsystem passes cannot simply be added up into an overall pass.

The first quality gate is user acceptance of the overall direction. If the city remains repetitive and sparse, speed feels flat, exhaust resembles sheets or a water jet, neon blows out, or UI resembles generic panels, address the specific root causes before expansion. Be candid about unmet criteria. Technical completion cannot stand in for artistic acceptance.

### Phase 3: expand to the full circuit and all existing interfaces

After user acceptance of the exemplar, extend the same standard to the remaining route. Design density, brightness, and enclosed/open rhythms segment by segment. Reuse the modular language without copying the same row of buildings. Cover all existing menus, pause, settings, results, and their states with consistent typography, transitions, and feedback.

Finally check whole-race performance, resource loading/reclamation, repeated play, restart, pause/resume, setting persistence, and complete UI interaction. Completing those checks and receiving the user's overall acceptance are required to approach completion of the presentation rebuild. Finishing the exemplar is not finishing the whole game.

## Organization and boundaries

Follow the user's latest four-task assignment: 01 / GPT-5.6 Sol owns Unity scenes, environment/road/lighting code, and final integration; 02 / GPT-6 Astra owns unified art direction and all assets; 03 / GPT-5.6 Sol owns camera, speed, ship-state, and collision effects code; 04 / GPT-5.6 Sol owns UI animation and audio playback/mixing code. Blender and asset-generation scripts, models, UV/LOD, materials/textures, VFX atlases, fonts/UI graphics, and new source audio all belong to Astra. Technical import, shader code, and binding belong to the relevant Sol task.

01 owns the single production scene, Bootstrap, global rendering settings, shared interfaces, and preferences. 02 provides composition, assets, and appearance specifications for 01 to assemble. 03 exclusively owns camera/VFX code; 04 exclusively owns HUD/audio code. Each task has an independent workspace and explicit file list. File owners implement cross-boundary requests. Astra's early samples must span city assets, propulsion/collisions, and UI so native integration begins early.

Start at [START_HERE.en.md](START_HERE.en.md). Start 01 to establish the baseline, workspaces, and contracts. After WORKSPACES_READY, the user starts 02–04 and continues 01 for incremental integration. Preparing these handoffs has not created tasks or implemented the game. A user request to execute the relevant handoff authorizes that assignment. The four tasks first deliver phases 0–2 together; phase 3 follows overall user acceptance. Do not automatically revive older handoffs or dispatch independent reviewers.

Preserve racing rules, input, lap count, AI competition logic, boost consumption/speeds, and saved records. Camera presentation, visual assets, UI styling/motion, lighting, and existing sound may change. Any local banking change requires joint validation of rendered meshes, guardrails, collision surfaces, and TrackFrame; retain the centerline and record handling effects. Do not separate collision-surface edits from visible-road edits to make the road appear smooth.

## Validation and delivery constraints

Detailed criteria are in [ACCEPTANCE.en.md](ACCEPTANCE.en.md). The proposed performance target is sustained 60 fps-class operation at native 1080p on the current M4 Max, measured across the complete segment and a collision stress segment. This is a target, not existing evidence. Record available CPU/GPU measurements, p50/p95/p99, long frames, resolution, and quality settings. A brief average does not establish sustained smoothness. Budget fog, transparent overdraw, blur, additional lights, and city assets, then adjust from measurements.

Do not promise a schedule. Estimate remaining work from the selected methods only after the first native sample and asset audit.

## Sources and limitations

Sources: user feedback in this conversation, the approximately 10-second reference video and collision screenshot, source spot checks, and the blue-exhaust and road-candidate reports. The road report's 233 tests and short-window performance results belong to that earlier task. They were not rerun for planning and do not transfer to the future rebuild. The full asset audit and independent listening review of a new version remain pending. All new presentation proposals are production hypotheses awaiting native validation.
