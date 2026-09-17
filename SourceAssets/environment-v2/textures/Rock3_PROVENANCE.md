# Rock 3 — material evaluation source

Downloaded for the Vector Rush coastal-material trial, 2026-09-07 Pacific / 2026-09-08 UTC.

- Asset: [Rock 3](https://polyhaven.com/a/rock_3)
- Author: Rob Tuytel, as credited on the asset page.
- License: [Poly Haven CC0](https://polyhaven.com/license). Public-domain dedication; compatible with redistribution in the game source/assets.
- Source physical width: 1.5 metres according to the asset page. Keep a plausible surface scale when mapping to a large cliff.
- Selected originals: 2K diffuse JPG (sRGB), OpenGL normal PNG (linear/non-color), roughness JPG (linear/non-color).
- Download API manifest is preserved in [Rock3_files-manifest.json](Rock3_files-manifest.json); exact source URLs and SHA256 hashes are in [Rock3_download-record.json](Rock3_download-record.json). Download bytes were checked against the API's MD5 values.

These are third-party source textures, not authored by the Vector Rush agents. The current procedural cliff meshes remain original authored geometry. Keep this provenance if the material is integrated. Do not overwrite the original maps; any packed or color-adjusted derivatives should have separate filenames and an explicit generation record.

For Unity URP metallic workflow, use normal-map import for the GL normal, nonmetallic stone, and smoothness equal to one minus roughness. A material trial is not visual acceptance: compare actual authored cliff renders and the native driving view before choosing the texture.
