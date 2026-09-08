# Native pointer check — HUD pass

The delivered candidate before the final timer-spacing-only adjustment ran windowed at 1280×800 on macOS. CUA clicked visible native controls; no button callbacks were invoked directly. Screens were inspected after every action.

- A click immediately to the right of Start did not start the race.
- Clicking Start entered countdown.
- Escape opened Pause; clicking Resume returned to Racing.
- Escape reopened Pause; clicking Restart entered a reset countdown.
- Escape reopened Pause; clicking Quit closed the app.

`player.log` records actual InputSystem window coordinates, corrected IMGUI coordinates, hit rectangles, activations and phases. This specifically exercises the preserved macOS pointer correction at a non-unit HUD scale. It does not establish physical gamepad support, results-screen pointer activation, or sustained human driving feel.
