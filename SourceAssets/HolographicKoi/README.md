# Giant aerial koi — authored source

Owner direction: luminous red-orange cyberpunk koi, visibly sweeping tail and free swimming travel, giant enough to drive closely beneath. The old ceramic fish and amber hoop were rejected.

`build_model.py` authors the continuous body, anatomy, membranes and 202 swept fin rays in Blender. `HolographicKoi.blend` is editable and `HolographicKoi.fbx` is the Unity interchange asset. The two owner-supplied luminous koi images guided the geometry and lighting; no new bitmap generation was used.

The Stage 10 authoring entry is `VectorRush.Editor.HolographicKoiSetup.Prepare`. It builds from unchanged Stage 8, imports and combines five material groups, places the fish above the bend, retains an off-road projection platform, and creates candidate-only subdued VOLT/ORBIT materials. The HolographicKoi component moves the fish in world space; one GPU traveling wave deforms the body, eyes, fins and tail together. Bright scale contours and translucent density break up the surface.

The wet-road effect captures the moving fish into a dedicated HDR texture and projects it onto the banked road through reflected view rays. This is a planar projection approximation; it is not a physically complete reflection of the scene. The fish and platform have no colliders. The authoring validation checks all original collision transforms and meshes, and samples the padded animated fish bounds above the course over a complete swim cycle.

Rebuild geometry with `/Applications/Blender.app/Contents/MacOS/Blender -b --python SourceAssets/HolographicKoi/build_model.py`. Then use the pinned Unity 6000.6.0f1 Editor with `-batchmode -quit -projectPath <project>/UnityProject -executeMethod VectorRush.Editor.HolographicKoiSetup.Prepare -productionEvidence <fresh output> -logFile <log>`. Prepare overwrites this candidate and its generated assets. Ordinary builds should use the serialized scene through `tools/current-game.sh holo-koi-build <fresh app> <fresh evidence>`.

Builds, raw native captures, audio and logs remain outside source control. The native review package contains actual automated driving footage, not an artistic acceptance claim.
