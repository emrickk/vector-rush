# Reference-led giant aerial koi

The owner approved rebuilding the giant koi against `Reference/2026-09-17/koi-overhead-target-01.png`: natural anatomy, a longer body taper, layered flowing fins, translucent crimson volume and a bright crest. Stage 10 revision 08 keeps the lower overhead anchor from revision 05. The reference image is generated concept art; the game uses native geometry and shaders.

Revision 08 uses independent monotone profiles for back, belly and width, with conformal cheek eyes, a shallow mouth and curved opercula. It addresses the owner-rejected revision 07 head/body proportions while preserving fin design and motion.

`build_model.py` authors the editable `HolographicKoi.blend` and exported `HolographicKoi.fbx`. The current fish has 13 folded Bezier fin membranes and 39 sparse veins/edge filaments, rather than the earlier radial fin fans. Dorsal roots span the back. The forehead, mouth, integrated eye and curved gill contours follow the body surface. Five combined material groups remain.

Fin vertices carry root-to-tip flexibility and a shared phase in their colour channels. Unity preserves these when importing/combining meshes; an EditMode assertion checks this contract. The body has zero fin flexibility. The GPU applies one traveling body wave plus delayed fin deformation, keeping roots attached. Imported X is reversed: the head is positive X, the tail negative X. Shader normals account for the body-wave shear. Fin normals approximate the extra flexibility gradient; they are not a full cloth simulation.

The body shader uses offset open scale arcs, selective glints, a bright crest and a dark crimson interior. Layered translucent membranes have softer internal striations; fewer curved veins carry their structure. Far-side eye details are faded to avoid an apparent extra eye through the cheek. Whole-body travel remains an authored looping path.

The wet-road effect captures the moving fish into a dedicated HDR texture and projects it onto the banked road through reflected view rays. This is a planar approximation, not a complete ray-traced scene reflection. Candidate-only local lights and subdued VOLT/ORBIT materials remain. No new gameplay, HUD, course, ship or weather change.

Rebuild geometry with `/Applications/Blender.app/Contents/MacOS/Blender -b --python SourceAssets/HolographicKoi/build_model.py`. Then run pinned Unity 6000.6.0f1 with `-batchmode -quit -projectPath <project>/UnityProject -executeMethod VectorRush.Editor.HolographicKoiSetup.Prepare -productionEvidence <fresh output> -logFile <log>`. Prepare overwrites this candidate and its generated assets. Ordinary builds should use the serialized scene through `tools/current-game.sh holo-koi-build <fresh app> <fresh evidence>`.

The authoring validation preserves the original collision transforms/mesh references and samples the padded fish bounds over a full whole-body loop. Bounds include the maximum body/fin deformation allowance. The first trial needed less whole-body bank to retain the same lower anchor. This is sampled clearance evidence, not an exhaustive manual-camera guarantee.

Native builds, raw captures and logs stay outside source control. See `docs/current-game/koi-head-body-2026-09-17/README.md` for the final native comparison and current validation. The owner judges artistic acceptance.
