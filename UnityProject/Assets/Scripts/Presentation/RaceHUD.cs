using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VectorRush
{
    /// <summary>Live player presentation. Race rules and finish adjudication remain in RaceDirector.</summary>
    public sealed partial class RaceHUD : MonoBehaviour
    {
        static RaceHUD instance;
        static int pauseConsumedFrame = -1;
        public static bool KeyboardCaptureActive { get; private set; }
        public static bool BlocksPauseInput => ProductionSettingsUI.BlocksRaceInput || KeyboardCaptureActive || (instance && (instance.settingsOpen || instance.bindingsOpen || instance.bindingInputNeedsRelease)) || pauseConsumedFrame == Time.frameCount;
        public string CurrentScreen => bindingsOpen ? "bindings" : settingsOpen ? "settings-" + settingsTab.ToString().ToLowerInvariant() : director ? director.Phase.ToString().ToLowerInvariant() : "uninitialized";
        public string FocusedAction => focus == null ? "" : focus.SelectedAction;
        public string ActiveInputDevice => gamepadActive ? "gamepad" : "keyboard-pointer";

        RaceDirector director;
        ChaseCamera chaseCamera;
        PlayerPreferences preferences;
        Texture2D white, sideShade, bottomShade, stroke, mapShade, cutCorner;
        Texture2D uiSelection, uiBoostTrack, uiBoostFill, uiChevron, uiCollision, uiRankUp, uiRankDown, uiWordmark;
        Font regular, condensed;
        GUIStyle textStyle, numberStyle;
        Vector2[] circuitPoints;
        Vector2 circuitMin, circuitMax;
        TrackPath circuit;
        float displayedSpeed, lapNotice, positionNotice, sectorNotice, boostCue;
        float screenElapsed, menuExitElapsed, selectionElapsed, collisionCue, drawAlpha = 1f;
        int observedLap = 1, observedPosition, observedSector;
        RacePhase observedPhase;
        string positionMessage = "";
        readonly Color ivory = new Color(.957f, .953f, .914f);
        readonly Color muted = new Color(.64f, .714f, .75f);
        readonly Color acid = new Color(.835f, 1f, .341f);
        readonly Color turquoise = new Color(.439f, .937f, 1f);
        readonly Color amber = new Color(1f, .72f, .36f);
        readonly Color faint = new Color(.76f, .85f, .88f, .25f);
        readonly Color ink = new Color(.024f, .075f, .11f, .82f);
        float width, height;
        bool ready, inputEvidence, gamepadActive, pointerActive;
        Vector2 lastPointer;
        MenuFocusController focus;
        RacePhase focusPhase;
        int menuFocus;
        bool settingsOpen, bindingsOpen, focusSettingsOpen, focusBindingsOpen;
        SettingsTab settingsTab, focusTab;
        PlayerAction? bindingCapture;
        int bindingCaptureFrame = -1;
        int bindingInputConsumedFrame = -1;
        bool bindingInputNeedsRelease;
        string sliderGestureId;
        float sliderGestureMinimum, sliderGestureMaximum, sliderGestureX, sliderGestureWidth;
        int sliderGestureFrame, sliderReleaseSequenceAtStart;
        bool sliderUsesInputSystem, sliderReleaseQueued, sliderAllowsCachedRelease;
        int sliderInputReleaseFrame = -1, sliderInputReleaseSequence;
        Vector2 sliderInputReleasePosition;
        bool sliderInputReleaseCoalesced;
        string bindingError, saveError;
        float nextStickRepeat;
        int lastStickDirection;
        bool showRivalCue;
        string rivalName, rivalGapLabel;
        string motionScreen, motionFocus;
        bool menuExitActive;
        int rivalMetres = -1, rivalRelation;
        enum SettingsTab { Audio, Controls, Comfort }
        static readonly PlayerAction[] bindingActions = (PlayerAction[])Enum.GetValues(typeof(PlayerAction));
        readonly Dictionary<string, TrackedRun> trackedLabels = new Dictionary<string, TrackedRun>();
        sealed class TrackedRun
        {
            public GUIContent[] glyphs;
            public float[] advances;
            public float width;
        }

        public void Initialize(RaceDirector raceDirector)
        {
            director = raceDirector;
            instance = this;
            pauseConsumedFrame = -1;
            KeyboardCaptureActive = false;
            chaseCamera = FindAnyObjectByType<ChaseCamera>();
            circuit = VectorBootstrap.Instance != null ? VectorBootstrap.Instance.Track : FindAnyObjectByType<TrackPath>();
            inputEvidence = Array.IndexOf(Environment.GetCommandLineArgs(), "-inputEvidence") >= 0;
            preferences = PlayerPreferences.Current;
            ApplyPreferences();
            motionScreen = CurrentScreen;
            motionFocus = "";
        }

        void Update()
        {
            if (!director) return;
            UpdateSliderInput();
            UpdateRivalCue();
            float speed = director.Player ? director.Player.SpeedKph : 0;
            displayedSpeed = Mathf.Lerp(displayedSpeed, speed, 1f - Mathf.Exp(-18f * Time.deltaTime));
            UpdateArcadeBoost(speed);
            if (director.Phase == RacePhase.Countdown && observedPhase != RacePhase.Countdown)
            {
                observedLap = 1; lapNotice = positionNotice = sectorNotice = displayedSpeed = 0;
                observedPosition = director.Position; observedSector = 0;
            }
            if (director.Phase == RacePhase.Racing)
            {
                if (director.Lap > observedLap) { observedLap = director.Lap; lapNotice = 3f; observedSector = 0; }
                if (observedPosition > 0 && director.Position != observedPosition)
                {
                    positionMessage = director.Position < observedPosition ? "POSITION GAINED" : "POSITION LOST";
                    positionNotice = RacePresentationMotion.PositionCueSeconds;
                }
                observedPosition = director.Position;
                var replay = ReplayPolishController.Instance;
                if (replay && replay.HasLatestSectorDelta && replay.LatestSectorNumber != observedSector)
                { observedSector = replay.LatestSectorNumber; sectorNotice = 5f; }
                lapNotice = Mathf.Max(0, lapNotice - Time.deltaTime);
                positionNotice = Mathf.Max(0, positionNotice - Time.deltaTime);
                sectorNotice = Mathf.Max(0, sectorNotice - Time.deltaTime);
            }
            float targetBoost = director.Player && director.Player.IsBoosting && director.Phase == RacePhase.Racing ? 1f : 0f;
            boostCue = RacePresentationMotion.Approach(boostCue, targetBoost, Time.unscaledDeltaTime, 12.5f, 5.56f, preferences.ReducedInterfaceMotion);
            observedPhase = director.Phase;
            EnsureFocus();
            UpdatePresentationMotion(Time.unscaledDeltaTime);
            if (ProductionSettingsUI.BlocksRaceInput) return;
            if (BlockConsumedBindingInput()) return;
            bool accept = preferences.WasPressedThisFrame(PlayerAction.Confirm), restart = preferences.WasPressedThisFrame(PlayerAction.Restart);
            bool up = false, down = false, left = false, right = false, back = false, cancelCapture = false, tab = false;
#if ENABLE_LEGACY_INPUT_MANAGER
            accept |= Input.GetKeyDown(KeyCode.KeypadEnter);
            up = Input.GetKeyDown(KeyCode.UpArrow); down = Input.GetKeyDown(KeyCode.DownArrow);
            left = Input.GetKeyDown(KeyCode.LeftArrow); right = Input.GetKeyDown(KeyCode.RightArrow);
            back = Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape);
            cancelCapture = Input.GetKeyDown(KeyCode.Backspace); tab = Input.GetKeyDown(KeyCode.Tab);
#endif
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                accept |= keyboard.numpadEnterKey.wasPressedThisFrame;
                up |= keyboard.upArrowKey.wasPressedThisFrame; down |= keyboard.downArrowKey.wasPressedThisFrame;
                left |= keyboard.leftArrowKey.wasPressedThisFrame; right |= keyboard.rightArrowKey.wasPressedThisFrame;
                back |= keyboard.backspaceKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame;
                cancelCapture |= keyboard.backspaceKey.wasPressedThisFrame; tab |= keyboard.tabKey.wasPressedThisFrame;
                if (keyboard.anyKey.wasPressedThisFrame) { gamepadActive = false; pointerActive = false; }
            }
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 p = mouse.position.ReadValue();
                if ((p - lastPointer).sqrMagnitude > 1f || mouse.leftButton.wasPressedThisFrame)
                { gamepadActive = false; pointerActive = true; }
                lastPointer = p;
                if (inputEvidence && (mouse.leftButton.wasPressedThisFrame || mouse.leftButton.wasReleasedThisFrame))
                    Debug.Log($"[InputEvidence] InputSystem frame={Time.frameCount} position={p} down={mouse.leftButton.wasPressedThisFrame} up={mouse.leftButton.wasReleasedThisFrame} screen={Screen.width}x{Screen.height} screenName={CurrentScreen}");
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                int direction = Mathf.Abs(stick.y) > .65f ? (stick.y > 0 ? -1 : 1) : 0;
                bool stickStep = direction != 0 && (direction != lastStickDirection || Time.unscaledTime >= nextStickRepeat);
                if (stickStep) nextStickRepeat = Time.unscaledTime + (direction != lastStickDirection ? .35f : .13f);
                lastStickDirection = direction;
                bool padUp = pad.dpad.up.wasPressedThisFrame || (stickStep && direction < 0);
                bool padDown = pad.dpad.down.wasPressedThisFrame || (stickStep && direction > 0);
                bool padLeft = pad.dpad.left.wasPressedThisFrame, padRight = pad.dpad.right.wasPressedThisFrame;
                bool padAccept = pad.buttonSouth.wasPressedThisFrame, padBack = pad.buttonEast.wasPressedThisFrame;
                up |= padUp; down |= padDown; left |= padLeft; right |= padRight; accept |= padAccept; back |= padBack; cancelCapture |= padBack;
                if (padUp || padDown || padLeft || padRight || padAccept || padBack || pad.startButton.wasPressedThisFrame || pad.rightTrigger.ReadValue() > .1f || stick.sqrMagnitude > .1f)
                { gamepadActive = true; pointerActive = false; }
            }
