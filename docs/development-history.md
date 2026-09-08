# Development history

The first design commit was recorded before implementation. GitHub was requested after the first native playtest and during the second visual iteration. Earlier uncommitted intermediate edits cannot be recovered as historical commits; the implementation checkpoint preserves their current combined result.

## Recorded milestones

1. **Design and ownership** — original anti-gravity racer, bounded playable scope, interface contracts and parallel plan. Commit `ca58eec`.
2. **Integrated implementation checkpoint** (`acc8252`) — Unity project, authored Blender craft, course, hover physics, five rivals, three-lap race, HUD/audio, reusable GPT prompt and tool wrappers. Includes first runtime evidence and independent critiques. First rule-test run: 17 passed. First native automated race: 3 laps in 155.43 seconds with 7 player recoveries. First visual review failed. Current checkpoint includes the next font, lighting, ocean, coastline and AI corrections; those corrections compile/build successfully but still await their own complete native playtest.

## Next steps, each with its own commit

- Validate the second native build, capture current evidence, and obtain an independent visual review.
- Address remaining blocking findings with separately recorded corrective commits and relevant rechecks.
- Record a short gameplay video and verify menu/keyboard interaction.
- Finalize delivery documentation and known limitations.

Every subsequent completed plan step should update this file and be committed and pushed to the authorized private repository. Build products, caches and temporary frames remain local; source, authoring assets, selected evidence and critiques belong in history.

### Second native validation

The private repository `https://github.com/emrickk/vector-rush` was created with explicit user approval and verified private. Both initial commits were pushed. `evidence/run-02` records a full automated race through ordinary player physics: 3 laps in 112.58 seconds, zero player recoveries, and three restart/countdown-pause checks passed. NOVA recovered once. At 1920×1080 on Apple M2 Max with VSync, 3,758 frame samples had P95 16.82 ms and P99 16.89 ms. These are frame intervals, not isolated GPU measurements. HUD fonts are visibly repaired; independent review still rejects AAA visual quality.
