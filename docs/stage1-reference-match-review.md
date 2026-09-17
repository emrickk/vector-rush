# Vector Rush: Stage 1 city review

A playable banked city passage is ready for review. It has denser architecture, an illuminated graphite road, clearer turn guidance and fuller blue exhaust. It moves toward the two selected Scenario targets; it does not claim an exact visual match.

Open the review package at `../../../artifacts/stage1-reference-match/delivery/Stage1-review.html` for the references, native comparisons and an 8.54-second moving clip. The original native PNGs are in `images/`. Clean comparison captures hide the HUD; `native-hud.png` and both clips show the ordinary HUD.

## Playable build

- App: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/builds/Vector Rush Stage1 City 05.app`
- Build GUID: `7cba6941ae2a469b83a9c807ba5ac957`
- Source milestone: `f1fb7fd1fe89ccef753cea5fc7d77c203c420bd1` on `stage1/astra-reference-city`.
- Default controls: W / Up throttle, A-D / Left-Right steer, S / Down brake, Space boost, Escape pause. Existing remappings remain supported.

## What is implemented

The new persistent Unity scene includes 140 building footprints across foreground, lower city and distant skyline layers; stepped towers, paired shafts, framed ads, rooftop plant, service structures, bank-following rails, supports and direction-correct chevrons. Cyan/magenta emission, local lights and a surface response make the road readable. The original ship geometry uses the delivered finish maps, masked boost emission, brighter nozzle cores and broader layered blue particles. Track geometry, driving rules and the chase camera remain unchanged. The full course remains playable; the art pass is bounded to the opening city, approximately progress 0 to 0.32.

## Verification

- Strict import: all 30 corrected FBX LOD models pass the unchanged bounds/material contract.
- Tests: **248 / 248 passed**, zero failures or skips.
- Captures: all 432 native 1920 × 1080 motion frames verified. The three chosen before/after stills have exact recorded camera position, rotation, FOV and racer-position parity. Across all 432 frames, position/FOV/path/speed/racer positions are identical; maximum non-anchor camera rotation difference is 0.3357°.
- Motion: 8.54-second excerpt plus full 18-second sequence, 24 Hz simulation time, automated normal physics, silent, no frame interpolation. Both videos fully decoded. This is separate from real-time performance or human driving.
- Opening performance, isolated on Apple M4 Max at 1920 × 1080: before mean 8.341 ms / P95 9.118 ms / P99 9.333 ms; candidate mean 8.353 ms / P95 9.137 ms / P99 9.336 ms. These are delivered frame intervals under the existing 120 fps cap, not isolated GPU costs.
- One complete real-time automated race: all six racers finished, zero recoveries; player time 129.166 seconds, post-finish result and pause state held. See the original records in `checks/`.

## Remaining gaps and scope

Foreground facade/material variation remains more stylized and repetitive than the generated targets. Rail response combines local lights and an authored surface approximation; full dynamic city reflections are not implemented. The busy left facade competes with existing HUD text. The video target's complete motion treatment, later UI polish and full-circuit art rollout remain outside this step. Human handling/controller comfort are unverified. The earlier report's third-consecutive-race timeout was not revalidated in this visual pass, which checks one complete race.

The two Scenario images are concept targets, not gameplay. Their ship and route differ from the retained native geometry; the original video remains the route/banking authority. Artistic acceptance remains with the owner.
