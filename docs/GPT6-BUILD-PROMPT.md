# GPT-6 build brief — Vector Rush

Build an original, visually exceptional anti-gravity racing game in Unity. Use Wipeout Omega Collection as a benchmark for speed, readability, atmosphere, and presentation. Create original ships, tracks, branding, music, and interface design.

The target is a polished, playable vertical slice: one memorable circuit, one fully developed player craft, credible opponents, and a complete race experience. Prioritize craft and coherence over feature count. Aim for AAA presentation, but do not describe the result as AAA or perfect without evidence. Finish with an honest account of remaining gaps.

Work autonomously within this scope. Make reasonable creative and technical decisions, document them, and continue through implementation, integration, testing, and refinement. Ask only when a missing answer prevents meaningful progress, an external account or license requires my action, or a proposed action is destructive, paid, or public.

## Platform and foundations

Target Apple Silicon macOS on an M2 Max with 32 GB RAM. Inspect installed tools first. Use the installed supported Unity 6 LTS editor and choose URP unless a short, measured prototype demonstrates a compelling reason for another render pipeline. Record exact versions and pin dependencies. Use Blender for authored assets and a repeatable export workflow. Produce a standalone macOS build as well as a usable Unity project.

## Art direction before asset production

Develop a distinctive visual identity, then generate reference images for the hero craft, track environment, and actual racing-camera composition. Translate these into a compact art guide covering silhouette, scale, color, materials, lighting, signage, and interface hierarchy.

References express intent; they are not evidence that the game achieves it. Build the playable camera and graybox early, then prove the visual direction in a representative track section before expanding it around the circuit.

## Delegate with explicit integration contracts

Fan out independent sub-agents for:

- Vehicle handling, opponent behavior, race rules, and automated checks.
- Blender assets, track environment, materials, lighting, and effects.
- Interface, audio, presentation, and accessibility.
- Independent visual and gameplay criticism.

Use the available concurrency sensibly; schedule roles sequentially when necessary. Before parallel edits, define file ownership and shared contracts: world scale, track coordinate system, craft forward axis, input actions, race states, events, asset naming, prefab interfaces, and scene ownership. Keep shared scenes and settings under one integration owner. Agents must hand over working assets or code with verification instructions. Integrate continuously.

## Playable racing

Implement responsive hover handling with suspension, damping, acceleration, braking, airbrakes, steering, controlled lateral slip, banking, collisions, and reliable recovery. Run physics on a fixed timestep. Keep presentation interpolation separate and make handling tunable through shared configuration. Verify behavior at different rendering frame rates.

Build a closed circuit with readable corners, elevation changes, banking, boost opportunities, safe track boundaries, and a satisfying rhythm. Include opponents that complete races and respond coherently to the track.

Deliver title/start flow, countdown, three-lap race, ordered checkpoints, lap timing, position, finish results, pause, restart, and exit. Prevent reverse crossings, skipped checkpoints, or respawns from awarding false progress. Support keyboard and controller, clear control instructions, and configurable camera shake.

## Assets and presentation

Author a distinctive craft with a strong racing silhouette, cockpit detail, thrusters, readable materials, and appropriate collision geometry. Build modular track and environment assets in Blender with consistent units, applied transforms, deliberate pivots, sensible UVs, collision proxies, and LODs where useful. Retain editable source files and export scripts.

Create speed through camera behavior, parallax, lighting, trackside detail, particles, audio, and selective motion effects. Keep the racing line and opponents readable. Complete the experience with coherent HUD typography, transitions, engine response, boost audio, impacts, countdown, and finish feedback. Record licenses and provenance for external assets.

## Verification and independent critique

After each meaningful milestone, run the actual game. Capture labeled runtime screenshots from the racing camera and short gameplay videos covering the start, fast sections, difficult corners, collisions, and finish. Include crowded and visually difficult situations.

A separate critic must inspect those captures and evaluate composition, material quality, lighting, asset consistency, motion, speed sensation, readability, interface, and visible defects. It must distinguish observed problems from speculation and identify the three highest-impact fixes. A critic's approval is not proof of AAA quality.

Perform up to three substantial review-and-fix cycles. Each cycle must include implemented fixes and new captures. If serious issues remain, report them plainly instead of repeating cosmetic changes indefinitely.

Profile the standalone build on the target machine at 1920×1080 after warm-up. Target 60 fps; report measured frame-time percentiles, CPU/GPU bottlenecks, memory usage, settings, and test conditions. Verify a complete race and repeated restart without accumulating errors. Do not invent measurements.

Deliver the playable build, project, editable asset sources, reference board, controls, verification evidence, and a concise completion report covering what works, measured performance, known limitations, and the next improvements with the greatest impact.
