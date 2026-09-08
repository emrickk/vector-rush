# VECTOR RUSH

An original anti-gravity racing experiment: Kestrel 07 over Solstice Circuit, a 1.84 km elevated ocean course. Built with Unity and authored Blender geometry, with parallel specialist agents and independent criticism.

## Play

The native build is generated at **Builds/Vector Rush.app**. Open it, choose **Start race**, and complete three laps against five rivals.

| Action | Keyboard | Gamepad |
|---|---|---|
| Start / confirm | Enter | A / south button |
| Accelerate | W / Up | Right trigger |
| Steer | A/D or Left/Right | Left stick |
| Brake | S / Down | Left trigger |
| Airbrakes | Q / E | Left / right shoulder |
| Boost | Space | A / south button |
| Recover | R | Y / north button |
| Pause | Escape / P | Start |

Pause menu includes restart and camera-shake settings. The results screen supports another race.

## Project and experiment materials

- `UnityProject/` — Unity 6000.6.0f1 project, URP 17.6.0. Open `Assets/Scenes/Solstice.unity` and press Play.
- `docs/GPT6-BUILD-PROMPT.md` — reusable prompt for repeating or extending the GPT-6 experiment.
- `references/solstice-chase-concept.png` — generated art-direction target; prompt alongside it.
- `SourceAssets/Kestrel07.blend` — editable original craft; export script and material guide alongside it.
- `evidence/asset-renders/` — actual Blender renders, distinct from gameplay evidence.
- `evidence/editmode-results.xml` — actual Unity race-rule test results.
- `docs/critique-01.md` and later critiques — independent findings and review history.
- `docs/toolchain.md` — installed tools, exact versions, documentation and asset provenance.

## Build from source

With the matching Editor installed, run `./tools/unity.sh prepare`, then `./tools/unity.sh test` and `./tools/unity.sh build`. Close this project's Editor before batch commands. `./tools/unity.sh open` opens it interactively. Blender assets are reproduced with `./tools/blender.sh`.

“AAA” is the aspiration from the original prompt. Only actual game captures, measured performance and recorded review findings support claims about the delivered build. Concept art and studio renders do not establish gameplay quality.
