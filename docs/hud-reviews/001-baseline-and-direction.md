# Independent HUD review 001 — baseline and premium racing direction

Reviewer: Astra. Date: 2026-09-07 PDT. **Decision: replace the baseline race HUD's fragmented card layout with four compact, coordinated instruments.** This is a design prescription, not acceptance of an implemented premium/AAA HUD. Scope is interface presentation and truthful use of existing race state; world and vehicle changes are excluded.

## Inspected baseline

Directly inspected the current native [gallery race view](../../evidence/night-v4-motion-02/selected/amber-mid.png) and [boost view](../../evidence/night-v4-motion-02/selected/observed-boost.png), both at 1920 × 1080. Their [capture identity](../../evidence/night-v4-motion-02/asset-identity.json) preserves the build context. Also read the original `RaceHUD.cs` telemetry layout and the published race/vehicle/track state available to it.

The baseline is legible in isolation but does not yet feel like a composed racing instrument. Its biggest problems are:

- The 442 × 185 speed card and separate 410 × 143 energy card occupy opposing lower corners. Reading acceleration and remaining boost requires a large eye movement.
- The 108 px speed number and heavy rectangular backplates compete with the relatively small player craft. A broad lower-screen shade adds weight beyond the content that needs protection.
- Position/lap are split into two cards, while the top-right clock has the scale of a headline. Labels, separators and leading zeroes create more display mass than necessary.
- Permanent centered circuit/brand text and a center-bottom progress ruler add visual noise without helping the next driving decision. The ruler gives no useful shape information about upcoming bends.
- The boost change is mainly a small text replacement and a meter drain across the other corner. It lacks one clear, locally grouped state transition.
- A bright scene lamp passes through the translucent timing area in the boost view. Backing contrast must work over both black sky and bright emitters.

## Controlling visual direction

Use precise race instrumentation: strong numerals, compact supporting text, disciplined alignment and generous open space over the craft and racing line. Preserve ivory primary text and charcoal backing; use citron for energy/race emphasis and cyan for active propulsion. Use accent color to communicate state. Extra borders, fake technical data and constant animation will not make the HUD more premium.

The parent's chosen layout is appropriate:

| Zone | Content and hierarchy | Starting size at 1920 × 1080 |
| --- | --- | --- |
| Top left | One compact position/lap group: large `1`, subordinate `/ 6`, then `LAP 1 / 3` on the same alignment system. | About 280–320 px wide, 70–85 px high; position 40–46 px. |
| Top right | Race time with subordinate milliseconds; a quiet best-lap row. Keep labels truthful to the available values. | About 290–330 px wide; primary timer 30–36 px, supporting figures 16–20 px. |
| Lower right | Unified speed and boost instrument: speed first, unit second, one adjacent energy rail and concise state. | About 300–340 px wide, 160–190 px high; speed 80–88 px. |
| Lower left | Actual circuit outline with a strong player arrow, smaller rival dots and a start/finish mark. | Approximately 200–240 × 150–185 px; preserve the route's aspect ratio. |

These are starting bounds for native comparison, not rigid requirements. Use a shared roughly 50–60 px safe margin at 1080p, with consistent baselines and spacing. Keep the center-bottom progress ruler and permanent center branding out of ordinary racing. A brief lap/countdown event may occupy otherwise clear center space, then leave.

## Details that will determine quality

**Typography and backing:** bundle the chosen font so the native player renders the same face. A condensed numeral face can provide racing character; labels must stay easy to read. Use stable-width numerals or fixed digit fields so the timer and speed do not shift as values change. Avoid artificial `000`/`01` padding where it adds no information. Prefer a tight local dark backing and restrained text shadow/keyline to large opaque cards or a blanket lower-screen gradient. Verify the actual face and contrast in the native player over warm lamps, blue road sheen and dark sky.

**Speed/energy relationship:** place the energy rail directly beneath or beside speed. Make its remaining amount legible at a glance, with a small percentage only if it helps. The active state should change the rail/one accent and show a short `BOOST` label in that same instrument. Low energy can use a clear label plus color change; it must not rely on color alone. Use a brief transition on state entry rather than continual flashing. Do not invent RPM, gears, heat or cooldown data that the game does not provide.

**Map:** use the actual sampled `TrackPath` shape, with markers positioned from published race/track progress. Preserve a stable orientation and enough bend detail to recognize the course. Distinguish the player by shape as well as color; keep rivals quieter and avoid numbering every dot. A cropped or decorative loop is not a substitute for correct navigation. Cache static map geometry; only markers need regular updates.

**Timing and events:** existing public state includes total race time, best/last lap, position, lap, boost and track progress. Label those values accurately. Do not show a purported live lap time, sector delta or rival time gap unless it is correctly derived. A lap event should come from a real lap transition and disappear after a short hold. Countdown, pause and results should use the same typographic hierarchy and accent rules without blocking their controls or revealing a stale race state.

## Native review gate

Review the implemented HUD in ordinary cruise, active boost, low/empty energy, lap transition, countdown, pause and results. Include bright-gallery and dark-exterior backgrounds. Use the planned 16:9, 16:10 and ultrawide captures to check safe margins, scaling, map proportions, clipped labels and overlapping controls. Preserve the existing pointer-coordinate correction when changing visual layout, and confirm drawn controls and hit areas remain aligned.

The first acceptance question is whether speed, energy, position and the next route shape can be read quickly while the craft and road remain the visual focus. Judge real state changes and a short continuous passage before claiming temporal refinement. A successful compile or a new font alone does not establish premium or AAA quality. Advance from this pass when the coherent native layout works; keep any further refinements bounded to the HUD.
