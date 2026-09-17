# Independent review 009 — targeted ship contact repair confirmed natively

Reviewer: Astra. Date: 2026-09-07 PDT. **Decision: accept the targeted contact repair and advance with the corrected geometry. Keep the major form frozen.** The inspected neutral native views resolve the previously observed jagged canopy/intake/nozzle overlaps. This control retains pass03 maps; it is not acceptance of the pending final maps, race motion or overall AAA quality.

## Evidence and control conditions

Directly inspected all eight images in `evidence/ship-native-v4-control-01`:

| Capture | Scope |
| --- | --- |
| [00 — ordinary gameplay chase](../../evidence/ship-native-v4-control-01/00-native-gameplay-chase.png) | Player framing and intact ship integration; gallery/plume updates are also present. |
| [01 — rear three-quarter](../../evidence/ship-native-v4-control-01/01-studio-rear-three-quarter.png) | Both aft housings, intake edges, canopy sill, markings and overall form. |
| [02 — front three-quarter](../../evidence/ship-native-v4-control-01/02-studio-front-three-quarter.png) | Forward form, both intake returns and glazing connection. |
| [03 — starboard side](../../evidence/ship-native-v4-control-01/03-studio-starboard-side.png) | Low profile, shell transitions and preserved large volumes. |
| [04 — orthographic top](../../evidence/ship-native-v4-control-01/04-studio-top-ORTHOGRAPHIC.png) | Both intake openings, livery and negative spaces. |
| [05 — engine emission OFF](../../evidence/ship-native-v4-control-01/05-studio-engine-emission-OFF.png) | Clean outer armor/Graphite boundary with physical nozzle surfaces visible. |
| [06 — engine emission ON](../../evidence/ship-native-v4-control-01/06-studio-engine-emission-ON.png) | Same boundary and pose with the compact core and thin annulus restored. |
| [07 — canopy close](../../evidence/ship-native-v4-control-01/07-studio-canopy-close.png) | The previously defective lower glazing/sill interface and an intake mouth. |

For direct before/after comparison, used the pass03 [canopy](../../evidence/ship-native-v3-01/07-studio-canopy-close.png), [engine ON](../../evidence/ship-native-v3-01/06-studio-engine-emission-ON.png) and [rear](../../evidence/ship-native-v3-01/01-studio-rear-three-quarter.png) counterparts. The wider pass03 set was inspected in review 006.

The [control identity](../../evidence/ship-native-v4-control-01/asset-identity.json) records corrected FBX SHA-256 `f2cfabf5a3ff1f5785debb1c2b81a238f39073f784fc8ec134b4e71c1c397386`; the engine-anchor file retains SHA-256 `ade72d97ffe3c6ce32b360544272cc993199cf3026840503577ff7f356a0718d`. I compared all 16 recorded runtime surface-map hashes against the pass03 map manifest: every hash matches. Native material records and studio lighting descriptions also match the earlier inspection.

The [scope](../../evidence/ship-native-v4-control-01/inspection-scope.json) records actual 1920 × 1080 Unity 6000.6.0f1 screenshots on Apple M2 Max, captured 2026-09-08 04:39:11–04:39:20 UTC, with eight calibration checks and a completed reflection probe. Neutral views retain fixed light, ACES at exposure 0, no bloom/vignette/fog and no propulsion effect. Studio camera/lens parameters match pass03, apart from a negligible floating-point side-camera height difference; the recorded race clock differs while the studio craft is stationary.

Frame 00 is useful integration context, but it is not a controlled comparison against the old pass03 gameplay image: the plume/lighting implementation and sampled race pose have also changed. The geometry-repair conclusion comes from the matched neutral views and unchanged maps.

## Visual finding

**The lower canopy is now continuous.** In old frame 07 a broad silver/gray shard with a stepped boundary intruded across the lower black glazing. The new close view replaces that interruption with a clean, continuous glazing region and a narrow defined sill beneath it. The intended crown/shoulder highlights remain. Rear three-quarter confirms that the conspicuous angular fragment is gone at full-ship scale as well.

**The intake and nozzle borders are clean.** Old frame 07 and the rear view showed ragged pale intrusions along the intake mouths. The new close/rear/front/top views show deliberate straight returns and clean corners on both nacelles. In the matched engine pair, the old serrated overlap at the upper white armor/Graphite boundary and lower white return is absent. The shell and backing read as distinct adjacent surfaces. A dark gap remains where construction is recessed; it does not appear as a new missing major part.

**The accepted ship identity is retained.** The low twin-nacelle proportions, white armor, dark center, glazing crown, physical chambers, compact cores, thin cyan annuli and flush 07/citron markings remain consistent. No major silhouette change, displaced engine or lost primary graphic is visible. The wider frames reveal no new conspicuous defect attributable to this local repair.

The source [correction report](../../evidence/ship-v3/pass-04-contacts/README.md) and [validation summary](../../evidence/ship-v3/pass-04-contacts/validation-summary.json) now provide a measured explanation for the earlier visual artifacts: competing Graphite surfaces at the glazing and armor returns. They record changes to three Graphite objects and 1,165 runtime vertices, with armor/glass/engine positions, runtime topology and all UV coordinates retained. The named competing coplanar contacts were removed, and sill/glazing near-parallel centroids within 2 mm fell from 145 to zero. Other hidden/noncoplanar assembly contacts remain; this is not a claim that every surface intersection in the asset was removed. Those measurements were reviewed as supplied technical evidence, not independently recomputed here. The native comparison independently supports the intended visual correction.

## Triangle-count concern is accounted for

Review 006 left the 38,036 staged versus 37,686 native triangle difference unresolved. The subsequently supplied [triangle reconciliation](../../evidence/ship-v3/triangle-accounting-pass03/README.md) and [detailed record](../../evidence/ship-v3/triangle-accounting-pass03/triangle-reconciliation.json) use the exact pass03 FBX identity and account for all 350 differences: 330 Ceramic and 20 Engine triangles. Direct FBX parsing found 332 duplicate-position degenerates; converting centimeter coordinates to float32 collapses 18 more. That emulation matches every per-mesh native reduction, and all seven mesh categories are present.

This is a sufficient explanation for the earlier count discrepancy and supports import-time degenerate removal rather than a missing object. The exact internal importer operation was not instrumented, so do not present the emulation as an importer call trace. The current controlled native capture retains 37,686 triangles and the same per-mesh triangle totals. Its Graphite vertex count changes from 9,328 to 9,354 while its 9,034 triangles remain unchanged; the source/runtime position correction should not be described as an identical imported vertex inventory.

## Remaining scope and next check

Proceed with the corrected geometry and refreshed contact-dependent maps. This unchanged-map control deliberately does not validate new Graphite grain/AO or any cross-material contact-occlusion update. Inspect the final map identity and matched native canopy/intake/nozzle/rear views after the refreshed maps are imported, then check ordinary race framing and continuous motion.

Soft/blocky paint edges close to the dark spine, simple panel/mount finish, the scene's patterned road highlights and its dark/repetitive architecture remain separate follow-ups. They are not evidence that this contact repair failed. No further broad ship redesign is warranted by this control set, and no final-map, motion, playability or AAA pass is awarded here.
