# Ship surface pipeline

Use this only after the independent form/construction review accepts a candidate. The script does not replace visual review.

`bake_materials.py` opens the exact joined runtime UV meshes from an `EXPORT_RUNTIME` collection or objects named `Kestrel07_*`. It requires one object for each of Ivory, Graphite, Metal and Ceramic. It preserves geometry and UV coordinates, writes to a fresh directory, and saves a separate blend using the baked maps for honest preview. The editable input and live Unity project are untouched.

Run with the installed Blender executable in background mode, passing the script followed by `-- --blend <runtime.blend> --out <new-directory>`. Use `--size 1024 --samples 16` for a first material candidate; increase only after review. The default is 2048 pixels and 24 CPU samples with four threads.

The initial coating recipe is deliberately restrained: very slight variation between connected panels, fine manufactured roughness, and small surface relief. It does not add blanket dirt, random damage or extra geometry. Recipes remain candidates until their rendered response is accepted.

| Payload | Contract |
|---|---|
| `*_BaseColor.png` | sRGB albedo; material tint multiplies this color. Use white tint to reproduce the baked preview. |
| `*_Normal.png` | Tangent-space OpenGL +Y; Unity NormalMap import, no green inversion. |
| `*_MetallicSmoothness.png` | Linear; RGB metallic, alpha = 1 minus linear roughness. Unity smoothness multiplier 1. |
| `*_Occlusion.png` | Linear AO in RGB; Unity consumes green, with a restrained strength. |
| `*_Roughness.png` | Linear authoring/inspection source; not required at runtime after packing. |

Selected reviewed payloads will be copied into `Assets/Resources/Art/ShipSurfaces`. `ShipSurfaceMaps` is opt-in ship material plumbing; it keeps environment materials separate. The native template must retain normal, metallic/smoothness and occlusion variants before integration. No texture payload is yet selected by this pipeline scaffold.
