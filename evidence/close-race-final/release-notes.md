# Close-racing update — 2026-09-08

The player now uses a 53 m/s cruise and 72 m/s boost preset. Rival steering follows the pursuit geometry, and bounded lateral assistance helps AI craft clear outer-wall contact through ordinary physics.

In the final pacing run, the nearest rival stayed within 60 m of validated race progress for 80.9% of the post-start race, sustained across all three laps. All six racers had zero recoveries. The nearest finish gap was 100.85 m; no overtake was demonstrated in this automated run.

The requested final visual critique led to a compact nearest-rival HUD cue and softer cool-gallery lighting. The native macOS build and current silent gameplay preview are attached. Recordings use automated steering at 24 simulation frames per second; they are separate from the real-time performance check.

Source, editable assets, tests, rejected pacing candidates and independent critiques are preserved in repository history. This remains a one-course prototype; broader city expansion and subjective human driving/audio assessment remain future work.

Validation: 42/42 Unity tests passed. Separate real-time race completed three laps in 128.32 seconds with zero recoveries, and restart/countdown-pause checks passed. At 1080p on Apple M2 Max, observed P95 frame interval was 9.22 ms (not isolated GPU timing). Archive integrity and hashes are preserved in the source repository.
