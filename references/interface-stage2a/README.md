# Vector Rush interface references

Editable visual direction for the HUD and player screens. These are design mockups over exact native game captures, not screenshots of an implemented Unity interface.

[Open the interactive reference](index.html) to switch between the current HUD and the proposed race, boost, title, results, pause and settings screens. The page works from disk, with local fonts and images; no server is required. [View the complete board](reference-board.jpg).

## Direction

Use a motorsport identity with tall, strong numerals, stable alignment, fewer framed panels and generous space around the racing line. Warm-white carries information; cyan indicates propulsion; lime marks selection and improvement. Position and speed lead, with lap, energy, rival gap and timing subordinate.

The speed instrument replaces the large circular dial with a compact numeric readout, speed line and segmented boost gauge. The proposed boost state changes the energy treatment and adds a restrained peripheral cue. This is a HUD-state reference only: the background is the same cruising capture, and new exhaust effects have not been authored.

Title, results, pause and settings use the same typefaces, accents, spacing and selection treatment. Clear action labels take precedence over promotional slogans. Settings use actual range inputs and toggles in the prototype; they do not change game preferences.

## Images

| File | View |
| --- | --- |
| [01-race-hud.png](01-race-hud.png) | Normal racing HUD |
| [02-boost-hud.png](02-boost-hud.png) | Proposed boosted HUD |
| [03-title.png](03-title.png) | Title and primary actions |
| [04-results.png](04-results.png) | Position, race order and retry |
| [05-pause.png](05-pause.png) | Pause and navigation |
| [06-settings.png](06-settings.png) | Audio settings |

The exact local design brief is implemented in `index.html`, `style.css` and `app.js`. Open Font License Barlow and Barlow Condensed font files are included with their license.

## Provenance and limits

The built-in image generator was unavailable. No image-generation API or paid fallback was used. Designs were authored in HTML/CSS and rendered with Playwright.

Native background build: `cd2cc2e880034d589b3c88c562148114`. Diagnostic source commit: `32d1cf2`. The diagnostic hides RaceHUD only with explicit opening-capture flags; normal launch is unchanged. All recorded camera and vehicle poses match Stage 1. The same build with normal HUD produces three PNGs byte-identical to Stage 1. Captures and checks live under `artifacts/opening-city/stage2a-ui-reference` in the shared workspace.

UI numbers are illustrative fixtures, not a new performance result or recorded gameplay state. Rival gaps use metres, matching the existing game. Personal-best/sector fixtures illustrate the eligible-record case. Implementation must suppress these values for invalid or absent records, use saved bindings and authoritative gameplay state, and distinguish ghost disabled from unavailable. HUD scale and reduced interface motion are proposed controls.

Independent review found stronger hierarchy and coherence than Stage 1 but did not establish AAA quality. Corrections made: larger secondary labels, stronger minimap backing/strokes/markers, metres for rival gaps, removal of a detached title arrow and generic slogan, and accurate prototype controls copy.

Verified six full-resolution PNGs, local file loading, font/map loading, screen navigation, pause/resume, a keyboard-adjusted slider, toggles, baseline comparison and 1280-wide preview fitting. This does not establish native gamepad navigation, UI animation quality, propulsion effects, in-game performance or owner acceptance.

## Next implementation handoff

Use these as the 2A interface direction for review. The remaining 2A propulsion-shape reference and the native 2B implementation are still open. Do not import these full-screen screenshots as the game UI. Rebuild the approved elements as live views driven by existing state, then compare the native result at the same gameplay scale.
