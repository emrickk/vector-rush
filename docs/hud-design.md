# Racing HUD

The Nocturne HUD uses four corner groups: position/lap, race timing, the live course map, and a combined speed/boost instrument. The road center stays clear during ordinary racing. Brief countdown and lap-transition messages use that space only when relevant.

Rajdhani SemiBold provides bundled native typography. Copyright (c) 2014 Indian Type Foundry; distributed under SIL Open Font License 1.1. Font and complete license are in `UnityProject/Assets/Resources/Fonts/`. Source: https://github.com/google/fonts/tree/main/ofl/rajdhani . No system-installed font is required.

The map samples the real TrackPath with fixed orientation and preserved proportions. Markers use live world positions, so they can show a craft departing the course. The player has a directional lime arrow; rivals have smaller white squares; a white cross-track line marks the start. Speed follows actual speed with a short smoothing response; the arc fills against the vehicle's authored boost speed. Energy is the actual boost reserve. Active boost changes both the text and energy color. No RPM, gears, sector delta or invented timing is displayed.

The race timer uses smaller milliseconds. Best lap remains unavailable until an actual lap is completed. Lap messages are triggered by real lap transitions, fade in over 0.2 seconds and leave after 3 seconds of race simulation. Pausing freezes the simulation. Pause and results omit underlying telemetry to avoid stale values competing with their controls.

The root HUD preserves the original native pointer-coordinate correction. Lines compose their local rotation after the canvas scale; the first capture exposed a RotateAroundPivot scaling error, retained in the first native evidence set and independent critique. The second capture validates that correction at non-unit scales.

Native screenshot coverage and automated driving are evidence of rendering and state coverage, not a human handling, physical-controller or AAA-quality certification.
