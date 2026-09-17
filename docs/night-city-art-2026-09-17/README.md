# Night city art kit — September 17, 2026

The Game P5 art-first package is ready for asset review. It contains **19 prefabs, 38 LOD meshes, 33 Unity materials, 69 texture maps, six editable sign designs and three Scenario billboard artworks**. Open `index.html` for the categorized gallery and full-size images. This is a reusable city asset kit in a dedicated inspection scene; it is not a completed city rollout or a racing integration.

## Delivered assets

- Architecture: apartment, office and service towers; shopfront; maintenance bridge; transit gantry.
- Rooftop equipment: twin-fan cooling unit, insulated duct bank, extractor, power cabinet, water tank and cable hangers.
- Signs: vertical hotel, market fascia, three illustrated billboard frames, circular noodle-shop sign and rooftop neon lettering.
- Materials: concrete, painted graphite, corrugated steel, brushed alloy, oxidized copper, ceramic tile, rubber and asphalt, each dry/wet; three facade surfaces, six drawn signs, three Scenario ads and five supporting solid materials.

Source: `SourceAssets/NightCityKit/` (Blender assembly, recipe scripts, metric FBX, PBR maps, SVG masters, font licenses, scan sources, Scenario requests and asset IDs). Engine assets: `UnityProject/Assets/Art/NightCityKit/`. UVs are authored in Blender. Unity uses Y-up; mesh-manifest dimensions are Blender X/Y/Z, while gallery dimensions are Unity W/H/D. Prefab pivots use their authored mounting origin; sign feet and small rounded parts can extend slightly below zero. LOD transitions are 25% and 2.5% screen-relative height and still require tuning in the eventual racing placement.

## Verified

Unity 6000.6.0f1 imported all 38 meshes. Metric bounds/orientation, triangle counts, UV presence and explicit material bindings passed. All 282 existing EditMode tests passed after the asset/volume correction. The later inspection-lighting/layout-only change compiled and rebuilt successfully. Native viewer build `b154fc6beded40999e21c9d816c53aba` ran and captured both views after warming the renderer. Fresh independent package checks verify source/engine hashes, map decoding/color spaces, prefab pairs, persistent post-processing references, scan checksums and unchanged racing scenes. See `package-validation.json` and `import-validation.json`.

Gallery browser QA passed: category filtering, material and architecture full-size previews, and close controls. Image files resolve locally; catalog images load lazily as they enter view. Authored code/document whitespace checks pass; original license files and Unity-generated serialization retain their native formatting.

The individual model images and dry/wet pairs are Blender renders. Hero and rooftop close-up are actual standalone Unity images. The owner reference is marked separately. The old composite catalog was regenerated from the latest individual renders; obsolete first-pass capture imagery is not used by the gallery.

## Corrections made during continuation

Retained the recovered Game P5 work. Removed directional patterns from procedural base maps, added ceramic grout and copper patina response, completed balcony rail supports, improved round-part tessellation, retained all wet materials in the editable Blender file, fixed missing serialized tonemapping/bloom subassets, grounded rooftop sign mounts, separated occluded signs and added neutral inspection fill light. The current race, ship, HUD, handling, course and weather remain untouched.

## Rebuild

Use Python with Pillow and NumPy and Blender 5.2.1 LTS. Fonts, existing scan sources and Scenario artwork are included. Existing images require no credentials; `scenario_art.py` is optional regeneration and reads `SCENARIO_API_TOKEN` only from the environment. Do not regenerate advertisements merely to open or import this package.

Run `build_surfaces.py`, then Blender `--background --factory-startup --python-exit-code 1 --python SourceAssets/NightCityKit/build_models.py`. The recipe exports models and saves the editable assembly. `render_catalog.py`, `render_materials.py` and `render_assembly.py` create inspection images. In Unity, run `VectorRush.Editor.NightCityKitAuthoring.Prepare` with `-nightCityEvidence <absolute output folder>` to import the kit and author **only** `Assets/Art/NightCityKit/NightCityAssetReview.unity`. `BuildViewer` additionally takes `-nightCityBuild <fresh absolute .app path>`. The app's `-nightCityCapture <output folder>` mode writes warmed native captures and exits. With no capture flag, it opens the static assembly view.

`verify_package.py` verifies source/engine parity without changing assets. `package_review.py --evidence <output folder>` builds the gallery, using the final `native-review-light` captures. Native builds and logs stay outside source control.

## Remaining integration work

Owner visual acceptance is open. The kit must still be selected and placed along the track, checked at real racing distances, integrated with Game P4's separate animated billboard candidate and then paired with the separately built speed-blur candidate. The building facades still share a modular language; this kit alone does not establish the density or finish of the reference city. No racing performance, collision clearance or finished full-city fidelity is claimed. Work is kept local; no GitHub publication in this continuation.
