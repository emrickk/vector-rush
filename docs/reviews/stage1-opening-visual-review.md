# Independent Stage 1 opening-city visual review

Date: 2026-09-14 (America/Los_Angeles)

Status: **OWNER_REVIEW_READY for the inspected construction/composition checkpoint.**

The three inspected native stills show useful, visible progress and support owner review of the Stage 1 direction. They do not establish target-quality acceptance, integrated race acceptance, or motion quality.

## Evidence and method

Workspace: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/opening-city`

Inspected checkpoint build GUID: `8ff3c8c3fec64b40ac98f17ac97e990c`, verified in `../artifacts/opening-city/stage1-checkpoint-01/build/build-identity.json`.

Directly inspected these native sequence images:

- `../artifacts/opening-city/stage1-checkpoint-01/after/frames/frame-0131.png`: approach, visible race time `00:07.080`.
- `../artifacts/opening-city/stage1-checkpoint-01/after/frames/frame-0193.png`: bend, visible race time `00:09.660`.
- `../artifacts/opening-city/stage1-checkpoint-01/after/frames/frame-0265.png`: reveal, visible race time `00:12.660`.

Compared against the natural control anchors `01-approach.png`, `02-bend.png`, and `03-reveal.png` in `../artifacts/opening-city/early-02/before/`, plus the concept target `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush/references/nocturne-v2/A-opening-city.png`. The prior critique `docs/aaa-rebuild/critic-002.md` informed the target-direction checks. Invalid `*-matched` captures were excluded.

## Three visible improvements

1. **A second infrastructure layer is readable.** The amber lower viaduct adds a substantial curved route beneath the race road in approach and bend, strengthening the sense of elevation.
2. **The landmark has a distinct silhouette and better clearance.** The asymmetric connected tower appears left of the approach curve and separately on the right at the bend. It removes the previous pale structure crowding the bend's right edge. A useful new landmark does not require copying the original twin-prong tower.
3. **The lower city has more intermediate detail.** Stepped buildings and articulated facades introduce additional masses between the foreground and distant skyline, especially at the reveal.

The road boundaries, cyan rails, player craft, visible rivals, and HUD remain readable in all three stills.

## Remaining quality and limits

The reference still has much stronger construction, material separation, and controlled road reflections. The candidate road remains broadly smooth and flat; supports and roofs look primitive; the reveal contains a large blank roof in the lower left. Black gaps persist below the city, lower-road light pools repeat conspicuously, and the bright lamp strips have irregular edges.

These are three stills. No claim is made about motion continuity, popping, flicker, clipping during traversal, audio, performance, or human handling. Matching visible HUD times and poses does not independently establish full capture parity.

The art lane reports that the subsequent candidate retains this art and changes capture code only. **Parity between that newer candidate and this inspected checkpoint remains pending external validation.** This review applies only to the GUID and images listed above.

Recommendation: present this bounded Stage 1 checkpoint for owner direction review. Keep finished visual acceptance open, with road response and weak foreground construction as the next quality priorities.
