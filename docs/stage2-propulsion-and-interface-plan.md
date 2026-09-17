# Stage 2: propulsion and the complete player interface

Status: implementation authorized across 2A, 2B, 2C and 2D. The owner's instruction "finish 2ABCD" supersedes the intermediate review stops below. Complete the integrated native candidate, then report and stop before Stage 3.

## Outcome

Make Vector Rush feel powerful and coherent from launch through racing, results and retry. Exhaust visibly communicates thrust; the HUD makes decisions readable at speed; every player-facing screen belongs to the same game.

Stage 1's named playable candidate is the baseline. Preserve its opening-city composition, track, handling, camera behavior and craft geometry. Stage 2 changes propulsion presentation and player interfaces. Broader environment surface/lighting work remains Stage 3.

## Current evidence

The owner's close exhaust screenshot shows a thin, short blue plume with little visible volume. The proposed change is fuller propulsion with clearly distinct cruise, acceleration and boost states; its quality must be judged in motion from the driving camera.

Source inspection finds the existing player UI in `UnityProject/Assets/Scripts/Presentation/RaceHUD.cs`: title, telemetry/minimap, countdown, lap messages, pause, results, settings and binding capture. Preferences and menu navigation live in `PlayerPreferences.cs`. `ProductionSettingsUI.cs` is a separate production tool and must be distinguished from player settings before changing its visibility.

The current HUD separates position/lap, clock, minimap and speed/boost into four corners. The direction below retains familiar information locations while improving hierarchy and state feedback. Current volume controls are button-driven incremental values; replace these with clear adjustable controls. Settings and bindings use long fixed-position lists, so fitting and navigation at smaller resolutions need explicit checks.

Propulsion already has throttle/boost inputs in `IonPropulsion.cs` and related effects in `VehicleVFX.cs`. Inspect and improve those paths before adding a competing exhaust system. UI currently renders with runtime IMGUI; keep authoritative gameplay and preferences intact, and avoid making a framework migration a prerequisite for visible progress.

## Shared visual direction

Precise futuristic racing instruments, with strong typography and restrained decoration. Use dark translucent surfaces, warm-white primary text, cyan for propulsion/energy, lime for selection or positive results, and amber for warnings. Pair color with labels, shapes or icons.

Use one type scale, spacing system, panel treatment, button family, focus treatment and motion language across every screen. Keep the driving line and craft unobstructed. Animate meaningful transitions: boost ignition/release, position changes, countdown, lap/sector results and menu focus. Persistent readouts should remain stable rather than constantly pulsing.

## Delivery order

### 2A — establish the visible direction

Create one concise design board using current native gameplay imagery: normal HUD, boosted HUD, title and results. Include button/focus states and a settings row. Show intended exhaust shape in the context of the normal chase view, clearly labeled as a target until it is native.

Define shared colors, typography, spacing, input prompts and propulsion state vocabulary. Identify the legacy UI elements to retain, replace or remove. Capture the baseline screens and the actual cruise/acceleration/boost sequence before implementation.

Deliverable: visual direction board, screen inventory, exact implementation ownership and acceptance checks. Review this direction before applying it to every screen.

### 2B — propulsion and the racing HUD

One propulsion owner implements:

- Fuller tapered main-engine plumes with a white-blue core, softer cyan envelope and irregular restrained turbulence.
- Cruise, acceleration, sustained boost and release transitions driven by real vehicle state. Thrust follows demand; speed alone must not determine flame strength.
- Distinct boost ignition, longer/wider sustained plume, short trailing particles and smooth decay. The small central nozzle remains subordinate to the main pair.
- Local nozzle/body/road response where supported, with limited brightness and no obscuring haze. Avoid excessive overdraw or a uniform blue wash.
- Correct behavior during braking/coasting, empty boost, countdown, pause, restart and finish. No particles continuing to simulate behind a paused game.

One UI owner implements:

- A coherent speed/boost instrument with an immediately readable speed and a stronger energy gauge. Distinguish available, active, low and recharging boost.
- Compact position/lap information; useful nearby-rival cues with understandable direction and gap.
- Secondary clock/best-lap/sector information with stable alignment. Show ghost disabled/unavailable/active and lap eligibility accurately.
- Clear countdown, final-lap, position-change and sector messages that expire without obscuring the bend.
- A legible minimap with distinct player/rival markers and reduced decorative labels.

