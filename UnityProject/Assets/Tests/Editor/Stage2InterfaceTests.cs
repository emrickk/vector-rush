using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using System.Reflection;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace VectorRush.Tests
{
    public sealed class Stage2InterfaceTests
    {
        [TestCase(false, true, true, "GHOST DISABLED")]
        [TestCase(true, false, false, "GHOST UNAVAILABLE")]
        [TestCase(true, true, false, "GHOST READY")]
        [TestCase(true, true, true, "GHOST ACTIVE")]
        public void GhostPromptUsesEnabledAvailableAndVisibleState(bool enabled, bool available, bool visible, string expected)
            => Assert.That(RaceHUD.GhostStateLabel(enabled, available, visible), Is.EqualTo(expected));

        [TestCase(true, .5f, "ACTIVE")]
        [TestCase(false, 0f, "RECHARGING")]
        [TestCase(false, .1f, "LOW")]
        [TestCase(false, .6f, "RECHARGING")]
        [TestCase(false, 1f, "READY")]
        public void EnergyStatusCommunicatesActiveLowAndRecharging(bool active, float charge, string expected)
            => Assert.That(RaceHUD.BoostStateLabel(active, charge), Is.EqualTo(expected));

        [Test]
        public void ClassificationNeverInventsATimeForPendingOrDnfPilot()
        {
            var lifecycle = new RaceFinishLifecycle(new[] {
                new RacerIdentity("player", "YOU", true), new RacerIdentity("rival", "NOVA", false)
            }, 1f);
            lifecycle.Advance(10f);
            var player = lifecycle.RecordFinish("player", 9.5f);
            Assert.That(RaceHUD.FinishStatusLabel(player), Is.EqualTo("00:09.500"));
            Assert.That(RaceHUD.FinishStatusLabel(null), Is.EqualTo("RACING"));
            lifecycle.Advance(2f);
            Assert.That(RaceHUD.FinishStatusLabel(lifecycle.Records[1]), Is.EqualTo("DNF"));
        }

        [Test]
        public void ResultMessagesDistinguishSaveFailureExcludedAndNoEligibleLap()
        {
            Assert.That(RaceHUD.ResultRecordLabel(true, 30f, null, "Could not save"), Is.EqualTo("Could not save"));
            Assert.That(RaceHUD.ResultRecordLabel(false, 30f, null, ""), Does.Contain("EXCLUDED"));
            Assert.That(RaceHUD.ResultRecordLabel(true, 0f, null, ""), Does.Contain("NO ELIGIBLE LAP"));
            Assert.That(RaceHUD.ResultRecordLabel(true, 30f, null, ""), Does.Contain("UNAVAILABLE"));
        }

        [Test]
        public void BindingPromptsUseReadableActualKeys()
        {
            Assert.That(RaceHUD.KeyLabel(KeyCode.LeftShift), Is.EqualTo("L SHIFT"));
            Assert.That(RaceHUD.KeyLabel(KeyCode.Return), Is.EqualTo("ENTER"));
            Assert.That(RaceHUD.KeyLabel(KeyCode.B), Is.EqualTo("B"));
        }

        [Test]
        public void ComfortPreferencesPersistAndClampWithoutChangingBindings()
        {
            WithProfile(directory => {
                var preferences = new PlayerPreferences(directory);
                preferences.TryRebind(PlayerAction.Boost, KeyCode.B, out _);
                preferences.SetHudScale(9f);
                preferences.SetReducedInterfaceMotion(true);
                preferences.Save();
                var loaded = new PlayerPreferences(directory);
                Assert.That(loaded.HudScale, Is.EqualTo(1.2f));
                Assert.That(loaded.ReducedInterfaceMotion, Is.True);
                Assert.That(loaded.BindingFor(PlayerAction.Boost), Is.EqualTo(KeyCode.B));
                loaded.SetHudScale(float.NaN);
                Assert.That(loaded.HudScale, Is.EqualTo(1f));
                loaded.SetHudScale(0);
                Assert.That(loaded.HudScale, Is.EqualTo(.85f));
            });
        }

        [TestCase(1)]
        [TestCase(2)]
        public void LegacyFilesKeepCustomBindingsAndReceiveComfortDefaults(int version)
        {
            WithProfile(directory => {
                var preferences = new PlayerPreferences(directory);
                preferences.TryRebind(PlayerAction.Boost, KeyCode.B, out _);
                preferences.SetVolumes(.37f, .21f, .54f);
                preferences.SetHudScale(1.2f);
                preferences.SetReducedInterfaceMotion(true);
                preferences.Save();
                string json = File.ReadAllText(preferences.FilePath).Replace("\"version\":3", "\"version\":" + version);
                json = System.Text.RegularExpressions.Regex.Replace(json, ",\"hudScale\":[0-9.]+", "");
                json = json.Replace(",\"reducedInterfaceMotion\":true", "");
                File.WriteAllText(preferences.FilePath, json);
                var loaded = new PlayerPreferences(directory);
                Assert.That(loaded.BindingFor(PlayerAction.Boost), Is.EqualTo(KeyCode.B));
                Assert.That(loaded.MasterVolume, Is.EqualTo(.37f));
                Assert.That(loaded.HudScale, Is.EqualTo(1f));
                Assert.That(loaded.ReducedInterfaceMotion, Is.False);
                Assert.That(loaded.GhostEnabled, Is.True);
            });
        }

        [Test]
        public void ResetKeyboardPreservesSoundAndComfortPreferences()
        {
            WithProfile(directory => {
                var preferences = new PlayerPreferences(directory);
                preferences.TryRebind(PlayerAction.Boost, KeyCode.B, out _);
                preferences.SetVolumes(.4f, .3f, .2f);
                preferences.SetHudScale(1.15f);
                preferences.SetReducedInterfaceMotion(true);
                preferences.ResetBindings();
                Assert.That(preferences.BindingFor(PlayerAction.Boost), Is.EqualTo(KeyCode.Space));
                Assert.That(preferences.MasterVolume, Is.EqualTo(.4f));
                Assert.That(preferences.HudScale, Is.EqualTo(1.15f));
                Assert.That(preferences.ReducedInterfaceMotion, Is.True);
            });
        }

        [Test]
        public void PlayerSettingsOwnPauseAndBackReturnsToThePausedRace()
        {
            using (var race = new AAARaceFixture())
            {
                race.Director.TogglePause();
                var hud = race.Add<RaceHUD>("Stage 2 interface");
                hud.Initialize(race.Director);
                AAARaceFixture.Set(hud, "preferences", new PlayerPreferences(race.Folder));
                AAARaceFixture.Invoke(hud, "OpenSettings");
                Assert.That(hud.CurrentScreen, Is.EqualTo("settings-audio"));
                Assert.That(RaceHUD.BlocksPauseInput, Is.True);
                AAARaceFixture.Invoke(hud, "Back");
                Assert.That(hud.CurrentScreen, Is.EqualTo("paused"));
                Assert.That(race.Director.Phase, Is.EqualTo(RacePhase.Paused));
                Assert.That(hud.FocusedAction, Is.EqualTo("resume"));
                Assert.That(RaceHUD.BlocksPauseInput, Is.True, "The closing frame still owns its pause/back press.");
            }
        }

        [Test]
        public void SettingsNavigationReachesControlsBindingsAndComfort()
        {
            using (var race = new AAARaceFixture())
            {
                race.Director.TogglePause();
                var hud = race.Add<RaceHUD>("Stage 2 interface");
                hud.Initialize(race.Director);
                AAARaceFixture.Set(hud, "preferences", new PlayerPreferences(race.Folder));
                AAARaceFixture.Invoke(hud, "OpenSettings");
                AAARaceFixture.Invoke(hud, "SelectFocus", "tab:controls");
                AAARaceFixture.Invoke(hud, "ActivateFocused");
                Assert.That(hud.CurrentScreen, Is.EqualTo("settings-controls"));
                AAARaceFixture.Invoke(hud, "SelectFocus", "key bindings");
                AAARaceFixture.Invoke(hud, "ActivateFocused");
                Assert.That(hud.CurrentScreen, Is.EqualTo("bindings"));
                AAARaceFixture.Invoke(hud, "Back");
                Assert.That(hud.FocusedAction, Is.EqualTo("key bindings"));
                AAARaceFixture.Invoke(hud, "SelectFocus", "tab:comfort");
                AAARaceFixture.Invoke(hud, "ActivateFocused");
                Assert.That(hud.CurrentScreen, Is.EqualTo("settings-comfort"));
            }
        }

        [Test]
        public void KeyboardSliderAdjustmentClampsAtMaximumWithoutWrappingToMute()
        {
            using (var race = new AAARaceFixture())
            {
                race.Director.TogglePause();
                var hud = race.Add<RaceHUD>("Stage 2 interface");
                hud.Initialize(race.Director);
                var preferences = new PlayerPreferences(race.Folder);
                AAARaceFixture.Set(hud, "preferences", preferences);
                AAARaceFixture.Invoke(hud, "OpenSettings");
                AAARaceFixture.Invoke(hud, "SelectFocus", "master volume");
                AAARaceFixture.Invoke(hud, "AdjustFocusedSetting", 1);
                Assert.That(preferences.MasterVolume, Is.EqualTo(1f));
                AAARaceFixture.Invoke(hud, "AdjustFocusedSetting", -1);
                Assert.That(preferences.MasterVolume, Is.EqualTo(.95f).Within(.0001f));
                Assert.That(new PlayerPreferences(race.Folder).MasterVolume, Is.EqualTo(.95f).Within(.0001f));
            }
        }

        [TestCase("master volume")]
        [TestCase("music volume")]
        public void RapidPointerDownThenUpAppliesAndSavesReleasePositionWithoutDragEvents(string id)
        {
            using (var race = new AAARaceFixture())
            {
                race.Director.TogglePause();
                var hud = race.Add<RaceHUD>("Stage 2 rapid slider");
                hud.Initialize(race.Director);
                var preferences = new PlayerPreferences(race.Folder);
                AAARaceFixture.Set(hud, "preferences", preferences);
                AAARaceFixture.Invoke(hud, "OpenSettings");
                bool captured = SliderPointer(hud, id, EventType.MouseDown, new Vector2(1426, 350), false, out bool consumed);
                Assert.That(captured, Is.True);
                Assert.That(consumed, Is.True);
                Assert.That(id == "master volume" ? preferences.MasterVolume : preferences.MusicVolume, Is.GreaterThan(.98f));

                // Reproduce native coalescing: no intermediate MouseDrag exists.
                captured = SliderPointer(hud, id, EventType.MouseUp, new Vector2(1240, 350), captured, out consumed);
                Assert.That(captured, Is.False);
                Assert.That(consumed, Is.True);
                float expected = 42f / 230f;
                Assert.That(id == "master volume" ? preferences.MasterVolume : preferences.MusicVolume, Is.EqualTo(expected).Within(.0001f));
                var relaunched = new PlayerPreferences(race.Folder);
                Assert.That(id == "master volume" ? relaunched.MasterVolume : relaunched.MusicVolume, Is.EqualTo(expected).Within(.0001f));
            }
        }

        [Test]
        public void UncapturedPointerReleaseCannotChangeOrSaveASlider()
        {
            using (var race = new AAARaceFixture())
            {
                race.Director.TogglePause();
                var hud = race.Add<RaceHUD>("Stage 2 slider ownership");
                hud.Initialize(race.Director);
                var preferences = new PlayerPreferences(race.Folder);
                AAARaceFixture.Set(hud, "preferences", preferences);
                AAARaceFixture.Invoke(hud, "OpenSettings");
                bool captured = SliderPointer(hud, "master volume", EventType.MouseUp, new Vector2(1240, 350), false, out bool consumed);
                Assert.That(captured, Is.False);
                Assert.That(preferences.MasterVolume, Is.EqualTo(1f));
                Assert.That(File.Exists(preferences.FilePath), Is.False);
                Assert.That(consumed, Is.False);
            }
        }
        [TestCase("master volume", true)]
        [TestCase("master volume", false)]
        [TestCase("music volume", true)]
        [TestCase("music volume", false)]
        public void StaleGuiReleaseReconcilesWithProcessedInputSnapshotInEitherOrder(string id, bool inputUpdateFirst)
        {
            using (var race = new AAARaceFixture())
            {
                race.Director.TogglePause();
                var hud = race.Add<RaceHUD>("Stage 2 ordered slider release");
                hud.Initialize(race.Director);
                var preferences = new PlayerPreferences(race.Folder);
                AAARaceFixture.Set(hud, "preferences", preferences);
                AAARaceFixture.Invoke(hud, "OpenSettings");
                int frame = Time.frameCount;
                Vector2 release = new Vector2(1240, 350);
                if (inputUpdateFirst)
                    AAARaceFixture.Invoke(hud, "ProcessSliderInput", release, true, true, frame);

                // GUI events both carry the old source position, exactly as in
                // the native failure. InputSystem coalesces Down+Up separately.
                bool captured = SliderPointer(hud, id, EventType.MouseDown, new Vector2(1426, 350), false, out _, true);
                captured = SliderPointer(hud, id, EventType.MouseUp, new Vector2(1426, 350), captured, out _, true);
                Assert.That(captured, Is.False);
                Assert.That(File.Exists(preferences.FilePath), Is.False, "A stale GUI release must not be persisted.");

                if (!inputUpdateFirst)
                    AAARaceFixture.Invoke(hud, "ProcessSliderInput", release, true, true, frame);
                else
                    AAARaceFixture.Invoke(hud, "ProcessSliderInput", new Vector2(1500, 600), false, false, frame + 1);

                float expected = 42f / 230f;
                Assert.That(id == "master volume" ? preferences.MasterVolume : preferences.MusicVolume, Is.EqualTo(expected).Within(.0001f));
                var relaunched = new PlayerPreferences(race.Folder);
                Assert.That(id == "master volume" ? relaunched.MasterVolume : relaunched.MusicVolume, Is.EqualTo(expected).Within(.0001f));
                AAARaceFixture.Invoke(hud, "ProcessSliderInput", new Vector2(900, 200), false, false, frame + 2);
                Assert.That(id == "master volume" ? preferences.MasterVolume : preferences.MusicVolume, Is.EqualTo(expected).Within(.0001f),
                    "Movement after release must not replace the saved release snapshot.");
            }
        }

        static bool SliderPointer(RaceHUD hud, string id, EventType type, Vector2 position, bool captured, out bool consumed, bool useInputSystem = false)
        {
            // Synthetic mouse Event.type is normalized outside native OnGUI.
            // Feed the same plain data that the native adapter passes to its handler.
            object[] arguments = { id, new Rect(1188, 330, 250, 48), 0f, 1f, 1198f, 230f, type, 0, position, useInputSystem, captured, false };
            bool result = (bool)AAARaceFixture.Invoke(hud, "HandleSliderPointer", arguments);
            consumed = (bool)arguments[11];
            return result;
        }
        static void WithProfile(Action<string> run)
        {
            string directory = Path.Combine(Path.GetTempPath(), "VectorRush-Stage2UI-" + Guid.NewGuid().ToString("N"));
            try { run(directory); }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }
    }
#if ENABLE_INPUT_SYSTEM
    // Real component methods consume InputSystem state and native-style IMGUI key
    // events. EditMode does not advance Time.frameCount; only the consumed-frame
    // stamp is advanced between simulated frames, never between competing consumers.
    public sealed class Stage2BindingInputTests
    {
        AAARaceFixture race;
        RaceHUD hud;
        PlayerPreferences preferences;
        Keyboard keyboard;
        Gamepad pad;
        object inputManager;
        PropertyInfo playerUpdates;
        bool previousPlayerUpdates;
        InputSettings.UpdateMode previousMode;

        [SetUp]
        public void SetUp()
        {
            inputManager = typeof(InputSystem).GetField("s_Manager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            playerUpdates = inputManager.GetType().GetProperty("runPlayerUpdatesInEditMode");
            previousPlayerUpdates = (bool)playerUpdates.GetValue(inputManager);
            previousMode = InputSystem.settings.updateMode;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            playerUpdates.SetValue(inputManager, true);
            keyboard = InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent();
            pad = InputSystem.AddDevice<Gamepad>(); pad.MakeCurrent();
            race = new AAARaceFixture(); race.Director.TogglePause();
            hud = race.Add<RaceHUD>("Stage 2 binding input"); hud.Initialize(race.Director);
            preferences = new PlayerPreferences(race.Folder);
            AAARaceFixture.Set(hud, "preferences", preferences);
            AAARaceFixture.Invoke(hud, "OpenControls");
            AAARaceFixture.Invoke(hud, "OpenBindings");
            Frame(new GamepadState());
            _ = keyboard.fKey.wasPressedThisFrame; _ = keyboard.backspaceKey.wasPressedThisFrame;
            _ = keyboard.enterKey.wasPressedThisFrame; _ = pad.buttonEast.wasPressedThisFrame;
        }

        [TearDown]
        public void TearDown()
        {
            race?.Dispose();
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (pad != null && pad.added) InputSystem.RemoveDevice(pad);
            if (playerUpdates != null)
            {
                InputSystem.settings.updateMode = previousMode;
                playerUpdates.SetValue(inputManager, previousPlayerUpdates);
            }
            Time.timeScale = 1f;
        }

        void Frame(GamepadState gamepad, params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.QueueStateEvent(pad, gamepad);
            typeof(InputSystem).GetMethod("Update", BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] { typeof(InputUpdateType) }, null).Invoke(null, new object[] { InputUpdateType.Dynamic });
            keyboard.MakeCurrent(); pad.MakeCurrent();
        }
        void UpdateHud() => AAARaceFixture.Invoke(hud, "Update");
        void Begin(PlayerAction action)
        {
            AAARaceFixture.Invoke(hud, "SelectFocus", "bind:" + action);
            AAARaceFixture.Invoke(hud, "ActivateFocused");
            AAARaceFixture.Set(hud, "bindingCaptureFrame", Time.frameCount - 1);
        }
        void NativeKey(KeyCode key)
        {
            // Event.current exists only inside native OnGUI. Invoke its identical
            // handler explicitly instead of assuming a synthetic current event.
            var current = new Event { type = EventType.KeyDown, keyCode = key };
            AAARaceFixture.Invoke(hud, "HandleBindingEvent", current);
        }
        void PastDeliveryFrames() => AAARaceFixture.Set(hud, "bindingInputConsumedFrame", Time.frameCount - 2);

        [TestCase(true)]
        [TestCase(false)]
        public void RebindingConfirmCannotReopenCaptureRegardlessOfConsumerOrder(bool updateFirst)
        {
            Begin(PlayerAction.Confirm);
            Frame(new GamepadState(), Key.F);
            if (updateFirst) UpdateHud();
            NativeKey(KeyCode.F);
            UpdateHud();
            NativeKey(KeyCode.F); // A duplicate native event must not reopen the row.
            UpdateHud();
            Assert.That(preferences.BindingFor(PlayerAction.Confirm), Is.EqualTo(KeyCode.F));
            Assert.That(new PlayerPreferences(race.Folder).BindingFor(PlayerAction.Confirm), Is.EqualTo(KeyCode.F));
            Assert.That(RaceHUD.KeyboardCaptureActive, Is.False);
            Assert.That(hud.CurrentScreen, Is.EqualTo("bindings"));
            Assert.That(hud.FocusedAction, Is.EqualTo("bind:Confirm"));

            PastDeliveryFrames();
            UpdateHud(); // The held key is still fenced even after the delivery frames.
            Assert.That(RaceHUD.KeyboardCaptureActive, Is.False);
            Frame(new GamepadState()); UpdateHud(); // Neutral frame rearms navigation.
            Frame(new GamepadState(), Key.F); UpdateHud();
            Assert.That(RaceHUD.KeyboardCaptureActive, Is.True, "A fresh press of the new Confirm binding must activate the row.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BackspaceCancelsExactlyOneLayerInEitherConsumerOrder(bool updateFirst)
        {
            Begin(PlayerAction.Throttle);
            Frame(new GamepadState(), Key.Backspace);
            if (updateFirst) UpdateHud();
            NativeKey(KeyCode.Backspace);
            UpdateHud();
            Assert.That(RaceHUD.KeyboardCaptureActive, Is.False);
            Assert.That(hud.CurrentScreen, Is.EqualTo("bindings"));
            Assert.That(preferences.BindingFor(PlayerAction.Throttle), Is.EqualTo(KeyCode.W));
            PastDeliveryFrames(); UpdateHud();
            Assert.That(hud.CurrentScreen, Is.EqualTo("bindings"), "Holding cancel must not close the binding list.");
            Frame(new GamepadState()); UpdateHud();
            Frame(new GamepadState(), Key.Backspace); UpdateHud();
            Assert.That(hud.CurrentScreen, Is.EqualTo("settings-controls"), "A fresh cancel press may leave the binding list.");
            Assert.That(race.Director.Phase, Is.EqualTo(RacePhase.Paused));
        }

        [Test]
        public void ControllerCancelRequiresReleaseBeforeBackingOutOfBindings()
        {
            Begin(PlayerAction.Throttle);
            Frame(new GamepadState().WithButton(GamepadButton.East)); UpdateHud(); UpdateHud();
            Assert.That(RaceHUD.KeyboardCaptureActive, Is.False);
            Assert.That(hud.CurrentScreen, Is.EqualTo("bindings"));
            PastDeliveryFrames(); UpdateHud();
            Assert.That(hud.CurrentScreen, Is.EqualTo("bindings"));
            Frame(new GamepadState()); UpdateHud();
            Frame(new GamepadState().WithButton(GamepadButton.East)); UpdateHud();
            Assert.That(hud.CurrentScreen, Is.EqualTo("settings-controls"));
        }
    }
#endif

}
