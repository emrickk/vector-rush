# Vector Rush asset sources

The current runtime uses the **sculpted Kestrel 07 V2** and **Solstice environment V2** copied from the sibling Vector Rush Review staging project. The original files at this folder's top level are **legacy V1**, retained for provenance and comparison; they are not the current hero asset.

| Asset | Editable source / generator | Runtime destination |
| --- | --- | --- |
| Current hero V2 | [hero-v2/Kestrel07-v2.blend](hero-v2/Kestrel07-v2.blend), [build_hero_v2.py](hero-v2/build_hero_v2.py) | `UnityProject/Assets/Resources/Art/HeroShip.fbx` |
| Current coastal kit V2 | [environment-v2/Solstice_Environment_Kit.blend](environment-v2/Solstice_Environment_Kit.blend), [build_environment.py](environment-v2/build_environment.py) | `UnityProject/Assets/Resources/Art/Environment/Solstice_*.fbx` |
| Legacy hero V1 | [Kestrel07.blend](Kestrel07.blend), [build_assets.py](build_assets.py) | Historical only; direct execution would overwrite the live `HeroShip.fbx` |

Use `tools/blender.sh --help` from the project root. The wrapper now requires an explicit version/operation and always creates a fresh staging directory. Running it without arguments does not launch Blender. Even `legacy-v1` writes into an isolated project-shaped staging directory, protecting the reviewed live V2 export.

See [asset regeneration and runtime-copy instructions](../docs/asset-regeneration.md) for exact commands, dependency status and source-path behavior. Generation and native acceptance are separate steps.

## Current hero contract

The frozen V2 has 73,364 triangles in nine material batches and 142 editable source objects. Its live FBX SHA256 was verified against [frozen-export.json](hero-v2/frozen-export.json): `61f634392f52c054a152641f16770b10b3c6eab7d37f5323cb627d2c084ee035`. Regeneration produces a candidate; a new FBX is not automatically the frozen, reviewed export.

Source coordinates are nose −Y, up +Z, in metres. Existing FBX conversion maps Unity coordinates to `(source X, source Z, −source Y)`. Main nozzle exits are Unity `(±1.68, −0.035, −3.405)`, facing −Z. Use [engine-anchors.json](hero-v2/engine-anchors.json), [INTEGRATION.md](hero-v2/INTEGRATION.md) and [REVISION-NOTES.md](hero-v2/REVISION-NOTES.md) for the current geometry/material contract. The hero uses original geometry and geometric markings, with no required external textures. Material names remain Ivory, Graphite, Glass, Signal, Engine, Metal, Ceramic, Ink and WhiteMark.

## Environment and texture provenance

The selected cliff uses **Rock 3 by Rob Tuytel / Poly Haven (CC0)**, not the superseded procedural limestone trial. See [environment integration](environment-v2/INTEGRATION.md), [texture provenance](environment-v2/textures/Rock3_PROVENANCE.md) and the packaged [download record](environment-v2/textures/Rock3_download-record.json). The three original source maps match the SHA256 values in that record. The separately named metallic/smoothness map is a derived packing: RGB zero, alpha one minus source roughness.

The copied integration notes describe their original staging handoff and may say the Unity project was not edited; that is historical context. Runtime FBXs and selected textures are now integrated. Studio renders demonstrate authored assets, while native captures remain the evidence for in-game appearance.
