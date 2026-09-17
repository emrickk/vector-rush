using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace VectorRush.Tests
{
    // These are source interaction tests. Reflection invokes real component methods,
    // but neither advances Unity physics nor supplies a native IMGUI event loop.
    internal sealed class AAARaceFixture : IDisposable
    {
        internal const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        readonly List<GameObject> objects = new List<GameObject>();
        internal readonly string Folder = Path.Combine(Path.GetTempPath(), "vector-aaa-" + Guid.NewGuid().ToString("N"));
        internal string RecordPath => Path.Combine(Folder, "records.json");
        internal TrackPath Track;
        internal RaceDirector Director;
        internal HoverVehicle Player, Rival;
        internal RaceFinishLedger Ledger => Get<RaceFinishLedger>(Director, "finishLedger");

        internal AAARaceFixture()
        {
            Track = Add<TrackPath>("AAA test track"); Track.Ensure();
            Player = Add<HoverVehicle>("AAA test player"); Player.Initialize(Track, true, 0);
            Rival = Add<HoverVehicle>("AAA test rival"); Rival.Initialize(Track, false, 1);
            Director = Add<RaceDirector>("AAA test director"); Invoke(Director, "Awake");
            Director.Initialize(Track, new List<HoverVehicle> { Player, Rival });
            Set(Director, "records", new RaceRecords(RecordPath));
            Set(Director, "courseIdentity", "test-course");
            Director.StartRace();
            Property(Director, "Phase", RacePhase.Racing);
        }
        internal T Add<T>(string name) where T : Component
        {
            var go = new GameObject(name); objects.Add(go); return go.AddComponent<T>();
        }
        internal static object Invoke(object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, Private).Invoke(target, args);
        internal static T Get<T>(object target, string field) => (T)target.GetType().GetField(field, Private).GetValue(target);
        internal static void Set(object target, string field, object value) => target.GetType().GetField(field, Private).SetValue(target, value);
        internal static void Property(object target, string property, object value) => target.GetType().GetProperty(property).SetValue(target, value);
        public void Dispose()
        {
            for (int i = objects.Count - 1; i >= 0; i--) if (objects[i]) UnityEngine.Object.DestroyImmediate(objects[i]);
            if (Directory.Exists(Folder)) Directory.Delete(Folder, true);
            Time.timeScale = 1f;
        }
    }

    public sealed class AAAInteractionTests
    {
        [TestCase(69.999f, true)]
        [TestCase(70f, true)]
        [TestCase(70.001f, false)]
        public void TimeoutStepAdjudicatesCrossingBeforeAtAndAfterDeadline(float crossing, bool accepted)
        {
            var ledger = new RaceFinishLedger(3, 0);
            ledger.Advance(10); ledger.Cross(0, 10);
            ledger.Advance(59.995f);
            ledger.BeginStep(.01f);
            Assert.That(ledger.IsComplete(1), Is.False, "Clock advancement must leave crossings eligible for adjudication.");
            Assert.That(ledger.Cross(1, crossing), Is.EqualTo(accepted));
            ledger.FinalizeTimeouts();
            Assert.That(ledger.AllComplete, Is.True);
            Assert.That(ledger.Finishes[1].RacerId, Is.EqualTo(1));
            Assert.That(ledger.Finishes[1].DidNotFinish, Is.EqualTo(!accepted));
            Assert.That(ledger.Finishes[1].Time, Is.EqualTo(accepted ? crossing : 0));
            Assert.That(ledger.Finishes[2].DidNotFinish, Is.True);
            Assert.That(ledger.PlayerFinishTime, Is.EqualTo(10));
            ledger.FinalizeTimeouts();
            Assert.That(ledger.Finishes.Count, Is.EqualTo(3));
        }

        [Test] public void ExactDeadlineClockStillAcceptsCrossingBeforeFinalization()
        {
            var ledger = new RaceFinishLedger(2, 0);
            ledger.Advance(10); ledger.Cross(0, 10);
            ledger.BeginStep(60);
            Assert.That(ledger.Cross(1, 70), Is.True);
            ledger.FinalizeTimeouts();
            Assert.That(ledger.Finishes[1].DidNotFinish, Is.False);
        }

        [TestCase(false)] [TestCase(true)]
        public void DirectorSamplesCrossingsInTimeoutStepAndFreezesAcceptedOrDNFCraft(bool late)
        {
            using (var race = new AAARaceFixture())
            {
                // Establish a real ordered-gate crossing on the next Sample, while
                // directly seeding the preceding validated lap state (no physics run).
                var progress = race.Rival.ProgressTracker;
                AAARaceFixture.Property(progress, "HasStarted", true);
                AAARaceFixture.Property(progress, "CompletedLaps", 2);
                AAARaceFixture.Property(progress, "NextCheckpoint", RaceProgress.SectorCount);
                AAARaceFixture.Property(progress, "LastProgress", .999f);
                var frame = race.Track.Evaluate(.001f);
                race.Rival.Body.position = frame.Position + frame.Up * race.Rival.HoverHeight;
                race.Rival.Body.isKinematic = false;
                race.Rival.Body.linearVelocity = frame.Forward * 50;
                race.Ledger.Advance(10); race.Ledger.Cross(0, 10);
                race.Ledger.Advance(60 - Time.fixedDeltaTime * (late ? .25f : .75f));
                AAARaceFixture.Property(race.Director, "Phase", RacePhase.Finished);
                AAARaceFixture.Invoke(race.Director, "FixedUpdate");
                Assert.That(progress.CompletedLaps, Is.EqualTo(3), "Fixture must actually sample the finish gate.");
                Assert.That(race.Ledger.Finishes.Count, Is.EqualTo(2));
                Assert.That(race.Ledger.Finishes[1].DidNotFinish, Is.EqualTo(late));
                Assert.That(race.Rival.Body.isKinematic, Is.True);
                Assert.That(race.Rival.Body.detectCollisions, Is.False);
                Assert.That(race.Director.FinishTime, Is.EqualTo(10));
            }
        }

        [TestCase("manual", false)]
        [TestCase("start", true)]
        [TestCase("update", true)]
        [TestCase("simulation", true)]
        [TestCase("vehicleUpdate", true)]
        public void DirectorRecordsOnlyEntirelyManualRacesAndResetsEligibilityOnRestart(string automation, bool excluded)
        {
            using (var race = new AAARaceFixture())
            {
                if (automation == "start")
                {
                    race.Player.AutopilotForTesting = true;
                    race.Director.RestartRace();
                    AAARaceFixture.Property(race.Director, "Phase", RacePhase.Racing);
                }
                else if (automation != "manual")
                {
                    race.Player.AutopilotForTesting = true;
                    if (automation == "update") AAARaceFixture.Invoke(race.Director, "Update");
                    else
                    {
                        // Exercise the existing vehicle-to-director permission call,
                        // including automation entirely between director updates.
                        race.Player.Body.isKinematic = false;
                        AAARaceFixture.Invoke(race.Player, automation == "simulation" ? "FixedUpdate" : "Update");
                    }
                }
                race.Player.AutopilotForTesting = false;
                Assert.That(race.Director.AutomatedRecordExcluded, Is.EqualTo(excluded));
                AAARaceFixture.Property(race.Director, "BestLap", 40f);
                race.Ledger.Advance(125); race.Ledger.Cross(0, 125);
                AAARaceFixture.Invoke(race.Director, "FixedUpdate");
                Assert.That(race.Director.Phase, Is.EqualTo(RacePhase.Finished));
                Assert.That(race.Director.NewPersonalBest, Is.EqualTo(!excluded));
                var saved = new RaceRecords(race.RecordPath).Get("test-course", RaceRecords.DrivingRules);
                Assert.That(saved == null, Is.EqualTo(excluded));
                race.Director.RestartRace();
                Assert.That(race.Director.AutomatedRecordExcluded, Is.False);
                Assert.That(race.Director.FinishRecords, Is.Empty);
            }
        }

        [Test] public void DuplicateKeysNormalizeToIndependentMinimaAndStayNormalizedAfterImprovement()
        {
            using (var race = new AAARaceFixture())
            {
                Directory.CreateDirectory(race.Folder);
                File.WriteAllText(race.RecordPath, "{\"version\":1,\"records\":[" +
                    "{\"course\":\"c\",\"rules\":\"r\",\"lap\":40,\"race\":125}," +
                    "{\"course\":\"c\",\"rules\":\"r\",\"lap\":50,\"race\":160}," +
                    "{\"course\":\"c\",\"rules\":\"r\",\"lap\":38,\"race\":140}," +
                    "null,{\"course\":\"c\",\"rules\":\"r\",\"lap\":-1,\"race\":1}," +
                    "{\"course\":\"other\",\"rules\":\"r\",\"lap\":55,\"race\":170}]}");
                var records = new RaceRecords(race.RecordPath);
                Assert.That(records.Get("c", "r").lap, Is.EqualTo(38));
                Assert.That(records.Get("c", "r").race, Is.EqualTo(125));
                Assert.That(records.Record("c", "r", 39, 124, false), Is.True);
                var reloaded = new RaceRecords(race.RecordPath);
                Assert.That(reloaded.Get("c", "r").lap, Is.EqualTo(38));
                Assert.That(reloaded.Get("c", "r").race, Is.EqualTo(124));
                Assert.That(reloaded.Get("other", "r").lap, Is.EqualTo(55));
                Assert.That(Regex.Matches(File.ReadAllText(race.RecordPath), "\"course\"\\s*:\\s*\"c\"").Count, Is.EqualTo(1));
                Assert.That(reloaded.Record("c", "r", 45, 150, false), Is.False);
            }
        }

        [TestCase("{broken")]
        [TestCase("{\"version\":99,\"records\":[]}")]
        [TestCase("{\"version\":1,\"records\":null}")]
        public void CorruptOrUnsupportedRecordsDoNotEscapeAndCanBeReplaced(string json)
        {
            using (var race = new AAARaceFixture())
            {
                Directory.CreateDirectory(race.Folder); File.WriteAllText(race.RecordPath, json);
                var store = new RaceRecords(race.RecordPath);
                Assert.That(store.Get("c", "r"), Is.Null);
                Assert.That(store.Record("c", "r", 40, 125, false), Is.True);
                Assert.That(new RaceRecords(race.RecordPath).Get("c", "r").race, Is.EqualTo(125));
            }
        }

        [Test] public void WriteAndCleanupFailurePreserveOldRecordsAndDoNotInterruptResults()
        {
            using (var race = new AAARaceFixture())
            {
                var original = new RaceRecords(race.RecordPath);
                original.Record("test-course", RaceRecords.DrivingRules, 50, 150, false);
                string previousDisk = File.ReadAllText(race.RecordPath);
                bool cleanupCalled = false;
                Action<string, string> failedWrite = (path, json) => { File.WriteAllText(path, "partial"); throw new IOException("injected write failure"); };
                Action<string> failedCleanup = path => { cleanupCalled = true; throw new UnauthorizedAccessException("injected cleanup failure"); };
                var store = (RaceRecords)Activator.CreateInstance(typeof(RaceRecords), AAARaceFixture.Private, null,
                    new object[] { race.RecordPath, failedWrite, failedCleanup }, null);
                AAARaceFixture.Set(race.Director, "records", store);
                AAARaceFixture.Property(race.Director, "BestLap", 40f);
                race.Ledger.Advance(125); race.Ledger.Cross(0, 125);
                LogAssert.Expect(LogType.Warning, new Regex("Could not save personal best: injected write failure"));
                LogAssert.Expect(LogType.Warning, new Regex("Could not clean up personal best temporary file: injected cleanup failure"));
                Assert.DoesNotThrow(() => AAARaceFixture.Invoke(race.Director, "FixedUpdate"));
                Assert.That(cleanupCalled, Is.True);
                Assert.That(race.Director.Phase, Is.EqualTo(RacePhase.Finished));
                Assert.That(race.Director.NewPersonalBest, Is.False);
                Assert.That(store.Get("test-course", RaceRecords.DrivingRules).race, Is.EqualTo(150));
                Assert.That(File.ReadAllText(race.RecordPath), Is.EqualTo(previousDisk));
                Assert.That(new RaceRecords(race.RecordPath).Get("test-course", RaceRecords.DrivingRules).lap, Is.EqualTo(50));
            }
        }

        [Test] public void CleanupFailureAfterSuccessfulReplaceKeepsTheCommittedImprovement()
        {
            using (var race = new AAARaceFixture())
            {
                new RaceRecords(race.RecordPath).Record("c", "r", 50, 150, false);
                Action<string, string> write = File.WriteAllText;
                Action<string> cleanup = path => throw new IOException("injected cleanup failure after replace");
                var store = (RaceRecords)Activator.CreateInstance(typeof(RaceRecords), AAARaceFixture.Private, null,
                    new object[] { race.RecordPath, write, cleanup }, null);
                LogAssert.Expect(LogType.Warning, new Regex("Could not clean up personal best temporary file: injected cleanup failure after replace"));
                Assert.That(store.Record("c", "r", 40, 125, false), Is.True);
                Assert.That(store.Get("c", "r").race, Is.EqualTo(125));
                Assert.That(new RaceRecords(race.RecordPath).Get("c", "r").lap, Is.EqualTo(40));
            }
        }

        [TestCase(KeyCode.Return)] [TestCase(KeyCode.Space)] [TestCase(KeyCode.KeypadEnter)]
        public void MenuGUIKeyEventClassifierConsumesActivationKeys(KeyCode key)
        {
            // Event.current is only authoritative during Unity's native OnGUI pass.
            // Exercise the same production predicate with explicit event instances
            // instead of manufacturing a GUI context that this fixture does not own.
            var keyEvent = new Event { type = EventType.KeyDown, keyCode = key };
            Assert.That(PlayerPreferences.ConsumeMenuGUIKeyEvent(keyEvent), Is.True);
            Assert.That(keyEvent.type, Is.EqualTo(EventType.Used));
            var mouseEvent = new Event { type = EventType.MouseUp, button = 0 };
            // Outside OnGUI Unity can expose mouseUp as Ignore. Assert that this
            // keyboard-only helper leaves the native event state untouched.
            var mouseTypeBefore = mouseEvent.type;
            var mouseRawTypeBefore = mouseEvent.rawType;
            Assert.That(PlayerPreferences.ConsumeMenuGUIKeyEvent(mouseEvent), Is.False);
            Assert.That(mouseEvent.type, Is.EqualTo(mouseTypeBefore));
            Assert.That(mouseEvent.rawType, Is.EqualTo(mouseRawTypeBefore));
        }
    }

#if ENABLE_INPUT_SYSTEM
    public sealed class AAASettingsInputTests
    {
        AAARaceFixture race;
        ProductionSettingsUI settings;
        RaceHUD hud;
        Gamepad pad;
        Keyboard keyboard;
        object inputManager;
        PropertyInfo playerUpdates;
        bool previousPlayerUpdates;
        InputSettings.UpdateMode previousMode;

        [SetUp] public void SetUp()
        {
            inputManager = typeof(InputSystem).GetField("s_Manager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            playerUpdates = inputManager.GetType().GetProperty("runPlayerUpdatesInEditMode");
            Assert.That(playerUpdates, Is.Not.Null, "InputSystem EditMode override must match the existing input fixture.");
            previousPlayerUpdates = (bool)playerUpdates.GetValue(inputManager);
            previousMode = InputSystem.settings.updateMode;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            playerUpdates.SetValue(inputManager, true);
            pad = InputSystem.AddDevice<Gamepad>(); pad.MakeCurrent();
            keyboard = InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent();
            race = new AAARaceFixture(); race.Director.TogglePause();
            settings = race.Add<ProductionSettingsUI>("AAA test settings"); settings.Initialize(race.Director);
            hud = race.Add<RaceHUD>("AAA test HUD"); hud.Initialize(race.Director);
            Frame(new GamepadState());
            // Enable InputSystem press tracking before the tested transitions.
            _ = pad.buttonSouth.wasPressedThisFrame; _ = pad.buttonEast.wasPressedThisFrame;
            _ = pad.buttonNorth.wasPressedThisFrame; _ = pad.dpad.down.wasPressedThisFrame;
            _ = keyboard.enterKey.wasPressedThisFrame; _ = keyboard.escapeKey.wasPressedThisFrame;
            _ = keyboard.f1Key.wasPressedThisFrame; _ = keyboard.rKey.wasPressedThisFrame;
        }

        [TearDown] public void TearDown()
        {
            race?.Dispose();
            if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (playerUpdates != null)
            {
                InputSystem.settings.updateMode = previousMode;
                playerUpdates.SetValue(inputManager, previousPlayerUpdates);
            }
            Time.timeScale = 1;
        }

        void Frame(GamepadState state, params Key[] keys)
        {
            // EditMode reflection does not advance Time.frameCount. Reset only the
            // production frame stamps between simulated render frames, never between
            // the two component Update calls whose ordering is under test.
            if (settings != null)
                foreach (string field in new[] { "processedFrame", "ownedFrame", "actionFrame" }) AAARaceFixture.Set(settings, field, -1);
            InputSystem.QueueStateEvent(pad, state);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            typeof(InputSystem).GetMethod("Update", BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] { typeof(InputUpdateType) }, null).Invoke(null, new object[] { InputUpdateType.Dynamic });
            pad.MakeCurrent(); keyboard.MakeCurrent();
        }
        void Updates(bool settingsFirst)
        {
            if (settingsFirst) AAARaceFixture.Invoke(settings, "Update");
            AAARaceFixture.Invoke(hud, "Update");
            AAARaceFixture.Invoke(race.Director, "Update");
            if (!settingsFirst) AAARaceFixture.Invoke(settings, "Update");
        }
        void Open(int focus)
        {
            AAARaceFixture.Invoke(settings, "SetOpen", true);
            AAARaceFixture.Set(settings, "focus", focus);
        }

        [TestCase(true, true, RacePhase.Menu, 0)] [TestCase(false, true, RacePhase.Menu, 0)]
        [TestCase(true, false, RacePhase.Menu, 0)] [TestCase(false, false, RacePhase.Menu, 0)]
        [TestCase(true, true, RacePhase.Paused, 0)] [TestCase(false, true, RacePhase.Paused, 0)]
        [TestCase(true, false, RacePhase.Paused, 0)] [TestCase(false, false, RacePhase.Paused, 0)]
        [TestCase(true, true, RacePhase.Paused, 1)] [TestCase(false, true, RacePhase.Paused, 1)]
        [TestCase(true, false, RacePhase.Paused, 1)] [TestCase(false, false, RacePhase.Paused, 1)]
        [TestCase(true, true, RacePhase.Finished, 0)] [TestCase(false, true, RacePhase.Finished, 0)]
        [TestCase(true, false, RacePhase.Finished, 0)] [TestCase(false, false, RacePhase.Finished, 0)]
        public void ClosingSettingsCannotActivateUnderlyingMenuInEitherUpdateOrder(bool settingsFirst, bool controller, RacePhase phase, int menuFocus)
        {
            AAARaceFixture.Property(race.Director, "Phase", phase);
            AAARaceFixture.Set(hud, "focusPhase", phase); AAARaceFixture.Set(hud, "menuFocus", menuFocus);
            Open(14);
            Frame(controller ? new GamepadState().WithButton(GamepadButton.South) : new GamepadState(),
                controller ? Array.Empty<Key>() : new[] { Key.Enter });
            Updates(settingsFirst);
            Assert.That(ProductionSettingsUI.IsOpen, Is.False);
            Assert.That(ProductionSettingsUI.BlocksRaceInput, Is.True);
            Assert.That(race.Director.Phase, Is.EqualTo(phase));
            Frame(new GamepadState()); Updates(settingsFirst);
            Assert.That(ProductionSettingsUI.BlocksRaceInput, Is.False);
            Frame(controller ? new GamepadState().WithButton(GamepadButton.South) : new GamepadState(),
                controller ? Array.Empty<Key>() : new[] { Key.Enter });
            Updates(settingsFirst);
            Assert.That(race.Director.Phase, Is.EqualTo(phase == RacePhase.Paused && menuFocus == 0 ? RacePhase.Racing : RacePhase.Countdown),
                "A fresh press must still activate the underlying menu.");
        }

        [TestCase(true, true)] [TestCase(false, true)] [TestCase(true, false)] [TestCase(false, false)]
        public void OpeningSettingsOwnsSimultaneousAcceptInEitherUpdateOrder(bool settingsFirst, bool controller)
        {
            Frame(controller ? new GamepadState().WithButton(GamepadButton.North).WithButton(GamepadButton.South) : new GamepadState(),
                controller ? Array.Empty<Key>() : new[] { Key.F1, Key.Enter });
            Updates(settingsFirst);
            Assert.That(ProductionSettingsUI.IsOpen, Is.True);
            Assert.That(race.Director.Phase, Is.EqualTo(RacePhase.Paused));
            Assert.That(AAARaceFixture.Get<int>(settings, "rebinding"), Is.EqualTo(-1));
        }

        [TestCase(true)] [TestCase(false)]
        public void ControllerBCancelsEachRebindingBeforeClosingPanel(bool settingsFirst)
        {
            for (int index = 5; index <= 12; index++)
            {
                Open(index);
                Frame(new GamepadState().WithButton(GamepadButton.South)); Updates(settingsFirst);
                Assert.That(AAARaceFixture.Get<int>(settings, "rebinding"), Is.EqualTo(index - 5));
                Frame(new GamepadState()); Updates(settingsFirst);
                Frame(new GamepadState().WithButton(GamepadButton.East)); Updates(settingsFirst);
                Assert.That(AAARaceFixture.Get<int>(settings, "rebinding"), Is.EqualTo(-1));
                Assert.That(ProductionSettingsUI.IsOpen, Is.True);
                Frame(new GamepadState().WithButton(GamepadButton.DpadDown)); Updates(settingsFirst);
                Assert.That(AAARaceFixture.Get<int>(settings, "focus"), Is.EqualTo(index + 1));
                Frame(new GamepadState()); Updates(settingsFirst);
            }
            Frame(new GamepadState().WithButton(GamepadButton.East)); Updates(settingsFirst);
            Assert.That(ProductionSettingsUI.IsOpen, Is.False);
            Assert.That(race.Director.Phase, Is.EqualTo(RacePhase.Paused));
        }

        [TestCase(true)] [TestCase(false)]
        public void EscapeCancelsBindingThenClosesWithoutResumingEvenWhenDirectorRunsFirst(bool directorFirst)
        {
            Open(5); AAARaceFixture.Invoke(settings, "Activate", 5);
            for (int step = 0; step < 2; step++)
            {
                Frame(new GamepadState(), Key.Escape);
                if (directorFirst) AAARaceFixture.Invoke(race.Director, "Update");
                Updates(!directorFirst);
                Assert.That(race.Director.Phase, Is.EqualTo(RacePhase.Paused));
                Assert.That(AAARaceFixture.Get<int>(settings, "rebinding"), Is.EqualTo(-1));
                Assert.That(ProductionSettingsUI.IsOpen, Is.EqualTo(step == 0));
                Frame(new GamepadState()); Updates(!directorFirst);
            }
        }

        [Test] public void BackActivationAndReopenResetPendingBinding()
        {
            Open(5); AAARaceFixture.Invoke(settings, "Activate", 5);
            // Invoke the same callback used by mouse Back. Actual GUI.Button hit
            // testing/focus requires the native event loop and remains a parent check.
            AAARaceFixture.Invoke(settings, "Activate", 14);
            Assert.That(AAARaceFixture.Get<int>(settings, "rebinding"), Is.EqualTo(-1));
            Assert.That(ProductionSettingsUI.BlocksRaceInput, Is.True);
            AAARaceFixture.Invoke(settings, "SetOpen", true);
            Assert.That(AAARaceFixture.Get<int>(settings, "rebinding"), Is.EqualTo(-1));
            Assert.That(ProductionSettingsUI.IsOpen, Is.True);
        }

        [TestCase(true)] [TestCase(false)]
        public void ControllerToggleCloseAndReopenCannotRetainBindingCapture(bool settingsFirst)
        {
            Open(5); AAARaceFixture.Invoke(settings, "Activate", 5);
            Frame(new GamepadState().WithButton(GamepadButton.North)); Updates(settingsFirst);
            Assert.That(ProductionSettingsUI.IsOpen, Is.False);
            Assert.That(AAARaceFixture.Get<int>(settings, "rebinding"), Is.EqualTo(-1));
            Frame(new GamepadState()); Updates(settingsFirst);
            Frame(new GamepadState().WithButton(GamepadButton.North)); Updates(settingsFirst);
            Assert.That(ProductionSettingsUI.IsOpen, Is.True);
            Assert.That(AAARaceFixture.Get<int>(settings, "rebinding"), Is.EqualTo(-1));
            Assert.That(race.Director.Phase, Is.EqualTo(RacePhase.Paused));
        }
    }
#endif
}
