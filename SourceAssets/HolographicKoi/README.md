# Reference-led giant aerial koi

The owner approved rebuilding the giant koi against `Reference/2026-09-17/koi-overhead-target-01.png`: natural anatomy, a longer body taper, layered flowing fins, translucent crimson volume and a bright crest. The current turn candidate keeps the giant scale and uses an oblique rear-quarter encounter. The reference image is generated concept art; the game uses native geometry and shaders.

Revision 08 uses independent monotone profiles for back, belly and width, with conformal cheek eyes, a shallow mouth and curved opercula. It addresses the owner-rejected revision 07 head/body proportions while preserving fin design and motion.

`build_model.py` authors the editable `HolographicKoi.blend` and exported `HolographicKoi.fbx`. The current fish has 13 folded Bezier fin membranes and 39 sparse veins/edge filaments, rather than the earlier radial fin fans. Dorsal roots span the back. The forehead, mouth, integrated eye and curved gill contours follow the body surface. Five combined material groups remain.

Fin vertices carry root-to-tip flexibility and a shared phase in their colour channels. Unity preserves these when importing/combining meshes; an EditMode assertion checks this contract. The body has zero fin flexibility. The GPU bends the complete cross section along a curved spine, then adds a traveling body wave and delayed fin deformation, keeping roots attached. A subtle flank-width contraction is paired with reciprocal vertical expansion to preserve cross-sectional area. Imported X is reversed: the head is positive X, the tail negative X. A cofactor Jacobian corrects normals through spine curvature and contraction. Fin normals still approximate the flexibility gradient; this is authored deformation, not muscle or cloth simulation.

The body shader uses offset open scale arcs, selective glints, a bright crest and a dark crimson interior. Layered translucent membranes have softer internal striations; fewer curved veins carry their structure. Far-side eye details are faded to avoid an apparent extra eye through the cheek. Whole-body travel remains an authored looping path.

The wet-road effect captures the moving fish into a dedicated HDR texture and projects it onto the banked road through reflected view rays. This is a planar approximation, not a complete ray-traced scene reflection. Candidate-only local lights and subdued VOLT/ORBIT materials remain. No new gameplay, HUD, course, ship or weather change.

Rebuild geometry with `/Applications/Blender.app/Contents/MacOS/Blender -b --python SourceAssets/HolographicKoi/build_model.py`. Then run pinned Unity 6000.6.0f1 with `-batchmode -quit -projectPath <project>/UnityProject -executeMethod VectorRush.Editor.HolographicKoiSetup.Prepare -productionEvidence <fresh output> -logFile <log>`. Prepare overwrites this candidate and its generated assets. Ordinary builds should use the serialized scene through `tools/current-game.sh holo-koi-build <fresh app> <fresh evidence>`.

The shader and CPU authoring deformation share an 18-second cycle. Rendering bounds sample all source vertices over 90 poses with padding. Road clearance samples all deformed vertices at 120 complete swim poses against 1,600 road positions, with a 0.5 m pad on each world-space side. Authoring lifts the anchor only as needed to retain an 8 m sampled envelope margin. This is sampled evidence, not exhaustive continuous-time or manual-camera proof; keep the CPU and shader functions synchronized.

Native builds, raw captures and logs stay outside source control. See `docs/current-game/koi-turn-away-2026-09-17/README.md` for the final native comparison and current validation. The owner judges artistic acceptance.