#endif
            if (bindingCapture.HasValue)
            {
                if (cancelCapture) CancelBinding();
                return;
            }
            if (settingsOpen && preferences.WasPressedThisFrame(PlayerAction.Pause)) back = true;
            if (back && (settingsOpen || bindingsOpen)) { Back(); return; }
            if (cancelCapture && director.Phase == RacePhase.Paused)
            { pauseConsumedFrame = Time.frameCount; director.TogglePause(); return; }
            if (cancelCapture && director.Phase == RacePhase.Finished) { ReturnToTitle(); return; }
            if (director.Phase == RacePhase.Racing || director.Phase == RacePhase.Countdown) return;
            if (up) focus.Move(-1);
            if (down || tab) focus.Move(1);
            if (up || down || tab || left || right) pointerActive = false;
            menuFocus = focus.SelectedIndex;
            if (settingsOpen && (left || right)) AdjustFocusedSetting(right ? 1 : -1);
            if (accept) ActivateFocused();
            else if (restart && !settingsOpen && !bindingsOpen && (director.Phase == RacePhase.Finished || director.Phase == RacePhase.Paused)) director.RestartRace();
        }

        void UpdatePresentationMotion(float deltaTime)
        {
            string screen = CurrentScreen;
            string selected = focus == null ? "" : focus.SelectedAction;
            if (screen != motionScreen)
            {
                motionScreen = screen;
                motionFocus = selected;
                screenElapsed = selectionElapsed = 0f;
            }
            else screenElapsed += deltaTime;

            if (selected != motionFocus)
            {
                bool announce = !string.IsNullOrEmpty(motionFocus) && !string.IsNullOrEmpty(selected);
                motionFocus = selected;
                selectionElapsed = 0f;
                if (announce && director.Phase != RacePhase.Racing && director.Phase != RacePhase.Countdown)
                    RaceAudio.Instance?.PlayInterfaceMove();
            }
            else selectionElapsed += deltaTime;

            RaceAudio audio = RaceAudio.Instance;
            float contact = audio == null ? 0f : Mathf.Max(audio.ImpactFeedback01, audio.ScrapeFeedback01 * .78f);
            collisionCue = RacePresentationMotion.Approach(collisionCue, contact, deltaTime, 12f, 3.5f, preferences.ReducedInterfaceMotion);
            if (menuExitActive)
            {
                menuExitElapsed += deltaTime;
                if (menuExitElapsed >= RacePresentationMotion.MenuExitSeconds) menuExitActive = false;
            }
        }
        void UpdateRivalCue()
        {
            showRivalCue = false;
            var player = director.Player;
            if (director.Phase != RacePhase.Racing || !player || !player.Body ||
                player.ProgressTracker == null || player.ProgressTracker.CompletedLaps >= director.TotalLaps ||
                director.Racers == null) return;
            if (!circuit && VectorBootstrap.Instance != null) circuit = VectorBootstrap.Instance.Track;
            if (!circuit || circuit.Length <= 0) return;

            HoverVehicle nearest = null;
            float nearestGap = 0, nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < director.Racers.Count; i++)
            {
                var rival = director.Racers[i];
                if (!rival || rival == player || !rival.Body || rival.ProgressTracker == null ||
                    rival.ProgressTracker.CompletedLaps >= director.TotalLaps) continue;
                // Validated race distance preserves lap order; the two proximity
                // checks exclude lapped traffic and nearby, separate track branches.
                float gap = (rival.RaceProgress - player.RaceProgress) * circuit.Length;
                float distance = Mathf.Abs(gap);
                float trackGap = (Mathf.Repeat(rival.TrackProgress - player.TrackProgress + .5f, 1f) - .5f) * circuit.Length;
                if (float.IsNaN(gap) || distance > 100f || distance >= nearestDistance ||
                    Mathf.Abs(trackGap) > 100f || (rival.Body.position - player.Body.position).sqrMagnitude >= 10000f) continue;
                nearest = rival;
                nearestGap = gap;
                nearestDistance = distance;
            }
            if (!nearest) return;

            rivalName = string.IsNullOrEmpty(nearest.DisplayName) ? nearest.name : nearest.DisplayName;
            int metres = Mathf.RoundToInt(nearestDistance);
            int relation = nearestDistance < 2f ? 0 : nearestGap > 0 ? 1 : -1;
            if (metres != rivalMetres || relation != rivalRelation || rivalGapLabel == null)
            {
                rivalMetres = metres;
                rivalRelation = relation;
                rivalGapLabel = metres + " M  /  " + (relation == 0 ? "SIDE BY SIDE" : relation > 0 ? "AHEAD" : "BEHIND");
            }
            showRivalCue = true;
        }

        void Prepare()
        {
            if (ready) return;
            white = Texture2D.whiteTexture;
            regular = Resources.Load<Font>("ExperienceArt/fonts/vrx_font_text_a");
            condensed = Resources.Load<Font>("ExperienceArt/fonts/vrx_font_display_a");
            if (!regular) regular = Resources.Load<Font>("Fonts/Barlow-Medium");
            if (!regular) regular = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!condensed) condensed = Resources.Load<Font>("Fonts/Rajdhani-SemiBold");
            if (!condensed) condensed = Resources.Load<Font>("Fonts/BarlowCondensed-SemiBold");
            if (!condensed) condensed = regular;
            uiSelection = Resources.Load<Texture2D>("ExperienceArt/ui/vrx_ui_selection_a");
            uiBoostTrack = Resources.Load<Texture2D>("ExperienceArt/ui/vrx_ui_boost_track_a");
            uiBoostFill = Resources.Load<Texture2D>("ExperienceArt/ui/vrx_ui_boost_fill_a");
            uiChevron = Resources.Load<Texture2D>("ExperienceArt/ui/vrx_ui_chevron_a");
            uiCollision = Resources.Load<Texture2D>("ExperienceArt/ui/vrx_ui_collision_a");
            uiRankUp = Resources.Load<Texture2D>("ExperienceArt/ui/vrx_ui_rank_up_a");
            uiRankDown = Resources.Load<Texture2D>("ExperienceArt/ui/vrx_ui_rank_down_a");
            uiWordmark = Resources.Load<Texture2D>("ExperienceArt/ui/vrx_ui_wordmark_a");
            textStyle = new GUIStyle { font = regular, richText = false, clipping = TextClipping.Clip };
            numberStyle = new GUIStyle(textStyle) { font = condensed };
            stroke = new Texture2D(1, 16, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int i = 0; i < 16; i++) stroke.SetPixel(0, i, new Color(1, 1, 1, Mathf.Clamp01(Mathf.Min(i, 15 - i) / 2f)));
            stroke.Apply(false, true);
            cutCorner = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                cutCorner.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(.5f + (y - x) / 1.4f)));
            cutCorner.Apply(false, true);
            sideShade = Gradient(true); bottomShade = Gradient(false);
            mapShade = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float radius = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 32f;
                mapShade.SetPixel(x, y, new Color(0, 0, 0, .92f * (1f - Mathf.SmoothStep(.65f, 1f, radius))));
            }
            mapShade.Apply(false, true); ready = true;
        }

        Texture2D Gradient(bool horizontal)
        {
            var texture = new Texture2D(horizontal ? 128 : 1, horizontal ? 1 : 128, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int i = 0; i < 128; i++)
            {
                float t = i / 127f;
                float alpha = horizontal ? .97f * (1f - Mathf.SmoothStep(.26f, 1f, t)) : .66f * Mathf.Pow(1f - t, 2f);
                texture.SetPixel(horizontal ? i : 0, horizontal ? 0 : i, new Color(0, 0, 0, alpha));
            }
            texture.Apply(false, true); return texture;
        }

        void OnGUI()
        {
            if (!director) return;
            Prepare(); EnsureFocus();
            Matrix4x4 previous = GUI.matrix; Color previousColor = GUI.color;
            Vector2 rawMouse = Event.current.mousePosition, correctedMouse = rawMouse;
            float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            if (scale <= 0) return;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && (Event.current.isMouse || Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout))
            { Vector2 p = Mouse.current.position.ReadValue(); correctedMouse = new Vector2(p.x, Screen.height - p.y); }
