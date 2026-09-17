# Koi Lantern Tower

One new landmark for the owner-approved turn focal-point trial. The porcelain koi, ribbed bronze fins, open amber ring, structural cables, market tower, canopy and balcony are real meshes. Scenario generated the concept and enamel texture; the model is authored in Blender and imported into Unity. The sculpture is larger than the initial model to improve visibility during the native approach.

- Editable source: `KoiLanternTower.blend` (enamel texture packed).
- Portable model author: `build_model.py`; exports metric `KoiLanternTower.fbx` and `manifest.json`.
- Surface: `scenario/enamel/art.png`, reused by Unity's enamel material.
- [Concept prompt](scenario/concept/request.json) and [surface prompt](scenario/enamel/request.json); provider/model/job and asset IDs are adjacent. Generation uses Scenario `model_openai-gpt-image-2`, high quality. Credentials are read only from `SCENARIO_API_TOKEN` and are never stored in this package.
- `scenario_art.py submit` / `poll` are optional generation steps; existing outputs are reused by default. Do not regenerate to build the game.
- `render_asset.py` produces the Blender detail render; it is not gameplay.
- `package_review.py` makes the native before/after review and 9.5-second turn excerpt from completed captures. It requires Pillow and ffmpeg.

Run model authoring with Blender 5.2.1 LTS: `Blender -b --python SourceAssets/KoiLanternTower/build_model.py`. Import/place with Unity `-executeMethod VectorRush.Editor.KoiLanternSetup.Prepare -productionEvidence <fresh absolute directory>`. This copies Stage 8 to `Assets/Scenes/KoiLanternStage9.unity`, preserves its assets and maps the new material slots explicitly. The model has 13 renderers and 84,932 imported triangles (Blender's pre-export manifest counts 85,352). Four separate fin pivots animate by 2.5–4 degrees over roughly ten seconds; the tower and suspension geometry stay fixed. Two local spot lights illuminate the sculpture.

The wrapper's `koi-build` command builds the trial; the ordinary `build` command remains Stage 8 pending owner review. See `docs/current-game/koi-landmark-2026-09-17/README.md` for exact current native evidence and limitations. The other two proposed landmarks are not part of this trial.
