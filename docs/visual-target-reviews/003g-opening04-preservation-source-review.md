# Opening04 — road preservation source review

**Verdict: no source blocker; proceed to the separate opening04 build and native preservation comparison.** This does not establish a visual, motion or performance pass.

Reviewed the WorldBuilder-only diff against `6f7e3ba`, after the preserved opening03 candidate/rejection. Current WorldBuilder SHA256: `b60da7994250789d01cf8e651a19e79e196538856a9ba5306368ee978e914579`.

The change removes the `OpeningRoadFinish` field, gated component/configuration block and render-mesh partition hook. Comparing current WorldBuilder with pre-opening `2908112` leaves only an equivalent MeshRenderer local-variable assignment. Original road material construction, generated base/normal/wear maps, map scale, smoothness, complete mesh, shading normals/tangents and collider assignment are therefore restored in every preview mode. The original ownership/cleanup remains intact.

A focused runtime/scene search found no other component creation, callsite or scene reference that activates OpeningRoadFinish. Its retained helper and material are historical source/build assets; they do not alter the road through the reviewed startup path. Existing baseline-template retention checks remain necessary. Construction and the SurfaceEnabled lighting/gallery study are unchanged, so combined can still look different under those sources even though its road material/maps now match off.

No Unity, tests, captures, runtime edits or commits were performed by this reviewer. The native comparison must confirm restored road/gallery material legibility and retained frontage gains. If combined still regresses, the authorized fallback is the construction-only mode, with no further art correction implied by this source approval.
