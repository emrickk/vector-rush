# Lighting/material native delivery verification

2026-09-08. The implemented candidate is accepted for sampled native appearance in [independent review016](../environment-reviews/016-lighting-material-native-critique.md). Final delivery validation is **incomplete because the Mac display is locked**. No source defect or required art correction is open.

## Verified result

Build GUID`5c49b92643bd4d41820ba81857fdf8c7` supplies the accepted [1,440-frame full-circuit capture](../../evidence/lighting-depth-candidate-01/full-lap/environment-evidence.json). Every original PNG passed CRC and decompression validation. The critic inspected eight candidate/baseline pairs. Parent additionally inspected circuit frames360,455,678 and1004; the gallery approach/interior, crest and station remain readable. Frame678's warm craft wash also exists in the previous accepted capture, so it is not a newly demonstrated light-spill regression. All comparisons are nearby views, not exact-pixel matches.

The42.75-second full-lap video spans recorded start-line crossings15 through1040; the24-second preview is the continuous frame130–705 excerpt. Both are actual1080p native rendering with automated steering at24 simulation frames per second, silent. Their frame counts, durations, hashes and complete decoder passes are recorded in [video validation](../../evidence/lighting-depth-final/video-validation.json). No continuous subjective playback acceptance is claimed.

All42 existing tests pass.193 prebuild source/resource/settings hashes and189 application-file hashes remain unchanged. The local macOS ZIP contains441 entries, totals56,664,092 bytes and passes CRC integrity validation. [Build manifest](../../evidence/lighting-depth-final/build-manifest.json).

## Remaining native checks

The1280×800 capture stalled without frames while the Mac was locked. The computer-use tool explicitly reported that automatic unlock was unavailable. The process was stopped and its [incomplete attempt](../../evidence/lighting-depth-final/16x10-display-locked-incomplete/attempt-status.json) retained. No unrelated user process was stopped.

After manual unlock: capture fresh16:10 and ultrawide station context views; run the same binary through the separate real-time three-lap race/restart/countdown and frame-interval check. Preserve any poor first performance sample and repeat only if a demonstrated concern warrants it. No Editor build, image encoding or asset generation should overlap that measurement. Previous build race/performance numbers must not be presented as current-build results.

The stronger thermal service depth and restrained mast fill are accepted. Remaining broad art limits include simplified glazing and weak fine-texture evidence. This report does not expand the current art scope or authorize a GitHub release.
