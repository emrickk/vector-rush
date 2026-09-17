# Vector Rush — Scenario interface concepts

Four generated visual references based on the current native screenshots. **Concepts only: no game source, assets, features or build changed.**

[Open comparison gallery](index.html) · [Four-image overview](concept-contact-sheet.jpg) · [Provenance](provenance.json)

| Screen | Full-size concept | Current screenshot |
| --- | --- | --- |
| Racing HUD | [Generated](01-racing-hud-concept.png) | [Native](sources/01-racing-hud.png) |
| Boost HUD | [Generated](02-boost-hud-concept.png) | [Native](sources/02-boost-hud.png) |
| Title menu | [Generated](03-title-menu-concept.png) | [Native](sources/03-title-menu.png) |
| Pause menu | [Generated](04-pause-menu-concept.png) | [Native](sources/04-pause-menu.png) |

## Direction worth carrying forward

The menu pair makes the strongest case for the direction: a more distinctive forward-slanted wordmark, substantial selected buttons, textured surfaces and controlled luminous edges. The race pair applies the same typography to the position and speed readouts, with an angled translucent speed/boost instrument. Boost emphasizes a longer, brighter exhaust plume and concentrated cyan highlights.

Treat these as visual targets. Keep the existing interface's information and actions, and polish their appearance. The generated HUD scale is deliberately bold; final sizing needs to preserve the native driving view. Reduce decorative streaks on small text and the speed digits if they compromise readability. Judge any later animation in the game; static images cannot establish responsiveness or game feel.

## Deviations retained for review

- Image generation resynthesized parts of the city, craft, lamps and framing. Those changes are not approved environment work. The native screenshots remain the composition and content authority.
- The racing concept omits the temporary `POSITION GAINED` message and the small exhibition heading. Existing feedback should be preserved.
- The pause concept replaces the native `R` restart shortcut with a decorative arrow. Preserve the actual shortcut and input behavior.
- Letterforms, timer punctuation and boost segmentation are visual approximations, not authoritative text or data specifications.
- The normal and boosted instruments share a visual language but differ in detailed silhouette; reconcile that shape before any eventual implementation.

## Generation and checks

Provider: Scenario API, explicitly requested by the owner. Model: `model_openai-gpt-image-2`, high quality, one 2048 × 1152 PNG per screen. The native build GUID for all source images is `71c571078c934d3599898ec0eaf13382`. Race is the second style input for boost; title is the second style input for pause. Exact submitted prompts and request bodies are retained alongside the outputs. `provenance.json` records source/output hashes and job IDs.

All four generated images were visually inspected against their native inputs. All eight PNGs decode, and the local gallery's screen navigation, current/concept comparison and keyboard slider were checked in the browser with no reported console errors. No native build, gameplay test, performance result, animation acceptance or owner visual acceptance is claimed.

Private raw API responses are retained outside the source repository under the shared artifact directory. Credentials and signed output URLs are excluded from this reference pack. History is local; this step does not publish the images or push to the public remote.
