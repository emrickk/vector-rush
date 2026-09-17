# Integration report

Status: **IN_PROGRESS**

## Workspace and baseline

- Integration workspace: `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/integration`
- Baseline commit: `1762224deed3c9e994483f5307999b68ae386e38`
- Original checkout remains unchanged. Recoverable snapshot and all three lane workspaces are recorded in `../coordination.md`.

## Completed

- Preserved the source, original `.git` state, editable assets, Unity metadata, historical evidence and native apps in a copy-on-write snapshot.
- Audited potential secret paths privately before creating history. Browser snapshots were excluded; the Unity password field is blank.
- Created a clean local source baseline and independent Environment, Racing and Replay/presentation repositories from the same commit.
- Published ownership, report paths, start evidence and an atomic heavy-process lease.
- Published the minimal Racing/Replay clock, progress, compatibility, eligibility, recovery and sector-crossing contract.
- Added `playtest-sheet.md` with five participant observation sections and an unfilled decision rollup.
- Verified the configured GitHub repository is public. Push remains blocked because the packet permits only an authorized private repository.
- Independently recomputed Gallery02's canonical float-bit course identity as `8c2161b930d43c505c8380ce30a719e6833a453d671353994f79b75e4c67a79e`. Its exact JSON-byte SHA-256 is `598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059`. All four package documents still declare the JSON hash as `courseHash`, confirming the known reconciliation is required. All 40 sealed checksum entries and the STAGED checksum pass in the current package. The 19 station placements span `.8600000143` to `.9016666412`, maximum gap `.0074999928`; all 20 fixtures are spots. No package file was mutated.
- Launched the preserved baseline app directly at 1280 by 800 with its opt-in quick evidence mode. It exited 0 and produced intact title and race-start images plus a player log under `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/artifacts/integration/native-baseline-01`. The inspected images show the Nocturne title, six-pilot/three-lap presentation, elevated night-city start, player craft, adjacent rival and live race HUD. The player log contains no exception/error match. It does contain the known Unity shutdown thread-finalization warnings.
- Integrated the Environment, Racing, and Replay/presentation lanes, restored the missing production-editor files from blob-identical public history, and reconciled the compatibility surface without adding a second race clock.
- Passed the 70-test focused integration suite, the 40-test targeted repair suite, and the complete 164-test EditMode suite. After importing Gallery02, the complete EditMode suite passes again at 164/164 with zero failures or skips.
- Preserved the first Gallery02 import failure and diagnosed it with a two-setting Unity FBX probe. Disabling Unity's extra axis bake matches the package's declared `(x,z,-y)` bounds while retaining identity root transform, unit scale, and the strict 1 cm rejection.
- Completed the fresh Gallery02 technical import. All 12 station FBXs pass the bounds gate with maximum error `0.00000190734863 m`; six prefabs, 10 materials, 11 textures, 19 placements, and 20 local lights were created. Only `Assets/Resources/AAA/GalleryExemplar.asset` was published. Evidence is under `/Users/anping.wang/Documents/Stuff/AI Space/Vector Rush Workspaces/2026-09-14-playable-race/artifacts/integration/gallery-import-03`.

## Current evidence and limitations

- No Unity Editor, Blender GUI or native game process was active at capture. Existing helper processes were not terminated.
- The preserved `Vector Rush.app` has a stable executable hash and known bundle identity, but no source commit can be assigned to it. Strict signature verification fails because the original bundle already contains resource-fork/Finder detritus.
- The initial 37-error compile failure remains preserved as historical integration evidence; subsequent compatibility repairs and the current 164/164 result supersede it as the current source verdict.
- Gallery import status is `TECHNICAL_STAGING_COMPLETE`; its own report intentionally keeps `visualStatus: BLOCKED_VISUAL_REVIEW` and `nativeStatus: NOT_RUN`.
- No current-candidate visual, audio, performance, handling or controller acceptance is claimed.
- The quick native run establishes launch and rendered title/start only. It does not establish a complete race, current-source compatibility, audio quality, handling, performance or visual acceptance.

## Next action

Commit the verified imported resources, build one identified macOS candidate, then capture matched `off` and `gallery` entry, interior, exit, and uninterrupted-motion evidence from that same binary. Run the standard, player-front, player-back, and recovery scenarios plus restart, pause, settings, replay, eligibility, and save-corruption checks. Keep visual, audio, performance, handling, controller, and human-play claims open until their direct evidence exists.
