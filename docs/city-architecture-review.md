# City architecture review — 2026-09-16

A separate playable opening-city candidate is ready for owner review. Deep frames, recessed glazing, grouped occupancy, continuous glass spines, horizontal slab bands, mechanical floors and stepped crowns replace the prior scattered-window treatment. Lower city blocks now have foundations and streets; additional distant silhouettes fill the left side of the exit. Existing track geometry, rail treatment, ship, camera and gameplay code are preserved. Scope remains opening-city art and its visible backdrop.

- App: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/builds/Vector Rush Architecture 09.app`
- Build GUID: `c05f767bfbc74d59ac0284a8c417c602`
- Review: `/Users/anping.wang/Documents/Stuff/AI Space/Projects/Games/Vector Rush Family/Vector Rush Workspaces 2/2026-09-14-playable-race/artifacts/architecture-review/City Architecture Review.html`
- Clip: 8.96s, native HUD-visible, 24 Hz simulation time, silent automated physics, no interpolation.
- All 248 existing Editor tests passed. Native build and shader checks passed.
- All 432 native motion PNGs verified. Three anchors have exact camera position/rotation/FOV and racer-position parity with City05. All-frame position, FOV, speed and racer position match; maximum non-anchor rotation delta 0.35186 degrees.
- Separate fresh opening frame-delivery checks at 1920x1080 on Apple M4 Max: baseline mean 8.3565ms/P95 9.0726ms/P99 9.2536ms; candidate mean 8.3449ms/P95 9.1742ms/P99 9.2963ms. Existing120fps cap; these are delivered intervals, not isolated GPU costs.
- One full native automated race: all six finishers, zero recoveries, player 129.166s; frozen result, pause freeze and clean restart checked. Normal launch title screen inspected in the native app.

## Art review and limits

Compared native approach/bend/exit and six motion samples with both owner-selected Scenario concepts. The new structural systems, quieter glass sections and filled exit are visibly different. The result remains more stylized and repetitive than the concepts; roof equipment and interiors are simplified, and the glass lacks their full material richness. Artistic acceptance remains with the owner.

Reflections use three actual local scene captures on a material-scoped glazing pass, approximate box projection, Fresnel/roughness response and distance falloff. Captures exclude the reflection pass to prevent feedback. These are static city reflections; moving racers are not reflected. Global reflection probes were rejected because they darkened the accepted road. Full-circuit rollout, Stage2 motion/UI and human handling/controller assessment remain outside this pass. Three-consecutive-race stability was not retested.

The HTML preview could not be opened automatically because Browser Use blocked the local file URL. No alternate browser route was attempted. HTML media links were checked locally; native game images and normal app launch were visually inspected. Browser layout/slider playback interaction remains unverified.

## Reproduction

Run `tools/stage1/make_architecture_assets.py`, then the existing shared-lease runner with Unity `VectorRush.Editor.Stage1CitySetup.Prepare`. Build with `VectorRush.Editor.ProductionSceneSetup.BuildExperienceCandidate`, `-experienceScene Assets/Scenes/Stage1City.unity`, a fresh absolute `-productionEvidence` folder and separately named absolute `-experienceBuildOutput` app. Native captures use `-openingEvidence`, with `-openingStills -openingHideHud` for clean anchors and neither for the complete HUD-visible sequence. Use `tools/stage1/validate_delivery.py` against City05 and the candidate; package/render scripts remain beside the authoring script.