Deliverable: a playable candidate and an uncut native cruise → accelerate → boost → release sequence, plus matched normal/boost screenshots. Test low energy, pause/restart and a race finish.

Acceptance: with HUD hidden, thrust states are distinguishable; with HUD visible, speed, boost state, position and lap can be read at a glance. Neither exhaust nor interface obscures the route. The new boost presentation must be obviously different from the baseline at normal gameplay scale.

Stop and report after this playable milestone before rolling the treatment through the remaining screens.

### 2C — all player-facing screens

| Screen or state | Intended result |
| --- | --- |
| Title / main menu | Confident Vector Rush identity over the game world; Start Race is the dominant action; clear settings, controls and quit access. Present the existing circuit accurately. |
| Countdown / race start | Deliberate timing and hierarchy; controls and race information readable without a tutorial wall. |
| Pause | Immediate, legible Resume; separate Restart, Settings and Return to Title. Make quitting the application a distinct action. |
| Results | Final position and official time lead; readable race order, best lap and supported personal/sector comparisons; a prominent Race Again action. |
| Settings | Organized Audio, Controls and Display/Comfort groups using working sliders/toggles; support existing settings first. Add HUD scale and reduced interface motion if needed for the new design. |
| Controls / rebinding | Current bindings, device-appropriate prompts, clear listening/cancel/conflict states, and reset behavior that works with navigation. Do not promise controller remapping unless implemented. |
| Record / ghost messages | Honest first-run, no eligible lap, disabled ghost, incompatible record and save-error states, using the same visual system. No separate progression or records browser is required. |
| Navigation / transitions | Consistent pointer hover, keyboard/controller focus, back behavior, disabled states and readable transitions across every screen. |

Reuse the approved 2B visual system. Every visible option must work. Keep debugging and production-tuning controls out of normal player flows, while preserving developer access.

Deliverable: one playable build and a screen gallery covering normal, focused, disabled and error/empty states where applicable. Show the full launch → race → pause/settings → finish → retry loop.

### 2D — integration and delivery

Check the entire experience in one identified native build. Validate pointer input and keyboard-only navigation, then controller behavior on available physical hardware. State any hardware gap explicitly.

Check 1280 × 720, 1600 × 900, 1920 × 1080 and a wide aspect ratio. Verify safe margins, long labels, small text, selected controls and settings scrolling. Check persisted settings and bindings across relaunch, pause/input isolation, repeated restart, record/ghost states and race-result accuracy.

Run targeted tests for state/data regressions, native visual/input checks, and a separate before/after frame-time check around sustained boost. Silent simulation-time captures do not establish sound quality or real-time performance. Review sound only if audio is changed.

Deliverable: named default-on playable app, comparison images, continuous propulsion and full-interface walkthrough clips, concise findings and remaining issues. Stop for owner review. This completes Stage 2 delivery, not owner acceptance or Stage 3 authorization.

## Parallel ownership when implementation starts

| Role | Ownership | Boundaries |
| --- | --- | --- |
| Propulsion | `IonPropulsion.cs`, propulsion-only shaders/assets, exhaust-specific portions of `VehicleVFX.cs` | No HUD, handling, camera or world changes. |
| Interface | `RaceHUD.cs`, UI-only components/assets and shared visual styles | One owner for all screens; no separate agents editing HUD and menus in the same file. |
| Integration / validation | Build/capture tools, narrow shared preference/state contracts, native verification, packaging | Owns shared bootstrap changes and serializes heavy processes. |

One visual lead owns the overall image and consistency between HUD and propulsion. Independent critique occurs at the native milestones, with actual failures recorded. Use the established single-heavy-process lease for Blender, Unity and native captures.

Freeze the tiny shared contract before parallel edits: actual throttle demand, actual boosting state, boost fraction, race phase, selected input device and existing record/ghost state. UI and VFX observe gameplay; they do not invent race state or alter physics to produce a showcase.

## Completion standard

Visible improvement takes priority over implementation volume. Each milestone needs a result the owner can inspect in the running game. More widgets, particles, files or passing tests do not establish quality. Preserve the previous candidate and failed comparisons, commit scoped milestones, and follow the existing private-remote restriction.