#endif
            width = Screen.width / scale; height = Screen.height / scale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            Event.current.mousePosition = correctedMouse / scale;
            GUI.color = Color.white;
            // Texture row zero is the lower edge. Preserve normal UVs so the fade
            // meets the driving view transparently instead of exposing an opaque seam.
            GUI.DrawTexture(new Rect(0, height - 260, width, 260), bottomShade);
            CaptureBindingEvent();
            if (bindingsOpen) DrawBindings();
            else if (settingsOpen) DrawSettings();
            else if (director.Phase == RacePhase.Menu) DrawMenu();
            else if (director.Phase == RacePhase.Paused) DrawPause();
            else if (director.Phase == RacePhase.Finished) DrawResults();
            else
            {
                DrawTelemetry();
                if (director.Phase == RacePhase.Countdown || (director.Phase == RacePhase.Racing && director.RaceTime < .65f)) DrawCountdown();
            }
            if (menuExitActive && !preferences.ReducedInterfaceMotion) DrawMenuExitTrace();
            GUI.matrix = previous; Event.current.mousePosition = rawMouse; GUI.color = previousColor;
        }

        void MenuBackdrop(float extent = .8f)
        {
            Color before = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, drawAlpha);
            GUI.DrawTexture(new Rect(0, 0, width * extent, height), sideShade);
            GUI.color = before;
        }
        void Edition(string text, string detail = "")
        {
            Brandmark(70, 48, acid);
            TrackedText(text, 104, 43, 700, 26, 14, ivory, 1.8f);
            if (!string.IsNullOrEmpty(detail)) TrackedText(detail, width - 420, 43, 350, 26, 13, muted, 1.4f, TextAnchor.MiddleRight);
        }
        void Brandmark(float x, float y, Color color)
        {
            Line(new Vector2(x, y), new Vector2(x + 10, y + 14), 5, color);
            Line(new Vector2(x + 10, y + 14), new Vector2(x + 20, y), 4, color);
        }
        void DrawMenu()
        {
            MenuMotionSample motion = RacePresentationMotion.Menu(screenElapsed, 0f, false, preferences.ReducedInterfaceMotion);
            Matrix4x4 menuMatrix = GUI.matrix;
            Vector2 menuPointer = Event.current.mousePosition;
            float previousAlpha = drawAlpha;
            GUI.matrix = menuMatrix * Matrix4x4.Translate(new Vector3(motion.OffsetX, 0f, 0f));
            Event.current.mousePosition = menuPointer - new Vector2(motion.OffsetX, 0f);
            drawAlpha *= motion.Visibility;
            MenuBackdrop();
            Edition("ANTI-GRAVITY RACING", "VOL. 01");
            if (uiWordmark) Graphic(uiWordmark, new Rect(62, 170, 650, 114), Color.white);
            else Text("VECTOR RUSH", 62, 171, 650, 84, 56, ivory, true);
            Box(73, 473, 3, 56, acid);
            Text("NOCTURNE CIRCUIT", 92, 468, 620, 36, 27, ivory, true);
            TrackedText("MIDNIGHT EXHIBITION / " + RacerCount + " PILOTS / " + director.TotalLaps + " LAPS", 92, 506, 670, 27, 13, muted, .8f);
            if (Button("START RACE", ConfirmPrompt, 70, 575, 410, true)) QueueMenuAction(() => director.StartRace());
            if (Button("SETTINGS", "→", 70, 639, 410)) QueueMenuAction(OpenSettings);
            if (Button("CONTROLS", "→", 70, 703, 410)) QueueMenuAction(OpenControls);
            if (Button("QUIT APPLICATION", "", 70, 767, 410, false, 42, "quit")) QueueMenuAction(Application.Quit);
            TrackedText("NOCTURNE / 01", 72, height - 47, 540, 24, 13, muted, 1.3f);
            TrackedText("LOCAL EXHIBITION", width - 410, height - 47, 350, 24, 13, muted, 1.3f, TextAnchor.MiddleRight);
            drawAlpha = previousAlpha;
            GUI.matrix = menuMatrix;
            Event.current.mousePosition = menuPointer;
        }
        void DrawMenuExitTrace()
        {
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, RacePresentationMotion.MenuExitSeconds, menuExitElapsed));
            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, (1f - progress) * .55f);
            GUI.DrawTexture(new Rect(-width * .8f * progress, 0f, width * .8f, height), sideShade);
            GUI.color = previous;
            Color line = acid; line.a = (1f - progress) * .7f;
            Box(Mathf.Lerp(0f, width * .8f, progress), 0f, 3f, height, line);
        }
        int RacerCount => director.Racers == null ? 0 : director.Racers.Count;
        string ConfirmPrompt => gamepadActive ? "A / CROSS" : KeyLabel(preferences.BindingFor(PlayerAction.Confirm));
        string BackPrompt => gamepadActive ? "B / CIRCLE" : "ESC";
        string BoostPrompt => gamepadActive ? "A / CROSS" : KeyLabel(preferences.BindingFor(PlayerAction.Boost));
        string PausePrompt => gamepadActive ? "START" : KeyLabel(preferences.BindingFor(PlayerAction.Pause));
        Matrix4x4 ScaleHud(Vector2 anchor)
        {
            Matrix4x4 before = GUI.matrix;
            float s = preferences.HudScale;
            GUI.matrix = before * Matrix4x4.Translate(anchor) * Matrix4x4.Scale(new Vector3(s, s, 1)) * Matrix4x4.Translate(-anchor);
            return before;
        }
        void DrawTelemetry()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-legacyHud") >= 0) DrawLegacyTelemetry();
            else DrawArcadeTelemetry();
        }
        void DrawLegacyTelemetry()
        {
            bool boosting = director.Player && director.Player.IsBoosting && director.Phase == RacePhase.Racing;
            PresentationCuePriority priority = RacePresentationMotion.SelectPriority(collisionCue > .035f, positionNotice > 0f, boostCue > .035f);
            Matrix4x4 before = ScaleHud(new Vector2(58, 46));
            Brandmark(58, 49, acid);
            TrackedText("NOCTURNE / EXHIBITION", 90, 43, 300, 25, 13, ivory, 1.4f);
            Text(director.Position.ToString("00"), 56, 75, 125, 76, 58, ivory, true);
            Text("/" + RacerCount.ToString("00"), 172, 126, 70, 37, 24, muted);
            Box(249, 88, 1, 76, faint);
            TrackedText("LAP", 275, 82, 100, 25, 13, muted, 1.4f);
            Text(Mathf.Clamp(director.Lap, 1, director.TotalLaps).ToString("00"), 274, 104, 53, 43, 25, ivory, true);
            Text("/" + director.TotalLaps.ToString("00"), 328, 117, 56, 34, 24, muted, true);
            for (int i = 0; i < director.TotalLaps; i++) Box(277 + i * 27, 162, 23, 3, i < director.Lap ? acid : faint);
            if (showRivalCue)
            {
                Box(58, 187, 315, 1, faint);
                DirectionIcon(new Vector2(68, 214), rivalRelation, acid);
                TrackedText(rivalName.ToUpperInvariant(), 93, 201, 135, 30, 15, ivory, 1f);
                Text(rivalMetres + " m", 213, 198, 84, 32, 23, ivory, true, TextAnchor.MiddleRight);
                TrackedText(rivalRelation == 0 ? "ALONGSIDE" : rivalRelation > 0 ? "AHEAD" : "BEHIND", 305, 204, 100, 25, 12, muted, .9f);
            }
            if (priority == PresentationCuePriority.Position) DrawPositionNotice(showRivalCue ? 243 : 196);
            GUI.matrix = before;
            before = ScaleHud(new Vector2(width - 58, 47));
            DrawTiming();
            GUI.matrix = before;
            before = ScaleHud(new Vector2(55, height - 55));
            DrawCircuit();
            GUI.matrix = before;
            before = ScaleHud(new Vector2(width - 58, height - 57));
            DrawSpeed(boosting, priority == PresentationCuePriority.Boost);
            GUI.matrix = before;
            if (priority == PresentationCuePriority.Boost && boostCue > 0)
            {
                Color edge = turquoise; edge.a = .7f * boostCue;
                Box(width - 4, 0, 4, height, edge);
            }
            if (priority == PresentationCuePriority.Contact) DrawContactNotice();
            else if (lapNotice > 0)
            {
                Color c = NoticeColor(acid, lapNotice, 3f);
                Text(director.Lap == director.TotalLaps ? "FINAL LAP" : "LAP " + director.Lap, width / 2 - 230, 148, 460, 62, 46, c, true, TextAnchor.MiddleCenter);
                Text("LAST  " + TimeLabel(director.LastLap), width / 2 - 230, 211, 460, 27, 16, NoticeColor(ivory, lapNotice, 3f), false, TextAnchor.MiddleCenter);
            }
        }
        void DrawPositionNotice(float y)
        {
            float elapsed = RacePresentationMotion.PositionCueSeconds - positionNotice;
            float amount = RacePresentationMotion.PositionEnvelope(elapsed, preferences.ReducedInterfaceMotion);
            Color color = acid; color.a *= amount;
            float offset = preferences.ReducedInterfaceMotion ? 0f : (1f - amount) * -18f;
            Texture2D arrow = positionMessage == "POSITION GAINED" ? uiRankUp : uiRankDown;
            if (arrow) Graphic(arrow, new Rect(55 + offset, y - 1, 24, 24), new Color(1f, 1f, 1f, amount));
            Box(58 + offset, y + 4, 3, 19, color);
            TrackedText(positionMessage, (arrow ? 85 : 70) + offset, y, 303, 26, 13, color, .8f);
        }
        void DrawContactNotice()
        {
            RaceAudio audio = RaceAudio.Instance;
            bool scraping = audio != null && audio.ScrapeFeedback01 > audio.ImpactFeedback01;
            string label = scraping ? "SUSTAINED CONTACT" : collisionCue > .58f ? "HEAVY IMPACT" : "HULL CONTACT";
            Color color = amber; color.a *= Mathf.Clamp01(collisionCue);
            float x = width * .5f - 180f;
            if (uiCollision) Graphic(uiCollision, new Rect(x - 46, 148, 40, 40), new Color(1f, 1f, 1f, Mathf.Clamp01(collisionCue)));
            Box(x, 154, 3, 34, color);
            TrackedText(label, x + 17, 151, 326, 28, 15, color, 1.4f, TextAnchor.MiddleCenter);
            Box(x + 17, 188, 326 * Mathf.Clamp01(collisionCue), 2, color);
        }
        Color NoticeColor(Color color, float remaining, float duration)
        {
            if (!preferences.ReducedInterfaceMotion) color.a *= Mathf.Clamp01(remaining * 2) * Mathf.Clamp01((duration - remaining) * 8);
            return color;
        }
        void DrawTiming()
        {
            float x = width - 322;
            TrackedText("RACE", x, 49, 74, 25, 13, muted, 1.4f);
            Text(director.RaceTime > 0 ? TimeLabel(director.RaceTime) : "00:00.000", x + 75, 41, 189, 40, 25, ivory, true, TextAnchor.MiddleRight);
            Box(x, 93, 264, 1, faint);
            RaceBest best = director.PersonalBest;
            TrackedText("PERSONAL BEST", x, 105, 146, 24, 12, muted, 1.1f);
            Text(best != null && best.HasBestLap ? TimeLabel(best.BestLap) : "NO RECORD", x + 139, 101, 125, 29, 18, ivory, true, TextAnchor.MiddleRight);
            var replay = ReplayPolishController.Instance;
            bool eligible = director.ManualRunEligible && director.CurrentLapEligible;
            if (replay && replay.HasLatestSectorDelta && eligible && sectorNotice > 0)
            {
                Color c = replay.LatestSectorDelta <= 0 ? acid : amber;
                TrackedText("SECTOR " + replay.LatestSectorNumber.ToString("00"), x, 147, 125, 30, 13, muted, 1.1f);
                Text(DeltaLabel(replay.LatestSectorDelta), x + 116, 135, 131, 45, 31, c, true, TextAnchor.MiddleRight);
                Box(x + 261, 140, 3, 35, c);
            }
            TrackedText(GhostStateLabel(preferences.GhostEnabled, replay != null && replay.GhostAvailable, replay != null && replay.GhostVisible), x, 190, 264, 24, 12, muted, .6f, TextAnchor.MiddleRight);
            if (director.Phase == RacePhase.Racing && !eligible)
                TrackedText(director.ManualRunEligible ? "LAP INVALID / RECOVERY" : "RECORDS EXCLUDED / AUTOMATED", x - 60, 218, 324, 25, 12, amber, .6f, TextAnchor.MiddleRight);
        }
        void DrawSpeed(bool boosting, bool emphasizeBoost)
        {
            float x = width - 356, y = height - 337;
            float charge = director.Player ? Mathf.Clamp01(director.Player.Boost01) : 0;
            bool requiresRelease = director.Player && director.Player.BoostRequiresRelease;
            string state = BoostStateLabel(boosting, charge);
            Color energy = state == "LOW" ? amber : turquoise;
            if (emphasizeBoost && boostCue > 0)
            {
                Color c = turquoise; c.a *= boostCue;
                Box(x, y - 3, 5, 5, c);
                TrackedText("OVERDRIVE", x + 16, y - 13, 188, 25, 14, c, 2.3f);
                TrackedText("ENGAGED", x + 207, y - 10, 90, 22, 11, c, 1.2f, TextAnchor.MiddleRight);
                Box(x + 314, y, 2, 240, c);
            }
            Text(Mathf.RoundToInt(displayedSpeed).ToString("000"), x - 4, y + 42, 217, 92, 70, ivory, true);
            TrackedText("KM/H", x + 231, y + 84, 80, 25, 14, muted, 1.6f);
            for (int i = 0; i < 3; i++) Line(new Vector2(x + 235 + i * 15, y + 142), new Vector2(x + 246 + i * 15, y + 123), 3, boosting ? turquoise : ivory);
            float maximum = director.Player ? director.Player.BoostSpeed * 3.6f : 389f;
            SlantedBar(x, y + 169, 298, 3, faint);
            SlantedBar(x, y + 169, 298 * Mathf.Clamp01(displayedSpeed / maximum), 3, boosting ? turquoise : ivory);
            Line(new Vector2(x + 17, y + 186), new Vector2(x + 6, y + 201), 3, energy);
            Line(new Vector2(x + 6, y + 201), new Vector2(x + 16, y + 201), 3, energy);
            Line(new Vector2(x + 16, y + 201), new Vector2(x + 5, y + 214), 3, energy);
            TrackedText("BOOST", x + 28, y + 190, 77, 25, 14, ivory, 1.2f);
            TrackedText(state, x + 107, y + 192, 115, 24, 12, energy, .9f);
            Text(Percent(charge), x + 226, y + 185, 72, 30, 23, ivory, true, TextAnchor.MiddleRight);
            if (uiBoostTrack && uiBoostFill) DrawBoostStrip(new Rect(x, y + 218, 298, 37), charge);
            else for (int i = 0; i < 10; i++)
            {
                float fill = Mathf.Clamp01(charge * 10 - i);
                SlantedBar(x + i * 30, y + 225, 27, 9, faint);
                if (fill > 0) SlantedBar(x + i * 30, y + 225, 27 * fill, 9, energy);
            }
            TrackedText(BoostPrompt + "   " + (requiresRelease ? "RELEASE TO REARM" : boosting ? "RELEASE TO RECHARGE" : charge <= .02f ? "RECHARGING" : "HOLD TO BOOST"), x, y + 263, 320, 29, 12, requiresRelease ? amber : muted, .7f);
        }
        void DrawBoostStrip(Rect rect, float charge)
        {
            Graphic(uiBoostTrack, rect, Color.white);
            float width = rect.width * Mathf.Clamp01(charge);
            if (width <= 0f) return;
            GUI.BeginGroup(new Rect(rect.x, rect.y, width, rect.height));
            Graphic(uiBoostFill, new Rect(0f, 0f, rect.width, rect.height), Color.white);
            GUI.EndGroup();
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
            float scale = 145f / Mathf.Max(circuitMax.x - circuitMin.x, circuitMax.y - circuitMin.y);
            return new Vector2(139, height - 139) + (new Vector2(p.x, -p.z) - (circuitMin + circuitMax) * .5f) * scale;
        }
        void DrawCircuit()
        {
            PrepareCircuit();
            if (circuitPoints == null) return;
            GUI.DrawTexture(new Rect(45, height - 233, 188, 188), mapShade);
            for (int i = 1; i < circuitPoints.Length; i++)
            {
                Vector2 a = MapPoint(new Vector3(circuitPoints[i - 1].x, 0, -circuitPoints[i - 1].y));
                Vector2 b = MapPoint(new Vector3(circuitPoints[i].x, 0, -circuitPoints[i].y));
                Line(a, b, 6, ink); Line(a, b, 2.3f, ivory);
            }
            var start = circuit.Evaluate(0); Vector2 gate = MapPoint(start.Position);
            Vector2 tangent = new Vector2(start.Forward.x, -start.Forward.z).normalized;
            Vector2 across = new Vector2(-tangent.y, tangent.x) * 5;
            Line(gate - across, gate + across, 3, ivory);
            foreach (var racer in director.Racers)
            {
                if (!racer || racer == director.Player) continue;
                Vector2 dot = MapPoint(racer.transform.position);
                Box(dot.x - 4, dot.y - 4, 8, 8, ink); Box(dot.x - 2.5f, dot.y - 2.5f, 5, 5, ivory);
            }
            if (director.Player)
            {
                Vector2 p = MapPoint(director.Player.transform.position);
                Vector3 f = director.Player.transform.forward; Vector2 forward = new Vector2(f.x, -f.z).normalized;
                Vector2 right = new Vector2(-forward.y, forward.x);
                Line(p + forward * 8, p - forward * 5 + right * 5, 5, ink); Line(p + forward * 8, p - forward * 5 - right * 5, 5, ink);
                Line(p + forward * 8, p - forward * 5 + right * 5, 3, acid); Line(p + forward * 8, p - forward * 5 - right * 5, 3, acid);
            }
            Text("NOCTURNE", 241, height - 104, 205, 30, 22, ivory, true);
            TrackedText((circuit.Length / 1000f).ToString("0.00") + " KM / CIRCUIT 01", 242, height - 72, 235, 25, 12, muted, .8f);
        }
        void DrawCountdown()
        {
            bool go = director.Phase == RacePhase.Racing;
            string number = go ? "GO" : Mathf.Max(1, Mathf.CeilToInt(director.CountdownRemaining)).ToString();
            TrackedText(go ? "FULL THROTTLE" : "GET READY", width / 2 - 250, 221, 500, 30, 16, ivory, 2f, TextAnchor.MiddleCenter);
            Text(number, width / 2 - 150, 252, 300, 169, 146, acid, true, TextAnchor.MiddleCenter);
            for (int i = 0; i < 3; i++) Box(width / 2 - 56 + i * 40, 435, 32, 4, go || director.CountdownRemaining <= 3 - i ? acid : faint);
            if (!go) Text(gamepadActive ? "RT  THROTTLE    A / CROSS  BOOST" : KeyLabel(preferences.BindingFor(PlayerAction.Throttle)) + "  THROTTLE    " + BoostPrompt + "  BOOST", width / 2 - 330, height - 102, 660, 28, 14, muted, false, TextAnchor.MiddleCenter);
        }
        void DrawPause()
        {
            MenuBackdrop();
            Edition("NOCTURNE / LAP " + director.Lap.ToString("00") + " OF " + director.TotalLaps.ToString("00"));
            Text("RACE", 66, 145, 680, 133, 110, ivory, true);
            Text("PAUSED.", 66, 247, 720, 133, 110, acid, true);
            Text(director.Position.ToString("00") + " / " + RacerCount.ToString("00"), 73, 405, 150, 42, 29, muted, true);
            Text(director.RaceTime > 0 ? TimeLabel(director.RaceTime) : "00:00.000", 264, 405, 290, 42, 29, muted, true);
            if (Button("RESUME", PausePrompt, 70, 491, 410, true)) director.TogglePause();
            if (Button("RESTART RACE", KeyLabel(preferences.BindingFor(PlayerAction.Restart)), 70, 555, 410)) director.RestartRace();
            if (Button("SETTINGS", "→", 70, 619, 410)) OpenSettings();
            if (Button("RETURN TO TITLE", "→", 70, 683, 410)) ReturnToTitle();
            if (Button("QUIT APPLICATION", "", 70, 757, 410, false, 42, "quit")) Application.Quit();
        }
        void DrawResults()
        {
            MenuBackdrop(.89f);
            Edition("NOCTURNE CIRCUIT", "RACE COMPLETE");
            Text("RACE", 66, 123, 187, 101, 81, ivory, true);
            Text("COMPLETE.", 256, 123, 530, 101, 81, acid, true);
            Text(director.Position.ToString("00"), 64, 226, 228, 190, 170, acid, true);
            Text("OF " + RacerCount.ToString("00") + " PILOTS", 306, 279, 320, 28, 15, muted);
            Text(director.Position == 1 ? "RACE WINNER" : director.Position <= 3 ? "PODIUM FINISH" : "CLASSIFIED FINISH", 306, 325, 380, 44, 29, ivory, true);
            Box(71, 459, 550, 1, faint);
            Text("OFFICIAL RACE TIME", 71, 481, 275, 26, 13, muted);
            Text(TimeLabel(director.FinishTime), 71, 514, 279, 70, 51, ivory, true);
            Text("BEST VALID LAP", 375, 481, 210, 26, 13, muted);
            Text(TimeLabel(director.BestLap), 375, 514, 280, 70, 51, ivory, true);
            RaceRecordComparison record = director.CurrentRecordComparison;
            if (record != null && record.UpdatedLapBest && !record.ExcludedAutomatedRun)
                Text("NEW PB", 550, 481, 95, 26, 13, acid);
            Box(72, 600, 550, 1, faint);
            Text("VS PERSONAL BEST", 72, 616, 151, 47, 12, muted);
            var replay = ReplayPolishController.Instance;
            for (int i = 0; i < 3; i++)
            {
                bool valid = !director.AutomatedRecordExcluded && replay && replay.ResultSectorHasReference(i);
                float delta = valid ? replay.ResultSectorDeltas[i] : 0;
                Text("S" + (i + 1), 252 + i * 125, 610, 115, 22, 12, muted);
                Text(valid ? DeltaLabel(delta) : "—", 252 + i * 125, 633, 115, 36, 26, valid ? delta <= 0 ? acid : amber : muted, true);
            }
            Box(72, 681, 550, 1, faint);
            string recordStatus = ResultRecordLabel(director.ManualRunEligible, director.BestLap, record, director.RecordSaveError);
            Text(recordStatus, 72, 692, 760, 35, 13, !string.IsNullOrEmpty(director.RecordSaveError) ? amber : muted);
            if (Button("RACE AGAIN", ConfirmPrompt, 70, 746, 305, true)) director.RestartRace();
            if (Button("MAIN MENU", "→", 391, 746, 230, false, 54, "return to title")) ReturnToTitle();
            if (replay) Text(replay.StatusMessage, 72, 818, 820, 42, 13, muted);
            DrawClassification();
        }
        void DrawClassification()
        {
            float x = width - 538, y = 247;
            Box(x, y, 478, 454, ink);
            Text(director.HasPendingRivals ? "CLASSIFICATION / LIVE" : "FINAL CLASSIFICATION", x + 24, y + 17, 315, 30, 13, muted);
            Text(director.TotalLaps + " LAPS", x + 359, y + 17, 95, 30, 12, muted, false, TextAnchor.MiddleRight);
            Box(x + 24, y + 60, 430, 1, faint);
            int row = 0;
            foreach (var record in director.FinishRecords.OrderBy(value => value.Status == RacerResultStatus.Finished ? value.Position : int.MaxValue))
            {
                ClassificationRow(x + 24, y + 66 + row++ * 56, record.Position, record.IsPlayer ? "YOU" : record.DisplayName.ToUpperInvariant(), FinishStatusLabel(record), record.IsPlayer);
            }
            for (int i = 0; i < director.Racers.Count; i++)
            {
                string id = "racer-" + i;
                if (director.FinishRecords.Any(value => value.RacerId == id)) continue;
                var racer = director.Racers[i];
                if (racer) ClassificationRow(x + 24, y + 66 + row++ * 56, 0, racer.IsPlayer ? "YOU" : racer.DisplayName.ToUpperInvariant(), "RACING", racer.IsPlayer);
            }
            if (director.HasPendingRivals) Text("Remaining pilots are still finishing.", x + 24, y + 412, 430, 27, 13, muted);
        }
        void ClassificationRow(float x, float y, int position, string name, string time, bool player)
        {
            if (player) { Box(x - 3, y, 433, 53, new Color(acid.r, acid.g, acid.b, .10f)); Box(x - 3, y, 3, 53, acid); }
            Text(position > 0 ? position.ToString("00") : "—", x + 12, y + 8, 44, 36, 27, player ? acid : muted, true);
            Text(name, x + 61, y + 13, 172, 29, 16, player ? acid : ivory);
            Text(time, x + 237, y + 7, 181, 38, 26, ivory, true, TextAnchor.MiddleRight);
            Box(x, y + 54, 430, 1, new Color(1, 1, 1, .08f));
        }
        void DrawSettings()
        {
            Box(0, 0, width, height, new Color(.012f, .052f, .078f, .94f));
            Edition("VECTOR RUSH", "CONFIGURATION");
            Text("SETTINGS.", 67, 144, 490, 120, 88, acid, true);
            if (Button("AUDIO", "01", 70, 419, 365, false, 58, "tab:audio")) SelectTab(SettingsTab.Audio);
            if (Button("CONTROLS", "02", 70, 487, 365, false, 58, "tab:controls")) SelectTab(SettingsTab.Controls);
            if (Button("DISPLAY & COMFORT", "03", 70, 555, 365, false, 58, "tab:comfort")) SelectTab(SettingsTab.Comfort);
            float x = 585, w = width - 685;
            string title = settingsTab == SettingsTab.Audio ? "AUDIO" : settingsTab == SettingsTab.Controls ? "CONTROLS" : "DISPLAY & COMFORT";
            Text(title, x, 155, w, 65, 44, ivory, true);
            Text(settingsTab == SettingsTab.Audio ? "Find your balance between the machine and the music." : settingsTab == SettingsTab.Controls ? "Steering, keyboard bindings and your personal-best ghost." : "Keep your view clear and your instruments readable.", x, 228, w, 36, 16, muted);
            if (settingsTab == SettingsTab.Audio)
            {
                SliderRow("MASTER VOLUME", "Overall game volume", "master volume", preferences.MasterVolume, 0, 1, x, 307, w);
                SliderRow("MUSIC", "Soundtrack volume", "music volume", preferences.MusicVolume, 0, 1, x, 421, w);
                SliderRow("EFFECTS", "Engines, track and race cues", "effects volume", preferences.EffectsVolume, 0, 1, x, 535, w);
            }
            else if (settingsTab == SettingsTab.Controls)
            {
                SliderRow("STEERING SENSITIVITY", "Adjust steering response", "steering sensitivity", preferences.SteeringSensitivity, .5f, 1.5f, x, 307, w);
                ToggleRow("PERSONAL-BEST GHOST", "Show your compatible saved eligible lap", "personal best ghost", preferences.GhostEnabled, x, 421, w);
                SettingRule(x, 535, w, "key bindings");
                Text("KEYBOARD BINDINGS", x, 557, w - 280, 30, 16, ivory);
                Text("Reassign keys and resolve binding conflicts", x, 591, w - 280, 27, 13, muted);
                if (Button("EDIT BINDINGS", "→", x + w - 236, 558, 236, false, 49, "key bindings")) OpenBindings();
            }
            else
            {
                ToggleRow("CAMERA SHAKE", "Enable camera feedback", "camera shake", preferences.ShakeEnabled, x, 307, w);
                ToggleRow("REDUCED INTERFACE MOTION", "Use stable indicators and instant UI transitions", "reduced interface motion", preferences.ReducedInterfaceMotion, x, 421, w);
                SliderRow("HUD SCALE", "Size of race instruments", "hud scale", preferences.HudScale, .85f, 1.2f, x, 535, w);
            }
            Box(x, 650, w, 1, faint);
            string note = settingsTab == SettingsTab.Controls ? "CONTROLLER: stick steer · RT/LT throttle/brake · A / Cross boost" : "UP / DOWN to navigate. LEFT / RIGHT to adjust the selected control.";
            Text(note, x, 674, w, 34, 13, muted);
            if (settingsTab == SettingsTab.Controls)
            {
                Text("LB / RB airbrakes · Y / Triangle recover · START pause. Fixed layout.", x, 708, w, 29, 13, muted);
                var replay = ReplayPolishController.Instance;
                string ghostStatus = !preferences.GhostEnabled ? "Ghost disabled. Enable to display a saved lap." : replay ? replay.StatusMessage : "Complete an eligible lap to create your first ghost.";
                Text(ghostStatus, x, 744, w, 40, 13, muted);
            }
            if (!string.IsNullOrEmpty(saveError)) Text(saveError, x, 777, w, 27, 14, amber);
            if (Button("RESET DEFAULTS", "", x, 804, 245, false, 42, "reset defaults")) ResetPreferences();
            if (Button("BACK", BackPrompt, 70, 794, 365, false, 52, "back")) Back();
            Text(gamepadActive ? "DPAD  NAVIGATE / ADJUST     A / CROSS  SELECT     B / CIRCLE  BACK" : "ARROWS  NAVIGATE / ADJUST     " + ConfirmPrompt + "  SELECT     ESC  BACK", 70, height - 33, width - 140, 25, 12, muted);
        }
        void DrawBindings()
        {
            Box(0, 0, width, height, new Color(.012f, .052f, .078f, .97f));
            Edition("VECTOR RUSH", "CONTROLS / KEYBOARD");
            Text("KEY BINDINGS.", 67, 135, 950, 114, 84, acid, true);
            Text(bindingCapture.HasValue ? "PRESS A KEY FOR " + PlayerPreferences.ActionLabel(bindingCapture.Value).ToUpperInvariant() + "  /  BACKSPACE OR B TO CANCEL" : "Select an action, then press a key. Controller layout is fixed.", 72, 261, width - 144, 37, 17, bindingCapture.HasValue ? acid : muted);
            float columnWidth = (width - 194) * .5f;
            for (int i = 0; i < bindingActions.Length; i++)
            {
                int column = i / 6, row = i % 6;
                var action = bindingActions[i];
                bool listening = bindingCapture == action;
                float x = 70 + column * (columnWidth + 54), y = 324 + row * 64;
                if (Button(PlayerPreferences.ActionLabel(action).ToUpperInvariant(), listening ? "LISTENING…" : KeyLabel(preferences.BindingFor(action)), x, y, columnWidth, listening, 53, BindingFocusId(action), !bindingCapture.HasValue || listening)) BeginBinding(action);
            }
            if (!string.IsNullOrEmpty(bindingError)) Text(bindingError, 72, 726, width - 144, 48, 16, bindingCapture.HasValue ? amber : muted);
            if (!string.IsNullOrEmpty(saveError)) Text(saveError, 72, 772, width - 144, 32, 14, amber);
            if (Button("BACK", BackPrompt, 70, 811, 300, false, 49, "back")) Back();
            if (Button("RESET KEYBOARD", "", 414, 811, 300, false, 49, "reset bindings", !bindingCapture.HasValue)) { preferences.ResetBindings(); SaveAndApplyPreferences(); bindingError = "Default keyboard bindings restored."; }
        }
        void SettingRule(float x, float y, float w, string id)
        {
            Box(x, y, w, 1, faint);
            if (focus != null && focus.SelectedAction == id)
            {
                Box(x - 14, y + 16, 3, 76, acid);
                Box(x - 7, y + 1, w + 7, 109, new Color(acid.r, acid.g, acid.b, .035f));
            }
        }
        void SliderRow(string label, string description, string id, float value, float minimum, float maximum, float x, float y, float w)
        {
            SettingRule(x, y, w, id);
            Text(label, x, y + 24, 355, 29, 16, ivory);
            Text(description, x, y + 59, 380, 28, 13, muted);
            float sliderX = x + w - 302, sliderWidth = 230;
            Rect hit = new Rect(sliderX - 10, y + 29, sliderWidth + 20, 48);
            int control = GUIUtility.GetControlID(id.GetHashCode(), FocusType.Passive, hit);
            bool captured = GUIUtility.hotControl == control;
            Event current = Event.current;
            bool useInputSystem = false;
#if ENABLE_INPUT_SYSTEM
            useInputSystem = Mouse.current != null;
#endif
            bool captureAfter = HandleSliderPointer(id, hit, minimum, maximum, sliderX, sliderWidth,
                current.type, current.button, current.mousePosition, useInputSystem, captured, out bool consumed);
            if (consumed) current.Use();
            if (captureAfter != captured) GUIUtility.hotControl = captureAfter ? control : 0;
            float fill = Mathf.InverseLerp(minimum, maximum, value);
            Box(sliderX, y + 53, sliderWidth, 3, faint); Box(sliderX, y + 53, sliderWidth * fill, 3, acid);
            Box(sliderX + sliderWidth * fill - 4, y + 45, 8, 19, ivory);
            Text(id == "steering sensitivity" ? value.ToString("0.00") : Mathf.RoundToInt(value * 100).ToString(), x + w - 60, y + 30, 60, 48, 31, ivory, true, TextAnchor.MiddleRight);
        }
        bool HandleSliderPointer(string id, Rect hit, float minimum, float maximum, float sliderX, float sliderWidth,
            EventType type, int button, Vector2 position, bool useInputSystem, bool captured, out bool consumed)
        {
            bool begin = type == EventType.MouseDown && button == 0 && hit.Contains(position);
            bool drag = type == EventType.MouseDrag && captured;
            bool release = type == EventType.MouseUp && button == 0 && captured;
            consumed = begin || drag || release;
            if (!consumed) return captured;
            if (begin)
            {
                SelectFocus(id);
                sliderGestureId = id;
                sliderGestureMinimum = minimum; sliderGestureMaximum = maximum;
                sliderGestureX = sliderX; sliderGestureWidth = sliderWidth;
                sliderGestureFrame = Time.frameCount;
                sliderReleaseSequenceAtStart = sliderInputReleaseSequence;
                sliderUsesInputSystem = useInputSystem; sliderReleaseQueued = false;
                sliderAllowsCachedRelease = sliderInputReleaseFrame == sliderGestureFrame && sliderInputReleaseCoalesced;
            }
            if (sliderGestureId != id) return !release;
            if (release && sliderUsesInputSystem)
            {
                // Native GUI Up can precede the InputSystem position update. Keep
                // the gesture after releasing GUI capture; Update commits only a
                // matching processed input release snapshot.
                sliderReleaseQueued = true;
                return false;
            }
            SetSlider(id, Mathf.Lerp(minimum, maximum, Mathf.InverseLerp(sliderX, sliderX + sliderWidth, position.x)));
            if (release) { SaveAndApplyPreferences(); sliderGestureId = null; }
            return !release;
        }
        void UpdateSliderInput()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null) return;
            float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            if (scale <= 0) return;
            Vector2 position = mouse.position.ReadValue();
            ProcessSliderInput(new Vector2(position.x / scale, (Screen.height - position.y) / scale),
                mouse.leftButton.wasReleasedThisFrame, mouse.leftButton.wasPressedThisFrame, Time.frameCount);
