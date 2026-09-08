# Architecture finish kit — candidate 01 accepted in native views

Implementation resumed on the owner's explicit instruction and is complete within this asset task's scope. Parent owns NightLandmarks integration, lighting, native compilation and visual acceptance. No Unity or Blender process was run by this task.

## API

`NightArchitectureFinishes.Apply(Material material, string finishKey)` is a static boolean helper. Call once after constructing each landmark finish with `WorldBuilder.MakeMaterial`; all such SurfaceLit materials already have the retained emission keyword enabled. It leaves unsupported keys such as Glass, Occupied and Lamp unchanged and returns false. Supported keys return true once their maps are applied; missing resources or unsupported shader/variant leave the material unchanged.

| Existing finish key | Texture family | Mean metallic | Smoothness min / mean / max |
|---|---|---:|---|
| Ceramic | WarmCeramic | .0350 | .4000 / .4352 / .4667 |
| Concrete | CastConcrete | .0249 | .1529 / .2261 / .2902 |
| Titanium | SatinTitanium | .6646 | .5176 / .5745 / .6275 |
| Structure, Oxide | ServiceCoating | .3349 | .2980 / .3497 / .3961 |

The helper preserves caller base color, emission color/keyword and material ownership. It caches shared imported textures per family, with subsystem-registration reset for domain-reload-disabled play sessions. Resources assets are not destroyed by the helper. No per-frame work or texture generation is added.

Apply assigns `_BaseMap` and `_MetallicGlossMap`, resets map scale/offset to one/zero for the authored 4 m metric UV0, selects the metallic workflow and metallic-map alpha as smoothness source, and sets `_Smoothness=1`. R in the packed linear map directly controls metallic; alpha already contains the absolute authored smoothness. `_Metallic` is set to the family mean as a fallback. It disables specular workflow, albedo-alpha smoothness and normal-map use. No normal map is shipped, respecting the known mast tangent limitation.

Local URP `Shaders/LitInput.hlsl`, `SampleMetallicSpecGloss`, confirms that map alpha is multiplied by `_Smoothness` and scalar metallic is ignored while `_METALLICSPECGLOSSMAP` is enabled. Leaving the original .2–.6 material scalar in place would incorrectly multiply the authored finish again.

## Retained shader template

`Resources/Art/Environment/Finishes/ArchitectureFinishLit.mat` derives from existing SurfaceLit and references the ceramic maps. It serializes the exact `_EMISSION + _METALLICSPECGLOSSMAP` variant used here. Its default emission is black. The helper checks the template and caller shader, requires the existing WorldBuilder emission variant, and changes no emission value/state. It does not copy all template properties over the caller. No runtime-only shader keyword combination or normal-map variant is introduced for the landmarks.

## Textures and projection

Four 512 × 512 RGBA texture pairs provide subdued warm ceramic, rough cast concrete, cool satin titanium and neutral service coating. All source values were generated specifically for this project with deterministic CPU code; no external photographs or texture assets were used. BaseColor maps are near-white color multipliers with opaque alpha. Packed maps use linear R=metallic, G/B=unused zero, A=smoothness.

The existing pass-02 FBX recipe uses dominant-axis projection at 4 m per UV tile. On cylinder sides, V remains source Z/4 when the transverse projection switches between X and Y. Broad low-amplitude variation therefore uses V only; transverse detail is restrained isotropic micrograin. The inspected contact sheet shows no checker grid, directional brush stripe, or repeating rust motif. Native projection transitions still require visual review.

Every texture has sRGB enabled only for BaseColor, linear packed data, mipmaps, repeat wrapping, trilinear filtering, anisotropy 8, maximum size 512 and alpha-transparency processing off. The candidate is uncompressed to preserve small packed-channel variations; eight complete RGBA mip chains use approximately 10.67 MiB before platform overhead.

## Source and verification

- `generate_finishes.py`: editable generator. The immutable candidate-directory guard executes before any writes. Choosing a new output candidate is required to generate again; existing candidate 01 was not regenerated during integration.
- `candidate-01/`: original eight textures, actual channel statistics/hashes/GUIDs, and the inspected contact sheet. Top row is albedo; bottom row is actual smoothness alpha. Columns: ceramic, concrete, titanium, service coating.
- `validate_assets.py` and `candidate-01/validation.json`: independent decoding/checks of PNG signatures/CRC, dimensions, source-to-live hashes, packed-channel variation, exact repeat edges, metadata/color space/mips, material GUID references, retained keywords, black emission, and immutable guard preservation of every live kit file.
- New `NightArchitectureFinishes.cs` and its `.meta` implement the runtime helper. Existing landmark geometry and scripts were not edited by this asset task.

Data and metadata checks pass. Concrete alpha has real variation from .1529 to .2902; titanium remains smoother at .5176–.6275. These are texture/channel findings, not a native appearance pass. Parent must verify import/compile, correct shader sampling, subtle material separation, cylindrical seam visibility and lighting/performance in the native game.

## Native integration acceptance

Parent integration applies a .78 smoothness multiplier only to Concrete after the helper, for an approximate .1193–.2264 effective range and .1764 mean. Other mapped families keep the authored absolute smoothness. The native build5c49b92643bd4d41820ba81857fdf8c7 compiles and loads the kit without missing-resource errors. [Independent review016](../../docs/environment-reviews/016-lighting-material-native-critique.md) accepts eight compared native views with no required corrections: thermal service recesses and pipe/support connections improve clearly, while mast depth improves modestly and its amber pane remains fairly flat. No conspicuous seam was observed in those views; this is sampled-image acceptance, not a motion-stability claim.
