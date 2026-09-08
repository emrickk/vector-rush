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
        Texture2D white, sideShade, bottomShade, stroke;
        Font regular;
        Vector2[] circuitPoints;
        Vector2 circuitMin, circuitMax;
        TrackPath circuit;
        float displayedSpeed, lapNotice;
        int observedLap = 1;
        RacePhase observedPhase;
        readonly Color faint = new Color(.62f, .76f, .80f, .25f);
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
            float speed = director.Player == null ? 0 : director.Player.SpeedKph;
            displayedSpeed = Mathf.Lerp(displayedSpeed, speed, 1f - Mathf.Exp(-18f * Time.deltaTime));
            if (director.Phase == RacePhase.Countdown && observedPhase != RacePhase.Countdown) {
                observedLap = 1; lapNotice = 0; displayedSpeed = 0;
            }
            if (director.Phase == RacePhase.Racing && director.Lap > observedLap) {
                observedLap = director.Lap; lapNotice = 3f;
            }
            if (director.Phase == RacePhase.Racing) lapNotice = Mathf.Max(0, lapNotice - Time.deltaTime);
            observedPhase = director.Phase;
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
            regular = Resources.Load<Font>("Fonts/Rajdhani-SemiBold");
            if (!regular) regular = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textStyle = new GUIStyle { font = regular, richText = false, clipping = TextClipping.Clip };
            numberStyle = new GUIStyle(textStyle) { fontStyle = FontStyle.Normal };
            buttonStyle = new GUIStyle { normal = { background = null }, hover = { background = null }, active = { background = null } };
            stroke = new Texture2D(1, 16, TextureFormat.RGBA32, false);
            stroke.wrapMode = TextureWrapMode.Clamp; stroke.filterMode = FilterMode.Bilinear;
            for (int i = 0; i < 16; i++) stroke.SetPixel(0, i, new Color(1, 1, 1, Mathf.Clamp01(Mathf.Min(i, 15-i) / 2f)));
            stroke.Apply(false, true);
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
                if (director.Phase != RacePhase.Finished && director.Phase != RacePhase.Paused) DrawTelemetry();
                if (director.Phase == RacePhase.Countdown || (director.Phase == RacePhase.Racing && director.RaceTime < .65f)) DrawCountdown();
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
            var player = director.Player;
            bool boosting = player != null && player.IsBoosting && director.Phase == RacePhase.Racing;
            Color energyColor = boosting ? acid : turquoise;
            // Race order / lap: one compact header, away from the racing line.
            Box(54, 48, 332, 112, new Color(.007f, .016f, .024f, .65f));
            Box(54, 48, 3, 112, ivory);
            Text("POS", 74, 59, 65, 25, 18, muted);
            Text(director.Position.ToString("00"), 70, 77, 99, 80, 70, ivory, true);
            Text("/ " + (director.Racers == null ? 6 : director.Racers.Count).ToString("00"), 167, 113, 64, 32, 25, muted);
            Box(246, 69, 1, 67, faint);
            Text("LAP", 267, 59, 91, 25, 18, muted);
            Text(Mathf.Clamp(director.Lap, 1, director.TotalLaps).ToString(), 263, 88, 42, 61, 47, ivory, true);
            Text("/ " + director.TotalLaps, 310, 107, 55, 36, 26, muted);

            float tx = width - 368;
            Box(tx - 16, 48, 330, 126, new Color(.007f, .016f, .024f, .65f));
            Text("RACE TIME", tx, 57, 294, 24, 17, muted, false, TextAnchor.MiddleRight);
            string raceClock = director.RaceTime <= 0 ? "00:00.000" : TimeLabel(director.RaceTime);
            int fraction = raceClock.Length - 4;
            Text(raceClock.Substring(0, fraction), tx, 78, 232, 54, 43, ivory, true, TextAnchor.MiddleRight);
            Text(raceClock.Substring(fraction), tx + 238, 91, 56, 36, 27, muted, false, TextAnchor.MiddleLeft);
            Box(tx, 136, 294, 1, faint);
            Text("BEST LAP", tx, 143, 94, 24, 16, muted);
            Text(director.BestLap > 0 ? TimeLabel(director.BestLap) : "--", tx + 100, 140, 194, 28, 21, ivory, false, TextAnchor.MiddleRight);

            DrawCircuit();
            float x = width - 410, y = height - 290;
            // A single open instrument. The arc is speed; the independent bar is energy.
            Vector2 center = new Vector2(x + 186, y + 121);
            for (int i = 0; i < 45; i++) {
                float a = Mathf.Lerp(150, 390, i / 45f);
                float z = Mathf.Lerp(150, 390, (i + .70f) / 45f);
                float maximum = player != null ? player.BoostSpeed * 3.6f : 388.8f;
                Color c = i / 45f < Mathf.Clamp01(displayedSpeed / maximum) ? ivory : faint;
                Line(Polar(center, 118, a), Polar(center, 118, z), i >= 33 ? 6 : 3, c);
            }
            Text(Mathf.RoundToInt(displayedSpeed).ToString("000"), x + 33, y + 50, 303, 137, 116, ivory, true, TextAnchor.MiddleCenter);
            Text("KM/H", x + 122, y + 170, 130, 27, 20, muted, false, TextAnchor.MiddleCenter);
            float charge = player == null ? 0 : Mathf.Clamp01(player.Boost01);
            Text(boosting ? "BOOST ENGAGED" : charge < .1f ? "BOOST RECHARGING" : "BOOST", x + 6, y + 220, 248, 27, 19, energyColor);
            Text(Mathf.RoundToInt(charge * 100).ToString("00") + "%", x + 270, y + 215, 94, 33, 26, ivory, true, TextAnchor.MiddleRight);
            for (int i = 0; i < 30; i++) {
                float fill = Mathf.Clamp01(charge * 30 - i);
                Box(x + 7 + i * 12, y + 257, 9, 7, faint);
                if (fill > 0) Box(x + 7 + i * 12, y + 257, 9 * fill, 7, energyColor);
            }
            if (boosting) {
                Line(new Vector2(x + 8, y + 184), new Vector2(x + 24, y + 199), 3, acid);
                Line(new Vector2(x + 24, y + 199), new Vector2(x + 40, y + 184), 3, acid);
            }
            if (lapNotice > 0) {
                float alpha = Mathf.Clamp01(lapNotice) * Mathf.Clamp01((3f - lapNotice) * 5);
                Color c = new Color(acid.r, acid.g, acid.b, alpha);
                Text(director.Lap == director.TotalLaps ? "FINAL LAP" : "LAP " + director.Lap, width * .5f - 210, 190, 420, 57, 42, c, true, TextAnchor.MiddleCenter);
                Text("LAST  " + TimeLabel(director.LastLap), width * .5f - 210, 248, 420, 28, 21, new Color(ivory.r, ivory.g, ivory.b, alpha), false, TextAnchor.MiddleCenter);
            }
        }

        void PrepareCircuit()
        {
            if (circuitPoints != null) return;
            circuit = VectorBootstrap.Instance != null ? VectorBootstrap.Instance.Track : FindAnyObjectByType<TrackPath>();
            if (!circuit) return;
            circuitPoints = new Vector2[161];
            circuitMin = new Vector2(float.MaxValue, float.MaxValue);
            circuitMax = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < circuitPoints.Length; i++) {
                Vector3 p = circuit.Evaluate(i / 160f).Position;
                circuitPoints[i] = new Vector2(p.x, -p.z);
                circuitMin = Vector2.Min(circuitMin, circuitPoints[i]);
                circuitMax = Vector2.Max(circuitMax, circuitPoints[i]);
            }
        }

        Vector2 MapPoint(Vector3 p)
        {
            float scale = 187f / Mathf.Max(circuitMax.x - circuitMin.x, circuitMax.y - circuitMin.y);
            return new Vector2(148, height - 160) + (new Vector2(p.x, -p.z) - (circuitMin + circuitMax) * .5f) * scale;
        }

        void DrawCircuit()
        {
            PrepareCircuit();
            if (circuitPoints == null) return;
            Text("NOCTURNE", 54, height - 305, 240, 28, 21, ivory);
            Text("CIRCUIT  /  01", 54, height - 278, 240, 22, 17, muted);
            for (int i = 1; i < circuitPoints.Length; i++) {
                Vector2 a = MapPoint(new Vector3(circuitPoints[i-1].x, 0, -circuitPoints[i-1].y));
                Vector2 b = MapPoint(new Vector3(circuitPoints[i].x, 0, -circuitPoints[i].y));
                Line(a, b, 7, new Color(.005f, .013f, .022f, .85f));
                Line(a, b, 3, new Color(.68f, .79f, .82f, .65f));
            }
            var start = circuit.Evaluate(0);
            Vector2 gate = MapPoint(start.Position);
            Vector2 tangent = new Vector2(start.Forward.x, -start.Forward.z).normalized;
            Vector2 across = new Vector2(-tangent.y, tangent.x) * 7;
            Line(gate - across, gate + across, 3, ivory);
            foreach (var racer in director.Racers) {
                if (!racer || racer == director.Player) continue;
                Vector2 dot = MapPoint(racer.transform.position);
                Box(dot.x - 4, dot.y - 4, 8, 8, new Color(.04f, .10f, .13f));
                Box(dot.x - 2.5f, dot.y - 2.5f, 5, 5, ivory);
            }
            if (director.Player) {
                Vector2 p = MapPoint(director.Player.transform.position);
                Vector3 f = director.Player.transform.forward;
                Vector2 forward = new Vector2(f.x, -f.z).normalized;
                Vector2 right = new Vector2(-forward.y, forward.x);
                Line(p + forward * 8, p - forward * 5 + right * 5, 4, acid);
                Line(p + forward * 8, p - forward * 5 - right * 5, 4, acid);
            }
            Box(55, height - 44, 6, 6, acid);
            Text("YOU", 69, height - 52, 61, 24, 17, muted);
            Box(134, height - 44, 5, 5, ivory);
            Text("RIVALS", 147, height - 52, 100, 24, 17, muted);
        }

        static Vector2 Polar(Vector2 center, float radius, float angle)
        {
            float radians = angle * Mathf.Deg2Rad;
            return center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
        }

        void Line(Vector2 a, Vector2 b, float thickness, Color color)
        {
            if (Event.current.type != EventType.Repaint) return;
            Matrix4x4 before = GUI.matrix;
            Vector2 delta = b - a;
            // Compose in virtual-canvas space. RotateAroundPivot mixes screen and
            // GUI coordinates when the root canvas has a non-unit scale.
            GUI.matrix = before * Matrix4x4.TRS(new Vector3(a.x, a.y, 0),
                Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg), Vector3.one);
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(-.5f, -thickness * .5f, delta.magnitude + 1f, thickness), stroke);
            GUI.color = previousColor;
            GUI.matrix = before;
        }

        void DrawCountdown()
        {
            bool go = director.Phase == RacePhase.Racing;
            string number = go ? "GO" : Mathf.CeilToInt(director.CountdownRemaining).ToString();
            for (int i = 0; i < 3; i++)
                Box(width * .5f - 62 + i * 44, height * .32f + 174, 36, 4, go || director.CountdownRemaining <= 3 - i ? acid : faint);
            Text(go ? "FULL THROTTLE" : "GET READY", width * .5f - 300, height * .32f - 34, 600, 38, 20, ivory, false, TextAnchor.MiddleCenter);
            Text(number, width * .5f - 220, height * .32f, 440, 175, 146, acid, true, TextAnchor.MiddleCenter);
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
            Text("NOCTURNE CIRCUIT / " + director.TotalLaps + " LAPS", x + 268, y + 276, 460, 32, 17, muted);
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
            Text("WASD / ARROWS   PILOT     SPACE   BOOST     Q / E   AIRBRAKES", x, y, 1170, 30, 21, ivory);
            Text("R   RECOVER     ESC / P   PAUSE     CONTROLLER: STICK + RT / LT · A BOOST · LB / RB AIRBRAKES · Y RECOVER", x, y + 36, 1640, 29, 18, muted);
        }

        bool Button(string label, string hint, float x, float y, float w, bool primary = false, float h = 64)
        {
            var rect = new Rect(x, y, w, h);
            bool hover = rect.Contains(Event.current.mousePosition);
            Box(x, y, w, h, primary ? (hover ? ivory : acid) : (hover ? new Color(.18f, .28f, .28f, .94f) : ink));
            if (!primary) Box(x, y + h - 1, w, 1, new Color(.44f, .63f, .63f, .35f));
            Color color = primary ? new Color(.04f, .095f, .10f) : ivory;
            Text(label, x + 21, y + 1, w - 140, h - 2, primary ? 24 : 22, color, true, TextAnchor.MiddleLeft);
            Text(hint, x + w - 137, y + 1, 116, h - 2, 18, primary ? color : muted, false, TextAnchor.MiddleRight);
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
            if (stroke != null) Destroy(stroke);
            if (sideShade != null) Destroy(sideShade);
            if (bottomShade != null) Destroy(bottomShade);
            // Font resources and whiteTexture are shared assets.
        }
    }
}