#endif
        }
        void ProcessSliderInput(Vector2 position, bool released, bool pressed, int frame)
        {
            // Cache on the processed release, never on later pointer movement.
            // A coalesced Down+Up may arrive before either native GUI callback.
            if (released && frame != sliderInputReleaseFrame)
            {
                sliderInputReleaseFrame = frame;
                sliderInputReleasePosition = position;
                sliderInputReleaseCoalesced = pressed;
                sliderInputReleaseSequence++;
            }
            if (sliderGestureId == null || !sliderUsesInputSystem || !sliderReleaseQueued) return;
            bool matchingRelease = sliderInputReleaseSequence > sliderReleaseSequenceAtStart ||
                sliderAllowsCachedRelease && sliderInputReleaseSequence == sliderReleaseSequenceAtStart;
            if (!matchingRelease || sliderInputReleaseFrame < sliderGestureFrame) return;
            SetSlider(sliderGestureId, Mathf.Lerp(sliderGestureMinimum, sliderGestureMaximum,
                Mathf.InverseLerp(sliderGestureX, sliderGestureX + sliderGestureWidth, sliderInputReleasePosition.x)));
            SaveAndApplyPreferences();
            sliderGestureId = null; sliderReleaseQueued = false;
        }
        void ToggleRow(string label, string description, string id, bool value, float x, float y, float w)
        {
            SettingRule(x, y, w, id);
            Text(label, x, y + 24, w - 150, 29, 16, ivory);
            Text(description, x, y + 59, w - 150, 28, 13, muted);
            if (Button(value ? "ON" : "OFF", "", x + w - 110, y + 31, 110, false, 46, id)) ToggleSetting(id);
            Box(x + w - 110, y + 77, 110, 2, value ? acid : faint);
        }
        void EnsureFocus(bool force = false)
        {
            if (!force && focus != null && focusPhase == director.Phase && focusSettingsOpen == settingsOpen && focusBindingsOpen == bindingsOpen && focusTab == settingsTab) return;
            bool sameScreen = focus != null && focusPhase == director.Phase && focusSettingsOpen == settingsOpen && focusBindingsOpen == bindingsOpen;
            int selectedIndex = force || focus != null && !sameScreen ? 0 : menuFocus;
            focusPhase = director.Phase; focusSettingsOpen = settingsOpen; focusBindingsOpen = bindingsOpen; focusTab = settingsTab;
            if (bindingsOpen) focus = new MenuFocusController(bindingActions.Select(BindingFocusId).Concat(new[] { "back", "reset bindings" }));
            else if (settingsOpen)
            {
                string[] rows = settingsTab == SettingsTab.Audio ? new[] { "master volume", "music volume", "effects volume" } : settingsTab == SettingsTab.Controls ? new[] { "steering sensitivity", "personal best ghost", "key bindings" } : new[] { "camera shake", "reduced interface motion", "hud scale" };
                focus = new MenuFocusController(new[] { "tab:audio", "tab:controls", "tab:comfort" }.Concat(rows).Concat(new[] { "reset defaults", "back" }));
            }
            else if (director.Phase == RacePhase.Menu) focus = new MenuFocusController(new[] { "start race", "settings", "controls", "quit" });
            else if (director.Phase == RacePhase.Paused) focus = new MenuFocusController(new[] { "resume", "restart race", "settings", "return to title", "quit" });
            else if (director.Phase == RacePhase.Finished) focus = new MenuFocusController(new[] { "race again", "return to title" });
            else focus = new MenuFocusController(new[] { "pause" });
            // Keep explicit test/navigation index when building initial focus; new pages start on their primary action.
            focus.Select(selectedIndex);
            menuFocus = focus.SelectedIndex;
        }
        void SelectFocus(string id)
        {
            EnsureFocus();
            for (int i = 0; i < focus.Actions.Count; i++) if (focus.Actions[i] == id) { focus.Select(i); menuFocus = i; selectionElapsed = 0f; return; }
        }
        void ActivateFocused()
        {
            string action = focus.Activate();
            switch (action)
            {
                case "start race": QueueMenuAction(() => director.StartRace()); break;
                case "resume": director.TogglePause(); break;
                case "restart race": case "race again": director.RestartRace(); break;
                case "settings": if (director.Phase == RacePhase.Menu) QueueMenuAction(OpenSettings); else OpenSettings(); break;
                case "controls": if (director.Phase == RacePhase.Menu) QueueMenuAction(OpenControls); else OpenControls(); break;
                case "return to title": ReturnToTitle(); break;
                case "quit": if (director.Phase == RacePhase.Menu) QueueMenuAction(Application.Quit); else Application.Quit(); break;
                case "tab:audio": SelectTab(SettingsTab.Audio); break;
                case "tab:controls": SelectTab(SettingsTab.Controls); break;
                case "tab:comfort": SelectTab(SettingsTab.Comfort); break;
                case "camera shake": case "personal best ghost": case "reduced interface motion": ToggleSetting(action); break;
                case "key bindings": OpenBindings(); break;
                case "reset defaults": ResetPreferences(); break;
                case "reset bindings": preferences.ResetBindings(); SaveAndApplyPreferences(); bindingError = "Default keyboard bindings restored."; break;
                case "back": Back(); break;
                default:
                    if (action.StartsWith("bind:", StringComparison.Ordinal) && Enum.TryParse(action.Substring(5), out PlayerAction binding)) BeginBinding(binding);
                    break;
            }
        }
        void QueueMenuAction(Action action)
        {
            if (action == null) return;
            RaceAudio.Instance?.PlayInterfaceConfirm();
            menuExitElapsed = 0f;
            menuExitActive = !preferences.ReducedInterfaceMotion;
            action();
        }
        void AdjustFocusedSetting(int direction)
        {
            string id = focus.SelectedAction;
            if (id.StartsWith("tab:", StringComparison.Ordinal)) { SelectTab((SettingsTab)(((int)settingsTab + direction + 3) % 3)); return; }
            switch (id)
            {
                case "master volume": SetSlider(id, preferences.MasterVolume + direction * .05f); break;
                case "music volume": SetSlider(id, preferences.MusicVolume + direction * .05f); break;
                case "effects volume": SetSlider(id, preferences.EffectsVolume + direction * .05f); break;
                case "steering sensitivity": SetSlider(id, preferences.SteeringSensitivity + direction * .05f); break;
                case "hud scale": SetSlider(id, preferences.HudScale + direction * .05f); break;
                case "camera shake": preferences.SetShake(direction > 0); break;
                case "personal best ghost": preferences.SetGhostEnabled(direction > 0); break;
                case "reduced interface motion": preferences.SetReducedInterfaceMotion(direction > 0); break;
                default: return;
            }
            SaveAndApplyPreferences();
        }
        void SetSlider(string id, float value)
        {
            if (id == "master volume") preferences.SetVolumes(value, preferences.MusicVolume, preferences.EffectsVolume);
            else if (id == "music volume") preferences.SetVolumes(preferences.MasterVolume, value, preferences.EffectsVolume);
            else if (id == "effects volume") preferences.SetVolumes(preferences.MasterVolume, preferences.MusicVolume, value);
            else if (id == "steering sensitivity") preferences.SetSteeringSensitivity(value);
            else if (id == "hud scale") preferences.SetHudScale(value);
            ApplyPreferences();
        }
        void ToggleSetting(string id)
        {
            if (id == "camera shake") preferences.SetShake(!preferences.ShakeEnabled);
            else if (id == "personal best ghost") preferences.SetGhostEnabled(!preferences.GhostEnabled);
            else if (id == "reduced interface motion") preferences.SetReducedInterfaceMotion(!preferences.ReducedInterfaceMotion);
            SaveAndApplyPreferences();
        }
        void SelectTab(SettingsTab tab)
        {
            settingsTab = tab; EnsureFocus(true); SelectFocus("tab:" + tab.ToString().ToLowerInvariant());
        }
        void OpenSettings() { settingsOpen = true; bindingsOpen = false; settingsTab = SettingsTab.Audio; menuFocus = 0; EnsureFocus(true); }
        void OpenControls() { settingsOpen = true; bindingsOpen = false; settingsTab = SettingsTab.Controls; menuFocus = 0; EnsureFocus(true); SelectFocus("tab:controls"); }
        void OpenBindings() { bindingsOpen = true; bindingCapture = null; bindingError = null; KeyboardCaptureActive = false; EnsureFocus(true); }
        void Back()
        {
            pauseConsumedFrame = Time.frameCount;
            if (bindingCapture.HasValue) { CancelBinding(); return; }
            if (bindingsOpen) { bindingsOpen = false; KeyboardCaptureActive = false; EnsureFocus(true); SelectFocus("key bindings"); }
            else { settingsOpen = false; menuFocus = 0; EnsureFocus(true); }
        }
        void ReturnToTitle()
        {
            settingsOpen = bindingsOpen = false; KeyboardCaptureActive = false; bindingCapture = null;
            pauseConsumedFrame = Time.frameCount; menuFocus = 0; director.ReturnToTitle(); EnsureFocus(true);
        }
        void BeginBinding(PlayerAction action)
        {
            bindingCapture = action; bindingCaptureFrame = Time.frameCount;
            bindingError = "Listening… Backspace or controller B / Circle cancels."; KeyboardCaptureActive = true;
        }
        void ConsumeBindingInput()
        {
            bindingInputConsumedFrame = pauseConsumedFrame = Time.frameCount;
            bindingInputNeedsRelease = true;
        }
        bool BlockConsumedBindingInput()
        {
            if (!bindingInputNeedsRelease) return false;
            // IMGUI and InputSystem can report the same native key in adjacent
            // player frames. Own both, then require one neutral frame before
            // accepting another menu action (including a newly rebound Confirm).
            if (Time.frameCount <= bindingInputConsumedFrame + 1) return true;
            bool held = false;
#if ENABLE_LEGACY_INPUT_MANAGER
            held = Input.anyKey;
#endif
#if ENABLE_INPUT_SYSTEM
            held |= Keyboard.current != null && Keyboard.current.anyKey.isPressed;
            held |= Mouse.current != null && Mouse.current.leftButton.isPressed;
            var pad = Gamepad.current;
            if (pad != null) held |= pad.buttonSouth.isPressed || pad.buttonEast.isPressed || pad.startButton.isPressed ||
                pad.dpad.ReadValue().sqrMagnitude > .1f || pad.leftStick.ReadValue().sqrMagnitude > .1f;
#endif
            if (!held) bindingInputNeedsRelease = false;
            return true;
        }
        void CancelBinding()
        {
            bindingCapture = null; bindingError = "Binding cancelled."; KeyboardCaptureActive = false;
            ConsumeBindingInput();
        }
        void CaptureBindingEvent() => HandleBindingEvent(Event.current);
        void HandleBindingEvent(Event current)
        {
            if (current == null || !bindingsOpen || !bindingCapture.HasValue || bindingCaptureFrame == Time.frameCount || current.type != EventType.KeyDown) return;
            KeyCode key = current.keyCode;
            if (key == KeyCode.Backspace) { CancelBinding(); current.Use(); return; }
            if (key == KeyCode.None) return;
            if (preferences.TryRebind(bindingCapture.Value, key, out string error))
            {
                bindingError = PlayerPreferences.ActionLabel(bindingCapture.Value) + " bound to " + KeyLabel(key) + ".";
                bindingCapture = null; KeyboardCaptureActive = false; ConsumeBindingInput();
                SaveAndApplyPreferences();
            }
            else bindingError = error + ". Choose another key, or Backspace to cancel.";
            current.Use();
        }
        void ResetPreferences() { preferences.ResetDefaults(); SaveAndApplyPreferences(); }
        static string BindingFocusId(PlayerAction action) => "bind:" + action;
        void SaveAndApplyPreferences()
        {
            ApplyPreferences();
            try { preferences.Save(); saveError = null; }
            catch (Exception error) { saveError = "Settings could not be saved. Changes apply for this session."; Debug.LogWarning("Player settings save failed: " + error.Message); }
        }
        void ApplyPreferences()
        {
            AudioListener.volume = preferences.MasterVolume;
            if (!chaseCamera) chaseCamera = FindAnyObjectByType<ChaseCamera>();
            if (chaseCamera) chaseCamera.ShakeEnabled = preferences.ShakeEnabled;
        }
        bool Button(string label, string hint, float x, float y, float w, bool primary = false, float h = 54, string focusId = null, bool enabled = true)
        {
            string id = focusId ?? label.ToLowerInvariant();
            Rect rect = new Rect(x, y, w, h);
            bool hover = enabled && rect.Contains(Event.current.mousePosition) && pointerActive;
            bool selected = enabled && focus != null && focus.SelectedAction == id;
            bool activeTab = settingsOpen && id == "tab:" + settingsTab.ToString().ToLowerInvariant();
            float emphasis = selected ? RacePresentationMotion.SelectionEmphasis(selectionElapsed, preferences.ReducedInterfaceMotion) : 0f;
            Color fill = primary ? acid : new Color(0, 0, 0, 0);
            if (!primary && (selected || hover || activeTab)) fill = new Color(acid.r, acid.g, acid.b, selected ? Mathf.Lerp(.065f, .12f, emphasis) : .065f);
            if (!enabled) fill = new Color(.1f, .15f, .17f, .12f);
            if (!primary && selected && uiSelection)
                Graphic(uiSelection, new Rect(x, y, w, h), new Color(1f, 1f, 1f, Mathf.Lerp(.72f, 1f, emphasis)));
            if (primary)
            {
                CutButtonFill(x, y, w, h, 16, fill);
                if (selected || hover) CutButtonOutline(x, y, w, h, 16, Mathf.Lerp(2f, 4f, emphasis), ivory);
            }
            else
            {
                Box(x, y, w, h, fill);
                Box(x, y + h - 1, w, 1, faint);
                if (selected || activeTab) Box(x, y, Mathf.Lerp(1.5f, 3f, emphasis), h, acid);
                if (hover && !selected) Box(x + w - 3, y + 12, 3, h - 24, ivory);
            }
            Color color = !enabled ? muted * .6f : primary ? new Color(.043f, .098f, .114f) : activeTab ? acid : ivory;
            textStyle.fontSize = 13;
            float hintWidth = string.IsNullOrEmpty(hint) ? 0 : hint == "→" ? 32 : Mathf.Min(w * .39f, textStyle.CalcSize(new GUIContent(hint)).x + 16);
            TrackedText(label, x + 20, y + 1, w - 40 - hintWidth, h - 2, 23, color, 1.2f, TextAnchor.MiddleLeft);
            if (hint == "→")
            {
                if (uiChevron) Graphic(uiChevron, new Rect(x + w - 37, y + (h - 22) * .5f, 22, 22), primary ? color : Color.white);
                else
                {
                    Vector2 p = new Vector2(x + w - 28, y + h / 2);
                    Line(p - Vector2.right * 14, p, 1.5f, primary ? color : muted);
                    Line(p, p + new Vector2(-6, -5), 1.5f, primary ? color : muted);
                    Line(p, p + new Vector2(-6, 5), 1.5f, primary ? color : muted);
                }
            }
            else if (hintWidth > 0) Text(hint, x + w - hintWidth - 18, y + 1, hintWidth, h - 2, 13, primary ? color : muted, false, TextAnchor.MiddleRight);
            int control = GUIUtility.GetControlID(id.GetHashCode(), FocusType.Passive, rect);
            Event e = Event.current;
            if (enabled && !bindingInputNeedsRelease && e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            { GUIUtility.hotControl = control; SelectFocus(id); pointerActive = true; e.Use(); }
            else if (e.type == EventType.MouseUp && e.button == 0 && GUIUtility.hotControl == control)
            {
                GUIUtility.hotControl = 0; e.Use();
                if (enabled && !bindingInputNeedsRelease && rect.Contains(e.mousePosition))
                {
                    if (inputEvidence) Debug.Log($"[InputEvidence] BUTTON_ACTIVATED label={label} frame={Time.frameCount} screen={CurrentScreen}");
                    return true;
                }
            }
            return false;
        }
        void Text(string value, float x, float y, float w, float h, int size, Color color, bool bold = false, TextAnchor align = TextAnchor.UpperLeft)
        {
            GUIStyle style = bold ? numberStyle : textStyle;
            color.a *= drawAlpha;
            style.fontSize = size; style.alignment = align; style.normal.textColor = color;
            GUI.Label(new Rect(x, y, w, h), value, style);
        }
        void TrackedText(string value, float x, float y, float w, float h, int size, Color color, float tracking, TextAnchor align = TextAnchor.UpperLeft)
        {
            if (string.IsNullOrEmpty(value)) return;
            textStyle.fontSize = size;
            string key = size + "|" + value;
            if (!trackedLabels.TryGetValue(key, out TrackedRun run))
            {
                run = new TrackedRun { glyphs = new GUIContent[value.Length], advances = new float[value.Length] };
                for (int i = 0; i < value.Length; i++)
                {
                    run.glyphs[i] = new GUIContent(value[i].ToString());
                    run.advances[i] = textStyle.CalcSize(run.glyphs[i]).x;
                    run.width += run.advances[i];
                }
                trackedLabels.Add(key, run);
            }
            float spacing = value.Length > 1 ? Mathf.Max(0, Mathf.Min(tracking, (w - run.width) / (value.Length - 1))) : 0;
            float total = run.width + spacing * (value.Length - 1);
            bool right = align == TextAnchor.UpperRight || align == TextAnchor.MiddleRight || align == TextAnchor.LowerRight;
            bool center = align == TextAnchor.UpperCenter || align == TextAnchor.MiddleCenter || align == TextAnchor.LowerCenter;
            float cursor = x + (right ? w - total : center ? (w - total) * .5f : 0);
            textStyle.alignment = (int)align >= 6 ? TextAnchor.LowerLeft : (int)align >= 3 ? TextAnchor.MiddleLeft : TextAnchor.UpperLeft;
            color.a *= drawAlpha;
            textStyle.normal.textColor = color;
            for (int i = 0; i < run.glyphs.Length; i++)
            {
                GUI.Label(new Rect(cursor, y, run.advances[i] + 3, h), run.glyphs[i], textStyle);
                cursor += run.advances[i] + spacing;
            }
        }
        void CutButtonFill(float x, float y, float w, float h, float cut, Color color)
        {
            Box(x, y, w, h - cut, color);
            Box(x, y + h - cut, w - cut, cut, color);
            Color before = GUI.color;
            color.a *= drawAlpha;
            GUI.color = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
            GUI.DrawTexture(new Rect(x + w - cut, y + h - cut, cut, cut), cutCorner);
            GUI.color = before;
        }
        void CutButtonOutline(float x, float y, float w, float h, float cut, float margin, Color color)
        {
            Vector2 topLeft = new Vector2(x - margin, y - margin), topRight = new Vector2(x + w + margin, y - margin);
            Vector2 cutTop = new Vector2(x + w + margin, y + h - cut), cutBottom = new Vector2(x + w - cut, y + h + margin);
            Vector2 bottomLeft = new Vector2(x - margin, y + h + margin);
            Line(topLeft, topRight, 1.5f, color); Line(topRight, cutTop, 1.5f, color);
            Line(cutTop, cutBottom, 1.5f, color); Line(cutBottom, bottomLeft, 1.5f, color);
            Line(bottomLeft, topLeft, 1.5f, color);
        }
        void SlantedBar(float x, float y, float w, float h, Color color)
        {
            Matrix4x4 before = GUI.matrix;
            Vector2 pointer = Event.current.mousePosition;
            Matrix4x4 slant = Matrix4x4.identity;
            slant.m01 = -Mathf.Tan(25f * Mathf.Deg2Rad);
            slant.m03 = -slant.m01 * (y + h * .5f);
            GUI.matrix = before * slant;
            Box(x, y, w, h, color);
            GUI.matrix = before; Event.current.mousePosition = pointer;
        }
        void Box(float x, float y, float w, float h, Color color)
        {
            Color before = GUI.color;
            // Text styles use display-space colors; textured IMGUI draws in a
            // linear player require conversion to keep the same approved palette.
            color.a *= drawAlpha;
            GUI.color = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
            GUI.DrawTexture(new Rect(x, y, w, h), white); GUI.color = before;
        }
        void Graphic(Texture2D texture, Rect rect, Color color)
        {
            if (!texture) return;
            Color before = GUI.color;
            color.a *= drawAlpha;
            GUI.color = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
            GUI.color = before;
        }
        void Line(Vector2 a, Vector2 b, float thickness, Color color)
        {
            if (Event.current.type != EventType.Repaint) return;
            Vector2 pointer = Event.current.mousePosition;
            Matrix4x4 before = GUI.matrix; Vector2 delta = b - a;
            GUI.matrix = before * Matrix4x4.TRS(new Vector3(a.x, a.y, 0), Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg), Vector3.one);
            color.a *= drawAlpha;
            Color previous = GUI.color; GUI.color = QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
            GUI.DrawTexture(new Rect(-.5f, -thickness * .5f, delta.magnitude + 1f, thickness), stroke);
            GUI.color = previous; GUI.matrix = before; Event.current.mousePosition = pointer;
        }
        void DirectionIcon(Vector2 p, int direction, Color color)
        {
            if (direction == 0)
            {
                Line(p - Vector2.right * 7, p + Vector2.right * 7, 2, color);
                Line(p - Vector2.right * 7, p + new Vector2(-2, -5), 2, color);
                Line(p + Vector2.right * 7, p + new Vector2(2, 5), 2, color);
                return;
            }
            float d = direction > 0 ? -1 : 1;
            Vector2 tip = p + new Vector2(0, 8 * d);
            Line(p - new Vector2(0, 8 * d), tip, 2, color);
            Line(tip, tip + new Vector2(-5, -6 * d), 2, color);
            Line(tip, tip + new Vector2(5, -6 * d), 2, color);
        }
        static string Percent(float value) => Mathf.RoundToInt(value * 100f) + "%";
        public static string KeyLabel(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Return: return "ENTER"; case KeyCode.KeypadEnter: return "NUM ENTER";
                case KeyCode.Escape: return "ESC"; case KeyCode.LeftShift: return "L SHIFT";
                case KeyCode.RightShift: return "R SHIFT"; case KeyCode.LeftControl: return "L CTRL";
                case KeyCode.RightControl: return "R CTRL"; case KeyCode.LeftAlt: return "L ALT";
                case KeyCode.RightAlt: return "R ALT"; default: return key.ToString().ToUpperInvariant();
            }
        }
        public static string GhostStateLabel(bool enabled, bool available, bool visible) => !enabled ? "GHOST DISABLED" : !available ? "GHOST UNAVAILABLE" : visible ? "GHOST ACTIVE" : "GHOST READY";
        public static string BoostStateLabel(bool boosting, float charge) => boosting ? "ACTIVE" : charge <= .02f ? "RECHARGING" : charge < .2f ? "LOW" : charge < .995f ? "RECHARGING" : "READY";
        public static string FinishStatusLabel(RacerFinishRecord record) => record == null ? "RACING" : record.HasFinishTime ? TimeLabel(record.FinishTime) : "DNF";
        public static string ResultRecordLabel(bool manualEligible, float bestLap, RaceRecordComparison record, string error)
        {
            if (!string.IsNullOrEmpty(error)) return error;
            if (!manualEligible || record != null && record.ExcludedAutomatedRun) return "AUTOMATED RUN / PERSONAL RECORDS EXCLUDED";
            if (bestLap <= 0) return "NO ELIGIBLE LAP / PERSONAL RECORD NOT SAVED";
            if (record == null) return "PERSONAL RECORD UNAVAILABLE";
            if (!record.HadPreviousRaceBest) return "FIRST VALID RACE RECORD";
            return "PREVIOUS BEST  " + TimeLabel(record.PreviousBestRace) + "    " + DeltaLabel(record.RaceDelta);
        }
        public static string TimeLabel(float seconds)
        {
            if (seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return "—:——.———";
            int millis = Mathf.Max(0, Mathf.FloorToInt(seconds * 1000));
            return (millis / 60000).ToString("00") + ":" + ((millis / 1000) % 60).ToString("00") + "." + (millis % 1000).ToString("000");
        }
        public static string SignedTimeLabel(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds)) return "—";
            return (seconds < 0 ? "−" : "+") + TimeLabel(Mathf.Max(.001f, Mathf.Abs(seconds)));
        }
        public static string DeltaLabel(float seconds) => float.IsNaN(seconds) || float.IsInfinity(seconds) ? "—" : (seconds < 0 ? "−" : "+") + Mathf.Abs(seconds).ToString("0.000");
        public string EvidencePage => CurrentScreen;
        /// <summary>Opt-in capture harness navigation only. Never changes race phase, timing, or records.</summary>
        public void ShowEvidencePage(string page)
        {
            if (!Stage2EvidenceProfile.Enabled) throw new InvalidOperationException("UI evidence navigation requires -stage2Evidence.");
            bindingCapture = null; KeyboardCaptureActive = false;
            switch (page)
            {
                case "settings": OpenSettings(); break;
                case "controls": OpenControls(); break;
                case "bindings": OpenControls(); OpenBindings(); break;
                case "comfort": OpenSettings(); SelectTab(SettingsTab.Comfort); break;
                case "title": case "race": settingsOpen = bindingsOpen = false; menuFocus = 0; EnsureFocus(true); break;
                default: throw new ArgumentException("Unknown interface page: " + page, nameof(page));
            }
        }
        void OnDestroy()
        {
            DisposeArcadeArt();
            menuExitActive = false;
            if (instance == this) { instance = null; KeyboardCaptureActive = false; pauseConsumedFrame = -1; }
            if (stroke) Destroy(stroke); if (sideShade) Destroy(sideShade); if (bottomShade) Destroy(bottomShade); if (mapShade) Destroy(mapShade); if (cutCorner) Destroy(cutCorner);
        }
    }
}
