using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VectorRush
{
    /// <summary>Resolution-independent presentation. Race rules stay in RaceDirector.</summary>
    public sealed class RaceHUD : MonoBehaviour
    {
        RaceDirector director;
        ChaseCamera chaseCamera;
        Texture2D white, sideShade, bottomShade;
        Font regular;
        GUIStyle textStyle, numberStyle, buttonStyle;
        readonly Color ivory = new Color(.94f, .96f, .90f);
        readonly Color muted = new Color(.62f, .73f, .74f);
        readonly Color acid = new Color(.85f, 1f, .21f);
        readonly Color turquoise = new Color(.25f, .94f, .90f);
        readonly Color ink = new Color(.004f, .009f, .017f, .55f);
        float width, height;
        bool ready;
        bool inputEvidence;

        public void Initialize(RaceDirector raceDirector)
        {
            director = raceDirector;
            chaseCamera = FindAnyObjectByType<ChaseCamera>();
            inputEvidence = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-inputEvidence") >= 0;
            if (inputEvidence) Debug.Log("[InputEvidence] enabled; original and InputSystem-corrected pointer coordinates are recorded; HUD uses InputSystem window coordinates.");
        }

        void Update()
        {
            if (director == null) return;
            bool accept = false, restart = false;
#if ENABLE_LEGACY_INPUT_MANAGER
            accept = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
            restart = Input.GetKeyDown(KeyCode.R);
#endif
#if ENABLE_INPUT_SYSTEM
            if (inputEvidence && Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.leftButton.wasReleasedThisFrame))
                Debug.Log($"[InputEvidence] InputSystem frame={Time.frameCount} position={Mouse.current.position.ReadValue()} down={Mouse.current.leftButton.wasPressedThisFrame} up={Mouse.current.leftButton.wasReleasedThisFrame} held={Mouse.current.leftButton.isPressed} screen={Screen.width}x{Screen.height} phase={director.Phase}");
            accept |= Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
#if !ENABLE_LEGACY_INPUT_MANAGER
            accept |= Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
            restart |= Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#endif
#endif
            if (accept && director.Phase == RacePhase.Menu) director.StartRace();
            else if ((accept || restart) && director.Phase == RacePhase.Finished) director.RestartRace();
            else if (restart && director.Phase == RacePhase.Paused) director.RestartRace();
        }

        void Prepare()
        {
            if (ready) return;
            white = Texture2D.whiteTexture;
            // The built-in font is present in native players. An OS font can return
            // a non-null Font but fail to load its face when IMGUI builds the mesh.
            regular = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textStyle = new GUIStyle { font = regular, richText = false, clipping = TextClipping.Clip };
            numberStyle = new GUIStyle(textStyle) { fontStyle = FontStyle.Bold };
            buttonStyle = new GUIStyle { normal = { background = null }, hover = { background = null }, active = { background = null } };
            sideShade = Gradient(true);
            bottomShade = Gradient(false);
            ready = true;
        }

        Texture2D Gradient(bool horizontal)
        {
            var texture = new Texture2D(horizontal ? 64 : 1, horizontal ? 1 : 64, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            for (int i = 0; i < 64; i++)
            {
                float t = i / 63f;
                float alpha = horizontal ? .96f * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.28f, 1f, t))) : .55f * Mathf.Pow(1f - t, 2f);
                texture.SetPixel(horizontal ? i : 0, horizontal ? 0 : i, new Color(.003f, .007f, .013f, alpha));
            }
            texture.Apply(false, true);
            return texture;
        }

        void OnGUI()
        {
            if (director == null) return;
            Prepare();
            Matrix4x4 previous = GUI.matrix;
            Color previousColor = GUI.color;
            Vector2 rawMouse = Event.current.mousePosition;
            float scale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
            if (scale <= 0) return;
            Vector2 correctedMouse = rawMouse;
            bool pointerCorrected = false;
#if ENABLE_INPUT_SYSTEM
            // On macOS the native IMGUI event can carry a desktop-origin offset.
            // InputSystem reports player-window pixels; convert its bottom origin
            // before the GUI matrix converts those pixels into our virtual canvas.
            if (Mouse.current != null && (Event.current.isMouse || Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout))
            {
                Vector2 playerMouse = Mouse.current.position.ReadValue();
                correctedMouse = new Vector2(playerMouse.x, Screen.height - playerMouse.y);
                Event.current.mousePosition = correctedMouse;
                pointerCorrected = true;
            }
#endif
            width = Screen.width / scale;
            height = Screen.height / scale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            // Changing GUI.matrix reloads the native event position. Apply the
            // normalized pointer after that change, in the virtual canvas space.
            if(pointerCorrected)Event.current.mousePosition=correctedMouse/scale;
            if (inputEvidence && (Event.current.rawType == EventType.MouseDown || Event.current.rawType == EventType.MouseUp))
            {
                string systemMouse = "unavailable";
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null) systemMouse = Mouse.current.position.ReadValue().ToString();
#endif
                Debug.Log($"[InputEvidence] GUI frame={Time.frameCount} type={Event.current.type} rawType={Event.current.rawType} button={Event.current.button} raw={rawMouse} corrected={correctedMouse} pointerCorrected={pointerCorrected} virtual={Event.current.mousePosition} rawDivScale={rawMouse / scale} screen={Screen.width}x{Screen.height} scale={scale:F4} canvas={width:F1}x{height:F1} systemMouse={systemMouse} phase={director.Phase} hotControl={GUIUtility.hotControl}");
            }
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0, height - 320, width, 320), bottomShade);

            if (director.Phase == RacePhase.Menu) DrawMenu();
            else
            {
                DrawTelemetry();
                if (director.Phase == RacePhase.Countdown) DrawCountdown();
                if (director.Phase == RacePhase.Paused) DrawPause();
                if (director.Phase == RacePhase.Finished) DrawResults();
            }
            GUI.matrix = previous;
            Event.current.mousePosition = rawMouse;
            GUI.color = previousColor;
        }

        void DrawMenu()
        {
            GUI.DrawTexture(new Rect(0, 0, width * .72f, height), sideShade);
            float y = height * .20f;
            Box(72, y, 46, 6, acid);
            Text("ANTI-GRAVITY RACING / VOL. 01", 136, y - 10, 500, 30, 20, ivory);
            Text("VECTOR", 64, y + 28, 840, 140, 116, ivory, true);
            Text("RUSH", 64, y + 141, 840, 145, 116, acid, true);
            Text("NOCTURNE CIRCUIT", 76, y + 320, 680, 40, 30, ivory, true);
            Text("MIDNIGHT EXHIBITION  /  6 PILOTS  /  3 LAPS", 76, y + 367, 720, 32, 19, muted);
            if (Button("START RACE", "ENTER / A", 76, y + 440, 450, true)) director.StartRace();
            ShakeToggle(76, y + 525, 450);
            if (Button("QUIT", "", 76, y + 589, 450, false, 48)) Application.Quit();
            Controls(76, height - 128);
            Text("02 / NOCTURNE", width - 380, height - 99, 306, 32, 20, ivory, false, TextAnchor.MiddleRight);
            Text("ORIGINAL RACE PROTOTYPE", width - 420, height - 64, 346, 25, 14, muted, false, TextAnchor.MiddleRight);
        }

        void DrawTelemetry()
        {
            Box(60, 54, 6, 91, acid);
            Box(66, 54, 220, 91, ink);
            Text("POSITION", 88, 64, 170, 27, 15, muted);
            Text(director.Position.ToString("00"), 86, 88, 83, 58, 46, ivory, true);
            Text("/ " + (director.Racers == null ? 6 : director.Racers.Count).ToString("00"), 174, 108, 90, 31, 23, muted);
            Box(302, 54, 189, 91, ink);
            Text("LAP", 323, 64, 136, 27, 15, muted);
            Text(Mathf.Clamp(director.Lap, 1, director.TotalLaps).ToString("00") + " / " + director.TotalLaps.ToString("00"), 321, 96, 150, 43, 31, ivory, true);

            Text("NOCTURNE / MIDNIGHT", width * .5f - 210, 57, 420, 30, 19, ivory, false, TextAnchor.MiddleCenter);
            Text("VECTOR RUSH", width * .5f - 210, 88, 420, 26, 13, muted, false, TextAnchor.MiddleCenter);
            Box(width - 449, 54, 394, 128, ink);
            Text("RACE TIME", width - 391, 57, 315, 26, 15, muted, false, TextAnchor.MiddleRight);
            Text(TimeLabel(director.RaceTime), width - 440, 85, 364, 61, 43, ivory, true, TextAnchor.MiddleRight);
            Text("BEST  " + TimeLabel(director.BestLap), width - 400, 147, 324, 28, 17, muted, false, TextAnchor.MiddleRight);

            float speed = director.Player == null ? 0 : director.Player.SpeedKph;
            Box(60, height - 230, 442, 185, ink);
            Box(60, height - 230, 6, 185, turquoise);
            Text(Mathf.RoundToInt(speed).ToString("000"), 68, height - 218, 430, 125, 108, ivory, true);
            Text("KM/H", 78, height - 84, 130, 28, 18, muted);
            Text(director.Player != null && director.Player.IsBoosting ? "BOOST ACTIVE" : "VECTOR / 01", 226, height - 84, 275, 28, 18, turquoise);

            float charge = director.Player == null ? 0 : Mathf.Clamp01(director.Player.Boost01);
            float bx = width - 445;
            Box(bx - 20, height - 188, 410, 143, ink);
            Text("ENERGY RESERVE", bx, height - 155, 320, 30, 16, muted);
            Text(Mathf.RoundToInt(charge * 100) + "%", bx + 230, height - 166, 137, 40, 28, ivory, true, TextAnchor.MiddleRight);
            for (int i = 0; i < 20; i++)
            {
                float segment = Mathf.Clamp01(charge * 20 - i);
                Box(bx + i * 18.5f, height - 110, 14, 20, new Color(.30f, .45f, .45f, .50f));
                if (segment > 0) Box(bx + i * 18.5f, height - 110, 14 * segment, 20, acid);
            }
            Text("SPACE / A   BOOST", bx, height - 76, 370, 26, 15, muted, false, TextAnchor.MiddleRight);
            if (director.Player != null)
            {
                float progress = Mathf.Repeat(director.Player.RaceProgress, 1);
                float lineWidth = Mathf.Min(500, width - 1080);
                float x = (width - lineWidth) * .5f;
                Box(x, height - 85, lineWidth, 2, new Color(.7f, .85f, .85f, .25f));
                Box(x, height - 85, lineWidth * progress, 2, turquoise);
                Box(x + lineWidth * progress - 3, height - 89, 6, 10, ivory);
                for (int i = 0; i <= 4; i++) Box(x + lineWidth * i / 4, height - 84, 1, 7, muted);
                Text("CIRCUIT PROGRESS", x, height - 60, lineWidth, 24, 12, muted, false, TextAnchor.MiddleCenter);
            }
        }

        void DrawCountdown()
        {
            string number = director.CountdownRemaining > .1f ? Mathf.CeilToInt(director.CountdownRemaining).ToString() : "GO";
            Text("SYSTEMS READY", width * .5f - 300, height * .39f - 48, 600, 38, 20, ivory, false, TextAnchor.MiddleCenter);
            Text(number, width * .5f - 220, height * .39f, 440, 175, 146, acid, true, TextAnchor.MiddleCenter);
        }

        void DrawPause()
        {
            Box(0, 0, width, height, new Color(.015f, .04f, .05f, .75f));
            float x = width * .5f - 240, y = height * .5f - 230;
            Text("RACE PAUSED", x, y, 480, 70, 54, ivory, true);
            Text("NOCTURNE CIRCUIT", x + 3, y + 82, 480, 30, 18, muted);
            if (Button("RESUME", "ESC / START", x, y + 148, 480, true)) director.TogglePause();
            if (Button("RESTART RACE", "R", x, y + 233, 480)) director.RestartRace();
            ShakeToggle(x, y + 309, 480);
            if (Button("QUIT", "", x, y + 380, 480, false, 48)) Application.Quit();
            Controls(76, height - 112);
        }

        void DrawResults()
        {
            GUI.DrawTexture(new Rect(0, 0, width * .78f, height), sideShade);
            float x = 78, y = height * .20f;
            Text("MIDNIGHT EXHIBITION / COMPLETE", x, y, 790, 34, 19, acid);
            Text("RACE FINISHED", x - 7, y + 47, 1000, 111, 78, ivory, true);
            Text(director.Position.ToString("00"), x - 4, y + 181, 245, 165, 137, acid, true);
            Text("FINAL POSITION", x + 268, y + 231, 400, 40, 23, ivory);
            Text("NOCTURNE CIRCUIT / 3 LAPS", x + 268, y + 276, 460, 32, 17, muted);
            Box(x, y + 379, 650, 1, muted);
            Text("TOTAL TIME", x, y + 407, 270, 28, 17, muted);
            Text(TimeLabel(director.RaceTime), x, y + 442, 300, 55, 36, ivory, true);
            Text("BEST LAP", x + 359, y + 407, 270, 28, 17, muted);
            Text(TimeLabel(director.BestLap), x + 359, y + 442, 310, 55, 36, ivory, true);
            if (Button("RACE AGAIN", "ENTER / A", x, y + 541, 450, true)) director.RestartRace();
            if (Button("QUIT", "", x, y + 629, 450, false, 48)) Application.Quit();
        }

        void ShakeToggle(float x, float y, float w)
        {
            if (chaseCamera == null) chaseCamera = FindAnyObjectByType<ChaseCamera>();
            bool enabled = chaseCamera == null || chaseCamera.ShakeEnabled;
            if (Button("CAMERA SHAKE", enabled ? "ON" : "OFF", x, y, w, false, 49) && chaseCamera != null)
                chaseCamera.ShakeEnabled = !enabled;
        }

        void Controls(float x, float y)
        {
            Text("WASD / ARROWS   PILOT     SPACE   BOOST     Q / E   AIRBRAKES", x, y, 1170, 30, 17, ivory);
            Text("R   RECOVER     ESC / P   PAUSE     CONTROLLER: STICK + RT / LT · A BOOST · LB / RB AIRBRAKES · Y RECOVER", x, y + 36, 1480, 29, 14, muted);
        }

        bool Button(string label, string hint, float x, float y, float w, bool primary = false, float h = 64)
        {
            var rect = new Rect(x, y, w, h);
            bool hover = rect.Contains(Event.current.mousePosition);
            Box(x, y, w, h, primary ? (hover ? ivory : acid) : (hover ? new Color(.18f, .28f, .28f, .94f) : ink));
            if (!primary) Box(x, y + h - 1, w, 1, new Color(.44f, .63f, .63f, .35f));
            Color color = primary ? new Color(.04f, .095f, .10f) : ivory;
            Text(label, x + 21, y + 1, w - 140, h - 2, primary ? 21 : 18, color, true, TextAnchor.MiddleLeft);
            Text(hint, x + w - 137, y + 1, 116, h - 2, 14, primary ? color : muted, false, TextAnchor.MiddleRight);
            if (inputEvidence && (Event.current.rawType == EventType.MouseDown || Event.current.rawType == EventType.MouseUp))
                Debug.Log($"[InputEvidence] buttonProbe label={label} rect={rect} mouse={Event.current.mousePosition} contains={hover} type={Event.current.type} hotControl={GUIUtility.hotControl}");
            bool activated = GUI.Button(rect, GUIContent.none, buttonStyle);
            if (inputEvidence && activated)
                Debug.Log($"[InputEvidence] BUTTON_ACTIVATED label={label} frame={Time.frameCount} phaseBeforeCallback={director.Phase}");
            return activated;
        }

        void Text(string value, float x, float y, float w, float h, int size, Color color, bool bold = false, TextAnchor align = TextAnchor.UpperLeft)
        {
            GUIStyle style = bold ? numberStyle : textStyle;
            style.fontSize = size;
            style.alignment = align;
            style.normal.textColor = color;
            GUI.Label(new Rect(x, y, w, h), value, style);
        }

        void Box(float x, float y, float w, float h, Color color)
        {
            Color before = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(x, y, w, h), white);
            GUI.color = before;
        }

        public static string TimeLabel(float seconds)
        {
            if (seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return "—:——.———";
            int millis = Mathf.Max(0, Mathf.FloorToInt(seconds * 1000));
            return (millis / 60000).ToString("00") + ":" + ((millis / 1000) % 60).ToString("00") + "." + (millis % 1000).ToString("000");
        }

        void OnDestroy()
        {
            if (sideShade != null) Destroy(sideShade);
            if (bottomShade != null) Destroy(bottomShade);
            // LegacyRuntime.ttf is shared engine-owned data, not a runtime asset.
        }
    }
}
