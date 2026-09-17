# Vector Rush: Visual and motion acceptance criteria

Use this checklist to assess the same complete experience. A technical capability, higher object count, or attractive reference still does not establish a pass. Existing road and exhaust candidates are comparisons only.

| Dimension | User objective | Required finished behavior | Failure signals |
| --- | --- | --- | --- |
| City assets | Varied, dense, convincingly cyberpunk | Credible nearby construction; distinct middle-distance silhouettes/masses; overlapping distant layers; podiums, transport structures, and signage connect the city | One building repeated at different heights; identical small windows everywhere; isolated blocks and large meaningless gaps |
| City activity | An environment that feels alive | Selective animated screens, changing light points, and decorative distant traffic, with layered driving parallax | All windows flashing together; every screen demanding attention; random floating particles filling empty space |
| Neon and materials | Bright cores, halos, and colored light | Emissive shapes remain recognizable; soft local halos; plausible light on road/ship; material differences remain visible in dark areas | Flat bright strips; halos swallowing arrows/windows; the whole road becoming an emissive color patch |
| Fog and distance | Distant buildings can be indistinct while the city feels large | Contrast varies with distance; layered silhouettes and lights show through the atmosphere; nearby road and turns remain clear | Uniform gray fog; every building equally blurred; fog hiding insufficient foreground assets |
| Road and guardrails | Smooth banking in both directions and rich guidance | Continuous neighboring normals/banking; guardrails/arrows follow the road; nearby infrastructure provides speed references | Faceted road joins or normal seams; camera shake as a substitute; floating signs; purposeless neon clutter |
| Speed | Acceleration is visibly faster and more forceful | Foreground flow, bounded FOV/damping, and blur cooperate; boost transitions read clearly; the upcoming route remains readable | Only numbers increasing; constant shake; full-screen smearing; uncontrolled combined camera/ship roll |
| Ship state | Energy changes affect the whole hull | Recognizable material and local outline changes during existing boost activation, sustain, and release | Only a longer exhaust; disappearing ship; unjustified shield/transformation gameplay |
| Exhaust | Large, full, rapidly ejected, with particle layers | Nozzle-attached bright core, turbulent main body, slender bright particles, and dispersing wake; stable through turns, near-camera views, and release | Crossed-sheet edges, static glowing bands, uniform water columns, large foreground blobs, beading, or nozzle detachment |
| Collisions | Immediate bursts and a sense of contact | Real-contact flashes, gold-orange sparks along sliding direction, varied bright points; sustained scraping emits until separation | Identical explosions for every contact; particles from ship center; sparks continuing after contact; sustained road occlusion |
| Typography and UI | Racing identity and event-driven motion | Deliberate display/small-text pairing; consistent motion for existing rank/boost/collision feedback; menus inherit the same language | Generic panels; everything enlarged; constantly flashing text; invented data without gameplay |
| Sound coordination | More forceful acceleration and impacts | Synchronized existing engine/wind/boost/impact/notification envelopes; distinguishable severity; comfortable continuous driving | Explosive visuals with flat sound; stacked repetitive impacts; propulsion and collisions sounding indistinguishable |

## Demonstration scenarios

1. Main footage: approximately 30 seconds of actual operation, covering cruise into the city, acceleration, boost, a banked turn, release, and recovery. Use the normal chase camera and retain game audio.
2. Collision supplement: show a light graze, sustained scraping, and a heavier impact. Label scripted control where used. Drive effects from real contact, not fabricated triggers on a video timeline.
3. UI supplement: show complete entry/exit of existing rank changes and prompts, plus representative states of the existing main menu, pause, settings, and results. The exemplar phase establishes the HUD and one menu's shared language; full interface coverage belongs to expansion.
4. Comfort comparison: the route remains readable and events still communicate when motion is disabled/reduced, with preferences respected.

## Comparison method

- Retain two baseline levels: the complete game at task start, and individually disableable diagnostic effects within the current candidate. Do not accumulate unmanageable production switches.
- Match input, time, and camera as closely as possible for asset/lighting/VFX comparisons and report actual differences.
- Camera or banking changes naturally alter pixels/poses. Compare the same route interval and driving intent, explain camera/geometry changes separately, and do not claim exact camera matching.
- Use ordinary real-time native recordings for speed and audiovisual judgment. Label fixed simulation-time recordings as shape diagnostics; they are not real-time frame-rate evidence.
- Inspect UI at actual screen-space sizes and collisions in continuous footage. Inspect buildings across viewpoints and moving occlusion/LOD transitions.
- Existing automated tests do not establish artistic quality. Add only necessary behavior/lifecycle regression checks for each change, then inspect the complete scene. Accumulated pass counts do not replace the result.

## Performance and stability

The first-round target is stable 60 fps-class operation at native 1080p on the current machine. Repeatedly measure the full main segment and collision stress segment. Record sample duration, quality settings, p50/p95/p99, percentages of frames exceeding 16.7/33.3 ms, and available CPU/GPU measurements. Distinguish warmed-up operation from first entry. Use identical constraints for old/new measurements, and separate performance runs from recording/encoding. Allocate budgets to city rendering, shadows/lighting, post-processing, and transparent overdraw; do not report only triangle or particle counts.

Cover pause/resume, restart, return to title, automatic recovery, boost depletion, rapid repeated collisions, and resource reclamation after repeated track entry. If banking/meshes change, verify actual contact and ship stability. Check common UI resolutions, existing keyboard/mouse/controller navigation, and reduced motion. Obtain fresh results for the new version rather than inheriting older test/performance numbers.

## Quality gate

At phase 1/2 delivery, the owner of the integrated result marks every row “demonstrated,” “insufficient,” or “unverified,” with the shortest direct supporting evidence. Any clearly weak core dimension prevents declaring the complete experience satisfactory. Expand the route and all interfaces after the user accepts the overall direction. Final artistic acceptance belongs to the user.
