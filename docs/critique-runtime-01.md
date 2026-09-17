# Independent runtime visual critique 01

Evidence inspected directly: `evidence/run-01/01-title.png` and `02-start.png`, both 1920 × 1080 screenshots of the actual standalone game. Also inspected the repeated font errors in `evidence/player-run-01.log`. This review is separate from Blender asset renders and concept art. A complete race was still running elsewhere; this reviewer has not yet inspected its outcome.

**Verdict: visual quality fails the stated aspiration, and the current interface has a functional readability failure.** The runtime demonstrates that the track and craft render in the application. These two images do not demonstrate a playable full race, stable collisions, handling quality or performance. The runtime image is materially below the art-direction reference; that reference remains a target, not a delivered image.

## The three changes to make first

### 1. Repair the font path and verify every essential field

**Observed:** The title image has no large VECTOR/RUSH title. “ANTI-GRAVITY RACING / VOL. 01” repeats where several differently sized text items should be. The start button contains exhibition copy rather than a clean action label. The race image repeats POSITION and LAP while their numeric values are absent; race time, speed and reserve percentage are also missing or replaced. This breaks basic race awareness, not just decoration.

**Supporting log:** `player-run-01.log:32` begins repeated “Unable to load font face for [Avenir Next Demi Bold]” errors, interleaved with “Can't Generate Mesh, No Font Asset has been assigned.” The font failure is confirmed. The exact mechanism of every duplicated string is not proven from the log alone.

**Fix:** Use a bundled known-working font or a verified built-in font for every style; do not accept a non-null OS font object as proof the face can render. Recapture title, race HUD and results in the standalone player. Verify the actual game title, action labels, position, lap, running time, speed, percentage and finish time. Confirm the font errors have stopped before taking a meaningful performance sample.

### 2. Restore lighting and material separation in the actual chase view

**Observed:** In both images, the craft's graphite structure, right-side rival and road fall into near-black masses. On the other side, the sunlit barrier and pylon are pale yellow with little surface information. Hard shadows form strong rectangular bands along the running surface and visibly jagged edges around large nearby forms. This range makes the scene look flat and harsh even though some individual highlights are bright. The turquoise engine rings are visible as small blue details but do not read as luminous propulsion comparable to the asset render.

**Fix:** Establish ambient fill and usable environment reflections, then rebalance direct sun and exposure together. Preserve dark hull structure without crushing it into the road; retain texture/value variation on sunlit ivory. Verify shadow resolution/filtering and near-camera shadow quality. Judge the imported craft's metal, canopy and engines under this revised lighting; adding small mesh details before solving the value range will have little effect.

**Acceptance:** At full screenshot size and reduced viewing size, the player's nose, cockpit, pontoons and engines remain separable from the track and nearby rivals. Bright ivory retains shape and a controlled highlight. Check the same scene in both sun and shadow. The screenshots establish the defect, but do not alone identify whether its main cause is ambient setup, reflections, shadow settings, exposure or a combination.

### 3. Replace the strongest placeholder environment cues

**Observed:** The ocean consists of tightly repeated horizontal/wavy bands over nearly the whole visible water surface, with dense alias-like patterns at the horizon. The islands at left are smooth dark hemispheres. The grandstand is a set of bare, repetitive horizontal slabs under a huge plain canopy. These large screen areas dominate the view and visibly expose the environment's primitive construction. The running surface is likewise nearly a uniform dark plane broken by simple joints and stripes.

**Fix:** Start with the water because it fills the distant view: vary wave direction and scale, suppress high-frequency detail with distance, and create a more natural broad reflection gradient. Give the closest island an irregular rocky outline and the nearest grandstand a believable structural rhythm, seating treatment and supports. Add restrained road surface variation that leaves the racing line legible. Do not spread tiny decoration uniformly across the entire map.

**Acceptance:** The ocean should stop reading as a repeated striped texture in a stationary capture and in motion. The closest environment silhouettes should no longer look like a sphere and stacked boxes. A revised capture must show actual runtime results.

## Camera, composition and race readability

- The title camera places the course curve and water ahead, which establishes the direction of travel. The central craft's twin-pontoon outline is visible. Those are useful elements to preserve.
- In `02-start.png`, a pursuing craft fills much of the lower center and overlaps the rear of the farther central craft. It also competes directly with the thin circuit-progress strip. From the still image, this makes it difficult to identify immediately which ship the camera is following. It does **not** prove that craft collision failed; depth and motion need recorded evidence. Verify overtakes from behind, camera clearance and whether the camera can move through or be obscured by another racer.
- The overhead starting gantry becomes a solid black band across the top of `02-start.png`. The large grandstand canopy also takes a substantial upper-right area. Reassess camera height/focus and the grid presentation once the ship identities and typography are readable; these nearby forms currently crowd the first racing view.
- The barrier caps and lane lines clearly outline the upcoming left bend. However, heavy shadow bands and dark road values reduce its surface depth. Check a tight bend at speed after the lighting correction, rather than declaring readability from this start-line still.
- All visible craft carry **07**. Hull accents differ slightly, but dark lighting hides much of that differentiation. Once the font and light issues are fixed, verify that the player and competitors can be distinguished during a close pack. This is visual identification feedback, not a demand for separate hero models.

## What this review does not establish

No claim about frame rate, complete race success, race fairness, recovery correctness, collision behavior, camera motion comfort or shader shimmer in motion is made from these screenshots. Those require the corresponding recorded play, telemetry and results. The obvious static water repetition is observed; its temporal shimmer is a risk to check, not an observed fact.

Rebuild after the highest-impact corrections and capture the same title/start views plus a clean mid-lap chase image, bend, pause and results. Keep these initial images so the second runtime review can judge visible changes directly.
