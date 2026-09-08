# Visual critique and revision record

The first sculpted pass is rejected. Its preserved front and rear renders are in `first-pass/`. The original source and FBX for that exact pass are not retained; the current authoring script and export represent the corrected pass.

The independent review found inflated armor, padded intake rails, thick grille bars, detached race plaques crossed by livery, rubber-like fin edging, broad dark underbody filling the channels, and no visible engine emission from the rear.

The second pass corrects those visible construction choices:

- Ceramic sections now have controlled longitudinal hard edges and broader planar crowns, with longitudinal volume still changing along the craft.
- Outer flank armor is split into unequal front and rear panels over dark structure.
- Intake surrounds are narrower faceted rails; seven thin swept vanes sit deeper inside each dark channel.
- Race-number insets and white numerals follow the actual shoulder surface, with stripes ending before the number field.
- Fin livery is a thin conformal flat panel, and the rounded edge piping is removed.
- The central keel is narrower, opening channels with sparse structural crossmembers. The cockpit crown is lowered and its sill narrowed.
- Nozzles receive split formed armor cowls, and the carbon core ends ahead of the luminous chamber. The first pass accidentally occluded the throat with that core; this was a real mesh error. An intermediate emissive annulus is now 0.295 m behind the exit lip.
- The final localized correction cuts the carbon pod crown below each intake, so the dark well and individual louvers are actually visible. The central keel ends before the secondary thrust core, and that nozzle gains a shallow luminous annulus.
- Rear undertray strakes are shorter and taper from the central keel.
- Studio material targets: Ivory roughness 0.40 / metallic 0.18; Graphite roughness 0.46 / metallic 0.35; Ceramic roughness 0.49 / metallic 0.22; Glass roughness 0.14. Runtime remapping must preserve comparable hierarchy.

Current geometry is 73,364 triangles in nine material-batched meshes, with 142 editable source objects. The axes and main engine effect anchors are unchanged from the first staged sculpted pass.

The renders are studio inspection evidence, not a native-game visual pass. The exported mesh needs native chase-camera review after integration.
