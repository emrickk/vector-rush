# Toolchain

Host: Apple M2 Max, 32GB RAM, Apple Silicon macOS.

- Unity Editor: user-installed **6000.6.0f1** at `/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app`. The initial recommendation was 6.3 LTS; project follows the user's actual 6.6 supported release.
- URP: **17.6.0**, verified from the installed Editor's bundled package manifest.
- Input System **1.20.0**, Unity Test Framework **1.8.0**, likewise matched to installed Editor packages.
- Official Unity CLI: **1.0.0-beta.8**, installed at `/Users/anping/.unity/bin/unity`; installer verified SHA256 and configured shell path.
- Unity Pipeline **0.6.0-exp.1**, official experimental local Editor automation package, installed into this project. Development tooling, not proof of runtime quality.
- Blender: existing **5.2.0 LTS**, `/Applications/Blender.app/Contents/MacOS/Blender`.
- Codex CLI: existing **0.146.0**. This task uses the configured model; select GPT-6 in the app for another controlled experiment with the saved prompt.
- Xcode and ffmpeg were already available.

## Commands

Run from this folder: `./tools/unity.sh prepare`, `./tools/unity.sh test`, `./tools/unity.sh build`, or `./tools/unity.sh open`. Close this project's Editor before batch tests/builds. These scripts only operate on this project. Build output is `Builds/Vector Rush.app`.

## Primary documentation

- [Official Unity CLI announcement and installation](https://unity.com/blog/meet-the-unity-cli)
- [Unity CLI documentation](https://docs.unity.com/en-us/unity-cli/use-unity-cli)
- [Unity 6.6 release announcement](https://discussions.unity.com/t/unity-6-6-is-now-available/1735357)
- [Unity official graphics source](https://github.com/Unity-Technologies/Graphics)
- [Blender documentation](https://docs.blender.org/manual/en/latest/)

## Asset provenance

The concept in `references/` is generated with the built-in image tool; its prompt is alongside it. It is not runtime evidence. HeroShip is original procedural Blender geometry with editable source. Race audio is original synthesis. World geometry is project-authored C#. No Wipeout assets, branding, or music are included.

The V2 ship, tower kit and cliff geometry have editable Blender sources in `SourceAssets/hero-v2` and `SourceAssets/environment-v2`. Current cliff surface maps are **Rock 3 by Rob Tuytel / Poly Haven (CC0)**, not agent-authored textures. Original albedo/normal/roughness maps, source URLs and hash manifests are retained; the Unity metallic/smoothness mask is explicitly labeled as a derivative. See `SourceAssets/environment-v2/textures/Rock3_PROVENANCE.md` and `docs/asset-regeneration.md`.
