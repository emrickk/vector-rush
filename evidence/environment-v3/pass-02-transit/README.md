# Transit kit evidence

`01-transit-kit-studio.png`: actual Blender Cycles render of the three editable source assets, 1400×1000, 16 samples, four CPU threads, denoising off, no compositor bloom. This is source inspection, not a native game capture.

`export-roundtrip-audit.json`: all three final FBXs reimported and checked for bounds, topology, UV0 and tangents. All checks passed.

Source and Unity placements are documented in `SourceAssets/environment-v3/pass-02-transit/`. Parent owns the locked native baseline and subsequent candidate captures. Native validation is pending.
