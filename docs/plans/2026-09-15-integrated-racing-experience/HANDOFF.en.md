# Execution handoff entry point: four coordinated tasks

The user's latest assignment is GPT-5.6 Sol for programming and Unity integration, and GPT-6 Astra for all assets and Blender. This entry point replaces the older single-task execution handoff. This packet prepares the work; the user creates and starts the new tasks.

Read the [startup order and file ownership guide](START_HERE.en.md), then give each file to its assigned model:

| Order | Model | Handoff |
| --- | --- | --- |
| 01, start first | GPT-5.6 Sol | [Experience coordination and Unity scene integration](01-integration-sol.en.md) |
| 02, after workspaces are ready | GPT-6 Astra | [Unified art direction, all assets, and Blender](02-art-assets-astra.en.md) |
| 03, after workspaces are ready | GPT-5.6 Sol | [Speed, ship state, and collision effects implementation](03-motion-vfx-sol.en.md) |
| 04, after workspaces are ready | GPT-5.6 Sol | [UI animation and audio implementation](04-ui-audio-sol.en.md) |

Start 02–04 after 01 returns WORKSPACES_READY. Deliver work to 01 in batches and validate it early in the same native scene. Tasks do not wake each other automatically; the user continues the relevant task.

See the [overall plan](PLAN.en.md) and [acceptance checklist](ACCEPTANCE.en.md) for the shared objective and phase gates: first deliver an approximately 30-second playable exemplar, native footage with sound, a real-contact collision supplement, and a playable app. The user reviews the complete experience before deciding on full-circuit and all-interface expansion. Preserve gameplay and historical candidates; technical passes do not establish artistic acceptance.
