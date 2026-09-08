# Independent runtime critique 02

Evidence inspected directly: all four 1920 × 1080 standalone screenshots in `evidence/run-02`, `runtime-metrics.txt`, `race-verification.txt`, final `live-telemetry.txt`, selected rendering source, and the Solstice chase concept. The earlier runtime critique was read for comparison. No game or Editor was launched by this reviewer.

**Verdict: a materially improved, readable racing prototype with a demonstrated automated race completion. It still fails the AAA visual aspiration by a substantial margin.** The concept has natural terrain, convincing reflected light, rich but restrained surfaces and luminous propulsion; the delivered runtime still has obvious primitive architecture, flat/faceted terrain and serious road shading artifacts. The concept must not be presented as a screenshot of the delivered game.

## What visibly improved

The typography failure is repaired. The actual VECTOR RUSH title, Start Race action, position, lap, time, speed and energy values are readable and appropriately separated. The results screen clearly reports first place, three laps, 01:52.578 total time and 00:37.183 best lap. No font/error matches were found in the inspected run-02 log search.

Ambient fill makes the craft's cockpit, pontoons and engine housings substantially easier to distinguish from the running surface. The player is clearly framed in the midrace image, with a useful view of the next crest. The road boundaries remain readable. The water has broader, quieter waves than run 01, and the islands now have irregular rocky outlines instead of hemispherical silhouettes. Those are real improvements, although the scenery remains visibly procedural and coarse.

## Three highest-impact changes for the final limited polish

### 1. Remove the road shading bands and jagged distant streaks

This is the most urgent visible defect. In `03-race.png`, alternating dark/light bands run across the nearby road, while dense jagged lines converge near the crest. Similar streaks appear around the title/start bend. They obscure the surface and give the track an unstable, corrugated appearance. Motion shimmer is not established from these still images.

The likely causes need a controlled visual check, not more decorative texture. Source inspection found directional `shadowBias=.025f` and very thin expansion-joint boxes that retain default shadow casting. Surface shadow acne, joint shadows or track normal/bank sampling are plausible contributors; this reviewer has not experimentally isolated the cause. Briefly compare the same chase view without road-received shadows, use appropriate depth/normal bias, and disable shadow casting on the tiny deck joints if they cause artifacts. Inspect normals if banding survives that change.

**Acceptance:** the nearby surface and distant crest remain coherent, while meaningful craft and structure shadows remain present. Preserve joint rhythm without the dense black zigzags. Verify a native screenshot from the same midrace position.

### 2. Give propulsion a credible focal point and preserve hull material depth

The turquoise engine interiors are readable but resemble painted mechanical fans. Long flat turquoise trails dominate their actual brightness; they do not sell powered anti-gravity thrust. Light gray hull panels and ivory barriers also have little material depth, while the darkest lower hull areas still lose detail. The reference's engine glow and controlled specular variation are plainly absent.

For a bounded pass, concentrate on the two rear engine cores: use a compact bright emissive center, a restrained halo if the rendering pipeline supports it, and short tapered exhaust that reads at chase-camera distance. Retain the now-useful ambient fill. Adjust the hull/reflection response only where the same capture proves greater shape separation; indiscriminately increasing exposure would bleach the barriers further. All observed craft still carry 07, so the existing prototype offers weak competitor identity beyond position.

**Acceptance:** the propulsion reads as light at reduced viewing size, the engines remain distinct from flat trails, and the cockpit/pontoons remain legible in both shadow and sun.

### 3. Reduce the grandstand's strongest placeholder silhouette

The huge plain canopy and uninterrupted terrace strips fill much of the title/start/results background. The title composition and interface are now strong enough that this blank geometry stands out more. The islands have improved silhouettes, but their triangular color patches and the empty block towers still signal prototype scenery.

Spend the limited environment pass on the closest grandstand: add a believable support rhythm, divide the canopy into intentional bays, and break the continuous seating strips into readable blocks. A smaller, better resolved structure will improve these repeated views more than scattering small props around the map. This will not close the broader terrain/material gap to the concept.

**Acceptance:** the first racing view reads as a supported spectator structure rather than a large floating slab above stacked strips.

## Race health and performance scope

Run-02 verification records **FINISHED, 112.58 seconds, three laps, zero player recoveries** using autopilot through the actual hover physics. This is a substantial improvement over run-01's recorded 155.43 seconds and seven player recoveries. Three restart checks each report a frozen countdown while paused and zero reset laps. The results screenshot agrees with the recorded finish time after rounding.

At the player's finish, the five rivals were at roughly 2.59–2.76 laps and had been stopped by the finished race state. One rival had one recovery; the other four had none. This supports an operational race loop but does not prove all rivals completed, close competitive balance, or human handling quality. The large player lead remains a balance limitation in this test.

The measured sample contains 3,758 frame intervals on Apple M2 Max at 1920 × 1080 with VSync enabled: mean 15.97 ms, median 16.65 ms, p95 16.82 ms, p99 16.89 ms; allocated Unity memory 184.8 MiB. It supports stable sampled frame delivery near the display cadence. It is not isolated GPU timing, a minimum-spec claim, a broad performance certification or evidence about unmeasured portions of the race.

**Prototype acceptance:** readable title/race/results presentation and one verified automated full race pass. **AAA acceptance:** fail. Manual keyboard/controller comfort, overtakes and camera obstruction in motion, sustained performance on additional hardware, race balance and production visual finish remain unproven or below target.
