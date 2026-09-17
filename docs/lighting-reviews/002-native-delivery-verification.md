# Lighting/material native delivery verification

2026-09-08. The implemented candidate is accepted for sampled native appearance in [independent review016](../environment-reviews/016-lighting-material-native-critique.md). Final delivery validation is **complete** after the display became available and the native capture was relaunched. No required art correction is open.

## Verified result

Build GUID `5c49b92643bd4d41820ba81857fdf8c7` supplies the accepted [1,440-frame full-circuit capture](../../evidence/lighting-depth-candidate-01/full-lap/environment-evidence.json). Every original PNG passed CRC and decompression validation. The critic inspected eight candidate/baseline pairs. Parent additionally inspected circuit frames 360,455,678 and1004; the gallery approach/interior, crest and station remain readable. Frame 678's warm craft wash also exists in the previous accepted capture, so it is not a newly demonstrated light-spill regression. All comparisons are nearby views, not exact-pixel matches.

The 42.75-second full-lap video spans recorded start-line crossings 15 through 1040; the 24-second preview is the continuous frame 130–705 excerpt. Both are actual 1080p native rendering with automated steering at 24 simulation frames per second, silent. Their frame counts, durations, hashes and complete decoder passes are recorded in [video validation](../../evidence/lighting-depth-final/video-validation.json). No continuous subjective playback acceptance is claimed.

All 42 existing tests pass.193 prebuild source/resource/settings hashes and 189 application-file hashes remain unchanged. The local macOS ZIP contains 441 entries, totals 56,664,092 bytes and passes CRC integrity validation. [Build manifest](../../evidence/lighting-depth-final/build-manifest.json).

## Completed native checks and preserved interruptions

The first 1280×800 capture stalled while the Mac was locked. A resumed launch also began while the tool still reported a locked display and produced no frames. Both incomplete attempts are retained; only the task's own stalled player processes were stopped. After the unlock reply, a fresh launch completed normally. This required no source or build changes.

Fresh 16:10 and ultrawide captures each complete 360 simulated pose frames and five station-context images on the exact reviewed build. All 10 PNGs pass CRC/decompression checks. Parent inspected the 1280×800 warm-gallery middle and 1920×810 station reveal: road, craft and HUD remain readable. These checks do not cover alternate-aspect landmark approaches or continuous motion. [Capture verification](../../evidence/lighting-depth-final/capture-validation.json).

The separate 1920×1080 real-time native run completes three player laps in 128.32 seconds, with zero recoveries for all six racers. Both results-to-racing and paused-race-to-racing restart launches pass; all three countdown-pause checks pass. The 60-second sample has 7,184 frame intervals, mean 8.35ms, P95 9.21ms and P99 9.33ms, with 240.7MiB Unity allocated memory. This is consistent with the previous accepted repeat 8.38/9.23/9.33ms; no material regression is demonstrated and no repeat is required. No Editor build, image encoding or asset generation overlapped the measurement. A pre-existing idle Editor remained untouched. Observed frame intervals are not isolated GPU timings or a guarantee of all future sessions. [Race/performance comparison](../../evidence/lighting-depth-final/performance-comparison.json).

Post-run source/app/archive hashes remain unchanged. The earlier landmark-build hitch sample is preserved with undetermined cause; it is neither hidden nor substituted for this run.

The stronger thermal service depth and restrained mast fill are accepted. Remaining broad art limits include simplified glazing and weak fine-texture evidence. This report does not expand the current art scope or authorize a GitHub release.
