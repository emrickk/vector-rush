# Original animated billboard campaigns

Four original advertising campaigns generated through the owner-approved Scenario connection. Artwork model: `model_openai-gpt-image-2`. Video model: `model_kling-v3-i2v-pro`.

Each campaign retains the original image, image request and job provenance, the selected video, and the exact video prompt/request plus job provenance. The ECHO clip is the second iteration, selected after the first was too restrained. Its Unity playback runs forward and backward over 10 seconds to avoid a hard loop reset; the others loop over five seconds with a short boundary blend.

`animation-manifest.json` records the selected video hashes, frame dimensions and decoded-frame variation. That variation is a source sanity check, not proof of native rendering. Actual native gameplay and fixed-camera motion evidence are documented in the current-game milestone.

Repack from a completed generation folder with `python3 tools/billboards/pack_animations.py /absolute/generation-folder`. This command requires Python/Pillow and ffmpeg, but no API credentials. Unity plays the packed video frames on the GPU, interpolates adjacent frames, and shares four atlases across 10 screens in five architectural clusters. No video decoder or network access is required at runtime. Original videos are authoring sources; they are outside Unity's Assets folder.

Reference direction: [Michał Lemiesz, Cyberpunk 2077 Digital Billboards](https://www.artstation.com/artwork/ao8nP9) for artwork and mounting variety; [Víctor Navarro, Light City](https://vctornavor.artstation.com/projects/8bX4Nx) for layered displays and holographic landmarks. Those third-party images are references only and are not packaged as game textures.

The corrected layout uses a sparse high portrait, stacked portrait and ticker, two orthogonal corner displays, a synchronized three-panel video, and a low landscape display with a projecting blade. Display heights and headings are authored in world space. Screens stand 18 cm ahead of their housing; mip-filtered playback replaces the rejected fine scanline treatment. The first two native candidates are rejected layout iterations.

Candidate 05 subsequently enlarged and lowered the primary displays for chase-camera readability, moved the triptych to the visible outer bend, and turned the corner/landscape faces toward their road approaches. This changes placement and screen proportions only; the original campaign videos and animation packing are unchanged. See the driving-readability milestone.
