# Direct production candidate

The original project remains pinned to Unity 6000.6.0f1. The local candidate was built in `Builds/ProductionValidationProject` using the installed Unity 6000.5.3f1 and its resolved URP 17.5. This is an explicit compatibility branch of the build workflow, not proof of an identical render to 17.6.

`play-production.command` opens the separately named candidate app. Source geometry recipes are in `Assets/Editor/Production`; their generated persistent assets and scene are under `Assets/World/NocturneProduction` and `Assets/Scenes/NocturneProduction.unity`. The authoring entry refuses to overwrite an existing scene/asset family. Production builds never call legacy `Prepare` and restore prior graphics settings after building.

Batch entry points:

- `VectorRush.Editor.ProductionSceneSetup.ExportContext -productionEvidence <fresh absolute directory>`
- `VectorRush.Editor.ProductionSceneSetup.BuildScene -productionEvidence <fresh absolute directory>` (requires production output paths to be absent)
- `VectorRush.Editor.ProductionSceneSetup.BuildCandidate -productionEvidence <fresh absolute directory> -productionBuildOutput <fresh separately named absolute .app path>`

Native verification is opt-in: `-productionValidation <fresh absolute directory> races`, `preview`, or `performance`. Races use automated steering through ordinary hover physics. Preview records game-rendered screenshots and listener audio on a real-time clock; it is not a human gameplay recording or performance measurement. Encode only after the app exits with `python3 tools/production-preview.py <preview directory>`. Performance must run alone, after warm-up, without an Editor build, preview capture or encoder.

Default driving: WASD/arrows, Space boost, Q/E airbrakes, R recover, Esc/P pause. Gamepad: stick/RT/LT, A boost, LB/RB airbrakes, Y recover. In menus, arrows/D-pad and Enter/A select actions. F1/Y opens settings; D-pad adjusts rows, A activates, B closes. Settings bindings accept unused letters (excluding P), Space and Shift; arrow alternatives and menu keys remain reserved. No physical gamepad test has been performed.

The first environment uses deterministic Unity-authored meshes, not the planned Blender package. Baked lighting, LOD authoring, full decorative-clearance acceptance, human feel, continuous audiovisual inspection and visual-target acceptance remain unfinished. Do not interpret a build or automated race result as those passes.
