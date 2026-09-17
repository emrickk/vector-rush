# Gallery 02 source contract review

Date: 2026-09-09  
Status: **BLOCKED before import/build**

This is a source-only snapshot taken while the artist and Unity files were actively changing. The parent must re-read the cited files after both workers stop before applying corrections. The emitted `manifest.json`, `layout.json`, and `gallery-lighting.json` confirm the recipe mismatches below. `STAGED.json`, `checksums.json`, the source blend, and the art handoff were not yet present when captured, so their absence is not scored as a worker failure. I did not run Unity, Blender, a native build, Git, or inspect candidate appearance.

## Required corrections, in order

1. **Use the canonical course identity in the art package.** `build_exemplar_gallery02.py:22-26` assigns the exact JSON-byte SHA `598718...` from `geometry-context.json` to `course_hash`. The current importer requires every manifest/layout/staging/lighting `courseHash` to equal runtime `AAACourseIdentity.Compute`, whose independently recorded value is `8c2161b930d43c505c8380ce30a719e6833a453d671353994f79b75e4c67a79e` (`course-identity-check.json`; `AAAExemplarImport.cs:204-215`; `AAACourseIdentity.cs:23-36`). Minimal correction: compute the documented `VRCRS1` little-endian float32 stream from `course-data.json` and assign that digest to `course_hash`; keep `course_source_hash = sha256(exact JSON bytes)` only in `manifest.courseSource.sha256`. Do not weaken either importer check.

2. **Make the new package gallery-only.** The recipe still copies the two opening assets and previous stats at `build_exemplar_gallery02.py:327,461-468`, then emits `opening-civic-right` and `opening-service-left` as `viaduct` placements at `499-500`. Gallery import rejects every non-`station` placement (`AAAExemplarImport.cs:267-287`), and the opening placements also fail the `.86-.902` span gate (`355-367` in the current snapshot). Minimal correction: remove the two opening placements, the two copied opening assets, and the previous-package stats seed; initialize stats locally for Gallery 02. Preserve `exemplar-01` untouched as the separate legacy package.

3. **Allow an absent opening resource throughout the gallery-only path.** The authoritative parent note says no opening resource exists and absence is valid. Import still requires it before gallery publication, including in the newly added identity-migration branch (`AAAExemplarImport.cs:124-137`), and again through `RequirePublishedResources` plus `ValidateRequiredStaging` (`168-171`; `AAAExemplarValidation.cs:13-25`). The build repeats the same requirement and dereferences `opening.revision` (`AAAReviewBuild.cs:77-83`). Minimal correction: add a gallery-required preflight that requires and validates `GalleryExemplar`, validates and identity-migrates `OpeningExemplar` only when present, and use it in gallery import and `AAAReviewBuild`. Keep the legacy require-both helper only for the legacy two-resource import path. Update `AAAImportContractTests.cs:58-75` and add coverage proving gallery import/build preflight accepts a missing opening but rejects a missing or invalid gallery.

4. **Connect fixture metadata to the schema already consumed by Unity.** The recipe emits `sourcePlacement` and `sourcePart` (`build_exemplar_gallery02.py:548-557`), but Unity reads only `housingInstanceId` and `socket` (`AAAExemplarImport.cs:404-406,607`). The current package would silently discard its geometry-to-light association. Minimal correction: emit `housingInstanceId: instance['id']` and `socket: socket['sourcePart']`. Because the handoff requires fixtures to correspond to authored housings, make both fields mandatory for Gallery 02 and retain the existing placement-ID resolution check. This verifies declared association only; native capture must still judge physical correspondence.

## Verified compatible in this snapshot

- The seven gallery anchors run from frame 1032 (`.860000`) through frame 1082 (`.901667`), with no gap above `.0075`; the current boundary, count, and maximum-gap checks accept that station family after the opening placements are removed.
- The six renderer replacement names and two light-family names match `NightTrackLighting` exactly, and the continuous shell is excluded.
- The recipe derives 20 spot fixtures, within the 1-32 limit and above the three-spot minimum. Ceiling fixtures request soft shadows, but both checked pipelines have additional-light shadows disabled. Record them as requested settings, not effective shadow evidence.

No appearance, motion, playability, performance, or AAA-quality verdict is made here.
